
namespace GameCore
{
    public struct StreamOffset
    {
        public static readonly StreamOffset sEmpty = new (-1);
        public static readonly StreamOffset sZero = new (0);

        public StreamOffset(long offset)
        {
            Offset = offset;
        }

        public StreamOffset(StreamOffset streamOffset)
        {
            Offset = streamOffset.Offset;
        }

        public long Offset { get; private set; }
        public int Offset32 => (int)Offset;

        public static StreamOffset operator +(StreamOffset a, StreamOffset b)
        {
            return new StreamOffset(a.Offset + b.Offset);
        }

        public static StreamOffset operator +(StreamOffset a, int b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, uint b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, long b)
        {
            return new StreamOffset(a.Offset + b);
        }

        public static StreamOffset operator +(StreamOffset a, ulong b)
        {
            return new StreamOffset(a.Offset + (long)b);
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

        public void Align(long alignment)
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
