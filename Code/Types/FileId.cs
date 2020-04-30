using System;
using System.Collections.Generic;
using Core;

namespace Game.Data
{
    public class FileId : IFileId
    {
        private readonly Hash128 mFileId;
        private readonly IFileIdsProvider mProvider;

        public FileId()
        {
            mFileId = Hash128.Empty;
            mProvider = null;
        }
        private FileId(Hash128 id)
        {
            mFileId = id;
            mProvider = null;
        }
        public FileId(IFileIdsProvider provider)
        {
            mFileId = Hash128.Empty;
            mProvider = provider;
        }

        public static FileId NewInstance(Filename filename)
        {
            FileId fileId = new FileId(HashUtility.compute(filename.ToString().ToLower()));
            return fileId;
        }

        public Hash128 id
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
