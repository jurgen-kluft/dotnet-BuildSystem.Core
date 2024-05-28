using System;
using System.Diagnostics;

namespace GameCore
{
    public struct StreamOffset
    {
        #region Fields

        public readonly static StreamOffset Empty = new (-1);
        public readonly static StreamOffset Zero = new (0);

        #endregion
        #region Constructor

        public StreamOffset(Int64 offset)
        {
            Offset = offset;
        }

        public StreamOffset(StreamOffset streamOffset)
        {
            Offset = streamOffset.Offset;
        }

        #endregion
        #region Properties

        public Int64 Offset { get; set; }
        public Int32 Offset32 => (Int32)Offset;

        #endregion
        #region Operators

        public static StreamOffset operator +(StreamOffset a, StreamOffset b)
        {
            return new StreamOffset(a.Offset + b.Offset);
        }

        public static StreamOffset operator +(StreamOffset a, Int32 b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, UInt32 b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, Int64 b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, UInt64 b)
        {
            return new StreamOffset(a.Offset + (Int64)b);
        }

        public static StreamOffset operator -(StreamOffset a, StreamOffset b)
        {
            var c = a.Offset - b.Offset;
            if (c < 0)
                c = -1;
            return new (c);
        }

        public static bool operator ==(StreamOffset a, StreamOffset b)
        {
            return a.Offset == b.Offset;
        }

        public static bool operator !=(StreamOffset a, StreamOffset b)
        {
            return a.Offset != b.Offset;
        }

        #endregion
        #region Methods

        public void Align(Int64 alignment)
        {
            Offset = (Offset + (alignment - 1)) & ~(alignment - 1);
        }

        public override bool Equals(object o)
        {
            if (o is StreamOffset b)
            {
                return b.Offset == Offset;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode();
        }

        #endregion
    }

}
