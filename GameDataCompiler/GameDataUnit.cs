using System.Reflection;
using System.Runtime.Loader;
using GameCore;
using GameData;
using BigfileBuilder;
using DataBuildSystem;

namespace DataBuildSystem
{
    // A GameDataUnit is a single .dll file that contains compiled C# code structured as data.
    // Classes can contain IDataFile objects that can cook data, these 'compilers' need to
    // be tracked. When such a compiler is out-of-date, the GameDataCompilerLog will be updated
    // and the GameDataBigfiles and GameDataData will be rebuilt.

    public class GameDataUnits
    {
        private IRootDataUnit RootDataUnit { get; set; }
        private List<GameDataUnit> DataUnits { get; set; }
        private SignatureDataBase SignatureDatabase { get; set; }

        private Assembly GameDataAssembly { get; set; }

        public Assembly Initialize(string gameDataDllFilename)
        {
            SignatureDatabase = new SignatureDataBase();

            // Instantiate the data root (which is the root DataUnit)
            GameDataAssembly = LoadAssembly(gameDataDllFilename);
            RootDataUnit = GameDataUnit.FindRoot(GameDataAssembly);
            return GameDataAssembly;
        }

        private static Assembly LoadAssembly(string gameDataDllFilename)
        {
            AssemblyLoadContext gameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            var dllBytes = File.ReadAllBytes(Path.Join(BuildSystemConfig.GddPath, gameDataDllFilename));
            Assembly gameDataAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));
            return gameDataAssembly;
        }

        private static void BuildCompilerSignatures(List<IDataFile> dataFiles)
        {
            var memoryStream = new MemoryStream();
            var memoryWriter = new BinaryMemoryWriter();
            memoryWriter.Open(memoryStream, ArchitectureUtils.LittleArchitecture64);
            foreach (var cl in dataFiles)
            {
                memoryWriter.Reset();
                cl.BuildSignature(memoryWriter);
                cl.Signature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
            memoryWriter.Close();
        }

        public State Cook(string srcPath, string dstPath)
        {
            // Make sure the directory structure of @SrcPath is duplicated at @DstPath
            DirUtils.DuplicateFolderStructure(BuildSystemConfig.SrcPath, BuildSystemConfig.DstPath);

            // Determine the state of each DataUnit and load their compiler log
            foreach (var gdu in DataUnits)
            {
                gdu.DetermineState();
            }

            // Collect the data units that need to be rebuilt
            List<GameDataUnit> rebuilt = new();

            // Load the SignatureDatabase
            SignatureDatabase.Load(GameDataPath.GetFilePathFor("GameData", EGameData.GameDataSignatureDb));

            // For each data unit, using the loaded compiler log and a newly build compiler log, merge them
            // and use the merged compiler log to execute all compilers.
            foreach (var gdu in DataUnits)
            {
                // Collect the data compilers and build their signatures
                var currentLog = GameDataUnit.CollectDataCompilers(gdu.DataUnit);
                BuildCompilerSignatures(currentLog);

                // Load the compiler log so that they can be used to verify the source files
                var loadedLog = GameDataUnit.LoadDataFileLog(GameDataPath.GetFilePathFor(gdu.Name+"."+gdu.Id, EGameData.GameDataCompilerLog), currentLog);

                var mergeResult = GameDataFileLog.Merge(loadedLog, currentLog, out var mergedLog);

                // Cook, fundamentally this will make sure all the cooked files are up-to-date
                var cookResult = GameDataFileLog.Cook(mergedLog, out var finalDataFiles);
                if (cookResult != 0|| mergeResult != 0 || gdu.OutOfDate)
                {
                    SignatureDatabase.RemovePrimary(gdu.Index);
                    rebuilt.Add(gdu);
                }

                // Save the compiler log ?
                gdu.DataFileLog.DataFiles = finalDataFiles;
                gdu.DetermineState();
            }

            // Save all the out-of-date GameDataUnits, this means saving
            // the compiler log and the bigfile
            foreach (var gdu in rebuilt)
            {
                GameDataFileLog.Save(BuildSystemConfig.Platform, GameDataPath.GetFilePathFor(gdu.Name+"."+gdu.Id, EGameData.GameDataCompilerLog), gdu.DataFileLog.DataFiles);
            }

            foreach (var gdu in rebuilt)
            {
                gdu.SaveBigfile(gdu.Id, SignatureDatabase);
            }

            // Save the SignatureDatabase
            // - Signature = Bigfile index, Bigfile file index
            SignatureDatabase.Save(GameDataPath.GetFilePathFor("GameData", EGameData.GameDataSignatureDb));

            // Finally save the
            // - Game Code header file
            // - Game Code data file, this will be a Bigfile + Bigfile TOC
            var codeFileInfo = new FileInfo(GameDataPath.GetFilePathFor("GameData", EGameData.GameDataCppCode));
            var codeFileStream = codeFileInfo.Create();
            var codeFileWriter = new StreamWriter(codeFileStream);

            var bigfileGameCodeDataFilepath = GameDataPath.GetFilePathFor("GameData", EGameData.GameDataCppData);
            var bigfileDataFileInfo = new FileInfo(bigfileGameCodeDataFilepath);
            var bigfileDataStream = new FileStream(bigfileDataFileInfo.FullName, FileMode.Create);
            var bigfileDataStreamWriter = ArchitectureUtils.CreateBinaryFileWriter(bigfileDataStream, BuildSystemConfig.Platform);

            CppCodeStream2.Write2(BuildSystemConfig.Platform, RootDataUnit, codeFileWriter, bigfileDataStreamWriter, out var dataUnitsStreamPositions, out var dataUnitsStreamSizes);
            bigfileDataStreamWriter.Close();
            bigfileDataStream.Close();
            codeFileWriter.Close();
            codeFileStream.Close();

            var bigfileGameCodeFiles = new List<BigfileFile>();
            for (int i=0; i<dataUnitsStreamPositions.Count; ++i)
            {
                bigfileGameCodeFiles.Add(new BigfileFile() { Filename = "DataUnit", Offset = dataUnitsStreamPositions[i], Size = dataUnitsStreamSizes[i] });;
            }
            var bigfileGameCode = new Bigfile(0, bigfileGameCodeFiles);
            var bigfileGameCodeTocFilepath = GameDataPath.GetFilePathFor("GameData", EGameData.GameDataCppData);
            bigfileGameCodeTocFilepath = Path.ChangeExtension(bigfileGameCodeTocFilepath, BigfileConfig.BigFileTocExtension);
            BigfileToc.Save(bigfileGameCodeTocFilepath, [bigfileGameCode]);

            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            var currentDataUnits = GameDataUnit.CollectDataUnits(RootDataUnit);
            var idToDataUnit = new Dictionary<string, IDataUnit>(currentDataUnits.Count);
            foreach (var cdu in currentDataUnits)
            {
                idToDataUnit.Add(cdu.Signature, cdu);
            }

            var dataUnits = new Dictionary<uint, GameDataUnit>(currentDataUnits.Count + (currentDataUnits.Count / 4));

            var binaryFile = new BinaryFileReader();
            if (binaryFile.Open(Path.Join(dstPath, "GameDataUnits.log")))
            {
                var magic = binaryFile.ReadInt64();
                if (magic == StringTools.Encode_64_10('D', 'A', 'T', 'A', '.', 'U', 'N', 'I', 'T', 'S'))
                {
                    var numUnits = binaryFile.ReadInt32();
                    for (var i = 0; i < numUnits; i++)
                    {
                        var gdu = GameDataUnit.Load(binaryFile);
                        if (idToDataUnit.TryGetValue(gdu.Id, out IDataUnit du))
                        {
                            gdu.DataUnit = du;
                            idToDataUnit.Remove(gdu.Id);
                            dataUnits.Add(gdu.Index, gdu);
                        }
                    }
                }

                binaryFile.Close();
            }

            // Any new DataUnit? -> create them with an index that is not used
            var index = (uint)0;
            foreach ((string id, IDataUnit du) in idToDataUnit)
            {
                while (dataUnits.ContainsKey(index))
                    index++;
                var gdu = new GameDataUnit { Id = id, Name = gddPath, Index = index, DataUnit = du };
                dataUnits.Add(gdu.Index, gdu);
                index++;
            }

            // Finally, build the official list of DataUnits
            DataUnits = new List<GameDataUnit>(dataUnits.Count);
            foreach (var item in dataUnits)
            {
                DataUnits.Add(item.Value);
            }
        }

        public void Save(string dstPath)
        {
            var filepath = Path.Join(dstPath, "GameDataUnits.log");
            var writer = ArchitectureUtils.CreateBinaryFileWriter(filepath, LocalizerConfig.Platform);

            writer.Write(StringTools.Encode_64_10('D', 'A', 'T', 'A', '.', 'U', 'N', 'I', 'T', 'S'));
            writer.Write(DataUnits.Count);
            foreach (var gdu in DataUnits)
            {
                gdu.Save(writer);
            }

            writer.Close();
        }
    }

    // Each GameDataUnit consists of 8 files, these are:
    //    GameDataDll
    //    GameDataCompilerLog
    //    GameDataData
    //
    //    BigFileData
    //    BigFileToc
    //    BigFileFilenames
    //    BigFileHashes
    //

    public class GameDataUnit
    {
        public string Name { get;  init; }
        public string Id { get;  init; }
        public uint Index { get;  init; }
        public IDataUnit DataUnit { get; set; }
        private State[] States { get; init; }
        private Dependency Dep { get; init; }

        public GameDataFileLog DataFileLog { get; init; }

        public bool OutOfDate
        {
            get
            {
                foreach (var s in States)
                {
                    if (!s.IsOk)
                        return true;
                }

                return false;
            }
        }

        public void DetermineState()
        {
            Dep.Update(delegate(short idx, State state)
            {
                States[idx] = state;
                return DataCookResult.None;
            });
        }

        public static List<IDataFile> LoadDataFileLog(string filepath, List<IDataFile> currentDataFileLog)
        {
            return GameDataFileLog.Load(filepath, currentDataFileLog);
        }

        public void Save(IBinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Id);
            writer.Write(Index);
            foreach (var t in States)
                writer.Write(t.AsInt8);

            Dep.WriteTo(writer);
        }

        public static GameDataUnit Load(IBinaryReader reader)
        {
            var states = new State[Enum.GetValues<EGameData>().Length];
            for (var i = 0; i < states.Length; ++i)
                states[i] = new State(reader.ReadInt8());
            var dep = Dependency.ReadFrom(reader);

            var name = reader.ReadString();
            var id = reader.ReadString(); var index = reader.ReadUInt32();
            var gdu = new GameDataUnit()
            {
                DataUnit = null,
                Id = id,
                Name = name,
                Index = index,
                States = states,
                Dep = dep
            };
            return gdu;
        }

        public void SaveBigfile(string name, ISignatureDataBase database)
        {
            var filename = name + GameDataPath.GetExtFor(EGameData.GduBigFileData);
            SaveBigfile(Index, filename, DataFileLog.DataFiles, database);
        }

        private static void SaveBigfile(uint bigfileIndex, string filename, List<IDataFile> dataFiles, ISignatureDataBase database)
        {
            var memoryStream = new MemoryStream();
            var memoryWriter = new BinaryMemoryWriter();

            var bigfileFiles = new List<BigfileFile>();
            foreach (var cl in dataFiles)
            {
                var bigfileFile = new BigfileFile() { Filename = cl.CookedFilename };

                memoryWriter.Reset();
                cl.BuildSignature(memoryWriter);
                cl.Signature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                if (database.Register(cl.Signature, bigfileIndex, (uint)bigfileFiles.Count))
                {
                    bigfileFiles.Add(bigfileFile);
                }
            }

            var bigfile = new Bigfile(bigfileIndex, bigfileFiles);

            var bigFiles = new List<Bigfile>() { bigfile };
            BigfileBuilder.BigfileBuilder.Save(BuildSystemConfig.PubPath, BuildSystemConfig.DstPath, filename, bigFiles);
        }

        public static IRootDataUnit FindRoot(Assembly assembly)
        {
            try
            {
                IRootDataUnit root = AssemblyUtil.Create1<IRootDataUnit>(assembly);
                return root;
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static List<IDataFile> CollectDataCompilers(IDataUnit dataUnit)
        {
            var compilers = new List<IDataFile>();
            {
                Walk(dataUnit, delegate(object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return false;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataFile c)
                        {
                            compilers.Add(c);
                            return false;
                        }

                        return true;
                    },
                    delegate(Type type)
                    {
                        if (type  == null)
                            return false;

                        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                            return true;

                        foreach (var i in type.GetInterfaces())
                        {
                            if (i == typeof(IDataUnit))
                                return false;
                        }

                        return true;
                    }
                );
            }
            return compilers;
        }

        public static List<IDataUnit> CollectDataUnits(IRootDataUnit dataUnit)
        {
            var dataUnits = new List<IDataUnit>();
            {
                Walk(dataUnit, delegate(object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return false;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataUnit du)
                        {
                            dataUnits.Add(du);
                        }

                        return true;
                    }, delegate(Type type)
                    {
                        return (type != null && !type.IsPrimitive && !type.IsEnum);
                    }
                );
            }
            return dataUnits;
        }

        private delegate bool OnObjectDelegate(object compound);
        private delegate bool OnTypeDelegate(Type type);


        private static bool Walk(object compound, OnObjectDelegate ood, OnTypeDelegate ocw)
        {
            try
            {
                Queue<object> compounds = new();
                compounds.Enqueue(compound);

                while (compounds.Count > 0)
                {
                    compound = compounds.Dequeue();
                    var compoundTypeInfo = compound.GetType();

                    if (!ood(compound))
                        continue;

                    if (compoundTypeInfo.IsArray)
                    {
                        // Analyze element type
                        var elementType = compoundTypeInfo.GetElementType();
                        if (ocw(elementType))
                        {
                            if (compound is Array objectArray)
                            {
                                for (var i = 0; i < objectArray.Length; i++)
                                {
                                    var e = objectArray.GetValue(i);
                                    if (e != null)
                                    {
                                        if (ocw(e.GetType()))
                                            compounds.Enqueue(e);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var fields = compoundTypeInfo.GetFields(
                            BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.GetField
                        );
                        foreach (var f in fields)
                        {
                            var o = f.GetValue(compound);
                            if (o == null || f.IsInitOnly)
                                continue;

                            var objectTypeInfo = o.GetType();
                            if (objectTypeInfo.IsArray)
                            {
                                // Analyze element type
                                var elementType = objectTypeInfo.GetElementType();
                                if (ocw(elementType))
                                {
                                    if (o is Array objectArray)
                                    {
                                        for (var i = 0; i < objectArray.Length; i++)
                                        {
                                            var e = objectArray.GetValue(i);
                                            if (e != null)
                                            {
                                                if (ocw(e.GetType()))
                                                    compounds.Enqueue(e);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ocw(o.GetType()))
                                {
                                    compounds.Enqueue(o);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
