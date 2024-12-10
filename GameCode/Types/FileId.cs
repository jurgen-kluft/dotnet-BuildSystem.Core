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
        uint BigFileIndex { get; }
        uint FileIndex{ get; set; }
        IDataCompiler Compiler { get; }
    }

    public sealed class FileId : IFileId, IStruct
    {
        public static readonly FileId s_empty = new();

        private FileId() : this(null)
        {
        }

        public FileId(IDataCompiler compiler)
        {
            Compiler = compiler;
        }

        public uint BigFileIndex { get; set; }
        public uint FileIndex { get; set; }
        public string[] FileNames => Compiler.CompilerFileNames;
        public IDataCompiler Compiler { get; }

        public bool StructIsValueType => true;
        public int StructSize => 8;
        public int StructAlign => 8;
        public string StructName => "fileid_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(BigFileIndex);
            writer.Write(FileIndex);
        }
    }

    public class FileIdPtr : IFileId, IStruct
    {
        public uint BigFileIndex { get; set; }
        public uint FileIndex { get; set; }

        public IDataCompiler Compiler { get; protected init; }

        public bool StructIsValueType => true;
        public int StructSize => 8 * 2;
        public int StructAlign => 8;
        public string StructName => "fileid_ptr_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(ulong.MinValue); // T* ptr = nullptr;
            writer.Write(BigFileIndex);     // fileid_t fileid;
            writer.Write(FileIndex);
        }
    }

}
