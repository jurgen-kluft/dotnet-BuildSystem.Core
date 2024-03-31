using System;

namespace GameData
{
    public struct FVec2 : IStruct
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

        public int StructSize => 2 * sizeof(float);
        public int StructAlign => 4;
        public string StructName => "fvec2_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }
}
