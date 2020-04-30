using System;

namespace Game.Data
{
    public struct FRect : ICompound
    {
        public static readonly FRect sEmpty = new FRect();

        public float mLeft;													    ///< Left point of rectangle
        public float mTop;													    ///< Top point of rectangle
        public float mRight;													///< Right point of rectangle
        public float mBottom;                                                   ///< Bottom point of rectangle

        public FRect(float left, float top, float right, float bottom)
        {
            mLeft = left;
            mTop = top;
            mRight = right;
            mBottom = bottom;
        }
        public FRect(double left, double top, double right, double bottom)
        {
            mLeft = (float)left;
            mTop = (float)top;
            mRight = (float)right;
            mBottom = (float)bottom;
        }

        public Array Values { get { float[] a = new float[] { mLeft, mTop, mRight, mBottom }; return a; } }
    }
}
