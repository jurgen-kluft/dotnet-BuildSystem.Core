using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    public class SignatureDataBase : ISignatureDataBase
    {
        private List<uint> mBigfileIndexList = new();
        private List<uint> mFileIndexList = new();
        private Dictionary<Hash160, int> mSignatureToIndex = new();

        public (uint bigfileIndex, uint fileIndex) GetFileId(Hash160 signature)
        {
            if (mSignatureToIndex.TryGetValue(signature, out int index))
                return (mBigfileIndexList[index], mFileIndexList[index]);
            return (uint.MaxValue, uint.MaxValue);
        }

        public bool Register(Hash160 signature, uint bigfileIndex, uint fileIndex)
        {
            if (mSignatureToIndex.TryGetValue(signature, out int index))
                return false;

            mSignatureToIndex.Add(signature, mBigfileIndexList.Count);
            mBigfileIndexList.Add(bigfileIndex);
            mFileIndexList.Add(fileIndex);
            return true; // Success registering this signature
        }

        public bool Load(string filepath)
        {
            BinaryFileReader reader = new();
            if (!reader.Open(filepath))
                return false;

            mSignatureToIndex.Clear();
            mBigfileIndexList.Clear();
            mFileIndexList.Clear();

            while (reader.Position < reader.Length)
            {
                var signature = Hash160.ReadFrom(reader);
                var bigfileIndex = reader.ReadUInt32();
                var fileIndex = reader.ReadUInt32();

                mSignatureToIndex.Add(signature, mBigfileIndexList.Count);
                mBigfileIndexList.Add(bigfileIndex);
                mFileIndexList.Add(fileIndex);
            }

            reader.Close();
            return true;
        }

        public bool Save(string filepath)
        {
            var writer = ArchitectureUtils.CreateBinaryWriter(filepath, LocalizerConfig.Platform);
            if (writer == null) return false;

            foreach (var (e, index) in mSignatureToIndex)
            {
                writer.Write(e.Data, 0, e.Data.Length);
                writer.Write(mBigfileIndexList[index]);
                writer.Write(mFileIndexList[index]);
            }

            writer.Close();
            return true;
        }
    }
}
