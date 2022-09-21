using System;

namespace Game.Data
{
    public struct FVec4 : ICompound
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public FVec4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public FVec4(float v)
        {
            X = v;
            Y = v;
            Z = v;
            W = v;
        }
        public FVec4(double x, double y, double z, double w)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
            W = (float)w;
        }

        public Array Values { get { float[] a = new float[] { X, Y, Z, W }; return a; } }
    }
}
