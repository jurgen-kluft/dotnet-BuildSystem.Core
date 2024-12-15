using GameCore;
using GameData;

namespace DataBuildSystem
{
    public struct State
    {
        private enum StateEnum : sbyte
        {
            Ok = 0,
            Modified = 1,
            Missing = 2,
        }

        private sbyte StateValue { get; set; }
        public int AsInt8 => StateValue;

        public static readonly State Ok = new() { StateValue = (sbyte)StateEnum.Ok };
        public static readonly State Missing = new() { StateValue = (sbyte)StateEnum.Missing };
        public static readonly State Modified = new() { StateValue = (sbyte)StateEnum.Modified };

        public State(sbyte state)
        {
            StateValue = state;
        }

        public bool IsOk => StateValue == 0;
        public bool IsNotOk => StateValue != 0;
        public bool IsModified => (StateValue & (sbyte)(StateEnum.Modified)) != 0;
        public bool IsMissing => (StateValue & (sbyte)(StateEnum.Missing)) != 0;

        public static bool operator ==(State b1, State b2)
        {
            return b1.StateValue == b2.StateValue;
        }

        public static bool operator !=(State b1, State b2)
        {
            return b1.StateValue != b2.StateValue;
        }

        public override int GetHashCode()
        {
            return StateValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (State)obj;
            return this.StateValue == other.StateValue;
        }
    }

    public sealed class Dependency
    {
        public enum EState : byte
        {
            Uninitialized, // Not initialized
            NotFound, // File doesn't exist
            Changed, // File state has changed since previous check
            Unchanged, // File state is identical to previous check
        }

        public enum EMethod : byte
        {
            TimestampHash,
            ContentHash,
        }

        private int Count => Infos.Count;

        // Note: We could make each entry just a byte Array, this would be
        // a very specific optimizations which will speed up the loading.

        private readonly struct Info
        {
            public EGameDataPath DataPath { get; init; }
            public ushort Id { get; init; }
            public EMethod Method { get; init; }
        }

        private List<Info> Infos { get; set; } = [];
        private List<Hash160> Hashes { get; set; } = [];
        private List<string> FilePaths { get; set; } = [];

        public Dependency()
        {
        }

        private static string GetGameDataPath(EGameDataPath dp)
        {
            switch (dp)
            {
                case EGameDataPath.GameDataSrcPath:
                    return BuildSystemConfig.SrcPath;
                case EGameDataPath.GameDataGddPath:
                    return BuildSystemConfig.GddPath;
                case EGameDataPath.GameDataDstPath:
                    return BuildSystemConfig.DstPath;
                case EGameDataPath.GameDataPubPath:
                    return BuildSystemConfig.PubPath;
                default:
                    return string.Empty;
            }
        }

        public Dependency(EGameDataPath path, string filepath)
        {
            Add(0, path, filepath);
        }

        public void Add(ushort id, EGameDataPath gdp, string filepath)
        {
            Infos.Add(new Info { DataPath = gdp, Id = id, Method = EMethod.ContentHash });
            FilePaths.Add(filepath);
            Hashes.Add(Hash160.Empty);
        }

        // Return false if dependencies are up-to-date
        // Return true if dependencies where updated
        public delegate DataCookResult DataCompilerOutputUpdateDelegate(ushort id, State state);
        public DataCookResult Update(DataCompilerOutputUpdateDelegate ood)
        {
            var result = DataCookResult.None;

            for (var i = 0; i < Count; ++i)
            {
                var method = Infos[i].Method;

                // Return ids of dependencies that have changed
                var newHash = Hash160.Null;
                var gdp = GetGameDataPath(Infos[i].DataPath);
                var filepath = Path.Join(gdp, FilePaths[i]);
                switch (method)
                {
                    case EMethod.ContentHash:
                        {
                            var fileInfo = new FileInfo(filepath);
                            if (fileInfo.Exists)
                                newHash = HashUtility.Compute(fileInfo);
                        }
                        break;
                    case EMethod.TimestampHash:
                        {
                            var fileInfo = new FileInfo(filepath);
                            if (fileInfo.Exists)
                            {
                                newHash = Hash160.FromDateTime(File.GetLastWriteTime(filepath));
                            }
                        }
                        break;
                }

                if (newHash == Hash160.Null)
                {
                    Hashes[i] = newHash;
                    if (ood != null) result = ood(Infos[i].Id, State.Missing);
                }
                else if (newHash != Hashes[i])
                {
                    Hashes[i] = newHash;
                    if (ood != null) result = ood(Infos[i].Id, State.Modified);
                }
                else // (newHash == Hashes[i])
                {
                    if (ood != null) result = ood(Infos[i].Id, State.Ok);
                }
            }

            return result;
        }

        public static Dependency Load(GameDataPath path, string relativeFilepath)
        {
            BinaryFileReader reader = new();
            //var filepath = Path.Join(GameDataPath.GetPath(GameDataPath.Dst), Path.Join(GameDataPath.GetPath(path), filePath, ".dep"));
            var filepath = Path.Join(path.GetDirPath(), relativeFilepath);
            if (reader.Open(filepath))
            {
                var magic = reader.ReadInt64();
                if (magic == StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'))
                {
                    var dep = ReadFrom(reader);
                    reader.Close();
                    return dep;
                }
            }

            return null;
        }

        public bool Save()
        {
            var dirpath = BuildSystemConfig.DstPath;
            var filepath = Path.Join(dirpath, FilePaths[0] + ".dep");
            var writer = ArchitectureUtils.CreateBinaryFileWriter(filepath, Platform.Current);
            if (writer != null)
            {
                writer.Write(StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'));
                WriteTo(writer);
                writer.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Dependency ReadFrom(IBinaryReader reader)
        {
            Dependency dep = new();
            {
                var count = reader.ReadInt32();
                dep.Infos = new List<Info>(count);
                dep.Hashes = new List<Hash160>(count);

                for (var i = 0; i < count; i++)
                {
                    var id = reader.ReadUInt16();
                    var gdp = reader.ReadUInt8();
                    var method = reader.ReadInt8();

                    dep.Infos.Add(new Info() { DataPath = (EGameDataPath)gdp, Id = id, Method = (EMethod)method });
                    dep.Hashes.Add(Hash160.ReadFrom(reader));
                }

                dep.FilePaths = new List<string>(count);
                for (var i = 0; i < count; i++)
                {
                    var fp = reader.ReadString();
                    dep.FilePaths.Add(fp);
                }
            }
            return dep;
        }

        public void WriteTo(IBinaryWriter writer)
        {
            writer.Write(Count);
            for (var i = 0; i < Count; i++)
            {
                writer.Write(Infos[i].Id);
                writer.Write((byte)Infos[i].DataPath);
                writer.Write((byte)Infos[i].Method);
                Hashes[i].WriteTo(writer);
            }

            foreach (var i in FilePaths)
            {
                writer.Write(i);
            }
        }
    }
}
