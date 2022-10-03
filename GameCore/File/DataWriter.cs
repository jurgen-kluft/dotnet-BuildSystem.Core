using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    #region IDataWriter

    public interface IDataWriter : IBinaryWriter
    {
        void Begin();

        /// <summary>
        /// BeginBlock is responsible for setting up a new DataBlock.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="alignment"></param>
        /// <returns>True if successful, False if the reference was already processed before!</returns>
        bool BeginBlock(StreamReference reference, Int64 alignment);
        Int64 Write(StreamReference v);
        void Mark(StreamReference reference);
        void EndBlock();

        void End();

        void Final(IBinaryWriter dataWriter, out Dictionary<StreamReference, StreamContext> referenceDatabase, out Dictionary<StreamReference, StreamContext> unresolvedReferences, out Dictionary<StreamReference, StreamContext> markerDatabase);
    }

    #endregion

    /// <summary>
    ///
    /// A DataWriter is used to write DataBlocks, DataBlocks are stored and when
    /// Finalized data with identical (Hash) are collapsed.
    /// All references (pointers to blocks) are also resolved at the final stage.
    ///
    /// </summary>
    public class DataWriter : IDataWriter
    {
        #region DataBlock

        internal class DataBlock
        {
            #region Fields

            private Hash160 mHash = Hash160.Empty;

            private readonly Int64 mAlignment = 4;
            private readonly MemoryStream mDataStream = new MemoryStream();
            private readonly IBinaryWriter mDataWriter;

            private readonly MemoryStream mTypeStream = new MemoryStream();
            private readonly IBinaryWriter mTypeWriter;


            private readonly Dictionary<StreamReference, StreamContext> mReferenceContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            private readonly Dictionary<StreamReference, StreamContext> mMarkerContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());

            private enum EDataType : byte
            {
                FLOAT = 0,
                DOUBLE = 0,
                SBYTE = 0,
                BYTE = 0,
                INT16 = 0,
                UINT16 = 0,
                INT32 = 0,
                UINT32 = 0,
                INT64 = 0,
                UINT64 = 0,
                REFERENCE = 1,
            }

            #endregion
            #region Constructor

            internal DataBlock(Int64 alignment, EEndian endian)
            {
                mAlignment = alignment;
                mDataWriter = EndianUtils.CreateBinaryWriter(mDataStream, endian);
                mTypeWriter = EndianUtils.CreateBinaryWriter(mTypeStream, endian);

                mReferenceContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
                mMarkerContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            }

            #endregion
            #region Properties

            internal Int64 Position
            {
                get
                {
                    return mDataStream.Position;
                }
                set
                {
                    mDataStream.Position = value;
                }
            }

            internal Int32 Length
            {
                get
                {
                    return (Int32)mDataStream.Length;
                }
            }

            #endregion
            #region Methods

            internal void Mark(StreamReference reference)
            {
                StreamContext ctx;
                if (!mMarkerContextDatabase.TryGetValue(reference, out ctx))
                {
                    ctx = new StreamContext();
                    ctx.Offset = new StreamOffset(Position);
                    mMarkerContextDatabase.Add(reference, ctx);
                }
                else
                {
                    Console.WriteLine("ERROR: xDataWriter.DataBlock.Mark is used twice with the same reference!");
                }
            }

            internal void Write(byte[] v)
            {
                for (int i = 0; i < v.Length; ++i)
                    mTypeWriter.Write((byte)EDataType.BYTE);

                mHash = Hash160.Empty;
                mDataWriter.Write(v);
            }

            internal void Write(byte[] v, int index, int count)
            {
                for (int i = index; i < count; i++)
                    mTypeWriter.Write((byte)EDataType.BYTE);

                Debug.Assert((index + count) <= v.Length);
                mDataWriter.Write(v, index, count);
            }

            internal void Write(float v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int32)));
                mTypeWriter.Write((int) EDataType.FLOAT);
                mDataWriter.Write(v);
            }

            internal void Write(double v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt64)));
                mTypeWriter.Write((UInt64) EDataType.DOUBLE);
                mDataWriter.Write(v);
            }

            internal void Write(SByte v)
            {
                mHash = Hash160.Empty;
                mTypeWriter.Write((byte) EDataType.SBYTE);
                mDataWriter.Write(v);
            }

            internal void Write(Int16 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int16)));
                mTypeWriter.Write((short) EDataType.INT16);
                mDataWriter.Write(v);
            }

            internal void Write(Int32 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int32)));
                mTypeWriter.Write((Int32) EDataType.INT32);
                mDataWriter.Write(v);
            }

            internal void Write(Int64 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt64)));
                mTypeWriter.Write((Int64) EDataType.INT64);
                mDataWriter.Write(v);
            }

            internal void Write(Byte v)
            {
                mHash = Hash160.Empty;
                mTypeWriter.Write((byte) EDataType.BYTE);
                mDataWriter.Write(v);
            }

            internal void Write(UInt16 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int16)));
                mTypeWriter.Write((UInt16) EDataType.UINT16);
                mDataWriter.Write(v);
            }

            internal void Write(UInt32 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int32)));
                mTypeWriter.Write((UInt32) EDataType.UINT32);
                mDataWriter.Write(v);
            }

            internal void Write(UInt64 v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt64)));
                mTypeWriter.Write((UInt64) EDataType.UINT64);
                mDataWriter.Write(v);
            }

            internal void Write(StreamReference v)
            {
                mHash = Hash160.Empty;
                Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int32)));

                if (mReferenceContextDatabase.ContainsKey(v))
                {
                    mReferenceContextDatabase[v].Add(new StreamOffset(mDataWriter.Position));
                }
                else
                {
                    StreamContext streamContext = new StreamContext();
                    streamContext.Add(new StreamOffset(mDataWriter.Position));
                    mReferenceContextDatabase.Add(v, streamContext);
                }
                mTypeWriter.Write((Int32) EDataType.REFERENCE);
                mDataWriter.Write((Int32) v.id);
            }

            internal bool Seek(StreamOffset pos)
            {
                return mTypeWriter.Seek(pos) && mDataWriter.Seek(pos);
            }

            internal Hash160 ComputeMD5()
            {
                if (mHash == Hash160.Empty)
                {
                    mHash = HashUtility.compute(mDataStream.GetBuffer(), (Int32) mDataStream.Length, mTypeStream.GetBuffer(), (Int32) mTypeStream.Length);
                }
                return mHash;
            }

            #endregion
            #region ReplaceReference and Finalize

            internal void GetWrittenStreamReferences(ICollection<StreamReference> references)
            {
                foreach (KeyValuePair<StreamReference, StreamContext> p in mReferenceContextDatabase)
                    references.Add(p.Key);
            }

            internal bool HoldsStreamReference(StreamReference r)
            {
                return mReferenceContextDatabase.ContainsKey(r);
            }

            internal void ReplaceReference(StreamReference oldRef, StreamReference newRef)
            {
                if (HoldsStreamReference(oldRef))
                {
                    // Data will be modified, so MD5 will be invalid.
                    mHash = Hash160.Empty;

                    StreamContext oldContext = mReferenceContextDatabase[oldRef];
                    mReferenceContextDatabase.Remove(oldRef);

                    // Modify the data by replacing the oldRef with the newRef
                    StreamOffset currentPos = new StreamOffset(mDataStream.Position);
                    for (int i = 0; i < oldContext.Count; i++)
                    {
                        mDataWriter.Seek(oldContext[i]);
                        mDataWriter.Write(newRef.id);
                    }
                    mDataWriter.Seek(currentPos);

                    // Update reference and offsets
                    if (HoldsStreamReference(newRef))
                    {
                        StreamContext newContext = mReferenceContextDatabase[newRef];
                        for (int i = 0; i < oldContext.Count; i++)
                            newContext.Add(oldContext[i]);
                    }
                    else
                    {
                        mReferenceContextDatabase.Add(newRef, oldContext);
                    }
                }
            }

            internal void Finalize(IBinaryWriter outData, StreamContext currentContext, IDictionary<StreamReference, StreamContext> referenceDataBase, IDictionary<StreamReference, StreamContext> markerDataBase)
            {
                StreamUtils.Align(outData, mAlignment);
                currentContext.Offset = new StreamOffset(outData.Position);

                // (The order of handling markers and references is not important)
                // Handle markers (addresses)
                {
                    foreach (KeyValuePair<StreamReference, StreamContext> k in mMarkerContextDatabase)
                    {
                        StreamContext context;
                        bool exists = referenceDataBase.TryGetValue(k.Key, out context);
                        if (!exists)
                            context = new StreamContext();

                        // Every marker needs to get the offset that it has in the final stream
                        Debug.Assert(context.Offset == StreamOffset.Empty);
                        context.Offset = new StreamOffset(currentContext.Offset + k.Value.Offset);

                        for (int i = 0; i < k.Value.Count; i++)
                        {
                            Debug.Assert(k.Value[i].value != -1);
                            context.Add(new StreamOffset(currentContext.Offset + k.Value[i]));
                        }

                        if (!exists)
                            referenceDataBase.Add(k.Key, context);
                        if (!markerDataBase.ContainsKey(k.Key))
                            markerDataBase.Add(k.Key, context);
                    }
                }

                // Handle references (pointers)
                {
                    foreach (KeyValuePair<StreamReference, StreamContext> k in mReferenceContextDatabase)
                    {
                        StreamContext context;
                        bool exists = referenceDataBase.TryGetValue(k.Key, out context);
                        if (!exists)
                            context = new StreamContext();

                        for (int i = 0; i < k.Value.Count; i++)
                        {
                            Debug.Assert(k.Value[i].value != -1);
                            context.Add(new StreamOffset(currentContext.Offset + k.Value[i]));
                        }

                        if (!exists)
                            referenceDataBase.Add(k.Key, context);
                    }
                }

                // Write data
                outData.Write(mDataStream.GetBuffer(), 0, (Int32) mDataStream.Length);
            }

            #endregion
        }

        #endregion

        #region Fields

        internal struct StreamReferenceDataBlockPair
        {
            private readonly StreamReference mStreamReference;
            private readonly DataBlock mDataBlock;

            internal StreamReferenceDataBlockPair(StreamReference r, DataBlock d)
            {
                mStreamReference = r;
                mDataBlock = d;
            }

            internal StreamReference Reference
            {
                get
                {
                    return mStreamReference;
                }
            }

            internal DataBlock DataBlock
            {
                get
                {
                    return mDataBlock;
                }
            }
        }

        private readonly EEndian mEndian;
        private readonly Dictionary<StreamReference, Int32> mStreamReferenceToDataBlockIdxDictionary = new Dictionary<StreamReference, Int32>(new StreamReference.Comparer());
        private readonly List<StreamReferenceDataBlockPair> mDataBlocks = new List<StreamReferenceDataBlockPair>();

        private readonly Stack<Int32> mStack = new Stack<Int32>();
        private readonly StreamReference mMainReference;

        private Int32 mCurrentDataBlockIdx;
        private DataBlock mCurrentDataBlock;

        #endregion
        #region Constructor

        public DataWriter(EEndian endian)
        {
            mMainReference = StreamReference.Instance;
            mEndian = endian;
        }

        #endregion
        #region Methods

        public void Begin()
        {
            const Int64 alignment = sizeof(Int32);
            StreamReference reference = mMainReference;
            BeginBlock(reference, alignment);
        }

        public void End()
        {
            EndBlock();
        }

        public bool BeginBlock(StreamReference reference, Int64 alignment)
        {
            if (mStreamReferenceToDataBlockIdxDictionary.ContainsKey(reference))
                return false;

            // --------------------------------------------------------------------------------------------------------
            mCurrentDataBlockIdx = mDataBlocks.Count;
            mStreamReferenceToDataBlockIdxDictionary.Add(reference, mCurrentDataBlockIdx);
            mStack.Push(mCurrentDataBlockIdx);
            mCurrentDataBlock = new DataBlock(alignment, mEndian);
            mDataBlocks.Add(new StreamReferenceDataBlockPair(reference, mCurrentDataBlock));
            // --------------------------------------------------------------------------------------------------------

            return true;
        }

        public void Mark(StreamReference reference)
        {
            mCurrentDataBlock.Mark(reference);
        }

        public Int64 Write(float v)
        {
            mCurrentDataBlock.Write(v);
            return 4;
        }
        public Int64 Write(double v)
        {
            mCurrentDataBlock.Write(v);
            return 8;
        }
        public Int64 Write(sbyte v)
        {
            mCurrentDataBlock.Write(v);
            return 1;
        }
        public Int64 Write(short v)
        {
            mCurrentDataBlock.Write(v);
            return 2;
        }
        public Int64 Write(int v)
        {
            mCurrentDataBlock.Write(v);
            return 4;
        }
        public Int64 Write(Int64 v)
        {
            mCurrentDataBlock.Write(v);
            return 8;
        }
        public Int64 Write(byte v)
        {
            mCurrentDataBlock.Write(v);
            return 1;
        }
        public Int64 Write(ushort v)
        {
            mCurrentDataBlock.Write(v);
            return 2;
        }
        public Int64 Write(uint v)
        {
            mCurrentDataBlock.Write(v);
            return 4;
        }
        public Int64 Write(UInt64 v)
        {
            mCurrentDataBlock.Write(v);
            return 8;
        }
        public Int64 Write(byte[] v)
        {
            mCurrentDataBlock.Write(v);
            return v.Length;
        }
        public Int64 Write(byte[] v, int index, int count)
        {
            Debug.Assert((index + count) <= v.Length);
            mCurrentDataBlock.Write(v, index, count);
            return count;
        }
        public Int64 Write(string v)
        {
            foreach (Char c in v)
                mCurrentDataBlock.Write((byte)c);
            mCurrentDataBlock.Write((byte)0);
            return v.Length + 1;
        }
        public Int64 Write(StreamReference v)
        {
            mCurrentDataBlock.Write(v);
            return 4;
        }

        public Int64 Position
        {
            get
            {
                return mCurrentDataBlock.Position;
            }
            set
            {
                mCurrentDataBlock.Position = value;
            }
        }

        public Int64 Length
        {
            get
            {
                return mCurrentDataBlock.Length;
            }
        }

        public bool Seek(StreamOffset offset)
        {
            return mCurrentDataBlock.Seek(offset);
        }

        public void EndBlock()
        {
            // Obtain the information of the previous block (Stack.Pop)
            mCurrentDataBlockIdx = mStack.Pop();
            mCurrentDataBlock = mDataBlocks[mCurrentDataBlockIdx].DataBlock;
        }

        public void Close()
        {

        }

        #endregion
        #region Finalize

        public void Final(IBinaryWriter dataWriter, out Dictionary<StreamReference, StreamContext> referenceDatabase, out Dictionary<StreamReference, StreamContext> unresolvedReferences, out Dictionary<StreamReference, StreamContext> markerDatabase)
        {
            referenceDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            markerDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            unresolvedReferences = new Dictionary<StreamReference, StreamContext>();

            try
            {
                Dictionary<StreamReference, DataBlock> finalDataBlockDatabase = new Dictionary<StreamReference, DataBlock>(new StreamReference.Comparer());
                foreach (StreamReferenceDataBlockPair d in mDataBlocks)
                    finalDataBlockDatabase.Add(d.Reference, d.DataBlock);

                // Build dictionary collecting the DataBlocks for every StreamReference so that we do
                // not have to iterate over the whole list when replacing references.
                Dictionary<StreamReference, List<DataBlock>> streamReferenceInWhichDataBlocks = new Dictionary<StreamReference, List<DataBlock>>(new StreamReference.Comparer());
                foreach (StreamReferenceDataBlockPair d in mDataBlocks)
                    streamReferenceInWhichDataBlocks.Add(d.Reference, new List<DataBlock>());
                List<StreamReference> streamReferencesWrittenInDataBlock = new List<StreamReference>();
                foreach (StreamReferenceDataBlockPair d in mDataBlocks)
                {
                    streamReferencesWrittenInDataBlock.Clear();
                    d.DataBlock.GetWrittenStreamReferences(streamReferencesWrittenInDataBlock);
                    foreach (StreamReference s in streamReferencesWrittenInDataBlock)
                    {
                        List<DataBlock> dataBlocks;
                        if (streamReferenceInWhichDataBlocks.TryGetValue(s, out dataBlocks))
                            dataBlocks.Add(d.DataBlock);
                    }
                }

                // Note on further optimization:
                //    We can speed up the algorithm below by creating Buckets of different DataBlock size ranges.
                //    This will localize the search and reduce the hit count on the ContainsKey() function.

                // For all blocks, calculate an MD5
                // Collapse identical blocks, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the MD5 of a block due
                // to the fact that references get replaced and this in turn can suddenly
                // mean that some blocks are identical.
                Dictionary<Hash160, StreamReference> md5ToDataBlockDatabase = new Dictionary<Hash160, StreamReference>(new Hash160.Comparer());
                Dictionary<StreamReference, List<StreamReference>> duplicateDatabase = new Dictionary<StreamReference, List<StreamReference>>(new StreamReference.Comparer());

                bool collapse = true;
                while (collapse)
                {
                    duplicateDatabase.Clear();
                    md5ToDataBlockDatabase.Clear();

                    foreach (KeyValuePair<StreamReference, DataBlock> p in finalDataBlockDatabase)
                    {
                        DataBlock block = p.Value;
                        Hash160 md5 = block.ComputeMD5();
                        if (md5ToDataBlockDatabase.ContainsKey(md5))
                        {
                            // Encountering a block of data which has a duplicate.
                            // After the first iteration it might be the case that
                            // they have the same 'Reference' since they are collapsed.
                            StreamReference newRef = md5ToDataBlockDatabase[md5];
                            if (p.Key != newRef)
                            {
                                if (!duplicateDatabase.ContainsKey(newRef))
                                {
                                    List<StreamReference> duplicateReferences = new List<StreamReference>();
                                    duplicateReferences.Add(p.Key);
                                    duplicateDatabase.Add(newRef, duplicateReferences);
                                }
                                else
                                {
                                    duplicateDatabase[newRef].Add(p.Key);
                                }
                            }
                        }
                        else
                        {
                            // This block of data is unique
                            md5ToDataBlockDatabase.Add(md5, p.Key);
                        }
                    }

                    // Remove the old references from the finalDatabase
                    foreach (KeyValuePair<StreamReference, List<StreamReference>> p in duplicateDatabase)
                    {
                        foreach (StreamReference oldRef in p.Value)
                            finalDataBlockDatabase.Remove(oldRef);
                    }

                    // Did we find any duplicates, if so then we also replaced references and by doing so MD5 hashes have changed.
                    // Some blocks now might have an identical MD5 due to this, so we need to iterate again.
                    foreach (KeyValuePair<StreamReference, List<StreamReference>> p in duplicateDatabase)
                    {
                        StreamReference newRef = p.Key;

                        foreach (StreamReference oldRef in p.Value)
                        {
                            List<DataBlock> oldRefDataBlocks;
                            if (streamReferenceInWhichDataBlocks.TryGetValue(oldRef, out oldRefDataBlocks))
                            {
                                List<DataBlock> newRefDataBlocks;
                                streamReferenceInWhichDataBlocks.TryGetValue(newRef, out newRefDataBlocks);

                                foreach (DataBlock block in oldRefDataBlocks)
                                {
                                    // Is this new StreamReference already associated with this DataBlock?
                                    // If not than we have to remember that this new StreamReference now
                                    // also is referenced in this DataBlock after the ReplaceReference.
                                    if (!block.HoldsStreamReference(newRef))
                                        newRefDataBlocks.Add(block);

                                    block.ReplaceReference(oldRef, newRef);
                                }
                            }
                        }

                        foreach (StreamReference s in p.Value)
                            streamReferenceInWhichDataBlocks.Remove(s);
                    }
                    collapse = duplicateDatabase.Count > 0;
                }

                // Finalize the DataBlocks and resolve all references
                foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataBlockDatabase)
                {
                    StreamContext currentContext;
                    if (!referenceDatabase.TryGetValue(k.Key, out currentContext))
                    {
                        currentContext = new StreamContext();
                        referenceDatabase.Add(k.Key, currentContext);
                    }

                    k.Value.Finalize(dataWriter, currentContext, referenceDatabase, markerDatabase);
                }

                // Write all the 'pointer values (relocatable)' 'references'
                foreach (KeyValuePair<StreamReference, StreamContext> k in referenceDatabase)
                {
                    // TODO: Pointers (offsets), are forced to 32 bit
                    if (k.Key == StreamReference.Empty)
                    {
                        k.Value.ResolveToNull32(dataWriter);
                    }
                    else
                    {
                        if (!k.Value.Resolve32(dataWriter))
                            unresolvedReferences.Add(k.Key, k.Value);
                    }
                }

            }
            catch(Exception e)
            {
                referenceDatabase.Clear();
                unresolvedReferences.Clear();

                Console.WriteLine("[DataWrite:EXCEPTION] {0}", e.Message);
            }
        }

        #endregion
    }
}
