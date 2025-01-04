namespace GameData
{
    public readonly struct FRectCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                struct frect_t
                                {
                                    explicit frect_t(f32 left = 0.0f, f32 top = 0.0f, f32 right = 0.0f, f32 bottom = 0.0f)
                                        : m_left(left), m_top(top), m_right(right), m_bottom(bottom)
                                    {
                                    }
                                    inline f32 left() const { return m_left; }
                                    inline f32 top() const { return m_top; }
                                    inline f32 right() const { return m_right; }
                                    inline f32 bottom() const { return m_bottom; }
                                private:
                                    f32 m_left;
                                    f32 m_top;
                                    f32 m_right;
                                    f32 m_bottom;
                                };
                                """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
