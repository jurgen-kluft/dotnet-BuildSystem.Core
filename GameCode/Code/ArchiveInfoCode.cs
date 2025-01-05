namespace GameData
{
    public readonly struct ArchiveInfoCode : ICode
    {
        public ICode[] CodeDependency =>  new ICode[1] { new StringCode() };

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    struct archive_info_t
                                    {
                                        inline string_t const& getArchiveData() const { return m_Data; }
                                        inline string_t const& getArchiveToc() const { return m_Toc; }
                                        inline string_t const& getArchiveFdb() const { return m_Fdb; }
                                        inline string_t const& getArchiveHdb() const { return m_Hdb; }
                                    private:
                                        string_t m_Data;
                                        string_t m_Toc;
                                        string_t m_Fdb;
                                        string_t m_Hdb;
                                    };
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
