using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using GameCore;
using BinaryWriter = GameCore.BinaryWriter;
using Process = System.Diagnostics.Process;

namespace GameData
{
    namespace MetaCode
    {
        public static class CppDataStreamWriter2
        {
            private delegate void WriteMemberDelegate(int memberIndex, WriteContext ctx);

            private delegate void WriteProcessDelegate(int memberIndex, StreamReference dataBlockStreamReference, WriteContext ctx);

            private struct ProcessObject
            {
                public int MemberIndex { get; init; }
                public WriteProcessDelegate Process { get; init; }
                public StreamReference BlockReference { get; init; }
            }

            private struct ProcessDataUnit
            {
                public int MemberIndex { get; init; }
                public int DataUnitIndex { get; set; }
                public Hash160 Signature { get; set; }
            }

            private class WriteContext
            {
                public MetaCode2 MetaCode2 { get; init; }
                public StringTable StringTable { get; init; }
                public ISignatureDataBase SignatureDataBase { get; init; }
                public CppDataStream2 GameDataStream { get; init; }
                public WriteMemberDelegate[] WriteMemberDelegates { get; init; }
                public Queue<ProcessObject> ProcessObjectQueue { get; init; }
                public Queue<ProcessDataUnit> ProcessDataUnitQueue { get; init; }
            }

            public static void Write(MetaCode2 metaCode2, string rootSignature, StringTable stringTable, ISignatureDataBase signatureDb, CppDataStream2 dataStream)
            {
                var ctx = new WriteContext
                {
                    MetaCode2 = metaCode2,
                    StringTable = stringTable,
                    SignatureDataBase = signatureDb,
                    GameDataStream = dataStream,
                    ProcessObjectQueue = new Queue<ProcessObject>(),
                    ProcessDataUnitQueue = new Queue<ProcessDataUnit>(),
                    WriteMemberDelegates = new WriteMemberDelegate[MetaInfo.Count]
                    {
                        null, WriteBool, WriteBitset, WriteInt8, WriteUInt8, WriteInt16, WriteUInt16, WriteInt32, WriteUInt32, WriteInt64, WriteUInt64, WriteFloat, WriteDouble, WriteString, WriteEnum, WriteStruct, WriteClass, WriteArray,
                        WriteDictionary, WriteDataUnit
                    },
                };

                try
                {
                    // Write the data of the MetaCode to the DataStream
                    var rootHashSignature = HashUtility.Compute_ASCII(rootSignature);
                    var rootDataUnitIndex = ctx.GameDataStream.GetDataUnitIndex(rootHashSignature);
                    ctx.ProcessDataUnitQueue.Enqueue(new ProcessDataUnit() { MemberIndex = 0, DataUnitIndex = rootDataUnitIndex, Signature = rootHashSignature });

                    // Whenever we encounter a DataUnit we need to write the data of the DataUnit to the DataStream
                    while (ctx.ProcessDataUnitQueue.Count > 0)
                    {
                        var du = ctx.ProcessDataUnitQueue.Dequeue();

                        ctx.GameDataStream.OpenDataUnit(du.DataUnitIndex);
                        {
                            var mr = StreamReference.NewReference;
                            ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(du.MemberIndex));
                            ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = du.MemberIndex, Process = WriteClassDataProcess, BlockReference = mr});

                            while (ctx.ProcessObjectQueue.Count > 0)
                            {
                                var wp = ctx.ProcessObjectQueue.Dequeue();
                                wp.Process(wp.MemberIndex, wp.BlockReference, ctx);
                            }
                        }
                        ctx.GameDataStream.CloseDataUnit();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            private static void WriteBool(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((bool)member ? (sbyte)1 : (sbyte)0);
            }

            private static void WriteBitset(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((byte)member);
            }

            private static void WriteInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((sbyte)member);
            }

            private static void WriteUInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((byte)member);
            }

            private static void WriteInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((short)member);
            }

            private static void WriteUInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((ushort)member);
            }

            private static void WriteInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((int)member);
            }

            private static void WriteUInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((uint)member);
            }

            private static void WriteInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((long)member);
            }

            private static void WriteUInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((ulong)member);
            }

            private static void WriteFloat(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((float)member);
            }

            private static void WriteDouble(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.Write((double)member);
            }

            private static void WriteStringDataProcess(int memberIndex, StreamReference br, WriteContext ctx)
            {
                // A string is written as an array of UTF8 bytes
                ctx.GameDataStream.OpenBlock(br);
                {
                    var str = ctx.MetaCode2.MembersObject[memberIndex] as string;
                    var ms = ctx.MetaCode2.MembersCount[memberIndex];
                    var bytes = new byte[ms];
                    var bl = s_utf8Encoding.GetBytes(str, 0, str.Length, bytes, 0);
                    ctx.GameDataStream.Write(bytes, 0, bl);
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static readonly UTF8Encoding s_utf8Encoding = new ();

            private static void WriteString(int memberIndex, WriteContext ctx)
            {
                var str = ctx.MetaCode2.MembersObject[memberIndex] as string;

                var bl = s_utf8Encoding.GetByteCount(str);
                var rl = str.Length;
                var ms = CMath.AlignUp32(bl + 1, 8);
                ctx.MetaCode2.MembersCount[memberIndex] = ms;

                var br = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(br, ctx.MetaCode2.GetDataAlignment(memberIndex));

                ctx.GameDataStream.Write(bl); // Length in bytes
                ctx.GameDataStream.Write(rl); // Length in runes
                ctx.GameDataStream.WriteDataBlockReference(br); // const char* const, String

                // We need to schedule the content of this class to be written
                ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = memberIndex, Process = WriteStringDataProcess, BlockReference = br });
            }


            private static void WriteEnum(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var fet = ctx.MetaCode2.MembersType[msi];

                switch (fet.SizeInBytes)
                {
                    case 1:
                        switch (fet.IsSigned)
                        {
                            case true:
                                ctx.GameDataStream.Write((sbyte)member);
                                break;
                            case false:
                                ctx.GameDataStream.Write((byte)member);
                                break;
                        }
                        break;
                    case 2:
                        switch (fet.IsSigned)
                        {
                            case true:
                                ctx.GameDataStream.Write((short)member);
                                break;
                            case false:
                                ctx.GameDataStream.Write((ushort)member);
                                break;
                        }
                        break;
                    case 4:
                        switch (fet.IsSigned)
                        {
                            case true:
                                ctx.GameDataStream.Write((int)member);
                                break;
                            case false:
                                ctx.GameDataStream.Write((uint)member);
                                break;
                        }
                        break;
                    case 8:
                        switch (fet.IsSigned)
                        {
                            case true:
                                ctx.GameDataStream.Write((long)member);
                                break;
                            case false:
                                ctx.GameDataStream.Write((ulong)member);
                                break;
                        }
                        break;
                }
            }

            private static void WriteStruct(int memberIndex, WriteContext ctx)
            {
                if (ctx.MetaCode2.MembersObject[memberIndex] is IStruct mx)
                {
                    ctx.GameDataStream.Align(mx.StructAlign);
                    mx.StructWrite(ctx.GameDataStream);
                }
            }

            private static void WriteClassDataProcess(int memberIndex, StreamReference br, WriteContext ctx)
            {
                // A class is written as a collection of members, we are using the SortedMembersMap to
                // write out the members in sorted order.
                ctx.GameDataStream.OpenBlock(br);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        var mis = ctx.MetaCode2.MemberSorted[mi];
                        var et = ctx.MetaCode2.MembersType[mis];
                        ctx.WriteMemberDelegates[et.Index](mis, ctx);
                    }
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static void WriteClass(int memberIndex, WriteContext ctx)
            {
                var mr = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex));

                // A class as a member is just a pointer (Type* member)
                ctx.GameDataStream.WriteDataBlockReference(mr);

                // We need to schedule the content of this class to be written
                ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = memberIndex, Process = WriteClassDataProcess, BlockReference = mr});
            }

            private static void WriteArrayDataProcess(int memberIndex, StreamReference br, WriteContext ctx)
            {
                // An Array<T> is written as an array of elements
                ctx.GameDataStream.OpenBlock(br);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    var et = ctx.MetaCode2.MembersType[msi];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[et.Index](mi, ctx);
                    }
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static void WriteArray(int memberIndex, WriteContext ctx)
            {
                var count = (uint)ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex));
                ctx.GameDataStream.Write(mr, count, count);

                // We need to schedule this array to be written
                ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = memberIndex, Process = WriteArrayDataProcess, BlockReference = mr});
            }

            private static void WriteDictionaryDataProcess(int memberIndex, StreamReference br, WriteContext ctx)
            {
                // A Dictionary<key,value> is written as an array of keys followed by an array of values
                ctx.GameDataStream.OpenBlock(br);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    var kt = ctx.MetaCode2.MembersType[msi];
                    var vt = ctx.MetaCode2.MembersType[msi + count];
                    // First write the array of keys
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[kt.Index](mi, ctx);
                    }

                    // NOTE, alignment ?

                    // Second, write the array of values
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[vt.Index](mi, ctx);
                    }
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static void WriteDictionary(int memberIndex, WriteContext ctx)
            {
                var count = (uint)ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex));
                ctx.GameDataStream.Write(mr, count, count);

                // We need to schedule this array to be written
                ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = memberIndex, Process = WriteDictionaryDataProcess, BlockReference = mr});
            }

            private static void WriteDataUnit(int memberIndex, WriteContext ctx)
            {
                var dataUnit = ctx.MetaCode2.MembersObject[memberIndex] as IDataUnit;
                var signature = HashUtility.Compute_ASCII(dataUnit.Signature);
                var (bigfileIndex, fileIndex) = ctx.SignatureDataBase.GetEntry(signature);

                ctx.GameDataStream.Write((ulong)0);   // T* (8 bytes)
                ctx.GameDataStream.Write(bigfileIndex); // (4 bytes)
                ctx.GameDataStream.Write(fileIndex);    // (4 bytes)

                var dataUnitIndex = ctx.GameDataStream.GetDataUnitIndex(signature);

                // The 'class' member of this DataUnit is the (only) member
                var dataUnitClassMemberIndex = ctx.MetaCode2.MembersStart[memberIndex];

                ctx.ProcessDataUnitQueue.Enqueue(new ProcessDataUnit() { MemberIndex = dataUnitClassMemberIndex, DataUnitIndex = dataUnitIndex, Signature = signature });
            }
        }

        public class CppDataStream2 : IDataWriter, IGameDataWriter
        {
            private int _current;
            private readonly List<Hash160> _dataUnitHashes;
            private readonly Dictionary<Hash160, int> _hashToDataUnitIndex;
            private readonly Stack<int> _dataUnitStack;
            private readonly List<DataBlock> _dataBlocks;
            private readonly Dictionary<StreamReference, int> _referenceToDataBlock;
            private readonly StringTable _stringTable;
            private readonly ISignatureDataBase _signatureDb;
            private readonly MemoryStream _memoryStream;
            private readonly IStreamWriter _dataWriter;

            public CppDataStream2(EPlatform platform, StringTable strTable, ISignatureDataBase signatureDb)
            {
                _current = -1;
                _dataUnitHashes = new List<Hash160>();
                _hashToDataUnitIndex = new Dictionary<Hash160, int>();
                _dataUnitStack = new Stack<int>();
                _dataBlocks = new List<DataBlock>();
                _referenceToDataBlock = new Dictionary<StreamReference, int>();
                _stringTable = strTable;
                _signatureDb = signatureDb;
                _memoryStream = new MemoryStream();
                _dataWriter = ArchitectureUtils.CreateMemoryWriter(_memoryStream, platform);
            }

            private class DataBlock
            {
                private Dictionary<StreamReference, List<long>> BlockPointers { get; } = new();

                // Markers:
                // These are used when you are writing data to the stream, and you want to
                // 'remember' this as a pointer, so you can store a StreamReference and Offset.
                private Dictionary<StreamReference, long> Markers { get; } = new();

                public int DataUnitIndex{ get; set; }
                public int Alignment { get; init; }
                public int Offset { get; set; }
                public int Size { get; set; }
                public StreamReference Reference { get; init; }

                public static void End(DataBlock db, IBinaryWriter data)
                {
                    var gap = CMath.AlignUp(db.Size, db.Alignment) - db.Size;
                    // Write actual data to reach the size alignment requirement
                    const byte zero = 0;
                    for (var i = 0; i < gap; ++i)
                        GameCore.BinaryWriter.Write(data, zero);
                }

                public Hash160 ComputeHash(byte[] memoryBytes, byte[] workBuffer)
                {
                    //      Include the stream pointers in the hash computation otherwise we might
                    //      collapse two blocks that are not identical.
                    //
                    HashUtility.Begin();
                    HashUtility.Update(memoryBytes,Offset, Size);

                    if (BlockPointers.Count > 0)
                    {
                        var offset = 0;

                        // This is a magic 64-bit value that is unique but stable for all DataBlocks
                        const ulong magic = (ulong)0xDEADBEEFCAFEBABE;
                        ArchitectureUtils.LittleArchitecture64.Write(magic, workBuffer, offset);
                        offset += 8;

                        foreach ((StreamReference streamReference, List<long> pointers) in BlockPointers)
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

                private static void AlignTo(IBinaryStream writer, uint alignment)
                {
                    writer.Position = CMath.AlignUp(writer.Position, alignment);
                }

                private static bool IsAligned(IBinaryStream writer, uint alignment)
                {
                    return CMath.IsAligned(writer.Position, alignment);
                }

                internal static void Write(IStreamWriter writer, float v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(float)));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, double v)
                {
                    AlignTo(writer, sizeof(double));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, sbyte v)
                {
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, short v)
                {
                    AlignTo(writer, sizeof(short));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, int v)
                {
                    AlignTo(writer, sizeof(int));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, long v)
                {
                    AlignTo(writer, sizeof(long));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, byte v)
                {
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, ushort v)
                {
                    AlignTo(writer, sizeof(ushort));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, uint v)
                {
                    AlignTo(writer, sizeof(uint));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IStreamWriter writer, ulong v)
                {
                    AlignTo(writer, sizeof(ulong));
                    GameCore.BinaryWriter.Write(writer, v);
                }

                internal static void Write(IBinaryWriter writer, byte[] data, int index, int count)
                {
                    writer.Write(data, index, count);
                }

                internal static void Write(IBinaryWriter writer, ReadOnlySpan<byte> span)
                {
                    GameCore.BinaryWriter.Write(writer, span);
                }

                internal static void WriteDataBlockReference(IStreamWriter writer, DataBlock db, StreamReference v)
                {
                    AlignTo(writer, sizeof(ulong));
                    if (db.BlockPointers.TryGetValue(v, out var pointers))
                    {
                        pointers.Add(writer.Position - db.Offset);
                    }
                    else
                    {
                        pointers = new List<long>() { writer.Position - db.Offset };
                        db.BlockPointers.Add(v, pointers);
                    }

                    GameCore.BinaryWriter.Write(writer, (long)v.Id);
                }

                internal static void Mark(DataBlock db, StreamReference v, long position)
                {
                    db.Markers.Add(v, position - db.Offset);
                }

                internal static void ReplaceReference(IStreamWriter data, DataBlock db, StreamReference oldRef, StreamReference newRef)
                {
                    // See if we are using this reference (oldRef) in this data block
                    if (!db.BlockPointers.Remove(oldRef, out var oldOffsets)) return;

                    // Update the reference in the stream, replacing oldRef.Id with newRef.Id
                    foreach (var o in oldOffsets)
                    {
                        data.Seek(db.Offset + o); // Seek to the position that has the 'StreamReference'
                        BinaryWriter.Write(data, (long)newRef.Id); // The value we write here is the offset to the data computed in the simulation
                    }

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

                internal static void ProcessStreamMarkers(DataBlock db, long dataBlockOffset, IDictionary<StreamReference, long> dataOffsetDataBase)
                {
                    // Update the dataOffsetDatabase with any markers we have in this data block
                    foreach (var (sr, offset) in db.Markers)
                    {
                        dataOffsetDataBase.Add(sr, dataBlockOffset + offset);
                    }
                }

                internal static void CollectStreamPointers(DataBlock db, ICollection<StreamPointer> streamPointers, IDictionary<StreamReference, long> dataPositionDataBase)
                {
                    // BlockPointers are references that are written in this Block, each Reference might
                    // have been written more than once so that is why there is a list of Offsets per
                    // StreamReference.
                    foreach (var (sr, offsets) in db.BlockPointers)
                    {
                        // What is the offset of the data block we are pointing to
                        var exists = dataPositionDataBase.TryGetValue(sr, out var referencePosition);
                        Debug.Assert(exists);

                        // The offset are relative to the start of the DataBlock
                        foreach (var o in offsets)
                        {
                            // Remember this pointer
                            var sp = new StreamPointer() { Position = db.Offset + o, DataPosition = referencePosition };
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

                internal static void WriteDataBlock(long dataUnitStartPos, DataBlock db, IStreamReader data, IStreamWriter outData, IDictionary<StreamReference, long> dataOffsetDataBase, byte[] readWriteBuffer)
                {
                    // Verify the position of the data stream
                    dataOffsetDataBase.TryGetValue(db.Reference, out var outDataBlockOffset);
                    StreamUtils.Align(outData, db.Alignment);
                    Debug.Assert((outData.Position-dataUnitStartPos) == outDataBlockOffset);

                    // Read from the data stream at the start of the block and write the block to the output
                    data.Seek(db.Offset);

                    // Write the data of this block to the output, chunk for chunk
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

            public void Align(int align)
            {
                var offset = _dataWriter.Position;
                if (CMath.TryAlignUp(offset, align, out var alignment)) return;
                _dataWriter.Position = alignment;
            }

            public int GetDataUnitIndex(Hash160 hash)
            {
                if (!_hashToDataUnitIndex.TryGetValue(hash, out var index))
                {
                    index = _hashToDataUnitIndex.Count;
                    _hashToDataUnitIndex.Add(hash, index);
                    _dataUnitHashes.Add(hash);
                }

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

            public void Mark(StreamReference reference)
            {
                Debug.Assert(_current != -1);
                DataBlock.Mark(_dataBlocks[_current], reference, _dataWriter.Position);
            }

            public void Write(float v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(float v)
            {
                Align(4);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(double v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(double v)
            {
                Align(8);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(sbyte v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(byte v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(short v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(short v)
            {
                Align(2);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(int v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(int v)
            {
                Align(4);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(long v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(long v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(ushort v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(ushort v)
            {
                Align(2);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(uint v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(uint v)
            {
                Align(4);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(ulong v)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void AlignWrite(ulong v)
            {
                Align(8);
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, v);
            }

            public void Write(byte[] data, int index, int count)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, data, index, count);
            }

            public void Write(ReadOnlySpan<byte> span)
            {
                Debug.Assert(_current != -1);
                DataBlock.Write(_dataWriter, span);
            }

            //public void Write(string str)
            //{
                //Debug.Assert(_current != -1);
                //var idx = _stringTable.Add(str);
                //var len = (uint)_stringTable.ByteCountForIndex(idx);
                //var reference = _stringTable.StreamReferenceForIndex(idx);
                //Write(reference, len, (uint)str.Length);
            //}

            public void WriteFileId(Hash160 signature)
            {
                (uint bigfileIndex, uint fileIndex) = _signatureDb.GetEntry(signature);
                DataBlock.Write(_dataWriter, bigfileIndex);
                DataBlock.Write(_dataWriter, fileIndex);
            }

            public void WriteDataBlockReference(StreamReference v)
            {
                Debug.Assert(_current != -1);
                DataBlock.WriteDataBlockReference(_dataWriter, _dataBlocks[_current], v);
            }

            public void Write(StreamReference v, uint byteLength, uint runeLength)
            {
                Debug.Assert(_current != -1);
                DataBlock.WriteDataBlockReference(_dataWriter, _dataBlocks[_current], v);
                DataBlock.Write(_dataWriter, byteLength);
                DataBlock.Write(_dataWriter, runeLength);
            }

            public void Final(IStreamWriter dataWriter)
            {
            }

            private static MemoryBlock DeduplicateDataBlocks(MemoryStream memoryStream, Dictionary<StreamReference, DataBlock> blockDataBase)
            {
                // For all blocks that are part of this chunk:
                // Collapse identical blocks identified by hash, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the hash of a data block.

                var memoryBytes = memoryStream.ToArray();
                var memoryBlock = new MemoryBlock(ArchitectureUtils.LittleArchitecture64);
                memoryBlock.Setup(memoryBytes, 0, memoryBytes.Length);

                var duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                var dataHashDataBase = new Dictionary<Hash160, StreamReference>();

                var dataBlocks = new List<DataBlock>(blockDataBase.Count);
                foreach (var b in blockDataBase)
                    dataBlocks.Add(b.Value);

                var workBuffer = new byte[65536];

                while (true)
                {
                    duplicateDataBase.Clear();
                    dataHashDataBase.Clear();

                    foreach (var d in dataBlocks)
                    {
                        // TODO this can lead to incorrect matches, we should ask the DataBlock to compute the hash
                        //      since it can consider the stream pointers that are in the block

                        var hash = d.ComputeHash(memoryBytes, workBuffer);

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
                    foreach (var (_, db) in blockDataBase)
                    {
                        dataBlocks.Add(db);
                    }

                    // For each data block replace any occurence of an old reference with its unique reference
                    foreach (var db in dataBlocks)
                    {
                        foreach (var (uniqueRef, duplicateRefs) in duplicateDataBase)
                        {
                            foreach (var duplicateRef in duplicateRefs)
                            {
                                DataBlock.ReplaceReference(memoryBlock, db, duplicateRef, uniqueRef);
                            }
                        }
                    }

                    // Did we find any duplicates, if so then we also replaced references
                    // and by doing so hashes have changed.
                    // Some blocks now might have an identical hash due to this.
                    if (duplicateDataBase.Count == 0) break;
                }

                return memoryBlock;
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
                    Dictionary<StreamReference, DataBlock> blockDatabase;
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

                var readWriteBuffer = new byte[65536];
                foreach (var (dataUnitIndex,  dataUnitBlockDataBaseIndex) in dataUnitDataBase)
                {
                    // The signature of this DataUnit
                    dataUnitSignatures.Add(_dataUnitHashes[dataUnitIndex]);

                    // Dictionary for mapping a Reference to a DataBlock
                    var blockDataBase = blockDataBases[dataUnitBlockDataBaseIndex];

                    var memoryStream = DeduplicateDataBlocks(_memoryStream, blockDataBase);

                    // Compute stream offset for each data block, do this by simulating the writing process.
                    // All references (pointers) are written in the stream as a 64-bit offset (64-bit pointers)
                    // relative to the start of the data stream.
                    var streamReferenceToPositionDataBase = new Dictionary<StreamReference, long>();
                    var position = (long)0;
                    foreach (var (dbRef, db) in blockDataBase)
                    {
                        position = CMath.AlignUp(position, db.Alignment);
                        streamReferenceToPositionDataBase.Add(dbRef, position);
                        DataBlock.ProcessStreamMarkers(db, position, streamReferenceToPositionDataBase);
                        position += db.Size;
                    }

                    // Collect all pointers that are in the stream, we will build a Singly-LinkedList out of them
                    var streamPointers = new List<StreamPointer>();
                    foreach (var (_, db) in blockDataBase)
                    {
                        DataBlock.CollectStreamPointers(db, streamPointers, streamReferenceToPositionDataBase);
                    }

                    // Connect all StreamPointer into a Singly-LinkedList so that in the Runtime we
                    // only have to walk the list to patch the pointers, we do not need to load an
                    // additional file with pointer locations.
                    DataBlock.WriteStreamPointers(memoryStream, streamPointers);

                    // Align start of DataUnit at 16 bytes
                    StreamUtils.Align(dataWriter, 16);

                    // Store the start of the DataUnit
                    var dataUnitBeginPos = dataWriter.Position;
                    dataUnitStreamPositions.Add((ulong)dataUnitBeginPos);

                    // Write all DataBlocks
                    foreach (var (_, db) in blockDataBase)
                    {
                        DataBlock.WriteDataBlock(dataUnitBeginPos, db, memoryStream, dataWriter, streamReferenceToPositionDataBase, readWriteBuffer);
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
