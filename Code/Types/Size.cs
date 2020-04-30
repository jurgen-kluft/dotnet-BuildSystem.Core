using System;

namespace Game.Data
{
    public struct Size : ICompound
    {
        public static readonly Size sEmpty = new Size();

        public int mWidth;													    ///< Size:Width
        public int mHeight;													    ///< Size:Height

        public Size(int width, int height)
        {
            mWidth = width;
            mHeight = height;
        }

        public Array Values { get { int[] a = new int[] { mWidth, mHeight }; return a; } }
    }
}
