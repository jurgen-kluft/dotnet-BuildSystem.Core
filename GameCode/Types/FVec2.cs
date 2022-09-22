using System;

namespace GameData
{
    public struct FVec2 : ICompound
    {
        public float X;
        public float Y;

        public FVec2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public FVec2(float v)
        {
            X = v;
            Y = v;
        }
        public FVec2(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }

        public Array Values { get { float[] a = new float[] { X, Y }; return a; } }
    }
}
