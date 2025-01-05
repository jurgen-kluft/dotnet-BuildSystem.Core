using Environment = System.Environment;

namespace GameData
{
    public readonly struct DataFileCode : ICode
    {
        public ICode[] CodeDependency => new ICode[]
        {
            new ArchiveLoaderCode(),
            new FileIdCode(),
        };

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    template <typename T>
                                    struct datafile_t
                                    {
                                        T*       get() const { return g_loader->get_datafile_ptr<T>(m_fileid); }
                                        void*    load() const { return g_loader->load_datafile(m_fileid); }
                                        void     unload(T*& data) const { g_loader->unload_datafile(m_fileid, data); }
                                        fileid_t m_fileid;
                                    };
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
