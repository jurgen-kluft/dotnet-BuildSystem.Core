using System;

namespace Game.Data
{
    public struct Rect : ICompound
    {
        public static readonly Rect sEmpty = new Rect();

        public int mLeft;													    ///< Left point of rectangle
        public int mTop;													    ///< Top point of rectangle
        public int mRight;													    ///< Right point of rectangle
        public int mBottom;                                                     ///< Bottom point of rectangle

        public Rect(int left, int top, int right, int bottom)
        {
            mLeft = left;
            mTop = top;
            mRight = right;
            mBottom = bottom;
        }

        public Array Values { get { int[] a = new int[] { mLeft, mTop, mRight, mBottom }; return a; } }
    }
}
