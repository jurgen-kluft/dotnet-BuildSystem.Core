namespace GameData
{
    public struct StringCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                // A standard string (ASCII, UTF-8)
                                struct string_t
                                {
                                    inline string_t()
                                        : m_bytes(0)
                                        , m_count(0)
                                        , m_array("")
                                    {
                                    }

                                    inline string_t(u32 byteLength, u32 charLength, const char* data)
                                        : m_bytes(byteLength)
                                        , m_count(charLength)
                                        , m_array(data)
                                    {
                                    }

                                    inline u32         size() const { return m_count; }
                                    inline u32         bytes() const { return m_bytes; }
                                    inline const char* c_str() const { return m_array; }

                                private:
                                    const u32   m_bytes;
                                    const u32   m_count;
                                    char const* m_array;
                                };
                                """;
            return code.Split("\n");
        }
    }
}
