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

        public string[] StructCode()
        {
            const string code = """
                                template <typename T>
                                struct datafile_t
                                {
                                    T*                 get()                  { return (T*)g_gamedata->get_datafile_ptr(m_fileid); }
                                    void               load(loader_t &loader) { g_gamedata->load_datafile(loader, m_fileid); }
                                    fileid_t           m_fileid;
                                };
                                """;
            return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.WriteFileId(Signature);
        }
    }
}
