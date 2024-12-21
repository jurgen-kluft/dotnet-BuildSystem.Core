namespace GameCore
{
    public readonly struct StreamPointer
    {
        public int PositionInSourceStream { get; init; }   // The position in the original stream of where this pointer is located
        public int PositionInDestinationStream { get; init; }   // The position in the destination stream of where this pointer is located
        public int TargetPosition { get; init; } // Pointer is pointing to [Position + Offset]

        public void Write(IStreamWriter sourceStream, StreamPointer nextStreamPointer)
        {
            var next32 = nextStreamPointer.PositionInDestinationStream - PositionInDestinationStream;
            var offset32 = TargetPosition - PositionInDestinationStream;

            sourceStream.Position = PositionInSourceStream;
            BinaryWriter.Write(sourceStream, next32);
            BinaryWriter.Write(sourceStream, offset32);
        }
    }
}
