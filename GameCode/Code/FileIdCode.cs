namespace GameData
{
    /// <summary>
    /// Localization String
    ///
    /// The string will be converted to an Int64 which can be used to lookup the textual string in the Localization DB.
    ///
    /// </summary>
    public struct FileIdCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                struct fileid_t
                                {
                                    explicit fileid_t(u32 bigfileIndex, u32 fileIndex)
                                        : bigfileIndex(bigfileIndex)
                                        , fileIndex(fileIndex)
                                    {
                                    }

                                    inline u32 getBigfileIndex() const { return bigfileIndex; }
                                    inline u32 getFileIndex() const { return fileIndex; }

                                private:
                                    u32 bigfileIndex;
                                    u32 fileIndex;
                                };

                                const fileid_t INVALID_FILEID((u32)-1, (u32)-1);
                                """;
            return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
