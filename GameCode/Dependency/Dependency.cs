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

        public override bool Equals(object obj)
        {
            State other = (State)obj;
            return this.AsInt == other.AsInt;
        }
    }

    public class Dependency
    {
        public enum EState : byte
        {
            UNINITIALIZED,

            /// Not initialized
            NOT_FOUND,

            /// File doesn't exist
            CHANGED,

            /// File state has changed since previous check
            UNCHANGED,
            /// File state is identical to previous check
        }

        public enum EMethod : byte
        {
            TIMESTAMP_HASH,
            CONTENT_HASH,
            TIMESTAMP_AND_CONTENT_HASH,
        }

        private int Count
        {
            get { return Ids.Count; }
        }

        /// Note: We could make each entry just a byte Array, this would be
        /// a very specific optimizations which will speed up the loading.

        private List<byte> Paths { get; set; } = new List<byte>();
        private List<string> SubFilePaths { get; set; } = new List<string>();
        private List<int> Ids { get; set; } = new List<int>();
        private List<EMethod> Methods { get; set; } = new List<EMethod>();
        private List<Hash160> Hashes { get; set; } = new List<Hash160>();

        public Dependency()
		{

		}
        public Dependency(EGameDataPath path, string subfilepath)
        {
            Add(0, path, subfilepath);
        }

        public void Add(
            int id,
            EGameDataPath p,
            string subfilepath        )
        {
            Paths.Add((byte)p);
            SubFilePaths.Add(subfilepath);
            Ids.Add(id);
            Methods.Add(EMethod.TIMESTAMP_HASH);
            Hashes.Add(new Hash160());
        }

        public delegate void OnUpdateDelegate(int id, State state);

        public static void OnUpdateNop(int id, State state)
		{

		}

        public void Update(OnUpdateDelegate ood)
        {
            for (int i = 0; i < Count; ++i)
            {
                EMethod method = Methods[i];

                // Return ids of dependencies that have changed
                Hash160 newHash = null;
                string filepath = Path.Join(GameDataPath.GetPath((EGameDataPath)Paths[i]), SubFilePaths[i]);
                switch (method)
                {
                    case EMethod.CONTENT_HASH:
                        {
                            FileInfo fileInfo = new(filepath);
                            if (fileInfo.Exists)
                                newHash=HashUtility.compute(fileInfo);
                        }
                        break;
                    case EMethod.TIMESTAMP_HASH:
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

                if (newHash == null)
                {
                    Hashes[i] = newHash;
                    ood(Ids[i], State.Missing);
                }
                else if (newHash != Hashes[i])
                {
                    Hashes[i] = newHash;
                    ood(Ids[i], State.Modified);
                }
                else
                {
                    ood(Ids[i], State.Ok);
                }
            }
        }

        public static Dependency Load(EGameDataPath path, string subfilepath)
        {
            BinaryFileReader reader = new();
            string filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), Path.Join(GameDataPath.GetPath(path), subfilepath, ".dep"));
            if (reader.Open(filepath))
            {
                Dependency dep = ReadFrom(reader);
                reader.Close();
                return dep;
            }
            return null;
        }

        public bool Save()
        {
            BinaryFileWriter writer = new();
            string filepath = Path.Join(GameDataPath.GetPath(EGameDataPath.Dst), SubFilePaths[0] + ".dep");
            if (writer.Open(filepath))
            {
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
            Dependency dep = null;
            UInt32 magic = reader.ReadUInt32();
            if (magic == StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'))
            {
                Int32 count = reader.ReadInt32();

                dep = new ();
                dep.Paths = new(count);
                dep.SubFilePaths = new List<string>(count);
                dep.Ids = new List<int>(count);
                dep.Methods = new List<EMethod>(count);
                dep.Hashes = new List<Hash160>(count);

                for (int i = 0; i < count; i++)
                {
                    dep.Paths.Add(reader.ReadUInt8());
                }
                for (int i = 0; i < count; i++)
                {
                    dep.SubFilePaths.Add(reader.ReadString());
                }
                for (int i = 0; i < count; i++)
                {
                    dep.Ids.Add(reader.ReadInt32());
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
            writer.Write(StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'));
            writer.Write(Count);
            foreach (byte b in Paths)
            {
                writer.Write(b);
            }
            foreach (string subpath in SubFilePaths)
            {
                writer.Write(subpath);
            }
            foreach (int id in Ids)
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
