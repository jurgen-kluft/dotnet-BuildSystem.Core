using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameCore
{
    [DebuggerDisplay("ID: {ID}")]
    public struct StreamReference
    {
        #region Fields

        public static readonly StreamReference Empty = new StreamReference(0);
        private static UInt32 sID = 1;

        private readonly UInt32 mID;

        #endregion
        #region Constructor

        private StreamReference(UInt32 id)
        {
            mID = id;
        }

        public StreamReference(StreamReference r)
        {
            mID = r.mID;
        }

        #endregion
        #region Properties

        public UInt32 ID
        {
            get { return mID; }
        }

        #endregion
        #region Operators

        public static bool operator ==(StreamReference a, StreamReference b)
        {
            return a.ID == b.ID;
        }
        public static bool operator !=(StreamReference a, StreamReference b)
        {
            return a.ID != b.ID;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public class Comparer : IEqualityComparer<StreamReference>
        {
            public bool Equals(StreamReference lhs, StreamReference rhs)
            {
                return lhs.ID == rhs.ID;
            }

            public int GetHashCode(StreamReference r)
            {
                return (int)r.ID;
            }
        }

        #endregion
        #region Methods

        public static StreamReference NewReference
        {
            get
            {
                StreamReference sr = new(sID);
                ++sID;
                return sr;
            }
        }

        public override bool Equals(object obj)
        {
            return ((StreamReference)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return (int)ID;
        }

        #endregion
    }
}
