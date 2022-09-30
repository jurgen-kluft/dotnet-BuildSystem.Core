using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public interface IFileId
    {
        UInt64 Value { get; }
    }

    public class FileId : IFileId
    {
        private readonly IFilesProvider mProvider;

        public static readonly FileId sEmpty = new ();

        public FileId() : this (null)
        {
        }
        public FileId(IFilesProvider provider)
        {
            mProvider = provider;
        }

        public UInt64 Value
        {
            get 
            {
                return mProvider.FilesProviderId;
            }
        }
    }
}
