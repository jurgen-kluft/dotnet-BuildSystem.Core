namespace BigfileBuilder
{
    public sealed class BigfileFile
    {
        public string Filename { get; set; }
        public ulong Size { get; set; } = 0;
        public ulong Offset { get; set; } = long.MaxValue;
        public bool IsValid => Size > 0 && Offset != long.MaxValue;
        public byte[] ContentHash { get; set; } = new byte[20];
    }
}
