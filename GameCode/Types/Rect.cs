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
        public string[] StructCode()
        {
            return Array.Empty<string>();
        }

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, Left);
            GameCore.BinaryWriter.Write(writer, Top);
            GameCore.BinaryWriter.Write(writer, Right);
            GameCore.BinaryWriter.Write(writer, Bottom);
        }
    }
}
