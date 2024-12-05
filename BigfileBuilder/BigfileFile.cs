using System;
using GameCore;

namespace BigfileBuilder
{
    public sealed class BigfileFile
    {
        public static readonly BigfileFile sEmpty = new(string.Empty);

        public BigfileFile(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; }
        public long FileSize { get; set; } = 0;
        public StreamOffset FileOffset { get; set; } = StreamOffset.sEmpty;
        public ulong FileId { get; init; } = ulong.MaxValue;
        public Hash160 FileContentHash { get; set; } = Hash160.Empty;
        public List<BigfileFile> Children { get; set; } = new ();
    }
}
