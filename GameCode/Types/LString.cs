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

        public bool StructIsTemplate => false;
        public string StructTemplateType => string.Empty;
        public int StructSize => sizeof(long);
        public int StructAlign => 8;
        public string StructName => "lstring_t";

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(LStringId);
        }
    }
}
