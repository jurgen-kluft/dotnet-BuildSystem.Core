using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameCore
{
    public struct StreamPointer
    {
        public long Position { get; init; }   // The position in the stream of where the pointer is located
        public long DataOffset { get; init; } // Pointer is pointing to [Position + Offset]

        public void Write(IBinaryWriter writer, StreamPointer previousStreamPointer)
        {
            var next32 = (int)((previousStreamPointer.Position - Position) / 8);
            var offset32 = (int)(DataOffset / 8);
            writer.Write(next32);
            writer.Write(offset32);
        }
    }
}