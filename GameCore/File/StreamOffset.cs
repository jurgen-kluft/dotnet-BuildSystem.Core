
using System.Diagnostics;

namespace GameCore
{
    public struct StreamOffset
    {
        public static readonly StreamOffset sEmpty = new (ulong.MaxValue);
        public static readonly StreamOffset sZero = new (0);

        public StreamOffset(long offset)
        {
            Debug.Assert(offset >= 0);
            Offset = (ulong)offset;
        }
        public StreamOffset(ulong offset)
        {
            Offset = offset;
        }

        public StreamOffset(StreamOffset streamOffset)
        {
            Offset = streamOffset.Offset;
        }

        public ulong Offset { get; private set; }
        public uint Offset32 => (uint)Offset;

        public static StreamOffset operator +(StreamOffset a, StreamOffset b)
        {
            return new StreamOffset(a.Offset + b.Offset);
        }

        public static StreamOffset operator +(StreamOffset a, int b)
        {
                return new StreamOffset( (ulong)((long)a.Offset + b));
        }

        public static StreamOffset operator +(StreamOffset a, uint b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, long b)
        {
            return new StreamOffset((ulong)((long)a.Offset + b));
        }

        public static StreamOffset operator +(StreamOffset a, ulong b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator -(StreamOffset a, StreamOffset b)
        {
            var c = (long)a.Offset - (long)b.Offset;
            if (c < 0)
                c = 0;
            return new StreamOffset((ulong)c);
        }

        public static bool operator ==(StreamOffset a, StreamOffset b)
        {
            return a.Offset == b.Offset;
        }

        public static bool operator !=(StreamOffset a, StreamOffset b)
        {
            return a.Offset != b.Offset;
        }

        public void Align(ulong alignment)
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
    }

}
