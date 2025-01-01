namespace GameData
{
    public struct ColorCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                struct color_t
                                {
                                    explicit color_t(u32 color)
                                        : m_color(color)
                                    {
                                    }

                                    inline u32 getARGB() const { return m_color; }

                                    inline u8  A() const { return (u8)(m_color >> 24); }
                                    inline u8  R() const { return (u8)(m_color >> 16); }
                                    inline u8  G() const { return (u8)(m_color >> 8); }
                                    inline u8  B() const { return (u8)(m_color); }

                                private:
                                    u32 m_color;
                                };
                                const color_t sBlackColor(0xFF000000);
                                const color_t sWhiteColor(0xFFFFFFFF);

                                """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
