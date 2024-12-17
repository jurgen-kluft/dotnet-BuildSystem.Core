namespace GameData
{
    public struct ArrayCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                template <class T>
                                struct array_t
                                {
                                    inline array_t()
                                        : m_array(nullptr), m_bytes(0), m_count(0)
                                    {
                                    }

                                    inline array_t(const u32 count, T const* data)
                                        : m_array(data), m_bytes(count * sizeof(T)), m_count(count)
                                    {
                                    }

                                    inline array_t(const u32 count, const u32 bytes, T const* data)
                                        : m_array(data), m_bytes(bytes), m_count(count)
                                    {
                                    }

                                    inline u32 size() const { return m_count; }
                                    inline u32 bytes() const { return m_bytes; }

                                    inline const T& operator[](s32 index) const
                                    {
                                        ASSERT(index < m_count);
                                        return m_array[index];
                                    }

                                private:
                                    T const*  m_array;
                                    const u32 m_bytes;
                                    const u32 m_count;
                                };
                                """;
            return code.Split("\n");
        }
    }
}
