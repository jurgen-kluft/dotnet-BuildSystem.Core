using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    /// <summary>
    /// Localization String
    ///
    /// The string will be converted to an Int64 with looking up the textual string in the Localization DB.
    ///
    /// </summary>
    public struct LString : IStruct, ILStringIdProvider
    {
        #region Constructor

        public LString(string s)
        {
            LStringText = s;
            LStringId = -1;
        }

        public string LStringText { get; private set; }
        public Int64 LStringId { get; set; }

        #endregion

        #region Properties

        public bool StructIsValueType => true;
        public int StructAlign => 8;
        public string StructName => "lstring_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(LStringId);
        }

        #endregion
    }
}
