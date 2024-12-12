using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public readonly struct DataFileSignature : ISignature
    {
        private readonly IDataFile _dataFile;

        public DataFileSignature(IDataFile datafile)
        {
            _dataFile = datafile;
        }

        public Hash160 Signature => _dataFile.Signature;
    }

    public readonly struct DataFile : IStruct, ISignature
    {
        private readonly ISignature _signature;

        public DataFile(ISignature signature, string templateType)
        {
            _signature = signature;
            StructTemplateType = templateType;
        }

        public Hash160 Signature { get { return _signature.Signature; } }

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
