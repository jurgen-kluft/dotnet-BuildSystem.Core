using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    /// <summary>
    /// Localization String
    ///
    /// The string will be converted to an Int64 which can be used to lookup the textual string in the Localization DB.
    ///
    /// </summary>
    public struct LocStrCode : ICode
    {
        public string[] GetCode()
        {
            const string code = """
                                struct locstr_t
                                {
                                    explicit locstr_t(u64 id)
                                    : id(id)
                                    {
                                    }
                                    inline u64 getId() const { return id; }

                                private:
                                    u64 id;
                                };

                                const locstr_t INVALID_LOCSTR((u64)-1);

                                """;
            return code.Split("\n");
        }
    }
}
