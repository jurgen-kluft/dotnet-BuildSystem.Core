using System;

namespace Game.Data
{
    public struct Rect : ICompound
    {
        public static readonly Rect sEmpty = new Rect();

        public int Left;													    ///< Left point of rectangle
        public int Top;													    ///< Top point of rectangle
        public int Right;													    ///< Right point of rectangle
        public int Bottom;                                                     ///< Bottom point of rectangle

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Array Values { get { int[] a = new int[] { Left, Top, Right, Bottom }; return a; } }
    }
}
