using System.Reflection;
using System.Runtime.Loader;
using GameCore;
using GameData;
using BigfileBuilder;

namespace DataBuildSystem
{
    // A GameDataUnit is a single .dll file that contains compiled C# code structured as data.
    // Classes can contain IDataFile objects that can cook data, these 'compilers' need to
    // be tracked. When such a compiler is out-of-date, the GameDataCompilerLog will be updated
    // and the GameDataBigfiles and GameDataData will be rebuilt.

    public class GameDataUnits
    {
        private IDataUnit RootDataUnit { get; set; }
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
            var dllBytes = File.ReadAllBytes(Path.Join(BuildSystemDefaultConfig.GddPath, gameDataDllFilename));
            Assembly gameDataAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));
            return gameDataAssembly;
        }

        public State Cook(string srcPath, string dstPath)
        {
            // Make sure the directory structure of @SrcPath is duplicated at @DstPath
            DirUtils.DuplicateFolderStructure(BuildSystemDefaultConfig.SrcPath, BuildSystemDefaultConfig.DstPath);

            // Determine the state of each DataUnit and load their compiler log
            foreach (var gdu in DataUnits)
            {
                gdu.DetermineState();

                // Load the compiler log so that they can be used to verify the source files
                gdu.LoadCompilerLog(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataCompilerLog));
            }

            // Collect the data units that need to be rebuilt
            List<GameDataUnit> rebuilt = new();

            // Load the SignatureDatabase
            SignatureDatabase.Load(GameDataPath.GetFilePathFor("SignatureDatabase", EGameData.SignatureDatabase));

            // For each data unit, using the loaded compiler log and a newly build compiler log, merge them
            // and use the merged compiler log to execute all compilers.
            foreach (var gdu in DataUnits)
            {
                var currentLog = GameDataUnit.CollectDataCompilers(gdu.DataUnit);
                var mergeResult = GameDataCompilerLog.Merge(gdu.CompilerLog.DataFiles, currentLog, out var mergedLog);

                // Cook, fundamentally this will make sure all the cooked files are up-to-date
                var cookResult = GameDataCompilerLog.Cook(mergedLog, out var finalDataFiles);
                if (cookResult != 0|| mergeResult != 0 || gdu.OutOfDate)
                {
                    SignatureDatabase.RemoveBigfile(gdu.Index);
                    rebuilt.Add(gdu);
                }

                // Save the compiler log ?
                gdu.CompilerLog.DataFiles = finalDataFiles;
                gdu.DetermineState();
            }

            // Save all the out-of-date GameDataUnits, this means saving
            // the compiler log and the bigfile
            foreach (var gdu in rebuilt)
            {
                gdu.CompilerLog.Save(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataCompilerLog));
            }
            foreach (var gdu in rebuilt)
            {
                gdu.SaveBigfile(gdu.Id, SignatureDatabase);
            }

            // Save the SignatureDatabase
            // - Signature = Bigfile index, Bigfile file index
            SignatureDatabase.Save(GameDataPath.GetFilePathFor("SignatureDatabase", EGameData.SignatureDatabase));

            // Finally save the
            // - Game Code header file
            // - Game Code data file, this will be a Bigfile + Bigfile TOC
            var codeFileInfo = new FileInfo(GameDataPath.GetFilePathFor("GameData", EGameData.GameCodeHeader));
            var codeFileStream = codeFileInfo.Create();
            var codeFileWriter = new StreamWriter(codeFileStream);

            var bigfileGameCodeDataFilepath = GameDataPath.GetFilePathFor("GameData", EGameData.BigFileData);
            var bigfileDataFileInfo = new FileInfo(bigfileGameCodeDataFilepath);
            var bigfileDataStream = new FileStream(bigfileDataFileInfo.FullName, FileMode.Create);
            var bigfileDataStreamWriter = ArchitectureUtils.CreateBinaryWriter(bigfileDataStream, BuildSystemDefaultConfig.Platform);

            CppCodeStream2.Write2(BuildSystemDefaultConfig.Platform, RootDataUnit, codeFileWriter, bigfileDataStreamWriter, out var dataUnitsStreamPositions, out var dataUnitsStreamSizes);
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
            var bigfileGameCodeTocFilepath = GameDataPath.GetFilePathFor("GameData", EGameData.BigFileToc);
            BigfileToc.Save(BuildSystemDefaultConfig.Platform, bigfileGameCodeTocFilepath, [bigfileGameCode]);

            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            var currentDataUnits = GameDataUnit.CollectDataUnits(RootDataUnit);
            var idToDataUnit = new Dictionary<string, IDataUnit>(currentDataUnits.Count);
            foreach (var cdu in currentDataUnits)
            {
                idToDataUnit.Add(cdu.GetType().GUID.ToString(), cdu);
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
            foreach (var item in idToDataUnit)
            {
                while (dataUnits.ContainsKey(index))
                    index++;
                var gdu = new GameDataUnit(gddPath, index, item.Value);
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
            var writer = ArchitectureUtils.CreateBinaryWriter(filepath, LocalizerConfig.Platform);

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
    //    GameDataRelocation
    //    BigFileData
    //    BigFileToc
    //    BigFileFilenames
    //    BigFileHashes
    //
    // The name of the file will come from the IDataUnit.UnitId.

    public class GameDataUnit
    {
        public string Name { get; private init; }
        public string Id { get; private init; }
        public uint Index { get; private init; }
        public IDataUnit DataUnit { get; set; }
        private State[] States { get; set; } = new State[Enum.GetValues<EGameData>().Length];
        private Dependency Dep { get; set; }

        public GameDataCompilerLog CompilerLog { get; set; }

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

        private GameDataUnit() : this(string.Empty, uint.MaxValue, null) { }

        public GameDataUnit(string dirPath, uint index, IDataUnit dataUnit)
        {
            DataUnit = dataUnit;
            Name = dataUnit.GetType().Namespace + "." + dataUnit.GetType().Name;
            Id = dataUnit.GetType().GUID.ToString();
            Index = index;
            Dep = new();

            for (var i = 0; i < States.Length; ++i)
            {
                States[i] = State.Missing;
            }

            foreach (var e in Enum.GetValues<EGameData>())
            {
                var unitName = e == EGameData.GameDataDll ? "GameData" : Name;
                var filename = Path.Join(dirPath, unitName) + GameDataPath.GetExtFor(e);
                Dep.Add((short)e, GameDataPath.GetPathFor(e), filename);
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

        public bool LoadCompilerLog(string filepath)
        {
            CompilerLog = new GameDataCompilerLog();
            return CompilerLog.Load(filepath);
        }

        public void SaveCompilerLog(string filepath)
        {
            CompilerLog.Save(filepath);
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
            GameDataUnit gdu = new() { DataUnit = null, Name = reader.ReadString(), Id = reader.ReadString(), Index = reader.ReadUInt32() };
            for (var i = 0; i < gdu.States.Length; ++i)
                gdu.States[i] = new State(reader.ReadInt8());
            gdu.Dep = Dependency.ReadFrom(reader);
            return gdu;
        }

        public void SaveBigfile(string name, ISignatureDataBase database)
        {
            var filename = name + GameDataPath.GetExtFor(EGameData.BigFileData);
            SaveBigfile(Index, filename, CompilerLog.DataFiles, database);
        }

        private static void SaveBigfile(uint bigfileIndex, string filename, List<IDataFile> dataFiles, ISignatureDataBase database)
        {
            var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

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
            bfb.Save(BuildSystemDefaultConfig.PubPath, BuildSystemDefaultConfig.DstPath, filename, bigFiles);
        }

        public static IDataUnit FindRoot(Assembly assembly)
        {
            try
            {
                IDataUnit root = AssemblyUtil.Create1<IRootDataUnit>(assembly);
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
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataFile c)
                        {
                            compilers.Add(c);
                            return true;
                        }

                        return false;
                    }
                );
            }
            return compilers;
        }

        public static List<IDataUnit> CollectDataUnits(IDataUnit dataUnit)
        {
            var dataUnits = new List<IDataUnit>();
            {
                Walk(dataUnit, delegate(object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataUnit du)
                        {
                            dataUnits.Add(du);
                        }

                        return false;
                    }
                );
            }
            return dataUnits;
        }

        private delegate bool OnObjectDelegate(object compound);

        private static bool ElementRequiresWalking(Type type)
        {
            if (type != null && !type.IsPrimitive && !type.IsEnum)
            {
                return !TypeInfo2.HasGenericInterface(type, typeof(IDataUnit));
            }

            return false;
        }

        private static bool ObjectRequiresWalking(Type type)
        {
            if (type != null && !type.IsPrimitive && !type.IsEnum)
            {
                return !TypeInfo2.HasGenericInterface(type, typeof(IDataUnit));
            }

            return false;
        }

        private static bool Walk(object compound, OnObjectDelegate ood)
        {
            try
            {
                Stack<object> compounds = new();
                compounds.Push(compound);

                while (compounds.Count > 0)
                {
                    compound = compounds.Pop();
                    var compoundTypeInfo = compound.GetType();

                    if (ood(compound))
                        continue;

                    if (compound is IDataFile)
                        continue;

                    if (compoundTypeInfo.IsArray)
                    {
                        // Analyze element type
                        var elementType = compoundTypeInfo.GetElementType();
                        if (ElementRequiresWalking(elementType))
                        {
                            if (compound is Array objectArray)
                            {
                                for (var i = 0; i < objectArray.Length; i++)
                                {
                                    var e = objectArray.GetValue(i);
                                    if (e != null)
                                    {
                                        if (ObjectRequiresWalking(e.GetType()))
                                            compounds.Push(e);
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
                                if (ElementRequiresWalking(elementType))
                                {
                                    if (o is Array objectArray)
                                    {
                                        for (var i = 0; i < objectArray.Length; i++)
                                        {
                                            var e = objectArray.GetValue(i);
                                            if (e != null)
                                            {
                                                if (ObjectRequiresWalking(e.GetType()))
                                                    compounds.Push(e);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ObjectRequiresWalking(o.GetType()))
                                {
                                    compounds.Push(o);
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
