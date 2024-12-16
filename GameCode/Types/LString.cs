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
        public string StructMember => "lstring_t";
        public void StructCode(StreamWriter writer)
        {
            // already defined in C++ library charon
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(LStringId);
        }
    }
}
