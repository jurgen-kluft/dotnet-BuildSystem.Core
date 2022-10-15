using System;
using System.Collections.Generic;

using GameCore;

namespace GameData
{
    /// <summary>
    /// Localization String
    /// 
    /// The string will be converted to an Int64 with looking up the textual string in the Location DB.
    /// 
    /// </summary>
    public struct LString : IAtom
    {
        #region Fields

        private readonly Int64 mStringId;

        #endregion
        #region Constructor

        public LString(string s)
        {
            Text = s;
            mStringId = -1;
        }

        public string Text { get; private set; }

        #endregion
        #region Properties

        public object Value { get { return mStringId; } }

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
