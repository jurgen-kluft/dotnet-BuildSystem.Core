using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public struct DataFile : IStruct, IFileId
    {
        public DataFile(Hash160 signature, string templateType)
        {
            Signature = signature;
            StructTemplateType = templateType;
        }

        public Hash160 Signature { get; set; }

        public bool StructIsTemplate => true; // This is a template struct
        public string StructTemplateType { get; init; }

        public int StructAlign => 8; // This is the required memory alignment of the struct
        public int StructSize => 16; // This is the memory size of the struct
        public string StructName => "datafile_t"; // This is the name of the struct in the target code-base

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write((ulong)0);
            writer.WriteFileId(Signature);
        }
    }
}
