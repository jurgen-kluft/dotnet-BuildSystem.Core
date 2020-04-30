using System;

namespace Game.Data
{
    public struct FVector3 : ICompound
    {
        private float mX;
        private float mY;
        private float mZ;

        public FVector3(float x, float y, float z)
        {
            mX = x;
            mY = y;
            mZ = z;
        }
        public FVector3(float v)
        {
            mX = v;
            mY = v;
            mZ = v;
        }
        public FVector3(double x, double y, double z)
        {
            mX = (float)x;
            mY = (float)y;
            mZ = (float)z;
        }

        public Array Values { get { float[] a = new float[] { mX, mY, mZ }; return a; } }
    }
}
