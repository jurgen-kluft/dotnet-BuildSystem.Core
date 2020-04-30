using System;

namespace Game.Data
{
    public struct FVector2 : ICompound
    {
        private float mX;
        private float mY;

        public FVector2(float x, float y)
        {
            mX = x;
            mY = y;
        }
        public FVector2(float v)
        {
            mX = v;
            mY = v;
        }
        public FVector2(double x, double y)
        {
            mX = (float)x;
            mY = (float)y;
        }

        public Array Values { get { float[] a = new float[] { mX, mY }; return a; } }
    }
}
