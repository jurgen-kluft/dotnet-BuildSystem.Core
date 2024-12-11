using System;

namespace GameData
{
    public struct FSize : IStruct
    {
        public static readonly FSize SEmpty = new FSize();

        public float Width; // Size:Width
        public float Height; // Size:Height

        public FSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public bool StructIsTemplate => false;
        public string StructTemplateType => string.Empty;
        public int StructSize => 2 * sizeof(float);
        public int StructAlign => 4;
        public string StructName => "fsize_t";

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
        }
    }
}
