using System;

namespace GameData
{
    public struct FMat22 : IStruct
    {
        public readonly FVec2[] Mat;

        public FMat22(float v)
        {
            Mat = new FVec2[2];
            Mat[0].X = v;
            Mat[0].Y = v;
            Mat[1].X = v;
            Mat[1].Y = v;
        }

        public FMat22(float x1, float y1, float x2, float y2)
        {
            Mat = new FVec2[2];
            Mat[0].X = x1;
            Mat[0].Y = y1;
            Mat[1].X = x2;
            Mat[1].Y = y2;
        }

        public FMat22(FVec2 x, FVec2 y)
        {
            Mat = new FVec2[2];
            Mat[0] = x;
            Mat[1] = y;
        }

        public int StructSize => 2*2*4;
        public int StructAlign => 4;
        public string StructMember => "fmat22_t";

        public ICode StructCode => new FMat22Code();

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, Mat[0].X);
            GameCore.BinaryWriter.Write(writer, Mat[0].Y);
            GameCore.BinaryWriter.Write(writer, Mat[1].X);
            GameCore.BinaryWriter.Write(writer, Mat[1].Y);
        }
    }
}
