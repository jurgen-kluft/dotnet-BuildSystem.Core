using System;

namespace GameData
{
    public struct FRect : IStruct
    {
        public static readonly FRect SEmpty = new FRect();

        public float Left;

        // Left point of rectangle
        public float Top;

        // Top point of rectangle
        public float Right;

        // Right point of rectangle
        public float Bottom;

        // Bottom point of rectangle
        public FRect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public FRect(double left, double top, double right, double bottom)
        {
            Left = (float)left;
            Top = (float)top;
            Right = (float)right;
            Bottom = (float)bottom;
        }

        public int StructSize => 4*sizeof(float);
        public int StructAlign => 4;
        public string StructMember => "frect_t";

        public ICode StructCode => new FRectCode();

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, Left);
            GameCore.BinaryWriter.Write(writer, Top);
            GameCore.BinaryWriter.Write(writer, Right);
            GameCore.BinaryWriter.Write(writer, Bottom);
        }
    }
}
