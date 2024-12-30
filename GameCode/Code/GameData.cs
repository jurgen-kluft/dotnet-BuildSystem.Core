namespace GameData
{
    /// <summary>
    ///
    /// </summary>
    public struct GameDataCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                class filesystem_t
                                {
                                public:
                                    template <typename T>
                                    void* get_datafile_ptr(fileid_t fileid)
                                    {
                                        return (T*)v_get_datafile_ptr(fileid);
                                    }
                                    template <typename T>
                                    void* get_dataunit_ptr(u32 dataunit_index)
                                    {
                                        return (T*)v_get_dataunit_ptr(dataunit_index);
                                    }

                                    void load_datafile(loader_t& loader, fileid_t fileid) { v_load_datafile(loader, fileid); }
                                    void load_dataunit(loader_t& loader, u32 dataunit_index) { v_load_dataunit(loader, dataunit_index); }

                                protected:
                                    virtual void* v_get_datafile_ptr(fileid_t fileid)                   = 0;
                                    virtual void* v_get_dataunit_ptr(u32 dataunit_index)                = 0;
                                    virtual void  v_load_datafile(loader_t& loader, fileid_t fileid)    = 0;
                                    virtual void  v_load_dataunit(loader_t& loader, u32 dataunit_index) = 0;
                                };

                                filesystem_t* g_filesystem;
                                """;
            return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
