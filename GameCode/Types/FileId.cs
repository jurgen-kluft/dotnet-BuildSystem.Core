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

        public static FileId NewInstance(Filename filename)
        {
            FileId fileId = new FileId(Hash160.FromString(filename.ToString().ToLower()));
            return fileId;
        }

        public Hash160 id
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
                    return mProvider.fileIds[0].id;
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
