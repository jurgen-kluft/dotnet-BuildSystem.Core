using System;

namespace Game.Data
{
    public struct FVec3 : ICompound
    {
        public float X;
        public float Y;
        public float Z;

        public FVec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public FVec3(float v)
        {
            X = v;
            Y = v;
            Z = v;
        }
        public FVec3(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        public Array Values { get { float[] a = new float[] { X, Y, Z }; return a; } }
    }
}
