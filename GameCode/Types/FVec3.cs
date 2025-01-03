using System;

namespace GameData
{
    public struct FVec3 : IStruct
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

        public int StructSize => 3 * sizeof(float);
        public int StructAlign => 4;
        public string StructMember => "fvec3_t";
        public ICode StructCode => new FVec3Code();

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, X);
            GameCore.BinaryWriter.Write(writer, Y);
            GameCore.BinaryWriter.Write(writer, Z);
        }
    }
}
