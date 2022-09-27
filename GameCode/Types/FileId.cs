using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public class FileId : IFileId
    {
        private readonly Hash160 mFileId;
        private readonly IFileIdsProvider mProvider;

        public FileId()
        {
            mFileId = Hash160.Empty;
            mProvider = null;
        }
        private FileId(Hash160 id)
        {
            mFileId = id;
            mProvider = null;
        }
        public FileId(IFileIdsProvider provider)
        {
            mFileId = Hash160.Empty;
            mProvider = provider;
        }

        public static FileId NewInstance(string filename)
        {
            FileId fileId = new (HashUtility.Compute_UTF8(filename.ToLower()));
            return fileId;
        }

        public Hash160 ID
        {
            get
            {
                return mFileId;
            }
        }

        public object Value
        {
            get 
            {
                if (mProvider!=null)
                    return mProvider.FileIds[0].ID;
                else
                    return mFileId; 
            }
        }

        public override string ToString()
        {
            return mFileId.ToString();
        }
    }
}
