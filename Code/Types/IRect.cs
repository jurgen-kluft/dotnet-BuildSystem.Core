using System;

namespace Game.Data
{
    public struct IRect : ICompound
    {
        public static readonly IRect sEmpty = new IRect();

        public int mLeft;													    ///< Left point of rectangle
        public int mTop;													    ///< Top point of rectangle
        public int mRight;													    ///< Right point of rectangle
        public int mBottom;                                                     ///< Bottom point of rectangle

        public IRect(int left, int top, int right, int bottom)
        {
            mLeft = left;
            mTop = top;
            mRight = right;
            mBottom = bottom;
        }

        public Array Values { get { int[] a = new int[] { mLeft, mTop, mRight, mBottom }; return a; } }
    }
}
