using System;

namespace Game.Data
{
    public struct FSize : ICompound
    {
        public static readonly FSize sEmpty = new FSize();

        public float mWidth;													    ///< Size:Width
        public float mHeight;													    ///< Size:Height

        public FSize(float width, float height)
        {
            mWidth = width;
            mHeight = height;
        }

        public Array Values { get { float[] a = new float[] { mWidth, mHeight }; return a; } }
    }
}
