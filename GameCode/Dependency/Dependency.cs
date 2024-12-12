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

        private struct Info
        {
            public readonly uint Value;

            public Info(uint value)
            {
                Value = value;
            }

            public EGameDataPath Path
            {
                get => (EGameDataPath)(Value & 0xFF);
                init => Value = (Value & 0xFFFFFF00) | (uint)value;
            }

            public short Id
            {
                get => (short)((Value >> 8) & 0xFFFF);
                init => Value = (Value & 0xFFFF00FF) | ((uint)value << 8);
            }

            public EMethod Method
            {
                get => (EMethod)((Value >> 24) & 0xFF);
                init => Value = (Value & 0x00FFFFFF) | ((uint)value << 24);
            }
        }

        private List<Info> Infos { get; set; } = new();
        private List<Hash160> Hashes { get; set; } = new();
        private List<string> FilePaths { get; set; } = new();

        public Dependency()
        {
        }

        public Dependency(EGameDataPath path, string filepath)
        {
            Add(0, path, filepath);
        }

        public void Add(short id, EGameDataPath p, string filepath)
        {
            Infos.Add(new Info { Path = p, Id = id, Method = EMethod.ContentHash });
            FilePaths.Add(filepath);
            Hashes.Add(Hash160.Empty);
        }

        // Return false if dependencies are up-to-date
        // Return true if dependencies where updated
        public delegate DataCookResult DataCompilerOutputUpdateDelegate(short id, State state);
        public DataCookResult Update(DataCompilerOutputUpdateDelegate ood)
        {
            var result = DataCookResult.None;

            for (var i = 0; i < Count; ++i)
            {
                var method = Infos[i].Method;

                // Return ids of dependencies that have changed
                var newHash = Hash160.Null;
                var filepath = Path.Join(GameDataPath.GetPath(Infos[i].Path), FilePaths[i]);
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

        public static Dependency Load(EGameDataPath path, string filePath)
        {
            BinaryFileReader reader = new();
            var filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), Path.Join(GameDataPath.GetPath(path), filePath, ".dep"));
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
            var filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), FilePaths[0] + ".dep");
            var writer = ArchitectureUtils.CreateBinaryWriter(filepath, Platform.Current);
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
                    dep.Infos.Add(new Info(reader.ReadUInt32()));
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
                writer.Write(Infos[i].Value);
                Hashes[i].WriteTo(writer);
            }

            foreach (var i in FilePaths)
            {
                writer.Write(i);
            }
        }
    }
}
