using System.Diagnostics;

namespace GameCore
{
    /// <summary>
    /// A StreamContext contains a list of offsets of where a 'reference' was written to a block of data.
    /// It also holds the 'Offset' to this block of data. When everything is written we can resolve all
    /// 'references' by iterating over the list of offsets and writing the 'Offset' to the block of data
    /// at that offset in the stream.
    /// </summary>
    public sealed class StreamContext
    {
        private ulong mOffsetOfReferenceInStream = StreamOffset.sEmpty.Offset;
        private readonly List<ulong> mReferencesOfReferenceInStream = new ();

        public ulong Offset
        {
            get => mOffsetOfReferenceInStream;
            set => mOffsetOfReferenceInStream = value;
        }

        public int Count => mReferencesOfReferenceInStream.Count;

        public ulong this[int index] => mReferencesOfReferenceInStream[index];

        public StreamContext()
        {

        }

        public void Add(ulong offset)
        {
            mReferencesOfReferenceInStream.Add(offset);
        }

        public void ResolveToNull(IBinaryStreamWriter writer)
        {
            if (mReferencesOfReferenceInStream.Count > 0)
            {
                if (!writer.Architecture.Is64Bit)
                {
                    const int nullPointer32 = 0;
                    var currentOffset =  (writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.sEmpty.Offset);
                        writer.Seek((long)o);
                        writer.Write(nullPointer32);
                    }

                    writer.Seek(currentOffset);
                }
                else
                {
                    const ulong nullPointer64 = 0;
                    var currentOffset = (writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.sEmpty.Offset);
                        writer.Seek((long)o);
                        writer.Write(nullPointer64);
                    }

                    writer.Seek(currentOffset);
                }
            }
        }

        public bool Resolve(IBinaryStreamWriter writer)
        {
            if (mReferencesOfReferenceInStream.Count <= 0) return true;

            if (mOffsetOfReferenceInStream == StreamOffset.sEmpty.Offset)
            {
                // Encountered a StreamContext holding references that could not be resolved!
                //
                // Explanation:
                //     This means that the data is writing references but not the actual
                //     DataBlock or Marker of that reference.
                //
                return false;
            }

            if (!writer.Architecture.Is64Bit)
            {
                var offsetToWrite = (int)mOffsetOfReferenceInStream;
                var currentOffset = writer.Position;
                foreach (var o in mReferencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.sEmpty.Offset);
                    writer.Seek((long)o);
                    writer.Write(offsetToWrite);
                }

                writer.Seek(currentOffset);
            }
            else // 64-bit pointers
            {
                var offsetToWrite = (long)mOffsetOfReferenceInStream;
                var currentOffset = writer.Position;
                foreach (var o in mReferencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.sEmpty.Offset);
                    writer.Seek((long)o);
                    writer.Write(offsetToWrite);
                }
                writer.Seek(currentOffset);
            }
            return true;
        }

    }
}
