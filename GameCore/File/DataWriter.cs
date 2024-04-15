using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    #region IDataWriter

    public interface IDataWriter : IBinaryStreamWriter
    {
        void Begin();

        /// <summary>
        /// BeginBlock is responsible for setting up a new DataBlock.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="alignment"></param>
        /// <returns>True if successful, False if the reference was already processed before!</returns>
        bool BeginBlock(StreamReference reference, long alignment);
        void Write(StreamReference v);
        void Mark(StreamReference reference);
        void EndBlock();

        void End();

        void Final(IBinaryStreamWriter dataWriter, out Dictionary<StreamReference, StreamContext> referenceDatabase, out Dictionary<StreamReference, StreamContext> unresolvedReferences, out Dictionary<StreamReference, StreamContext> markerDatabase);
    }

    #endregion

    /// <summary>
    ///
    /// A DataWriter is used to write DataBlocks, DataBlocks are stored and when
    /// Finalized data with identical (Hash) are collapsed.
    /// All references (pointers to blocks) are also resolved at the final stage.
    ///
    /// </summary>
    public sealed class DataWriter : IDataWriter
    {
        #region DataBlock

        private class DataBlock
        {
            #region Fields

            private readonly long _alignment = 4;
            private readonly MemoryStream _dataStream = new MemoryStream();
            private readonly IBinaryStreamWriter _dataWriter;

            private readonly MemoryStream _typeStream = new MemoryStream();
            private readonly IBinaryStreamWriter _typeWriter;

            private readonly Dictionary<StreamReference, StreamContext> _referenceContextDatabase;
            private readonly Dictionary<StreamReference, StreamContext> _markerContextDatabase;

            private enum EDataType : byte
            {
                Float = 0,
                Double = 1,
                Sbyte = 2,
                Byte = 3,
                Int16 = 4,
                Uint16 = 5,
                Int32 = 6,
                Uint32 = 7,
                Int64 = 8,
                Uint64 = 9,
                Reference = 15,
            }

            #endregion
            #region Constructor

            internal DataBlock(long alignment, EPlatform platform)
            {
                Platform = platform;
                _alignment = alignment;
                _dataWriter = EndianUtils.CreateBinaryWriter(_dataStream, platform);
                _typeWriter = EndianUtils.CreateBinaryWriter(_typeStream, platform);

                _referenceContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
                _markerContextDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            }

            #endregion
            #region Properties

            internal EPlatform Platform { get; set; }

            internal long Position
            {
                get => _dataStream.Position;
                set => _dataStream.Position = value;
            }

            internal int Length => (int)_dataStream.Length;

            #endregion
            #region Methods

            internal void Mark(StreamReference reference)
            {
                if (!_markerContextDatabase.TryGetValue(reference, out var ctx))
                {
                    ctx = new StreamContext(Platform)
                    {
                        Offset = (Position)
                    };
                    _markerContextDatabase.Add(reference, ctx);
                }
                else
                {
                    Console.WriteLine("ERROR: xDataWriter.DataBlock.Mark is used twice with the same reference!");
                }
            }

            internal void Write(byte[] v)
            {
                for (var i = 0; i < v.Length; ++i)
                    _typeWriter.Write((byte)EDataType.Byte);
                _dataWriter.Write(v, 0, v.Length);
            }

            internal void Write(byte[] v, int index, int count)
            {
                Debug.Assert((index + count) <= v.Length);
                for (var i = index; i < count; i++)
                    _typeWriter.Write((byte)EDataType.Byte);
                _dataWriter.Write(v, index, count);
            }

            internal void Write(float v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(int)));
                _typeWriter.Write((int) EDataType.Float);
                _dataWriter.Write(v);
            }

            internal void Write(double v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(ulong)));
                _typeWriter.Write((ulong) EDataType.Double);
                _dataWriter.Write(v);
            }

            internal void Write(sbyte v)
            {
                _typeWriter.Write((byte) EDataType.Sbyte);
                _dataWriter.Write(v);
            }

            internal void Write(short v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(short)));
                _typeWriter.Write((short) EDataType.Int16);
                _dataWriter.Write(v);
            }

            internal void Write(int v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(int)));
                _typeWriter.Write((int) EDataType.Int32);
                _dataWriter.Write(v);
            }

            internal void Write(long v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(ulong)));
                _typeWriter.Write((long) EDataType.Int64);
                _dataWriter.Write(v);
            }

            internal void Write(byte v)
            {
                _typeWriter.Write((byte) EDataType.Byte);
                _dataWriter.Write(v);
            }

            internal void Write(ushort v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(short)));
                _typeWriter.Write((ushort) EDataType.Uint16);
                _dataWriter.Write(v);
            }

            internal void Write(uint v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(int)));
                _typeWriter.Write((uint) EDataType.Uint32);
                _dataWriter.Write(v);
            }

            internal void Write(ulong v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(ulong)));
                _typeWriter.Write((ulong) EDataType.Uint64);
                _dataWriter.Write(v);
            }

            internal void Write(StreamReference v)
            {
                Debug.Assert(StreamUtils.Aligned(_dataWriter, sizeof(int)));

                if (_referenceContextDatabase.TryGetValue(v, out var value))
                {
                    value.Add(_dataWriter.Position);
                }
                else
                {
                    var streamContext = new StreamContext(Platform);
                    streamContext.Add(_dataWriter.Position);
                    _referenceContextDatabase.Add(v, streamContext);
                }
                _typeWriter.Write((int) EDataType.Reference);
                _dataWriter.Write((int) v.Id);
            }

            internal long Seek(long pos)
            {
                _typeWriter.Seek(pos);
                return _dataWriter.Seek(pos);
            }

            internal Hash160 ComputeHash()
            {
                return HashUtility.Compute(_dataStream.GetBuffer(), (int) _dataStream.Length, _typeStream.GetBuffer(), (int) _typeStream.Length);
            }

            #endregion
            #region ReplaceReference and Finalize

            internal void GetWrittenStreamReferences(ICollection<StreamReference> references)
            {
                foreach (var p in _referenceContextDatabase)
                    references.Add(p.Key);
            }

            internal bool HoldsStreamReference(StreamReference r)
            {
                return _referenceContextDatabase.ContainsKey(r);
            }

            internal void ReplaceReference(StreamReference oldRef, StreamReference newRef)
            {
                if (HoldsStreamReference(oldRef))
                {
                    var oldContext = _referenceContextDatabase[oldRef];
                    _referenceContextDatabase.Remove(oldRef);

                    // Modify the data by replacing the oldRef with the newRef
                    var currentPos = _dataStream.Position;
                    for (var i = 0; i < oldContext.Count; i++)
                    {
                        _dataWriter.Seek(oldContext[i]);
                        _dataWriter.Write(newRef.Id);
                    }
                    _dataWriter.Seek(currentPos);

                    // Update reference and offsets
                    if (HoldsStreamReference(newRef))
                    {
                        var newContext = _referenceContextDatabase[newRef];
                        for (var i = 0; i < oldContext.Count; i++)
                            newContext.Add(oldContext[i]);
                    }
                    else
                    {
                        _referenceContextDatabase.Add(newRef, oldContext);
                    }
                }
            }

            internal void Finalize(IBinaryStreamWriter outData, StreamContext currentContext, IDictionary<StreamReference, StreamContext> referenceDataBase, IDictionary<StreamReference, StreamContext> markerDataBase)
            {
                StreamUtils.Align(outData, _alignment);
                currentContext.Offset = outData.Position;

                // (The order of handling markers and references is not important)
                // Handle markers (addresses)
                {
                    foreach (var k in _markerContextDatabase)
                    {
                        var exists = referenceDataBase.TryGetValue(k.Key, out var context);
                        if (!exists)
                            context = new StreamContext(Platform);

                        // Every marker needs to get the offset that it has in the final stream
                        Debug.Assert(context.Offset == StreamOffset.Empty.Offset);
                        context.Offset = currentContext.Offset + k.Value.Offset;

                        for (var i = 0; i < k.Value.Count; i++)
                        {
                            Debug.Assert(k.Value[i] != -1);
                            context.Add(currentContext.Offset + k.Value[i]);
                        }

                        if (!exists)
                            referenceDataBase.Add(k.Key, context);
                        markerDataBase.TryAdd(k.Key, context);
                    }
                }

                // Handle references (pointers)
                {
                    foreach (var k in _referenceContextDatabase)
                    {
                        var exists = referenceDataBase.TryGetValue(k.Key, out var context);
                        if (!exists)
                            context = new StreamContext(Platform);

                        for (var i = 0; i < k.Value.Count; i++)
                        {
                            Debug.Assert(k.Value[i] != -1);
                            context.Add(currentContext.Offset + k.Value[i]);
                        }

                        if (!exists)
                            referenceDataBase.Add(k.Key, context);
                    }
                }

                // Write data
                outData.Write(_dataStream.GetBuffer(), 0, (int) _dataStream.Length);
            }

            #endregion
        }

        #endregion

        #region Fields

        readonly struct StreamReferenceDataBlockPair
        {
            internal StreamReferenceDataBlockPair(StreamReference r, DataBlock d)
            {
                Reference = r;
                DataBlock = d;
            }

            internal StreamReference Reference { get; }
            internal DataBlock DataBlock { get; }
        }

        private readonly Dictionary<StreamReference, int> _streamReferenceToDataBlockIdxDictionary = new (new StreamReference.Comparer());
        private readonly List<StreamReferenceDataBlockPair> _dataBlocks = new ();

        private readonly Stack<int> _stack = new();
        private readonly StreamReference _mainReference;

        private int _currentDataBlockIdx;
        private DataBlock _currentDataBlock;

        private EPlatform Platform { get; set; }

        #endregion
        #region Constructor

        public DataWriter(EPlatform platform)
        {
            _mainReference = StreamReference.NewReference;
            Platform = platform;
        }

        #endregion
        #region Methods

        public void Begin()
        {
            const long alignment = sizeof(int);
            var reference = _mainReference;
            BeginBlock(reference, alignment);
        }

        public void End()
        {
            EndBlock();
        }

        public bool BeginBlock(StreamReference reference, long alignment)
        {
            if (_streamReferenceToDataBlockIdxDictionary.ContainsKey(reference))
                return false;

            // --------------------------------------------------------------------------------------------------------
            _currentDataBlockIdx = _dataBlocks.Count;
            _streamReferenceToDataBlockIdxDictionary.Add(reference, _currentDataBlockIdx);
            _stack.Push(_currentDataBlockIdx);
            _currentDataBlock = new DataBlock(alignment, Platform);
            _dataBlocks.Add(new StreamReferenceDataBlockPair(reference, _currentDataBlock));
            // --------------------------------------------------------------------------------------------------------

            return true;
        }

        public void Mark(StreamReference reference)
        {
            _currentDataBlock.Mark(reference);
        }

        public void Write(float v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(double v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(sbyte v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(short v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(int v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(long v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(byte v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(ushort v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(uint v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(ulong v)
        {
            _currentDataBlock.Write(v);
        }
        public void Write(byte[] v)
        {
            _currentDataBlock.Write(v);
        }

        public void Write(string v)
        {
            foreach (var c in v)
                _currentDataBlock.Write((byte)c);
            _currentDataBlock.Write((byte)0);
        }
        public void Write(StreamReference v)
        {
            _currentDataBlock.Write(v);
        }

        public void Write(byte[] data, int offset, int count)
        {
            _currentDataBlock.Write(data, offset, count);
        }

        public long Position
        {
            get => _currentDataBlock.Position;
            set => _currentDataBlock.Position = value;
        }

        public long Length
        {
            get => _currentDataBlock.Length;
            set
            {

            }
        }

        public long Seek(long offset)
        {
            return _currentDataBlock.Seek(offset);
        }

        public void EndBlock()
        {
            // Obtain the information of the previous block (Stack.Pop)
            _currentDataBlockIdx = _stack.Pop();
            _currentDataBlock = _dataBlocks[_currentDataBlockIdx].DataBlock;
        }

        public void Close()
        {

        }

        #endregion
        #region Finalize

        public void Final(IBinaryStreamWriter dataWriter, out Dictionary<StreamReference, StreamContext> referenceDatabase, out Dictionary<StreamReference, StreamContext> unresolvedReferences, out Dictionary<StreamReference, StreamContext> markerDatabase)
        {
            referenceDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            markerDatabase = new Dictionary<StreamReference, StreamContext>(new StreamReference.Comparer());
            unresolvedReferences = new Dictionary<StreamReference, StreamContext>();

            try
            {
                var finalDataBlockDatabase = new Dictionary<StreamReference, DataBlock>(new StreamReference.Comparer());
                foreach (var d in _dataBlocks)
                    finalDataBlockDatabase.Add(d.Reference, d.DataBlock);

                // Build dictionary collecting the DataBlocks for every StreamReference so that we do
                // not have to iterate over the whole list when replacing references.
                var streamReferenceInWhichDataBlocks = new Dictionary<StreamReference, List<DataBlock>>(new StreamReference.Comparer());
                foreach (var d in _dataBlocks)
                    streamReferenceInWhichDataBlocks.Add(d.Reference, new List<DataBlock>());
                var streamReferencesWrittenInDataBlock = new List<StreamReference>();
                foreach (var d in _dataBlocks)
                {
                    streamReferencesWrittenInDataBlock.Clear();
                    d.DataBlock.GetWrittenStreamReferences(streamReferencesWrittenInDataBlock);
                    foreach (var s in streamReferencesWrittenInDataBlock)
                    {
                        if (streamReferenceInWhichDataBlocks.TryGetValue(s, out var dataBlocks))
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
                Dictionary<Hash160, StreamReference> hashToDataBlockDatabase = new (new Hash160.Comparer());
                Dictionary<StreamReference, List<StreamReference>> duplicateDatabase = new (new StreamReference.Comparer());

                var collapse = true;
                while (collapse)
                {
                    duplicateDatabase.Clear();
                    hashToDataBlockDatabase.Clear();

                    foreach (var p in finalDataBlockDatabase)
                    {
                        var block = p.Value;
                        var hash = block.ComputeHash();
                        if (hashToDataBlockDatabase.TryGetValue(hash, out var newRef))
                        {
                            // Encountering a block of data which has a duplicate.
                            // After the first iteration it might be the case that
                            // they have the same 'Reference' since they are collapsed.
                            if (p.Key != newRef)
                            {
                                if (!duplicateDatabase.ContainsKey(newRef))
                                {
                                    List<StreamReference> duplicateReferences = new () { p.Key };
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
                            hashToDataBlockDatabase.Add(hash, p.Key);
                        }
                    }

                    // Remove the old references from the finalDatabase
                    foreach (var p in duplicateDatabase)
                    {
                        foreach (var oldRef in p.Value)
                            finalDataBlockDatabase.Remove(oldRef);
                    }

                    // Did we find any duplicates, if so then we also replaced references and by doing so MD5 hashes have changed.
                    // Some blocks now might have an identical MD5 due to this, so we need to iterate again.
                    foreach (var (newRef, value) in duplicateDatabase)
                    {
                        foreach (var oldRef in value)
                        {
                            if (streamReferenceInWhichDataBlocks.TryGetValue(oldRef, out var oldRefDataBlocks))
                            {
                                streamReferenceInWhichDataBlocks.TryGetValue(newRef, out var newRefDataBlocks);

                                foreach (var block in oldRefDataBlocks)
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

                        foreach (var s in value)
                            streamReferenceInWhichDataBlocks.Remove(s);
                    }
                    collapse = duplicateDatabase.Count > 0;
                }

                // Finalize the DataBlocks and resolve all references
                foreach (var (vRef, vDb) in finalDataBlockDatabase)
                {
                    if (!referenceDatabase.TryGetValue(vRef, out var currentContext))
                    {
                        currentContext = new StreamContext(Platform);
                        referenceDatabase.Add(vRef, currentContext);
                    }

                    vDb.Finalize(dataWriter, currentContext, referenceDatabase, markerDatabase);
                }

                // Write all the 'pointer values (relocatable)' 'references'
                foreach (var (vRef, vDb) in referenceDatabase)
                {
                    // TODO: Pointers (offsets), are forced to 32 bit
                    if (vRef == StreamReference.Empty)
                    {
                        vDb.ResolveToNull(dataWriter);
                    }
                    else
                    {
                        if (!vDb.Resolve(dataWriter))
                            unresolvedReferences.Add(vRef, vDb);
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
