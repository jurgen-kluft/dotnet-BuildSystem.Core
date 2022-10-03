using System;
using System.Collections.Generic;

namespace GameCore
{
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

        public UInt32 id
        {
            get { return mID; }
        }

        #endregion
        #region Operators

        public static bool operator ==(StreamReference a, StreamReference b)
        {
            return a.id == b.id;
        }
        public static bool operator !=(StreamReference a, StreamReference b)
        {
            return a.id != b.id;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public class Comparer : IEqualityComparer<StreamReference>
        {
            public bool Equals(StreamReference lhs, StreamReference rhs)
            {
                return lhs.id == rhs.id;
            }

            public int GetHashCode(StreamReference r)
            {
                return (int)r.id;
            }
        }

        #endregion
        #region Methods

        public static StreamReference Instance
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
            return ((StreamReference)obj).id == id;
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        #endregion
    }
}
