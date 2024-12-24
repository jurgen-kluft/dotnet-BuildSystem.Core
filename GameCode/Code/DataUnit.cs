namespace GameData
{
    /// <summary>
    /// DataUnit is a struct that contains a fileid_t which is a unique identifier for the data.
    ///
    /// </summary>
    public struct DataUnitCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                template <typename T>
                                struct dataunit_t
                                {
                                    T*                 get()                  { return (T*)g_gamedata->get_dataunit_ptr(m_dataunit_index); }
                                    void               load(loader_t &loader) { g_gamedata->load_dataunit(loader, m_dataunit_index); }
                                    u32                m_dataunit_index;
                                };

                                """;
            return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
