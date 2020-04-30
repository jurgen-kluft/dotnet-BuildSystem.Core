using System;

namespace Game.Data
{
    public struct FVector4 : ICompound
    {
        private float mX;
        private float mY;
        private float mZ;
        private float mW;

        public FVector4(float x, float y, float z, float w)
        {
            mX = x;
            mY = y;
            mZ = z;
            mW = w;
        }
        public FVector4(float v)
        {
            mX = v;
            mY = v;
            mZ = v;
            mW = v;
        }
        public FVector4(double x, double y, double z, double w)
        {
            mX = (float)x;
            mY = (float)y;
            mZ = (float)z;
            mW = (float)w;
        }

        public Array Values { get { float[] a = new float[] { mX, mY, mZ, mW }; return a; } }
    }
}
