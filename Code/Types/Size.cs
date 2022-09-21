using System;

namespace Game.Data
{
    public struct Size : ICompound
    {
        public static readonly Size sEmpty = new Size();

        public int Width;													    ///< Size:Width
        public int Height;													    ///< Size:Height

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Array Values { get { int[] a = new int[] { Width, Height }; return a; } }
    }
}
