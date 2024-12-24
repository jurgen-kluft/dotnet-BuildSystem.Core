using System.Diagnostics;
using System.Text;
using GameCore;
using BinaryWriter = GameCore.BinaryWriter;

namespace GameData
{
    namespace MetaCode
    {
        public class CppDataStream2 : IDataBlockWriter, IGameDataWriter
        {
            private int _current;
            private readonly List<Hash160> _dataUnitHashes;
            private readonly Dictionary<Hash160, int> _hashToDataUnitIndex;
            private readonly Stack<int> _dataUnitStack;
            private readonly List<DataBlock> _dataBlocks;
            private readonly Dictionary<StreamReference, int> _referenceToDataBlock;
            private readonly IReadOnlySignatureDataBase _signatureDb;
            private readonly MemoryStream _memoryStream;
            private readonly IStreamWriter _dataWriter;

            public CppDataStream2(EPlatform platform, IReadOnlySignatureDataBase signatureDb)
            {
                _current = -1;
                _dataUnitHashes = new List<Hash160>();
                _hashToDataUnitIndex = new Dictionary<Hash160, int>();
                _dataUnitStack = new Stack<int>();
                _dataBlocks = new List<DataBlock>();
                _referenceToDataBlock = new Dictionary<StreamReference, int>();
                _signatureDb = signatureDb;
                _memoryStream = new MemoryStream();
                _dataWriter = ArchitectureUtils.CreateMemoryWriter(_memoryStream, platform);
            }

            private class DataBlock
            {
                private Dictionary<StreamReference, List<int>> BlockPointers { get; } = new();

                public int DataUnitIndex{ get; set; }
                public int Alignment { get; init; }
                public int Offset { get; set; }
                public int Size { get; set; }
                public StreamReference Reference { get; init; }

                public static void End(DataBlock db, IWriter data)
                {
                    var gap = CMath.AlignUp(db.Size, db.Alignment) - db.Size;
                    // Write actual data to reach the size alignment requirement
                    const byte zero = 0;
                    for (var i = 0; i < gap; ++i)
                        BinaryWriter.Write(data, zero);
                }

                public Hash160 ComputeHash(DataStream dataStream, byte[] workBuffer)
                {
                    //      Include the stream pointers in the hash computation otherwise we might
                    //      collapse two blocks that are not identical.
                    //
                    HashUtility.Begin();
                    HashUtility.Update(dataStream.Data.Slice(Offset, Size));

                    if (BlockPointers.Count > 0)
                    {
                        var offset = 0;

                        // This is a magic 64-bit value that is unique but stable for all DataBlocks
                        const ulong magic = 0xDEADBEEFCAFEBABE;
                        ArchitectureUtils.LittleArchitecture64.Write(magic, workBuffer, offset);
                        offset += 8;

                        foreach ((StreamReference streamReference, List<int> pointers) in BlockPointers)
                        {
                            ArchitectureUtils.LittleArchitecture64.Write(streamReference.Id, workBuffer, offset);
                            offset += 4;

                            foreach (var pointer in pointers)
                            {
                                ArchitectureUtils.LittleArchitecture64.Write(pointer, workBuffer, offset);
                                offset += 8;
                            }
                        }

                        HashUtility.Update(workBuffer, 0, offset);
                    }

                    return HashUtility.End();
                }

                internal static void Write(IWriter writer, byte[] data, int index, int count)
                {
                    writer.Write(data, index, count);
                }

                internal static void WriteDataBlockReference(IStreamWriter writer, DataBlock db, StreamReference v)
                {
                    BinaryWriter.Align(writer, sizeof(ulong));
                    if (db.BlockPointers.TryGetValue(v, out var pointers))
                    {
                        pointers.Add((int)(writer.Position - db.Offset));
                    }
                    else
                    {
                        pointers = new List<int>() { (int)(writer.Position - db.Offset) };
                        db.BlockPointers.Add(v, pointers);
                    }

                    BinaryWriter.Write(writer, 0xFEEDF00DFEEDF00D);
                }

                internal static void ReplaceReference(DataBlock db, StreamReference oldRef, StreamReference newRef)
                {
                    // See if we are using this reference (oldRef) in this data block
                    if (!db.BlockPointers.Remove(oldRef, out var oldOffsets)) return;

                    // Update pointer and offsets
                    // It could be that we also are using newRef in this data block
                    if (db.BlockPointers.TryGetValue(newRef, out var newOffsets))
                    {
                        foreach (var o in oldOffsets)
                            newOffsets.Add(o);
                    }
                    else
                    {
                        db.BlockPointers.Add(newRef, oldOffsets);
                    }
                }

                internal static void CollectStreamPointers(DataBlock db, ICollection<StreamPointer> streamPointers, IDictionary<StreamReference, int> dataPositionDataBase)
                {
                    // BlockPointers are references that are written in this Block, each Reference might
                    // have been written more than once so that is why there is a list of Offsets per
                    // StreamReference.
                    dataPositionDataBase.TryGetValue(db.Reference, out var dataBlockPosition);
                    foreach (var (sr, offsets) in db.BlockPointers)
                    {
                        // What is the offset of the data block we are pointing to
                        var exists = dataPositionDataBase.TryGetValue(sr, out var referencePosition);
                        Debug.Assert(exists);

                        // The offset are relative to the start of the DataBlock
                        foreach (var o in offsets)
                        {
                            // Remember this pointer
                            var sp = new StreamPointer() { PositionInSourceStream = db.Offset + o, PositionInDestinationStream = dataBlockPosition + o, TargetPosition = referencePosition };
                            streamPointers.Add(sp);
                        }
                    }
                }

                internal static void WriteStreamPointers(IDataStream data, List<StreamPointer> streamPointers)
                {
                    if (streamPointers.Count == 0)
                        return;

                    for (var i = 0; i < streamPointers.Count - 1; i++)
                    {
                        streamPointers[i].Write(data, streamPointers[i + 1]);
                    }

                    // Last StreamPointer is written by giving it itself, so that the 'next' pointer is 0
                    streamPointers[^1].Write(data, streamPointers[^1]);
                }

                internal static void WriteDataBlock(long dataUnitStartPos, DataBlock db, IStreamReader data, IStreamWriter outData, IDictionary<StreamReference, int> dataOffsetDataBase, byte[] readWriteBuffer)
                {
                    // Verify the position of the data stream
                    dataOffsetDataBase.TryGetValue(db.Reference, out var outDataBlockOffset);
                    BinaryWriter.Align(outData, db.Alignment);
                    Debug.Assert((int)(outData.Position-dataUnitStartPos) == outDataBlockOffset);

                    // Read from the data stream at the start of the block and write the block to the output
                    data.Seek(db.Offset);

                    // Write the data of this block to 'outData'
                    var sizeToWrite = db.Size;
                    while (sizeToWrite > 0)
                    {
                        var chunkRead = Math.Min(sizeToWrite, readWriteBuffer.Length);
                        var actualRead = data.Read(readWriteBuffer, 0, chunkRead);
                        Debug.Assert(actualRead > 0);
                        outData.Write(readWriteBuffer, 0, actualRead);
                        sizeToWrite -= actualRead;
                    }
                }
            }

            public int GetDataUnitIndex(Hash160 hash)
            {
                if (_hashToDataUnitIndex.TryGetValue(hash, out var index))
                    return index;

                index = _hashToDataUnitIndex.Count;
                _hashToDataUnitIndex.Add(hash, index);
                _dataUnitHashes.Add(hash);
                return index;
            }

            public void OpenDataUnit(int index)
            {
                _dataUnitStack.Push(index);
            }

            public void CloseDataUnit()
            {
                Debug.Assert(_dataUnitStack.Count > 0);
                _dataUnitStack.Pop();
            }

            public void NewBlock(StreamReference reference, int alignment)
            {
                _referenceToDataBlock.Add(reference, _dataBlocks.Count);
                _dataBlocks.Add( new DataBlock()
                {
                    DataUnitIndex = -1,
                    Alignment = alignment,
                    Offset = -1,
                    Size = -1,
                    Reference = reference
                });
            }

            public void OpenBlock(StreamReference r)
            {
                // A DataBlock must have been registered (see NewBlock) before we can open it
                var exists = _referenceToDataBlock.TryGetValue(r, out var index);
                Debug.Assert(exists);

                // We do not allow nested blocks!!
                Debug.Assert(_current == -1);
                _current = index;

                Debug.Assert(_dataBlocks[index].DataUnitIndex == -1);
                Debug.Assert(_dataBlocks[index].Offset == -1);
                Debug.Assert(_dataBlocks[index].Size == -1);

                // Set the stream position to the start of the block
                _dataBlocks[index].Offset = (int)_dataWriter.Position;

                // Remember the DataUnitIndex for this block
                _dataBlocks[index].DataUnitIndex = _dataUnitStack.Peek();
            }

            public void CloseBlock()
            {
                Debug.Assert(_current >= 0);
                _dataBlocks[_current].Size = (int)(_dataWriter.Position - _dataBlocks[_current].Offset);

                DataBlock.End(_dataBlocks[_current], _dataWriter);

                _current = -1;
            }

            public void Write(byte[] data, int index, int count)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, data, index, count);
            }

            public void WriteFileId(Hash160 signature)
            {
                (uint bigfileIndex, uint fileIndex) = _signatureDb.GetEntry(signature);
                BinaryWriter.Write(_dataWriter, bigfileIndex);
                BinaryWriter.Write(_dataWriter, fileIndex);
            }

            public void WriteDataBlockReference(StreamReference v)
            {
                Debug.Assert(_current != -1);
                DataBlock.WriteDataBlockReference(_dataWriter, _dataBlocks[_current], v);
            }

            private static void DeduplicateDataBlocks(DataStream memoryBlock, Dictionary<StreamReference, DataBlock> blockDataBase)
            {
                // For all blocks that are part of this chunk:
                // Collapse identical blocks identified by hash, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the hash of a data block.

                var duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                var dataHashDataBase = new Dictionary<Hash160, StreamReference>();

                var dataBlocks = new List<DataBlock>(blockDataBase.Count);
                foreach ((var _, DataBlock db) in blockDataBase)
                    dataBlocks.Add(db);

                var workBuffer = new byte[65536];

                while (true)
                {
                    duplicateDataBase.Clear();
                    dataHashDataBase.Clear();

                    foreach (var d in dataBlocks)
                    {
                        // TODO this can lead to incorrect matches, we should ask the DataBlock to compute the hash
                        //      since it can consider the stream pointers that are in the block

                        var hash = d.ComputeHash(memoryBlock, workBuffer);

                        if (dataHashDataBase.TryGetValue(hash, out var newRef))
                        {
                            // Encountering a block of data which has a duplicate.
                            // After the first iteration it might be the case that
                            // they have the same 'Reference' since they are collapsed.
                            if (!d.Reference.IsEqual(newRef))
                            {
                                if (!duplicateDataBase.ContainsKey(newRef))
                                {
                                    if (blockDataBase.ContainsKey(d.Reference))
                                    {
                                        var duplicateReferences = new List<StreamReference>() { d.Reference };
                                        duplicateDataBase[newRef] = duplicateReferences;
                                    }
                                }
                                else
                                {
                                    if (blockDataBase.ContainsKey(d.Reference))
                                        duplicateDataBase[newRef].Add(d.Reference);
                                }

                                blockDataBase.Remove(d.Reference);
                            }
                        }
                        else
                        {
                            // This block of data is still unique
                            dataHashDataBase.Add(hash, d.Reference);
                        }
                    }

                    // Rebuild the list of data blocks
                    dataBlocks.Clear();
                    dataBlocks.Capacity = blockDataBase.Count;
                    foreach ((var _, DataBlock db) in blockDataBase)
                    {
                        dataBlocks.Add(db);
                    }

                    // For each data block replace any occurence of an old reference with its unique reference
                    foreach (var db in dataBlocks)
                    {
                        foreach ((StreamReference uniqueRef, List<StreamReference> duplicateRefs) in duplicateDataBase)
                        {
                            foreach (var duplicateRef in duplicateRefs)
                            {
                                DataBlock.ReplaceReference(db, duplicateRef, uniqueRef);
                            }
                        }
                    }

                    // Did we find any duplicates, if so then we also replaced references
                    // and by doing so hashes have changed.
                    // Some blocks now might have an identical hash due to this.
                    if (duplicateDataBase.Count == 0) break;
                }
            }

            public void Finalize(IStreamWriter dataWriter, out List<Hash160> dataUnitSignatures, out List<ulong> dataUnitStreamPositions, out List<ulong> dataUnitStreamSizes)
            {
                // NEW, DataUnits!
                // A DataUnit consists of Blocks.
                // For each DataUnit we need to de-duplicate Blocks, and once we have done that we
                // can serialize DataUnits to the dataWriter.
                // Also every serialized DataUnit needs a Header structure, so that when it is loaded in
                // the Runtime we have some information, like where the first Pointer is located to start
                // patching them.
                var blockDataBases = new List<Dictionary<StreamReference, DataBlock>>();
                var dataUnitDataBase = new Dictionary<int, int>(_dataBlocks.Count);
                foreach (var d in _dataBlocks)
                {
                    if (!dataUnitDataBase.TryGetValue(d.DataUnitIndex, out var blockDatabaseIndex))
                    {
                        blockDatabaseIndex = blockDataBases.Count;
                        blockDataBases.Add(new Dictionary<StreamReference, DataBlock>());
                        dataUnitDataBase.Add(d.DataUnitIndex, blockDatabaseIndex);
                    }

                    blockDataBases[blockDatabaseIndex].Add(d.Reference, d);
                }

                dataUnitSignatures = new List<Hash160>(dataUnitDataBase.Count);
                dataUnitStreamPositions = new List<ulong>(dataUnitDataBase.Count);
                dataUnitStreamSizes = new List<ulong>(dataUnitDataBase.Count);

                var memoryBlock = new DataStream(_memoryStream, ArchitectureUtils.LittleArchitecture64);

                var readWriteBuffer = new byte[65536];
                foreach (var (dataUnitIndex,  dataUnitBlockDataBaseIndex) in dataUnitDataBase)
                {
                    // The signature of this DataUnit
                    dataUnitSignatures.Add(_dataUnitHashes[dataUnitIndex]);

                    // Dictionary for mapping a Reference to a DataBlock
                    var blockDataBase = blockDataBases[dataUnitBlockDataBaseIndex];

                    DeduplicateDataBlocks(memoryBlock, blockDataBase);

                    // Compute stream offset for each data block, do this by simulating the writing process.
                    // All references (pointers) are written in the stream as a 64-bit offset (64-bit pointers)
                    // relative to the start of the data stream.
                    var streamReferenceToStreamPositionDataBase = new Dictionary<StreamReference, int>();
                    var streamPosition = 0;
                    foreach (var (dbRef, db) in blockDataBase)
                    {
                        streamPosition = CMath.AlignUp(streamPosition, db.Alignment);
                        streamReferenceToStreamPositionDataBase.Add(dbRef, streamPosition);
                        streamPosition += db.Size;
                    }

                    // Collect all pointers that are in the stream, we will build a Singly-LinkedList out of them.
                    // Furthermore, the stream pointers will write out 'relative' information, so it doesn't really
                    // matter where in the final output they are written.
                    var streamPointers = new List<StreamPointer>();
                    foreach ((_, DataBlock db) in blockDataBase)
                    {
                        DataBlock.CollectStreamPointers(db, streamPointers, streamReferenceToStreamPositionDataBase);
                    }

                    // Connect all StreamPointer into a Singly-LinkedList so that in the Runtime we
                    // only have to walk the list to patch the pointers, we do not need to load an
                    // additional file with pointer locations.
                    DataBlock.WriteStreamPointers(memoryBlock, streamPointers);

                    // Align start of DataUnit at 16 bytes
                    BinaryWriter.Align(dataWriter, 16);

                    // Remember the start of the DataUnit in the stream
                    var dataUnitBeginPos = dataWriter.Position;
                    dataUnitStreamPositions.Add((ulong)dataUnitBeginPos);

                    // We have to also write a 'pointer' at the start of the DataUnit that
                    // serves as the head of the linked list.
                    // So here we are writing a 16 bytes header, where we currently only use the first 4 bytes.
                    const int dataUnitHeaderSize = 16;
                    var linkedListHeadPtr = (streamPointers.Count > 0) ? (streamPointers[0].PositionInDestinationStream+dataUnitHeaderSize) : 0;
                    BinaryWriter.Write(dataWriter, linkedListHeadPtr);
                    BinaryWriter.Write(dataWriter, 0);
                    BinaryWriter.Write(dataWriter, 0);
                    BinaryWriter.Write(dataWriter, 0);

                    // Write all DataBlocks
                    foreach ((_, DataBlock db) in blockDataBase)
                    {
                        DataBlock.WriteDataBlock(dataUnitBeginPos+dataUnitHeaderSize, db, memoryBlock, dataWriter, streamReferenceToStreamPositionDataBase, readWriteBuffer);
                    }

                    // Remember the current location
                    var dataUnitEndPos = dataWriter.Position;

                    // Store the position and size of this DataUnit
                    dataUnitStreamSizes.Add((ulong)dataUnitEndPos - (ulong)dataUnitBeginPos);
                }
            }

            public IArchitecture Architecture => _dataWriter.Architecture;

            public long Position
            {
                get => _dataWriter.Position;
                set => _dataWriter.Position = value;
            }

            public long Length
            {
                get => _dataWriter.Length;
                set => _dataWriter.Length = value;
            }

            public long Seek(long offset)
            {
                return _dataWriter.Seek(offset);
            }

            public void Close()
            {
                _dataWriter.Close();
            }
        }
    }
}
