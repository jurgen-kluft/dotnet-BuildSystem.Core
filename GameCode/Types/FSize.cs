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

        public int StructSize => 2 * sizeof(float);
        public int StructAlign => 4;
        public string StructMember => "fsize_t";

        public string[] StructCode()
        {
            return Array.Empty<string>();
        }

        public void StructWrite(IGameDataWriter writer)
        {
            GameCore.BinaryWriter.Write(writer, Width);
            GameCore.BinaryWriter.Write(writer, Height);
        }
    }
}
