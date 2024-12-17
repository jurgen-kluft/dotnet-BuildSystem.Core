using System;

namespace GameData
{
    public struct Size : IStruct
    {
        public static readonly Size SEmpty = new Size();

        public int Width; // Size:Width
        public int Height; // Size:Height

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int StructSize => 2 * sizeof(int);
        public int StructAlign => 4;
        public string StructMember => "size_t";
        public string[] StructCode()
        {
            return [];
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
        }
    }
}
