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

        public bool StructIsTemplate => false;
        public string StructTemplateType => string.Empty;
        public int StructSize => 2 * sizeof(int);
        public int StructAlign => 4;
        public string StructName => "size_t";

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
        }
    }
}
