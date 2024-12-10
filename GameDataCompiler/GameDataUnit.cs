using System.Reflection;
using System.Runtime.Loader;
using GameCore;
using GameData;

namespace DataBuildSystem
{
    // A GameDataUnit is a single .dll file that contains compiled C# code structured as data.
    // Classes can contain FileId objects that hold a IDataCompiler, these compilers need to
    // be tracked. When such a compiler is out-of-date, the GameDataCompilerLog will be updated
    // and the GameDataBigfile and GameDataData will be rebuilt.

    public class GameDataUnits
    {
        private List<GameDataUnit> DataUnits { get; set; }

        public Assembly Initialize(string gameDataDllFilename)
        {
            _gameDataAssembly = LoadAssembly(gameDataDllFilename);
            _gameDataData = new GameDataData();

            // Instantiate the data root (which is the root DataUnit)
            _gameDataData.Instanciate(_gameDataAssembly);
            return _gameDataAssembly;
        }

        private Assembly _gameDataAssembly;
        private GameDataData _gameDataData;

        private static Assembly LoadAssembly(string gameDataDllFilename)
        {
            AssemblyLoadContext gameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            var dllBytes = File.ReadAllBytes(Path.Join(BuildSystemCompilerConfig.GddPath, gameDataDllFilename));
            Assembly gameDataAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));
            return gameDataAssembly;
        }

        public State Update(string srcPath, string dstPath)
        {
            // Foreach DataUnit that is out-of-date or missing
            foreach (var gdu in DataUnits)
            {
                gdu.DetermineState();

                var gduGameDataDll = gdu.StateOf(EGameData.GameDataDll);
                var gduCompilerLog = gdu.StateOf(EGameData.GameDataCompilerLog);
                var gduGameDataData = gdu.StateOf(EGameData.GameDataData, EGameData.GameDataRelocation);
                var gduBigfile = gdu.StateOf(EGameData.BigFileData, EGameData.BigFileToc, EGameData.BigFileFilenames, EGameData.BigFileHashes);

                if (gduGameDataDll.IsOk && gduCompilerLog.IsOk && gduGameDataData.IsOk && gduBigfile.IsOk)
                {
                    // All is up-to-date, but source files might have changed!
                    var gdCl = new GameDataCompilerLog(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataCompilerLog));

                    // Load the compiler log so that they can be used to verify the source files
                    var loadedCompilers = new List<IDataCompiler>();
                    gdCl.Load(loadedCompilers);

                    // Execute all compilers, every compiler will thus check its dependencies (source and destination files)
                    var result = gdCl.Execute(loadedCompilers, out var gdClOutput);
                    if (result.IsNotOk)
                    {
                        // Some (or all) compilers reported a change, now we have to load the assembly and build the Bigfile and Game Data.
                        var gdd = new GameDataData(gdu.DataUnit);

                        // We have to collect the data compilers from the gdd because we have to assign FileId's
                        // The number and order of data compilers should be identical with 'loaded_compilers'
                        var currentCompilers = gdd.CollectDataCompilers();
                        gdCl.Merge(loadedCompilers, currentCompilers, out var mergedCompilers);

                        // GameDataCompiler log is updated -> rebuild the Bigfile
                        // As long as all the FileId's will be the same we do not need to build/save the game data files
                        GameDataBigfile bff = new(gdu.Index);
                        bff.AssignFileId(gdClOutput);
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Id, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataData));

                        // Lastly we need to save the game data compiler log
                        gdCl.Save(mergedCompilers);
                        gdu.DetermineState();
                    }
                }
                else
                {
                    var gdd = new GameDataData(gdu.DataUnit);

                    var currentCompilers = gdd.CollectDataCompilers();
                    var loadedCompilers = new List<IDataCompiler>(currentCompilers.Count);

                    var gdCl = new GameDataCompilerLog(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataCompilerLog));
                    gdCl.Load(loadedCompilers);
                    gdCl.Merge(loadedCompilers, currentCompilers, out var mergedCompilers);

                    var result = gdCl.Execute(mergedCompilers, out var gdClOutput);
                    if (result.IsOk)
                    {
                        if (gduGameDataData.IsNotOk || gduBigfile.IsNotOk)
                        {
                            result = Result.OutOfDate;
                        }
                    }

                    if (result.IsOutOfDate)
                    {
                        // Rebuild the Bigfile and GameData file
                        var bff = new GameDataBigfile(gdu.Index);
                        bff.AssignFileId(gdClOutput);
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Id, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Id, EGameData.GameDataData));

                        // Everything is saved, now save the compiler log
                        gdCl.Save(mergedCompilers);
                    }

                    gdu.DetermineState();
                }
            }
            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            var idToDataUnit = new Dictionary<string, IDataUnit>();
            var currentDataUnits =  _gameDataData.CollectDataUnits();
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
            Dep.Update(delegate (short idx, State state)
            {
                States[idx] = state;
                return DataCompilerResult.None;
            });
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
            GameDataUnit gdu = new()
            {
                DataUnit = null,
                Id = reader.ReadString(),
                Index = reader.ReadUInt32()
            };
            for (var i = 0; i < gdu.States.Length; ++i)
                gdu.States[i] = new State(reader.ReadInt32());
            gdu.Dep = Dependency.ReadFrom(reader);
            return gdu;
        }
    }
}
