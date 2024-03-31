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

        public int StructSize => 4 * sizeof(float);
        public int StructAlign => 4;
        public string StructName => "frect_t";

        public void StructWrite(GameCore.IBinaryWriter writer)
        {
            writer.Write(Left);
            writer.Write(Top);
            writer.Write(Right);
            writer.Write(Bottom);
        }
    }
}
