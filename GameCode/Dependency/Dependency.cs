using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameCore;

namespace DataBuildSystem
{
    public class Dependency
    {
        public enum EPath : byte
        {
            Src,
            Gdd,
            Dst
        }

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

        public enum ERule : byte
        {
            ON_CHANGE,
            MUST_EXIST,
        }

        public enum EMethod : byte
        {
            TIMESTAMP_HASH,
            CONTENT_HASH,
            TIMESTAMP_AND_CONTENT_HASH,
        }

        public static string GetPath(EPath p)
        {
            return p switch
            {
                EPath.Src => BuildSystemCompilerConfig.SrcPath,
                EPath.Gdd => BuildSystemCompilerConfig.GddPath,
                EPath.Dst => BuildSystemCompilerConfig.DstPath,
                _ => string.Empty
            };
        }

        private int Count
        {
            get { return Ids.Count; }
        }
        private List<byte> Paths { get; set; } = new List<byte>();
        private List<string> SubFilePaths { get; set; } = new List<string>();
        private List<int> Ids { get; set; } = new List<int>();
        private List<ERule> Rules { get; set; } = new List<ERule>();
        private List<EMethod> Methods { get; set; } = new List<EMethod>();
        private List<Hash160> Hashes { get; set; } = new List<Hash160>();

        public Dependency()
		{

		}
        public Dependency(EPath path, string subfilepath)
        {
            Add(0, path, subfilepath);
        }

        public void Add(
            int id,
            EPath p,
            string subfilepath        )
        {
            Paths.Add((byte)p);
            SubFilePaths.Add(subfilepath);
            Ids.Add(id);
            Methods.Add(EMethod.TIMESTAMP_HASH);
            Rules.Add(ERule.ON_CHANGE);
            Hashes.Add(new Hash160());
        }

        public int Update(List<int> out_ids)
        {
            for (int i = 0; i < Count; ++i)
            {
                EMethod method = Methods[i];

                // Return ids of dependencies that have changed
                Hash160 newHash = null;
                string filepath = Path.Join(GetPath((EPath)Paths[i]), SubFilePaths[i]);
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
                                newHash=Hash160.FromDateTime(File.GetLastWriteTime(filepath));
                        }
                        break;
                    default:
                        break;
                }
                ERule rule = Rules[i];
                if (rule == ERule.MUST_EXIST)
                {
                    if (newHash == null || newHash != Hashes[i])
                    {
                        out_ids.Add(Ids[i]);
                        Hashes[i] = newHash;
                    }
                }
                else
                {
                    if (Hashes[i] != newHash)
                    {
                        out_ids.Add(Ids[i]);
                        Hashes[i] = newHash;
                    }
                }
            }
            return out_ids.Count;
        }

        public static Dependency Load(EPath path, string subfilepath)
        {
            BinaryFileReader reader = new();
            string filepath = Path.Join(GetPath(EPath.Dst), Path.Join(GetPath(path), subfilepath, ".dep"));
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
            string filepath = Path.Join(GetPath(EPath.Dst), SubFilePaths[0] + ".dep");
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
                dep.Rules = new List<ERule>(count);
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
                    dep.Rules.Add((ERule)reader.ReadUInt8());
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
            foreach (ERule b in Rules)
            {
                writer.Write((byte)b);
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
