using System.Reflection;
using System.Runtime.Loader;
using GameCore;
using GameData;
using BigfileBuilder;

namespace DataBuildSystem
{
    // A GameDataUnit is a single .dll file that contains compiled C# code structured as data.
    // Classes can contain FileId objects that hold a IDataCompiler, these compilers need to
    // be tracked. When such a compiler is out-of-date, the GameDataCompilerLog will be updated
    // and the GameDataBigfile and GameDataData will be rebuilt.

    public class GameDataUnits
    {
        private List<GameDataUnit> DataUnits { get; set; }
        private IDataUnit RootDataUnit { get; set; }
        private SignatureDataBase SignatureDatabase { get; set; }

        public Assembly Initialize(string gameDataDllFilename)
        {
            _gameDataAssembly = LoadAssembly(gameDataDllFilename);

            // Instantiate the data root (which is the root DataUnit)
            RootDataUnit = GameDataUnit.FindRoot(_gameDataAssembly);

            SignatureDatabase = new SignatureDataBase();

            return _gameDataAssembly;
        }

        private Assembly _gameDataAssembly;

        private static Assembly LoadAssembly(string gameDataDllFilename)
        {
            AssemblyLoadContext gameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            var dllBytes = File.ReadAllBytes(Path.Join(BuildSystemCompilerConfig.GddPath, gameDataDllFilename));
            Assembly gameDataAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));
            return gameDataAssembly;
        }


        private void PrepareDataCooking(List<IDataFile> compilers)
        {
            if (compilers.Count == 0)
                return;

            var memoryStream = new MemoryStream();
            var memoryWriter = new BinaryMemoryWriter();

            var signatureList = new List<KeyValuePair<Hash160, IDataFile>>(compilers.Count);
            foreach (var cl in compilers)
            {
                memoryWriter.Reset();
                cl.BuildSignature(memoryWriter);
                cl.Signature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                signatureList.Add(new KeyValuePair<Hash160, IDataFile>(cl.Signature, cl));
            }
        }

        public State Cook(string srcPath, string dstPath)
        {
            // Determine the state of each DataUnit and load their compiler log
            foreach (var gdu in DataUnits)
            {
                gdu.DetermineState();

                // Load the compiler log so that they can be used to verify the source files
                gdu.LoadCompilerLog(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataCompilerLog));
            }

            // Collect the data units that need to be rebuilt
            List<GameDataUnit> rebuilt = new();

            // For each data unit, using the loaded compiler log and a newly created compiler log, merge them
            // and use the merged compiler log to execute all compilers.
            foreach (var gdu in DataUnits)
            {
                var currentCompilers = GameDataUnit.CollectDataCompilers(gdu.DataUnit);
                var loadedCompilers = new List<IDataFile>(gdu.CompilerLog.CompilerLog);
                gdu.CompilerLog.Merge(loadedCompilers, currentCompilers, out var mergedCompilers);

                var result = gdu.CompilerLog.Cook(mergedCompilers, out var additionalDataFiles);
                if (result.IsOk)
                {
                    if (gdu.StateOf(EGameData.GameDataData).IsNotOk || gdu.StateOf(EGameData.BigFileData, EGameData.BigFileFilenames, EGameData.BigFileHashes, EGameData.BigFileToc).IsNotOk)
                    {
                        rebuilt.Add(gdu);
                    }
                }
                else
                {
                    rebuilt.Add(gdu);
                }

                gdu.DetermineState();
            }

            // Update the SignatureDatabase to ensure all the signatures have a BigfileIndex + FileIndex

            // Save all the out-of-date GameDataUnits, this means saving
            // - Bigfile data
            // - Bigfile TOC
            // - Bigfile filenames
            // - Bigfile hashes

            // Finally save the
            // - Game data file

            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            var idToDataUnit = new Dictionary<string, IDataUnit>();
            var currentDataUnits = GameDataUnit.CollectDataUnits(RootDataUnit);
            foreach (var cdu in currentDataUnits)
            {
                idToDataUnit.Add(cdu.UnitId, cdu);
            }

            var dataUnits = new Dictionary<uint, GameDataUnit>();

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

            // Finally, build the list of DataUnits
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
        public string Id { get; private init; }
        public uint Index { get; private init; }
        public IDataUnit DataUnit { get; set; }
        private State[] States { get; set; } = new State[Enum.GetValues<EGameData>().Length];
        private Dependency Dep { get; set; }

        public GameDataCompilerLog CompilerLog { get; set; }

        public State StateOf(EGameData u)
        {
            return States[(int)u];
        }

        public State StateOf(params EGameData[] pu)
        {
            var s = new State();
            foreach (var u in pu)
                s.Merge(States[(int)u]);
            return s;
        }

        private GameDataUnit() : this(string.Empty, uint.MaxValue, null) { }

        public GameDataUnit(string dirPath, uint index, IDataUnit dataUnit)
        {
            DataUnit = dataUnit;
            Id = dataUnit.UnitId;
            Index = index;
            Dep = new();

            for (var i = 0; i < States.Length; ++i)
            {
                States[i] = State.Missing;
            }

            foreach (var e in Enum.GetValues<EGameData>())
            {
                var unitName = e == EGameData.GameDataDll ? "GameData" : dataUnit.UnitId;
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
            CompilerLog = new GameDataCompilerLog(filepath);
            return CompilerLog.Load();
        }

        public void Save(IBinaryWriter writer)
        {
            writer.Write(DataUnit.UnitId);
            writer.Write(Index);
            foreach (var t in States)
                writer.Write(t.AsInt);

            Dep.WriteTo(writer);
        }

        public static GameDataUnit Load(IBinaryReader reader)
        {
            GameDataUnit gdu = new() { DataUnit = null, Id = reader.ReadString(), Index = reader.ReadUInt32() };
            for (var i = 0; i < gdu.States.Length; ++i)
                gdu.States[i] = new State(reader.ReadInt32());
            gdu.Dep = Dependency.ReadFrom(reader);
            return gdu;
        }

        private static void SaveBigfile(uint bigfileIndex, string filename, List<IDataFile> dataFiles, ISignatureDataBase database)
        {
            var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

            var memoryStream = new MemoryStream();
            var memoryWriter = new BinaryMemoryWriter();

            var bigfile = new Bigfile(bigfileIndex);
            foreach (var cl in dataFiles)
            {
                var mainBigfileFile = new BigfileFile(cl.CookedFilename);

                memoryWriter.Reset();
                cl.BuildSignature(memoryWriter);
                cl.Signature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                if (database.Register(cl.Signature, bigfile.Index, (uint)bigfile.Files.Count))
                {
                    bigfile.Files.Add(mainBigfileFile);
                }
            }

            var bigFiles = new List<Bigfile>() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
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
