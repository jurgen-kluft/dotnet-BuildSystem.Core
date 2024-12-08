using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    // A FileId is a combination of the index of a Bigfile and the index of a BigfileFile within the Bigfile.
    // The reason for building a FileId like this is that we can easily combine multiple Bigfiles and use the
    // Bigfile Index to index into a Section.
    public interface IFile
    {

    }

    public interface IFileId
    {
        uint BigfileIndex { get; }
        uint FileIndex { get; }
    }

    public sealed class FileId : IFileId, IStruct
    {
        public static readonly FileId s_empty = new();
        private readonly IFileIdProvider _provider;

        private FileId() : this(null)
        {
        }

        public FileId(IFileIdProvider provider)
        {
            this._provider = provider;
        }

        public uint BigfileIndex { get; set; }
        public uint FileIndex => _provider.FileIndex;

        public bool StructIsValueType => true;
        public int StructSize => 8;
        public int StructAlign => 8;
        public string StructName => "fileid_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(BigfileIndex);
            writer.Write(FileIndex);
        }
    }

    public class FileIdPtr : IFile, IFileId, IStruct
    {
        private FileIdPtr() : this(null, null)
        {
        }

        protected FileIdPtr(IFileIdProvider provider, Type objectType)
        {
            Provider = provider;
            ObjectType = objectType;
        }

        public uint BigfileIndex { get; set; }
        public uint FileIndex => Provider.FileIndex;

        private IFileIdProvider Provider { get; }
        public Type ObjectType { get; set; }

        public bool StructIsValueType => true;
        public int StructSize => 8 * 2;
        public int StructAlign => 8;
        public string StructName => "fileid_ptr_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(ulong.MinValue); // T* ptr = nullptr;
            writer.Write(BigfileIndex);     // fileid_t fileid;
            writer.Write(FileIndex);
        }
    }

}
