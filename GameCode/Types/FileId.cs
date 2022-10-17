using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public interface IFileId
    {
        Int64 Value { get; }
    }

    public sealed class FileId : IFileId, IStruct
    {
        private readonly IFileIdProvider mProvider;

        public static readonly FileId SEmpty = new ();

        public FileId() : this (null)
        {
        }
        public FileId(IFileIdProvider provider)
        {
            mProvider = provider;
        }

        public int StructSize => sizeof(Int64);
        public int StructAlign => 8;
        public string StructName => "fileid_t";
        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(mProvider.FileId);
        }
    }
}
