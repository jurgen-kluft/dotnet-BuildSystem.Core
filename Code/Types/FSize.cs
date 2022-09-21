using System;

namespace Game.Data
{
    public struct FSize : ICompound
    {
        public static readonly FSize sEmpty = new FSize();

        public float Width;													    ///< Size:Width
        public float Height;													    ///< Size:Height

        public FSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public Array Values { get { float[] a = new float[] { Width, Height }; return a; } }
    }
}
