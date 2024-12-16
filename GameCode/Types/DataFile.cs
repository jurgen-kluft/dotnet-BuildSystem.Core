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

    public class DataFile : IStruct, ISignature
    {
        private readonly ISignature _signature;

        public DataFile(ISignature signature, string templateType)
        {
            _signature = signature;
            StructMember = "datafile_t<" + templateType + ">";
        }

        public Hash160 Signature { get { return _signature.Signature; } }


        public int StructAlign => 8; // This is the required memory alignment of the struct
        public int StructSize => 16; // This is the memory size of the struct
        public string StructMember { get; set; }

        public void StructCode(StreamWriter writer)
        {
            // already defined in C++ library charon
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write((ulong)0);
            writer.WriteFileId(Signature);
        }
    }
}
