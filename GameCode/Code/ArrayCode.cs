namespace GameData
{
    public readonly struct ArrayCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    template <class T>
                                    struct array_t
                                    {
                                        inline array_t()
                                            : m_array(nullptr), m_bytes(0), m_count(0)
                                        {
                                        }
                                        inline array_t(u32 count, T* data)
                                            : m_array(data), m_bytes(count * sizeof(T)), m_count(count)
                                        {
                                        }
                                        inline array_t(u32 count, u32 bytes, T* data)
                                            : m_array(data), m_bytes(bytes), m_count(count)
                                        {
                                        }
                                        inline u32 size() const { return m_count; }
                                        inline u32 bytes() const { return m_bytes; }
                                        inline T& operator[](s32 index)
                                        {
                                            ASSERT(index < m_count);
                                            return m_array[index];
                                        }
                                        inline const T& operator[](s32 index) const
                                        {
                                            ASSERT(index < m_count);
                                            return m_array[index];
                                        }
                                    private:
                                        T*  m_array;
                                        u32 m_bytes;
                                        u32 m_count;
                                    };
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
