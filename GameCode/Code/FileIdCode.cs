namespace GameData
{
    /// <summary>
    /// Localization String
    ///
    /// The string will be converted to an Int64 which can be used to lookup the textual string in the Localization DB.
    ///
    /// </summary>
    public readonly struct FileIdCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    struct fileid_t
                                    {
                                        explicit fileid_t(u32 archiveIndex, u32 fileIndex)
                                            : m_ArchiveIndex(archiveIndex)
                                            , m_FileIndex(fileIndex)
                                        {
                                        }
                                        inline u32 getArchiveIndex() const { return m_ArchiveIndex; }
                                        inline u32 getFileIndex() const { return m_FileIndex; }
                                    private:
                                        u32 m_ArchiveIndex;
                                        u32 m_FileIndex;
                                    };
                                    const fileid_t INVALID_FILEID((u32)-1, (u32)-1);
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
