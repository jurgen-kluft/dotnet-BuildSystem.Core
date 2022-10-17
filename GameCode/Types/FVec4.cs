using System;

namespace GameData
{
    public struct FVec4 : IStruct
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

        public int StructSize => 4 * sizeof(float);
        public int StructAlign => 4;
        public string StructName => "fvec4_t";
        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);
        }
    }
}
