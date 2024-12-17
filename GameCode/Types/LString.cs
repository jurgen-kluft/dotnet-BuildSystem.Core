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
    public struct LString : IStruct, ILStringIdProvider
    {
        public LString(string s)
        {
            LStringText = s;
            LStringId = -1;
        }

        public string LStringText { get; private set; }
        public long LStringId { get; set; }

        public int StructSize => sizeof(long);
        public int StructAlign => 8;
        public string StructMember => "locstr_t";

        public string[] StructCode()
        {
            const string code = """
                                struct locstr_t
                                {
                                    explicit locstr_t(u64 id) : id(id) {}
                                    inline u64 GetId() const { return id; }
                                private:
                                    u64 id;
                                };
                                const locstr_t INVALID_LOCSTR((u64)-1);

                                """;
            return code.Split("\n");
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(LStringId);
        }
    }
}
