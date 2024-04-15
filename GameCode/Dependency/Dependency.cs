using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public int AsInt { get { return (int)StateValue; } }

        public static readonly State Ok = new() { StateValue = (sbyte)StateEnum.Ok };
        public static readonly State Missing = new() { StateValue = (sbyte)StateEnum.Missing };
        public static readonly State Modified = new() { StateValue = (sbyte)StateEnum.Modified };

        public static State FromRaw(sbyte b) { return new() { StateValue = (sbyte)(b & 0x3) }; }

        public State(int state)
        {
            StateValue = (sbyte)state;
        }

        public bool IsOk { get { return StateValue == 0; } }
        public bool IsNotOk { get { return StateValue != 0; } }
        public bool IsModified { get { return ((sbyte)StateValue & (sbyte)(StateEnum.Modified)) != 0; } }
        public bool IsMissing { get { return ((sbyte)StateValue & (sbyte)(StateEnum.Missing)) != 0; } }

        public void Merge(State s)
        {
            if (IsModified)
            {
                if (s.IsMissing)
                    StateValue = s.StateValue;
            }
            else if (IsOk)
            {
                StateValue = s.StateValue;
            }
        }

        public static bool operator ==(State b1, State b2)
        {
            return b1.AsInt == b2.AsInt;
        }

        public static bool operator !=(State b1, State b2)
        {
            return b1.AsInt != b2.AsInt;
        }

        public override int GetHashCode()
        {
            return AsInt;
        }

        public override bool Equals(object obj)
        {
            State other = (State)obj;
            return this.AsInt == other.AsInt;
        }
    }

    public sealed class Dependency
    {
        public enum EState : byte
        {
            Uninitialized,

            /// Not initialized
            NotFound,

            /// File doesn't exist
            Changed,

            /// File state has changed since previous check
            Unchanged,
            /// File state is identical to previous check
        }

        public enum EMethod : byte
        {
            TimestampHash,
            ContentHash,
            TimestampAndContentHash,
        }

        private int Count
        {
            get { return Ids.Count; }
        }

        /// Note: We could make each entry just a byte Array, this would be
        /// a very specific optimizations which will speed up the loading.

        private List<byte> Paths { get; set; } = new List<byte>();
        private List<string> FilePaths { get; set; } = new List<string>();
        private List<short> Ids { get; set; } = new List<short>();
        private List<EMethod> Methods { get; set; } = new List<EMethod>();
        private List<Hash160> Hashes { get; set; } = new List<Hash160>();

        public Dependency()
        {
        }

        public Dependency(EGameDataPath path, string filepath)
        {
            Add(0, path, filepath);
        }

        public void Add(short id, EGameDataPath p, string filepath)
        {
            Paths.Add((byte)p);
            FilePaths.Add(filepath);
            Ids.Add(id);
            Methods.Add(EMethod.TimestampHash);
            Hashes.Add(Hash160.Empty);
        }

        // Return false if dependencies are up-to-date
        // Return true if dependencies where updated
        public bool Update(Action<short, State> ood)
        {
            bool result = false;
            for (int i = 0; i < Count; ++i)
            {
                EMethod method = Methods[i];

                // Return ids of dependencies that have changed
                Hash160 newHash = Hash160.Null;
                string filepath = Path.Join(GameDataPath.GetPath((EGameDataPath)Paths[i]), FilePaths[i]);
                switch (method)
                {
                    case EMethod.ContentHash:
                        {
                            FileInfo fileInfo = new(filepath);
                            if (fileInfo.Exists)
                                newHash = HashUtility.Compute(fileInfo);
                        }
                        break;
                    case EMethod.TimestampHash:
                        {
                            FileInfo fileInfo = new(filepath);
                            if (fileInfo.Exists)
                            {
                                newHash = Hash160.FromDateTime(File.GetLastWriteTime(filepath));
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (newHash == Hash160.Null)
                {
                    result = true;
                    Hashes[i] = newHash;
                    ood?.Invoke(Ids[i], State.Missing);
                }
                else if (newHash != Hashes[i])
                {
                    result = true;
                    Hashes[i] = newHash;
                    ood?.Invoke(Ids[i], State.Modified);
                }
                else // (newHash == Hashes[i])
                {
                    ood?.Invoke(Ids[i], State.Ok);
                }
            }
            return result;
        }

        public static Dependency Load(EGameDataPath path, string _filepath)
        {
            BinaryFileReader reader = new();
            string filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), Path.Join(GameDataPath.GetPath(path), _filepath, ".dep"));
            if (reader.Open(filepath))
            {
                Int64 magic = reader.ReadInt64();
                if (magic == StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'))
                {
                    Dependency dep = ReadFrom(reader);
                    reader.Close();
                    return dep;
                }
            }
            return null;
        }

        public bool Save()
        {
            var filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), FilePaths[0] + ".dep");
            var writer = EndianUtils.CreateBinaryWriter(filepath, Platform.Current);
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
                Int32 count = reader.ReadInt32();
                dep.Paths = new(count);
                dep.FilePaths = new List<string>(count);
                dep.Ids = new List<short>(count);
                dep.Methods = new List<EMethod>(count);
                dep.Hashes = new List<Hash160>(count);

                for (int i = 0; i < count; i++)
                {
                    dep.Paths.Add(reader.ReadUInt8());
                }
                for (int i = 0; i < count; i++)
                {
                    string fp = reader.ReadString();
                    dep.FilePaths.Add(fp);
                }
                for (int i = 0; i < count; i++)
                {
                    dep.Ids.Add(reader.ReadInt16());
                }
                for (int i = 0; i < count; i++)
                {
                    dep.Methods.Add((EMethod)reader.ReadUInt8());
                }
                for (int i = 0; i < count; i++)
                {
                    dep.Hashes.Add(Hash160.ReadFrom(reader));
                }
            }
            return dep;
        }

        public void WriteTo(IBinaryWriter writer)
        {
            writer.Write(Count);
            foreach (byte b in Paths)
            {
                writer.Write(b);
            }
            foreach (string fp in FilePaths)
            {
                writer.Write(fp);
            }
            foreach (short id in Ids)
            {
                writer.Write(id);
            }
            foreach (EMethod b in Methods)
            {
                writer.Write((byte)b);
            }
            foreach (Hash160 h in Hashes)
            {
                h.WriteTo(writer);
            }
        }
    }
}
