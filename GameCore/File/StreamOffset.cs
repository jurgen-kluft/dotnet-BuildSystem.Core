using System;
using System.Diagnostics;

namespace GameCore
{
    public struct StreamOffset
    {
        #region Fields

        public readonly static StreamOffset Empty = new (-1);
        public readonly static StreamOffset Zero = new (0);

        private Int64 mOffset;

        #endregion
        #region Constructor

        public StreamOffset(Int64 offset)
        {
            mOffset = offset;
        }

        public StreamOffset(StreamOffset streamOffset)
        {
            mOffset = streamOffset.value;
        }

        #endregion
        #region Properties

        public bool isEmpty
        {
            get
            {
                return mOffset == -1;
            }
        }

        public bool isZero
        {
            get
            {
                return mOffset == 0;
            }
        }

        public Int64 value
        {
            set
            {
                mOffset = value;
            }
            get
            {
                return mOffset;
            }
        }

        #endregion
        #region Operators

        public static StreamOffset operator +(StreamOffset a, StreamOffset b)
        {
            StreamOffset c = a;
            c.mOffset += b.mOffset;
            return c;
        }

        public static StreamOffset operator +(StreamOffset a, Int32 b)
        {
            StreamOffset c = a;
            c.mOffset += b;
            return c;
        }

        public static StreamOffset operator +(StreamOffset a, UInt32 b)
        {
            StreamOffset c = a;
            c.mOffset += b;
            return c;
        }

        public static StreamOffset operator +(StreamOffset a, Int64 b)
        {
            StreamOffset c = a;
            c.mOffset += b;
            return c;
        }

        public static StreamOffset operator +(StreamOffset a, UInt64 b)
        {
            StreamOffset c = a;
            c.mOffset += (Int64)b;
            return c;
        }

        public static StreamOffset operator -(StreamOffset a, StreamOffset b)
        {
            StreamOffset c = a;
            c.mOffset -= b.mOffset;
            if (c.mOffset < 0)
                c.mOffset = -1;
            return c;
        }

        public static bool operator ==(StreamOffset a, StreamOffset b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a StreamOffset.Empty object as default!
            return a.value == b.value;
        }

        public static bool operator !=(StreamOffset a, StreamOffset b)
        {
            Debug.Assert((object)a != null && (object)b != null);               // Never compare to null, create a StreamOffset.Empty object as default!
            return a.value != b.value;
        }

        #endregion
        #region Methods

        public void Align(Int64 alignment)
        {
            mOffset = (mOffset + (alignment - 1)) & ~(alignment - 1);
        }

        public override bool Equals(object o)
        {
            if (o is StreamOffset)
            {
                StreamOffset b = (StreamOffset) o;
                return b.value == value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        #endregion
    }

}
