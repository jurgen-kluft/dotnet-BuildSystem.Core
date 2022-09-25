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
        private List<string> SubPaths { get; set; } = new List<string>();
        private List<string> Filenames { get; set; } = new List<string>();
        private List<int> Ids { get; set; } = new List<int>();
        private List<ERule> Rules { get; set; } = new List<ERule>();
        private List<EMethod> Methods { get; set; } = new List<EMethod>();
        private List<Hash160> Hashes { get; set; } = new List<Hash160>();

        public Dependency(EPath path, string subpath, string filename)
        {
            Add(0, path, subpath, filename);
        }

        public void Add(
            int id,
            EPath p,
            string subpath,
            string filename
        )
        {
            Paths.Add((byte)p);
            SubPaths.Add(subpath);
            Filenames.Add(filename);
            Ids.Add(id);
            Methods.Add(EMethod.TIMESTAMP_HASH);
            Rules.Add(ERule.ON_CHANGE);
            Hashes.Add(Hash160.Empty);
        }

        public int Update(List<int> out_ids)
        {
            for (int i = 0; i < Count; ++i)
            {
                EMethod method = Methods[i];

                // Return ids of dependencies that have changed
                Hash160 newHash;
                string filepath = Path.Join(GetPath((EPath)Paths[i]), SubPaths[i], Filenames[i]);
                switch (method)
                {
                    case EMethod.CONTENT_HASH:
                        {
                            FileInfo fileInfo = new(filepath);
                            newHash = fileInfo.Exists ? HashUtility.compute(fileInfo) : Hash160.Empty;
                        }
                        break;
                    case EMethod.TIMESTAMP_HASH:
                        {
                            newHash = Hash160.Empty;
                            if (File.Exists(filepath))
                                newHash = Hash160.FromDateTime(File.GetLastWriteTime(filepath));
                        }
                        break;
                    default:
                        newHash = Hash160.Empty;
                        break;
                }
                ERule rule = Rules[i];
                if (rule == ERule.MUST_EXIST)
                {
                    if (newHash == Hash160.Empty || newHash != Hashes[i])
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

        public bool Load()
        {
            BinaryFileReader reader = new();
            string filepath = Path.Join(GetPath(EPath.Dst), SubPaths[0], Filenames[0] + ".dep");
            if (reader.Open(filepath))
            {
                UInt32 magic = reader.ReadUInt32();
                if (magic == StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'))
                {
                    Int32 count = reader.ReadInt32();
                    Paths = new(count);
                    SubPaths = new List<string>(count);
                    Filenames = new List<string>(count);
                    Ids = new List<int>(count);
                    Rules = new List<ERule>(count);
                    Methods = new List<EMethod>(count);
                    Hashes = new List<Hash160>(count);

                    for (int i = 0; i < count; i++)
                    {
                        Paths.Add(reader.ReadUInt8());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        SubPaths.Add(reader.ReadString());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Filenames.Add(reader.ReadString());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Ids.Add(reader.ReadInt32());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Rules.Add((ERule)reader.ReadUInt8());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Methods.Add((EMethod)reader.ReadUInt8());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Hashes.Add(Hash160.ReadFrom(reader));
                    }

                    reader.Close();
                    return true;
                }
            }
            return false;
        }

        public bool Save()
        {
            BinaryFileWriter reader = new();
            string filepath = Path.Join(GetPath(EPath.Dst), SubPaths[0], Filenames[0] + ".dep");
            if (reader.Open(filepath))
            {
                reader.Write(StringTools.Encode_64_10('D', 'E', 'P', 'E', 'N', 'D', 'E', 'N', 'C', 'Y'));
                reader.Write(Count);
                foreach (byte b in Paths)
                {
                    reader.Write(b);
                }
                foreach (string subpath in SubPaths)
                {
                    reader.Write(subpath);
                }
                foreach (string subpath in Filenames)
                {
                    reader.Write(subpath);
                }
                foreach (int id in Ids)
                {
                    reader.Write(id);
                }
                foreach (ERule b in Rules)
                {
                    reader.Write((byte)b);
                }
                foreach (EMethod b in Methods)
                {
                    reader.Write((byte)b);
                }
                foreach (Hash160 h in Hashes)
                {
                    h.WriteTo(reader);
                }

                reader.Close();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
