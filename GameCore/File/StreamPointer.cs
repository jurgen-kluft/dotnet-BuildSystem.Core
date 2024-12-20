using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameCore
{
    public struct StreamPointer
    {
        public long Position { get; init; }   // The position in the stream of where this pointer is located
        public long DataPosition { get; init; } // Pointer is pointing to [Position + Offset]

        public void Write(IWriter writer, StreamPointer nextStreamPointer)
        {
            var next32 = (int)((nextStreamPointer.Position - Position));
            var offset32 = (int)(DataPosition - Position);
            GameCore.BinaryWriter.Write(writer, next32);
            GameCore.BinaryWriter.Write(writer, offset32);
        }
    }
}
