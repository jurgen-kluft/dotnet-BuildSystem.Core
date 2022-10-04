using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public interface IFileId
    {
        Int64 Value { get; }
    }

    public sealed class FileId : IFileId
    {
        private readonly IFileIdProvider mProvider;

        public static readonly FileId sEmpty = new ();

        public FileId() : this (null)
        {
        }
        public FileId(IFileIdProvider provider)
        {
            mProvider = provider;
        }

        public Int64 Value
        {
            get
            {
                return mProvider.FileId;
            }
        }
    }
}
