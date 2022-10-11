using System.Reflection;
using System.Runtime.Loader;
using GameCore;
using GameData;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    public class GameDataUnits
    {
        private List<GameDataUnit> DataUnits { get; set; } = new();

        public GameDataUnits() { }

        private AssemblyLoadContext mGameDataAssemblyContext;
        private Assembly mGameDataAssembly;

        private Assembly LoadAssembly(string gameDataDllFilename)
        {
            mGameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            byte[] dllBytes = File.ReadAllBytes(Path.Join(BuildSystemCompilerConfig.GddPath, gameDataDllFilename));
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
            foreach (GameDataUnit gdu in DataUnits)
            {
                gdu.Verify();

                State gduGameDataDll = gdu.StateOf(EGameData.GameDataDll);
                State gduCompilerLog = gdu.StateOf(EGameData.GameDataCompilerLog);
                State gduGameDataData = gdu.StateOf(EGameData.GameDataData, EGameData.GameDataRelocation);
                State gduBigfile = gdu.StateOf(EGameData.BigFileData, EGameData.BigFileToc, EGameData.BigFileFilenames, EGameData.BigFileHashes);

                if (gduGameDataDll.IsOk && gduCompilerLog.IsOk && gduGameDataData.IsOk && gduBigfile.IsOk)
                {
                    // All is up-to-date, but source files might have changed!
                    GameDataCompilerLog gdCl = new(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataCompilerLog));

                    // Load the compiler log so that they can be used to verify the source files
                    List<IDataCompiler> loadedCompilers = new();
                    gdCl.Load(loadedCompilers);

                    // Execute all compilers, every compiler will thus check it's dependencies (source and destination files)
                    Result result = gdCl.Execute(loadedCompilers, out List<DataCompilerOutput> gdClOutput);
                    if (result != Result.Ok)
                    {
                        // Some (or all) compilers reported a change, now we have to load the assembly and build the Bigfile and Game Data.
                        Assembly gdAsm = LoadAssembly(gdu.FilePath);
                        GameDataData gdd = new(gdAsm);

                        // We have to collect the data compilers from the gdd because we have to assign FileId's
                        // The number and order of data compilers should be identical with 'loaded_compilers'
                        List<IDataCompiler> currentCompilers = gdd.CollectDataCompilers();
                        gdCl.Merge(loadedCompilers, currentCompilers, out List<IDataCompiler> mergedCompilers);
                        gdCl.AssignFileId(gdu.Index, mergedCompilers);

                        // Compiler log is updated -> rebuild the Bigfile
                        // As long as all the FileId's will be the same we do not need to build/save the game data files
                        GameDataBigfile bff = new();
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataData));

                        // Lastly we need to save the game data compiler log
                        gdCl.Save(mergedCompilers);
                        gdu.Verify();

                        UnloadAssembly();
                    }
                }
                else
                {
                    Assembly gdAsm = LoadAssembly(gdu.FilePath);
                    GameDataData gdd = new(gdAsm);

                    List<IDataCompiler> currentCompilers = gdd.CollectDataCompilers();
                    List<IDataCompiler> loadedCompilers = new(currentCompilers.Count);

                    GameDataCompilerLog gdCl = new(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataCompilerLog));
                    gdCl.Load(loadedCompilers);
                    gdCl.Merge(loadedCompilers, currentCompilers, out List<IDataCompiler> mergedCompilers);

                    Result result = gdCl.Execute(mergedCompilers, out List<DataCompilerOutput> gdClOutput);
                    if (result == Result.Ok)
                    {
                        if (gduGameDataData.IsNotOk || gduBigfile.IsNotOk)
                        {
                            result = Result.OutOfData;
                        }
                    }

                    if (result == Result.OutOfData)
                    {
                        gdCl.AssignFileId(gdu.Index, mergedCompilers);

                        // Rebuild the Bigfiles and GameData files
                        GameDataBigfile bff = new();
                        bff.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.BigFileData), gdClOutput);
                        gdd.Save(GameDataPath.GetFilePathFor(gdu.Name, EGameData.GameDataData));

                        // Everything is saved, now save the compiler log
                        gdCl.Save(mergedCompilers);
                    }

                    gdu.Verify();
                    UnloadAssembly();
                }
            }
            return State.Ok;
        }

        public void Load(string dstPath, string gddPath)
        {
            // Scan the gddPath folder for all game data .dll's
            Dictionary<Hash160, string> hashToPath = new();
            foreach (var path in DirUtils.EnumerateFiles(gddPath, "GameData.*.dll", SearchOption.TopDirectoryOnly))
            {
                string filepath = path.RelativePath(gddPath.Length + 1).ToString();
                if (!Path.GetFileNameWithoutExtension(filepath).StartsWith("GameData")) continue;

                Hash160 hash = HashUtility.Compute_UTF8(filepath.ToLower());
                hashToPath.Add(hash, filepath);
            }

            Dictionary<int, GameDataUnit> dataUnits = new();

            BinaryFileReader binaryFile = new BinaryFileReader();
            if (binaryFile.Open(Path.Join(dstPath, "GameDataUnits.log")))
            {
                Int64 magic = binaryFile.ReadInt64();
                if (magic == StringTools.Encode_64_10('D', 'A', 'T', 'A', '.', 'U', 'N', 'I', 'T', 'S'))
                {
                    int numUnits = binaryFile.ReadInt32();
                    for (int i = 0; i < numUnits; i++)
                    {
                        GameDataUnit gdu = GameDataUnit.Load(binaryFile);

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
            int index = 0;
            foreach (var item in hashToPath)
            {
                while (dataUnits.ContainsKey(index))
                    index++;
                GameDataUnit gdu = new(item.Value, index);
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
            BinaryFileWriter binaryFile = new BinaryFileWriter();
            binaryFile.Open(Path.Join(dstPath, "GameDataUnits.log"));

            binaryFile.Write(StringTools.Encode_64_10('D', 'A', 'T', 'A', '.', 'U', 'N', 'I', 'T', 'S'));
            binaryFile.Write(DataUnits.Count);
            foreach (GameDataUnit gdu in DataUnits)
            {
                gdu.Save(binaryFile);
            }

            binaryFile.Close();
        }
    }

    public class GameDataUnit
    {
        public string FilePath { get; private init; }
        public string Name { get; }
        public Hash160 Hash { get; private init; }
        public Int32 Index { get; private init; }
        private State[] States { get; set; } = new State[8];
        private Dependency Dep { get; set; }

        public State StateOf(EGameData u)
        {
            return States[(int)u];
        }

        public State StateOf(params EGameData[] pu)
        {
            State s = new State();
            foreach (var u in pu)
                s.Merge(States[(int)u]);
            return s;
        }

        private GameDataUnit() : this(string.Empty, -1) { }

        public GameDataUnit(string filepath, Int32 index)
        {
            FilePath = filepath;
            Name = Path.GetFileNameWithoutExtension(filepath);
            Hash = HashUtility.Compute_UTF8(FilePath);
            Index = index;
            Dep = new();

            for (int i = 0; i < States.Length; ++i)
            {
                States[i] = State.Missing;
            }
            foreach (var e in (EGameData[])Enum.GetValues(typeof(EGameData)))
            {
                Dep.Add((short)e, GameDataPath.GetPathFor(e), Path.ChangeExtension(FilePath, GameDataPath.GetExtFor(e)));
            }
        }

        public void Verify()
        {
            Dep.Update(delegate (short idx, State state)
            {
                States[idx] = state;
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
                Index = reader.ReadInt32()
            };
            for (int i = 0; i < gdu.States.Length; ++i)
                gdu.States[i] = new State(reader.ReadInt32());
            gdu.Dep = Dependency.ReadFrom(reader);
            return gdu;
        }
    }
}
