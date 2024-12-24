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
                                class gamedata_t
                                {
                                public:
                                    virtual void* get_datafile_ptr(fileid_t fileid) = 0;
                                    virtual void* get_dataunit_ptr(u32 dataunit_index) = 0;
                                    virtual void  load_datafile(loader_t &loader, fileid_t fileid) = 0;
                                    virtual void  load_dataunit(loader_t &loader, u32 dataunit_index) = 0;
                                };

                                gamedata_t* g_gamedata;
                                """;
            return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
