using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    public class GameDataUnits
    {
        private Dictionary<Hash160, Int32> HashToIndex = new Dictionary<Hash160, Int32>();

        public GameDataUnits()
        {

        }

        public void Save(string dstpath)
        {
            Hash160[] hashes = new Hash160[HashToIndex.Count];
            foreach (KeyValuePair<Hash160, Int32> item in HashToIndex)
            {
                hashes[item.Value] = item.Key;
            }
            BinaryFileWriter binaryfile = new BinaryFileWriter();
            binaryfile.Open(Path.Join(dstpath, "GameDataUnitsHash.db"));

            binaryfile.Write((Int32)hashes.Length);
            foreach (Hash160 hash in hashes)
            {
                hash.WriteTo(binaryfile);
            }

            binaryfile.Close();
        }

        public void Load(string dstpath)
        {
            BinaryFileReader binaryfile = new BinaryFileReader();
            binaryfile.Open(Path.Join(dstpath, "GameDataUnitsHash.db"));
            
            Int32 numHashes = binaryfile.ReadInt32();
            Hash160[] hashes = new Hash160[numHashes];
            for (int i=0; i<numHashes; i++)
            {
                hashes[i] = Hash160.ReadFrom(binaryfile);
            }
            binaryfile.Close();

            HashToIndex = new Dictionary<Hash160, int>(numHashes);
            for (int i=0; i<numHashes; i++)
            {
                HashToIndex.Add(hashes[i], i);
            }
        }
    }

    [Flags]
    public enum EGameDataUnit : Int32
    {
        GameDataDll,
        GameDataCompilerLog,
        GameDataData,
        GameDataRelocation,
        BigFileData,
        BigFileToc,
        BigFileFilenames,
        BigFileHashes,
    }

    public class GameDataUnit
    {
        public Hash160 Hash { get; set; }
        public Int32 Index { get; set; }
        private EGameDataUnit Flags { get; set; }

        public bool IsUpToDate(EGameDataUnit u)
        {
            return Flags.HasFlag(u);
        }
    }
}
