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
    }
}
