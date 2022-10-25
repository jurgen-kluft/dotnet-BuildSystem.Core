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
        #region Fields

        private StreamOffset mOffsetOfReferenceInStream = StreamOffset.Empty;
        private readonly List<StreamOffset> mReferencesOfReferenceInStream = new List<StreamOffset>();

        #endregion
        #region Properties

        public EPlatform Platform { get; set; }

        public StreamOffset Offset
        {
            get => mOffsetOfReferenceInStream;
            set => mOffsetOfReferenceInStream = value;
        }

        public int Count => mReferencesOfReferenceInStream.Count;

        public StreamOffset this[int index] => mReferencesOfReferenceInStream[index];

        #endregion
        #region Constructor

        public StreamContext(EPlatform platform)
        {
            Platform = platform;
        }

        #endregion
        #region Methods

        public void Add(StreamOffset offset)
        {
            mReferencesOfReferenceInStream.Add(offset);
        }

        public void ResolveToNull(IBinaryStream writer)
        {
            if (mReferencesOfReferenceInStream.Count > 0)
            {
                if (!EndianUtils.IsPlatform64Bit(Platform))
                {
                    const Int32 NULL = 0;
                    var currentOffset = new StreamOffset(writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty);
                        writer.Seek(o);
                        writer.Write(NULL);
                    }

                    writer.Seek(currentOffset);
                }
                else
                {
                    const Int64 NULL = 0;
                    var currentOffset = new StreamOffset(writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty);
                        writer.Seek(o);
                        writer.Write(NULL);
                    }

                    writer.Seek(currentOffset);
                }
            }
        }

        public bool Resolve(IBinaryStream writer)
        {
            if (mReferencesOfReferenceInStream.Count > 0)
            {
                if (mOffsetOfReferenceInStream == StreamOffset.Empty)
                {
                    // Encountered a StreamContext holding references that could not be resolved!
                    //
                    // Explanation:
                    //     This means that the data is writing references but not the actual
                    //     DataBlock or Marker of that reference.
                    //
                    return false;
                }

                if (!EndianUtils.IsPlatform64Bit(Platform))
                {
                    Int32 offsetToWrite = (Int32)mOffsetOfReferenceInStream.Offset;
                    var currentOffset = new StreamOffset(writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty);
                        writer.Seek(o);
                        writer.Write(offsetToWrite);
                    }

                    writer.Seek(currentOffset);
                }
                else // EArch.Arch64, 64-bit pointers
                {
                    Int64 offsetToWrite = (Int64)mOffsetOfReferenceInStream.Offset;
                    var currentOffset = new StreamOffset(writer.Position);
                    foreach (var o in mReferencesOfReferenceInStream)
                    {
                        Debug.Assert(o != StreamOffset.Empty);
                        writer.Seek(o);
                        writer.Write(offsetToWrite);
                    }
                    writer.Seek(currentOffset);
                }
            }
            return true;
        }
        #endregion
    }
}
