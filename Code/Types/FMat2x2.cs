using System;

namespace Game.Data
{
    public struct FMat2x2 : ICompound
    {
        public readonly FVec2[] Mat;

        public FMat2x2(float v)
        {
            Mat = new FVec2[2];
            Mat[0].X = v;
            Mat[0].Y = v;
            Mat[1].X = v;
            Mat[1].Y = v;
        }
        public FMat2x2(float x1, float y1, float x2, float y2)
        {
            Mat = new FVec2[2];
            Mat[0].X = x1;
            Mat[0].Y = y1;
            Mat[1].X = x2;
            Mat[1].Y = y2;
        }
        public FMat2x2(FVec2 x, FVec2 y)
        {
            Mat = new FVec2[2];
            Mat[0] = x;
            Mat[1] = y;
        }

        public Array Values { get { float[] a = new float[] { Mat[0].X, Mat[0].Y, Mat[1].X, Mat[1].Y }; return a; } }
    }
}
