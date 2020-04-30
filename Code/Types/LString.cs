using System;
using System.Collections.Generic;

using Core;

namespace Game.Data
{
    /// <summary>
    /// Localization String
    /// 
    /// The string will be converted to a UInt64 value that is the 64bit hash of the textual ID.
    /// 
    /// On the user side we do have to use BinarySearch to obtain the value, for less than 2048 items
    /// that would translate into a maximum of 9 iterations (9 uint64 compares) to obtain the string.
    /// 
    /// Consequences:
    /// Any hash collision will result in a wrong localization string being obtained by the user.
    /// 
    /// </summary>
    public struct LString : IAtom
    {
        #region Fields

        private readonly string mString;
        private readonly UInt64 mStringHash64;

        #endregion
        #region Constructor

        public LString(string s)
        {
            mString = s;
            mStringHash64 = Hashing64.ComputeMurmur64(s);
        }

        #endregion
        #region Properties

        public object Value { get { return mStringHash64; } }

        #endregion
        #region Methods

        static public LString[] sArray(params string[] values)
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
