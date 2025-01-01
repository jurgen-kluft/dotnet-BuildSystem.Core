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
    public struct LocStr : IStruct, ILStringIdProvider
    {
        public LocStr(string s)
        {
            LocStrText = s;
            LocStrId = -1;
        }

        public string LocStrText { get; private set; }
        public long LocStrId { get; set; }

        public int StructSize => sizeof(long);
        public int StructAlign => 8;
        public string StructMember => "locstr_t";

        public ICode StructCode => new LocStrCode();

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, LocStrId);
        }
    }
}
