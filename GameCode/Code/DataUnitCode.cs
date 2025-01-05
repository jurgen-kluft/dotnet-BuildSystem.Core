namespace GameData
{
    /// <summary>
    /// DataUnit is a struct that contains a fileid_t which is a unique identifier for the data.
    ///
    /// </summary>
    public readonly struct DataUnitCode : ICode
    {
        public ICode[] CodeDependency => new ICode[]
        {
            new ArchiveLoaderCode(),
        };

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    template <typename T>
                                    struct dataunit_t
                                    {
                                        T*    get() { return g_loader->get_dataunit_ptr<T>(m_dataunit_index); }
                                        void* load() { return g_loader->load_dataunit(m_dataunit_index); }
                                        void  unload(void*& data) { g_loader->unload_dataunit(m_dataunit_index, data); }
                                        u32   m_dataunit_index;
                                    };

                                    struct dataunit_header_t
                                    {
                                        u32 m_patch_offset;
                                        s32 m_patch_count;
                                        u32 m_dummy0;
                                        u32 m_dummy1;
                                    };
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
