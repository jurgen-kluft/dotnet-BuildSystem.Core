using System;

namespace GameData
{
    public struct FileIdList : ICompound
    {
        private FileId[] mFileIds;
        private readonly IFileIdsProvider mProvider;

        public FileIdList(params FileId[] inFileIds)
        {
            mProvider = null;
            mFileIds = inFileIds;
        }
        public FileIdList(IFileIdsProvider provider)
        {
            mProvider = provider;
            mFileIds = null;
        }

        private void Collect()
        {
            if (mProvider != null)
                mFileIds = mProvider.fileIds;
        }

        public Array Values
        {
            get
            {
                Collect();
                return new object[] { mFileIds };
            }
        }
    }
}
