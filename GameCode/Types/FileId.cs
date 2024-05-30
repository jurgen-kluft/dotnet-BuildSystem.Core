using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public interface IFileId
    {
        long Value { get; }
    }

    public sealed class FileId : IFileId, IStruct
    {
        private readonly IFileIdProvider mProvider;

        public static readonly FileId SEmpty = new();

        public FileId() : this(null)
        {
        }

        public FileId(IFileIdProvider provider)
        {
            mProvider = provider;
        }

        public long Value
        {
            get { return mProvider.FileId; }
        }

        public bool StructIsValueType => true;
        public int StructSize => 8;
        public int StructAlign => 8;
        public string StructName => "fileid_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(mProvider.FileId);
        }
    }
}
