using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Core
{
    /// <summary>
    /// A StreamContext contains a list of offsets of where a 'reference' was written to a block of data.
    /// It also holds the 'Offset' to this block of data. When everything is written we can resolve all
    /// 'references' by iterating over the list of offsets and writing the 'Offset' to the block of data
    /// at that offset in the stream.
    /// </summary>
    public class StreamContext
    {
        #region Fields

        private StreamOffset mOffsetOfReferenceInStream = StreamOffset.Empty;
        private readonly List<StreamOffset> mReferencesOfReferenceInStream = new List<StreamOffset>();

        #endregion
        #region Properties

        public StreamOffset Offset
        {
            get
            {
                return mOffsetOfReferenceInStream;
            }
            set
            {
                mOffsetOfReferenceInStream = value;
            }
        }

        public int Count
        {
            get
            {
                return mReferencesOfReferenceInStream.Count;
            }
        }

        public StreamOffset this[int index]
        {
            get
            {
                return mReferencesOfReferenceInStream[index];
            }
        }

        #endregion
        #region Methods

        public void Add(StreamOffset offset)
        {
            mReferencesOfReferenceInStream.Add(offset);
        }

        public void ResolveToNull32(IBinaryWriter writer)
        {
            if (mReferencesOfReferenceInStream.Count > 0)
            {
                const Int32 NULL32 = 0;

                StreamOffset currentOffset = new StreamOffset(writer.Position);
                foreach (StreamOffset o in mReferencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.Empty);
                    writer.Seek(o);
                    writer.Write(NULL32);
                }
                writer.Seek(currentOffset);
            }
        }

        public bool Resolve32(IBinaryWriter writer)
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

                Int32 offsetToWrite = (Int32)mOffsetOfReferenceInStream.value;

                StreamOffset currentOffset = new StreamOffset(writer.Position);
                foreach (StreamOffset o in mReferencesOfReferenceInStream)
                {
                    Debug.Assert(o != StreamOffset.Empty);
                    writer.Seek(o);
                    writer.Write(offsetToWrite);
                }
                writer.Seek(currentOffset);
            }
            return true;
        }
        #endregion
    }
}
