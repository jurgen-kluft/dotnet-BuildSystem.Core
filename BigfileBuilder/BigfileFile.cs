using System;
using GameCore;

namespace BigfileBuilder
{
    public sealed class BigfileFile
    {
        public string Filename { get; set; }
        public ulong Size { get; set; } = 0;
        public ulong Offset { get; set; } = long.MaxValue;
        public bool IsValid => Size > 0 && Offset != long.MaxValue;
        public Hash160 ContentHash { get; set; } = Hash160.Empty;
    }
}
