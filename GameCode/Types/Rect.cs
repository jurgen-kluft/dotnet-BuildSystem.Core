using System;

namespace GameData
{
    public struct Rect : IStruct
    {
        public static readonly Rect SEmpty = new Rect();

        public int Left; // Left point of rectangle
        public int Top; // Top point of rectangle
        public int Right; // Right point of rectangle
        public int Bottom; // Bottom point of rectangle

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int StructSize => 4 * sizeof(int);
        public int StructAlign => 4;
        public string StructMember => "rect_t";
        public void StructCode(StreamWriter writer)
        {
            // already defined in C++ library charon
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(Left);
            writer.Write(Top);
            writer.Write(Right);
            writer.Write(Bottom);
        }
    }
}
