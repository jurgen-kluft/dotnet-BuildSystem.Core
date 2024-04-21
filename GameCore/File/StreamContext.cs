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
        private Int64 _offsetOfReferenceInStream = StreamOffset.Empty.Offset;
        private readonly List<Int64> _referencesOfReferenceInStream = new ();

        public Int64 Offset
        {
            get => _offsetOfReferenceInStream;
            set => _offsetOfReferenceInStream = value;
        }

        public int Count => _referencesOfReferenceInStream.Count;

        public Int64 this[int index] => _referencesOfReferenceInStream[index];

        public StreamContext()
        {

        }

        public void Add(Int64 offset)
        {
            _referencesOfReferenceInStream.Add(offset);
        }

        public void ResolveToNull(IBinaryStreamWriter writer)
        {
            if (_referencesOfReferenceInStream.Count > 0)
            {
                if (!writer.Architecture.Is64Bit)
                {
                    const Int32 NULL = 0;
                    var currentOffset =  (writer.Position);
                    foreach (var o in _referencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty.Offset);
                        writer.Seek(o);
                        writer.Write(NULL);
                    }

                    writer.Seek(currentOffset);
                }
                else
                {
                    const Int64 NULL = 0;
                    var currentOffset = (writer.Position);
                    foreach (var o in _referencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty.Offset);
                        writer.Seek(o);
                        writer.Write(NULL);
                    }

                    writer.Seek(currentOffset);
                }
            }
        }

        public bool Resolve(IBinaryStreamWriter writer)
        {
            if (_referencesOfReferenceInStream.Count <= 0) return true;

            if (_offsetOfReferenceInStream == StreamOffset.Empty.Offset)
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
                Int32 offsetToWrite = (Int32)_offsetOfReferenceInStream;
                var currentOffset = writer.Position;
                foreach (var o in _referencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.Empty.Offset);
                    writer.Seek(o);
                    writer.Write(offsetToWrite);
                }

                writer.Seek(currentOffset);
            }
            else // 64-bit pointers
            {
                Int64 offsetToWrite = (Int64)_offsetOfReferenceInStream;
                var currentOffset = writer.Position;
                foreach (var o in _referencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.Empty.Offset);
                    writer.Seek(o);
                    writer.Write(offsetToWrite);
                }
                writer.Seek(currentOffset);
            }
            return true;
        }

    }
}
