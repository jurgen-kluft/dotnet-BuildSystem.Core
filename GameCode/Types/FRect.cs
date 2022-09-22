using System;

namespace GameData
{
    public struct FRect : ICompound
    {
        public static readonly FRect sEmpty = new FRect();

        public float Left;													    ///< Left point of rectangle
        public float Top;													    ///< Top point of rectangle
        public float Right;													///< Right point of rectangle
        public float Bottom;                                                   ///< Bottom point of rectangle

        public FRect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public FRect(double left, double top, double right, double bottom)
        {
            Left = (float)left;
            Top = (float)top;
            Right = (float)right;
            Bottom = (float)bottom;
        }

        public Array Values { get { float[] a = new float[] { Left, Top, Right, Bottom }; return a; } }
    }
}
