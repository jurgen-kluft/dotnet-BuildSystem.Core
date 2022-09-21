using System;

namespace Game.Data
{
    public struct FMat2x2 : ICompound
    {
        public FVec2 mMatrix[2];

        public FMat2x2(float v)
        {
            mMatrix[0].X = v;
            mMatrix[0].Y = v;
            mMatrix[1].X = v;
            mMatrix[1].Y = v;
        }
        public FMat2x2(float x1, float y1, float x2, float y2)
        {
            mMatrix[0].X = x1;
            mMatrix[0].Y = y1;
            mMatrix[1].X = x2;
            mMatrix[1].Y = y2;
        }
        public FMat2x2(FVec2 x, FVec2 y)
        {
            mMatrix[0] = x;
            mMatrix[1] = y;
        }

        public Array Values { get { float[] a = new float[] { mMatrix[0].X, mMatrix[0].Y, mMatrix[1].X, mMatrix[1].Y }; return a; } }
    }
}
