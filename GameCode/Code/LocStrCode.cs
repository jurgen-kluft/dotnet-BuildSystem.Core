using Environment = System.Environment;

namespace GameData
{
    /// <summary>
    /// Localization String
    ///
    /// The string will be converted to an Int64 which can be used to lookup the textual string in the Localization DB.
    ///
    /// </summary>
    public readonly struct LocStrCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    struct locstr_t
                                    {
                                        s64 id;
                                    };
                                    const locstr_t INVALID_LOCSTR = {-1};
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
