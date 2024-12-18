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
                                    explicit fileid_t(u64 id)
                                    : id(id)
                                    {
                                    }
                                    inline u64 getId() const { return id; }

                                private:
                                    u64 id;
                                };

                                const fileid_t INVALID_FILEID((u64)-1);

                                """;
            return code.Split("\n");
        }
    }
}
