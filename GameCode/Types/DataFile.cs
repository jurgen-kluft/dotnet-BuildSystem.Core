using GameCore;
using Environment = System.Environment;

namespace GameData
{
    public class DataFile : IStruct, ISignature
    {
        private readonly ISignature _signature;

        public DataFile()
        {
            _signature = null;
            StructMember = "datafile_t<void>";
        }

        public DataFile(ISignature signature, string templateType)
        {
            _signature = signature;
            StructMember = "datafile_t<" + templateType + ">";
        }

        public Hash160 Signature { get { return _signature.Signature; } }

        public int StructAlign => 4; // This is the required memory alignment of the struct
        public int StructSize => 8; // This is the memory size of the struct
        public string StructMember { get; set; }

        public ICode StructCode => new DataFileCode();

        public void StructWrite(IGameDataWriter writer)
        {
            writer.WriteFileId(Signature);
        }
    }
}
