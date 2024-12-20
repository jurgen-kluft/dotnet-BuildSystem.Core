using GameCore;

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

        public int StructAlign => 8; // This is the required memory alignment of the struct
        public int StructSize => 16; // This is the memory size of the struct
        public string StructMember { get; set; }

        public string[] StructCode()
        {
            const string code = """
                                template <typename T>
                                struct datafile_t
                                {
                                    T*             m_ptr;
                                    const fileid_t m_fileid;
                                };

                                """;
            return code.Split("\n");
        }

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, (ulong)0);
            writer.WriteFileId(Signature);
        }
    }
}
