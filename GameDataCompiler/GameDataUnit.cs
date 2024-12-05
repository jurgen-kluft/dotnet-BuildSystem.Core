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

    //

    public class GameDataUnits
    {
        private List<GameDataUnit> DataUnits { get; set; } = new();

        public GameDataUnits() { }

        private AssemblyLoadContext mGameDataAssemblyContext;
        private Assembly mGameDataAssembly;

        private Assembly LoadAssembly(string gameDataDllFilename)
        {
            mGameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            var dllBytes = File.ReadAllBytes(Path.Join(BuildSystemCompilerConfig.GddPath, gameDataDllFilename));
            mGameDataAssembly = mGameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));
            return mGameDataAssembly;
        }
        private void UnloadAssembly()
        {
            mGameDataAssemblyContext.Unload();
            mGameDataAssembly = null;
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
                    var gdCl = new GameDataCompilerLog(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataCompilerLog));

                    // Load the compiler log so that they can be used to verify the source files
                    var loadedCompilers = new List<IDataCompiler>();
                    gdCl.Load(loadedCompilers);

                    // Execute all compilers, every compiler will thus check its dependencies (source and destination files)
                    var result = gdCl.Execute(loadedCompilers, out var gdClOutput);
                    if (result.IsNotOk)
                    {
                        // Some (or all) compilers reported a change, now we have to load the assembly and build the Bigfile and Game Data.
                        var gdAsm = LoadAssembly(gdu.FilePath);
                        var gdd = new GameDataData(gdAsm);

                        // We have to collect the data compilers from the gdd because we have to assign FileId's
                        // The number and order of data compilers should be identical with 'loaded_compilers'
                        var currentCompilers = gdd.CollectDataCompilers();
                        gdCl.Merge(loadedCompilers, currentCompilers, out var mergedCompilers);

                        // GameDataCompiler log is updated -> rebuild the Bigfile
                        // As long as all the FileId's will be the same we do not need to build/save the game data files
                        GameDataBigfile bff = new(gdu.Index);
                        bff.AssignFileId(gdClOutput);
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataData));

                        // Lastly we need to save the game data compiler log
                        gdCl.Save(mergedCompilers);
                        gdu.DetermineState();

                        UnloadAssembly();
                    }
                }
                else
                {
                    var gdAsm = LoadAssembly(gdu.FilePath);
                    var gdd = new GameDataData(gdAsm);

                    var currentCompilers = gdd.CollectDataCompilers();
                    var loadedCompilers = new List<IDataCompiler>(currentCompilers.Count);

                    var gdCl = new GameDataCompilerLog(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataCompilerLog));
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
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataData));

                        // Everything is saved, now save the compiler log
                        gdCl.Save(mergedCompilers);
                    }

                    gdu.DetermineState();
                    UnloadAssembly();
                }
            }
            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            // Scan the gddPath folder for all game data .dll's
            var hashToPath = new Dictionary<Hash160, string>();
            foreach (var path in DirUtils.EnumerateFiles(gddPath, "GameData.*.dll", SearchOption.TopDirectoryOnly))
            {
                var filepath = path.RelativePath(gddPath.Length + 1).ToString();
                if (!Path.GetFileNameWithoutExtension(filepath).StartsWith("GameData")) continue;

                var hash = HashUtility.Compute_UTF8(filepath.ToLower());
                hashToPath.Add(hash, filepath);
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

                        // Is this one still in the list of .dll's?
                        if (hashToPath.ContainsKey(gdu.Hash))
                        {
                            hashToPath.Remove(gdu.Hash);
                            dataUnits.Add(gdu.Index, gdu);
                        }
                    }
                }
                binaryFile.Close();
            }

            // Any new DataUnit? -> create them with an index that is not used
            var index = (uint)0;
            foreach (var item in hashToPath)
            {
                while (dataUnits.ContainsKey(index))
                    index++;
                var gdu = new GameDataUnit(item.Value, index);
                dataUnits.Add(gdu.Index, gdu);
                index++;
            }

            // Finally, build the list of DataUnits
            DataUnits = new(dataUnits.Count);
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
    // The filename extension is different for each of the above file types, the GameDataDll is a .dll file etc..
    // Most extensions are configurable, but the GameDataDll (.dll) and GameDataCompilerLog (.gdl) are fixed.
    //
    // So for example, the GameDataUnit "Levels" should have the following files:
    //    GameData.Levels.dll
    //    GameData.Levels.gdl
    //    GameData.Levels.gdd
    //    GameData.Levels.gdr
    //    GameData.Levels.gda
    //    GameData.Levels.gdt
    //    GameData.Levels.gdf
    //    GameData.Levels.gdh


    public class GameDataUnit
    {
        public string FilePath { get; private init; }
        public string Name { get; }
        public Hash160 Hash { get; private init; }
        public uint Index { get; private init; }
        private State[] States { get; set; } = new State[(int)EGameData.Count];
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

        private GameDataUnit() : this(string.Empty, uint.MaxValue) { }

        public GameDataUnit(string filepath, uint index)
        {
            FilePath = filepath;
            Name = Path.GetFileNameWithoutExtension(filepath);
            Hash = HashUtility.Compute_UTF8(FilePath);
            Index = index;
            Dep = new();

            for (var i = 0; i < States.Length; ++i)
            {
                States[i] = State.Missing;
            }
            foreach (var e in (EGameData[])Enum.GetValues(typeof(EGameData)))
            {
                Dep.Add((short)e, GameDataPath.GetPathFor(e), Path.ChangeExtension(FilePath, GameDataPath.GetExtFor(e)));
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
            writer.Write(FilePath);
            Hash.WriteTo(writer);
            writer.Write(Index);
            foreach (var t in States)
                writer.Write(t.AsInt);

            Dep.WriteTo(writer);
        }

        public static GameDataUnit Load(IBinaryReader reader)
        {
            GameDataUnit gdu = new()
            {
                FilePath = reader.ReadString(),
                Hash = Hash160.ReadFrom(reader),
                Index = reader.ReadUInt32()
            };
            for (var i = 0; i < gdu.States.Length; ++i)
                gdu.States[i] = new State(reader.ReadInt32());
            gdu.Dep = Dependency.ReadFrom(reader);
            return gdu;
        }
    }
}
