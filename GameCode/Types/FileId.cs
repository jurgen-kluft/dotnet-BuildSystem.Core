using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    // A FileId is a combination of the index of a Bigfile and the index of a BigfileFile within the Bigfile.
    // The reason for building a FileId like this is that we can easily combine multiple Bigfiles and use the
    // Bigfile Index to index into a Section.

    public interface IFileId
    {
        long Value { get; }
    }

    public sealed class FileId(IFileIdProvider provider) : IFileId, IStruct
    {
        public static readonly FileId sEmpty = new();

        private FileId() : this(null)
        {
        }

        public long Value => provider.FileId;

        public bool StructIsValueType => true;
        public int StructSize => 8;
        public int StructAlign => 8;
        public string StructName => "fileid_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(provider.FileId);
        }
    }
}
