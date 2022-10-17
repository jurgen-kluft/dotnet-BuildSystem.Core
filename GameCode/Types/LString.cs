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
    public struct LString : IStruct, ILStringProvider
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

        public int StructSize => sizeof(Int64);
        public int StructAlign => 8;
        public string StructName => "lstring_t";
        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(mStringId);
        }


        #endregion
        #region Methods

        static public LString[] SArray(params string[] values)
        {
            LString[] array = new LString[values.Length];
            int i = 0;
            foreach (string s in values)
                array[i++] = new LString(s);
            return array;
        }

        #endregion
    }
}
