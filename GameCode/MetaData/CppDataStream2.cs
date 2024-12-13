using System.Diagnostics;
using System.Text;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        public static class CppDataStreamWriter2
        {
            private delegate void WriteMemberDelegate(int memberIndex, WriteContext ctx);
            private delegate int CalcSizeOfTypeDelegate(int memberIndex, WriteContext ctx);
            private delegate void WriteProcessDelegate(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx);

            private struct WriteProcess
            {
                public int MemberIndex { get; init; }
                public WriteProcessDelegate Process { get; init; }
                public StreamReference BlockReference { get; init; }
                public StreamReference DataUnitReference { get; init; }
            }

            private class WriteContext
            {
                public MetaCode2 MetaCode2 { get; init; }
                public StringTable StringTable { get; init; }
                public ISignatureDataBase DataFileDataBase { get; init; }
                public CppDataStream2 GameDataStream { get; init; }
                public CalcSizeOfTypeDelegate[] CalcSizeOfTypeDelegates { get; init; }
                public CalcSizeOfTypeDelegate[] CalcDataSizeOfTypeDelegates { get; init; }
                public WriteMemberDelegate[] WriteMemberDelegates { get; init; }
                public Queue<WriteProcess> WriteProcessQueue { get; init; }
            }

            public static void Write(MetaCode2 metaCode2, StringTable stringTable, CppDataStream2 dataStream)
            {
                var ctx = new WriteContext
                {
                    MetaCode2 = metaCode2,
                    StringTable = stringTable,
                    GameDataStream = dataStream,
                    WriteProcessQueue = new Queue<WriteProcess>(),

                    WriteMemberDelegates = new WriteMemberDelegate[(int)MetaInfo.Count]
                    {
                        null,
                        WriteBool,
                        WriteBitset,
                        WriteInt8,
                        WriteUInt8,
                        WriteInt16,
                        WriteUInt16,
                        WriteInt32,
                        WriteUInt32,
                        WriteInt64,
                        WriteUInt64,
                        WriteFloat,
                        WriteDouble,
                        WriteString,
                        WriteEnum,
                        WriteStruct,
                        WriteClass,
                        WriteArray,
                        WriteDictionary,
                        WriteDataUnit
                    },

                    CalcSizeOfTypeDelegates = new CalcSizeOfTypeDelegate[(int)MetaInfo.Count]
                    {
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfStruct,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType
                    },

                    CalcDataSizeOfTypeDelegates = new CalcSizeOfTypeDelegate[(int)MetaInfo.Count]
                    {
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        CalcDataSizeOfString,
                        GetDataSizeOfType,
                        CalcDataSizeOfStruct,
                        CalcDataSizeOfClass,
                        CalcDataSizeOfArray,
                        CalcDataSizeOfDictionary,
                        GetDataSizeOfDataUnit
                    }
                };

                ctx.StringTable.Write(ctx.GameDataStream);

                var rootRef = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(rootRef, 8, 2 * 8);
                ctx.GameDataStream.OpenBlock(rootRef);
                {
                    ctx.GameDataStream.WriteBlockReference(stringTable.Reference); // String Table pointer
                    WriteClass(0, ctx); // Root Class pointer
                    ctx.GameDataStream.CloseBlock();

                    while (ctx.WriteProcessQueue.Count > 0)
                    {
                        var wp = ctx.WriteProcessQueue.Dequeue();
                        wp.Process(wp.MemberIndex, wp.BlockReference, wp.DataUnitReference, ctx);
                    }
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
                ctx.GameDataStream.AlignWrite((uint)member);
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
                ctx.GameDataStream.AlignWrite((short)member);
            }

            private static void WriteUInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((ushort)member);
            }

            private static void WriteInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((int)member);
            }

            private static void WriteUInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((uint)member);
            }

            private static void WriteInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((long)member);
            }

            private static void WriteUInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((ulong)member);
            }

            private static void WriteFloat(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((float)member);
            }

            private static void WriteDouble(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((double)member);
            }

            private static void WriteStringDataProcess(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx)
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

                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = CMath.AlignUp32(bl + 1, 8);
                ctx.MetaCode2.MembersCount[memberIndex] = ms;

                var br = StreamReference.NewReference;
                var dur = ctx.GameDataStream.NewBlock(br, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);

                ctx.GameDataStream.WriteBlockReference(br); // const char* const, String
                ctx.GameDataStream.Write(bl); // Length in bytes
                ctx.GameDataStream.Write(rl); // Length in runes

                // We need to schedule the content of this class to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteStringDataProcess, BlockReference = br, DataUnitReference = dur });
            }


            private static void WriteEnum(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.GameDataStream.AlignWrite((uint)member);
            }

            private static void WriteStruct(int memberIndex, WriteContext ctx)
            {
                if (ctx.MetaCode2.MembersObject[memberIndex] is IStruct mx)
                {
                    ctx.GameDataStream.Align(mx.StructAlign);
                    mx.StructWrite(ctx.GameDataStream);
                }
            }

            private static void WriteClassDataProcess(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx)
            {
                // A class is written as a collection of members, we are using the SortedMembersMap to
                // write out the members in sorted order.
                ctx.GameDataStream.OpenBlock(br);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        var rmi = ctx.MetaCode2.SortedMembersMap[mi];
                        var et = ctx.MetaCode2.MembersType[rmi];
                        ctx.WriteMemberDelegates[et.Index](rmi, ctx);
                    }
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static void WriteClass(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var mr = StreamReference.NewReference;
                var cr = ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.GameDataStream.WriteBlockReference(mr);

                // We need to schedule the content of this class to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteClassDataProcess, BlockReference = mr, DataUnitReference = cr});
            }

            private static void WriteArrayDataProcess(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx)
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
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                var cr =ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.GameDataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteArrayDataProcess, BlockReference = mr, DataUnitReference = cr});
            }

            private static void WriteDictionaryDataProcess(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx)
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
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                var cr = ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.GameDataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteDictionaryDataProcess, BlockReference = mr, DataUnitReference = cr});
            }

            private static void WriteDataUnitProcess(int memberIndex, StreamReference br, StreamReference cr, WriteContext ctx)
            {
                // A DataUnit (class) is written as a collection of members
                ctx.GameDataStream.OpenDataUnit(cr);
                {
                    ctx.GameDataStream.OpenBlock(br);
                    {
                        var msi = ctx.MetaCode2.MembersStart[memberIndex];
                        var count = ctx.MetaCode2.MembersCount[memberIndex];
                        for (var mi = msi; mi < msi + count; ++mi)
                        {
                            var et = ctx.MetaCode2.MembersType[mi];
                            ctx.WriteMemberDelegates[et.Index](mi, ctx);
                        }
                    }
                    ctx.GameDataStream.CloseBlock();
                }
                ctx.GameDataStream.CloseDataUnit();
            }

            private static void WriteDataUnit(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);

                var mr = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);

                var cr = StreamReference.NewReference;
                ctx.GameDataStream.OpenDataUnit(cr);

                ctx.GameDataStream.Write((ulong)0); // T*
                ctx.GameDataStream.WriteChunkReference(cr); // {offset,size}

                // We need to schedule the content of this DataUnit to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteDataUnitProcess, BlockReference = mr, DataUnitReference = cr });
            }

            private static int GetMemberSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode2.MembersType[memberIndex].SizeInBytes;
            }

            private static int GetMemberSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode2.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }


            private static int GetDataSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode2.MembersType[memberIndex].SizeInBytes;
            }

            private static int CalcDataSizeOfString(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersStart[memberIndex];
                return ctx.StringTable.ByteCountForIndex(member);
            }

            private static int CalcDataSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode2.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }

            private static int CalcDataSizeOfClass(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                // Alignment of this class is determined by the first member
                var classAlign = ctx.MetaCode2.GetMemberAlignment(msi);

                var size = 0;
                for (var mi = msi; mi < msi + count; ++mi)
                {
                    // Obtain the size of this member
                    var ms = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[mi].Index](mi, ctx);

                    // Align the size based on the member type alignment
                    size = CMath.AlignUp32(size, ctx.MetaCode2.GetMemberAlignment(mi));

                    size += ms;
                }

                size = CMath.AlignUp32(size, classAlign);

                return size;
            }

            private static int CalcDataSizeOfArray(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                // Determine the alignment of the element and see if we need to align the size
                var elementAlign = ctx.MetaCode2.GetMemberAlignment(msi);
                var elementSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[msi].Index](msi, ctx);
                elementSize = CMath.AlignUp32(elementSize, elementAlign);

                var size = count * elementSize;
                return size;
            }

            private static int CalcDataSizeOfDictionary(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                var keyIndex = msi;
                var keyAlign = ctx.MetaCode2.GetMemberAlignment(keyIndex);
                var keySize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[keyIndex].Index](keyIndex, ctx);
                keySize = CMath.AlignUp32(keySize, keyAlign);

                var valueIndex = msi + count;
                var valueAlign = ctx.MetaCode2.GetMemberAlignment(valueIndex);
                var valueSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[valueIndex].Index](valueIndex, ctx);
                valueSize = CMath.AlignUp32(valueSize, valueAlign);

                var size = CMath.AlignUp32(count * keySize, valueAlign) + count * valueSize;
                return size;
            }

            private static int GetDataSizeOfDataUnit(int memberIndex, WriteContext ctx)
            {
                // A DataUnit is just a class
                return CalcDataSizeOfClass(memberIndex, ctx);
            }
        }

        public class CppDataStream2 : IDataWriter, IGameDataWriter
        {
            private int mCurrent;
            private int mOffset;
            private readonly Dictionary<StreamReference, int> mReferenceToChunk;
            private readonly Stack<StreamReference> mDataUnitStack;
            private readonly List<DataBlock> mDataBlocks;
            private readonly Dictionary<StreamReference, int> mReferenceToBlock;
            private readonly StringTable mStringTable;
            private readonly MemoryStream mMemoryStream;
            private readonly IBinaryStreamWriter mDataWriter;

            public CppDataStream2(EPlatform platform, StringTable strTable)
            {
                mCurrent = -1;
                mOffset = 0;
                mReferenceToChunk = new();
                mDataUnitStack = new();
                mDataBlocks = new();
                mReferenceToBlock = new();
                mStringTable = strTable;
                mMemoryStream = new();
                mDataWriter = ArchitectureUtils.CreateBinaryWriter(mMemoryStream, platform);
            }

            private class DataBlock
            {
                private Dictionary<StreamReference, List<long>> BlockPointers { get; } = new();
                private Dictionary<StreamReference, List<long>> DataUnitPointers { get; } = new();

                // Markers:
                // These are used when you are writing data to the stream and you want to
                // 'remember' this as a pointer, so you can store a StreamReference and Offset.
                private Dictionary<StreamReference, long> Markers { get; } = new();

                public StreamReference DataUnitReference{ get; set; }
                public int Alignment { get; init; }
                public int Offset { get; init; }
                public int Size { get; init; }
                public StreamReference Reference { get; init; }

                public static void End(DataBlock db, IBinaryWriter data)
                {
                    var gap = CMath.AlignUp(db.Size, db.Alignment) - db.Size;
                    // Write actual data to reach the size alignment requirement
                    const byte zero = 0;
                    for (var i = 0; i < gap; ++i)
                        data.Write(zero);
                }

                private static void AlignTo(IBinaryStream writer, uint alignment)
                {
                    writer.Position = CMath.AlignUp(writer.Position, alignment);
                }

                private static bool IsAligned(IBinaryStream writer, uint alignment)
                {
                    return CMath.IsAligned(writer.Position, alignment);
                }

                internal static void Write(IBinaryStreamWriter writer, float v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(float)));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, double v)
                {
                    AlignTo(writer, sizeof(double));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, sbyte v)
                {
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, short v)
                {
                    AlignTo(writer, sizeof(short));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, int v)
                {
                    AlignTo(writer, sizeof(int));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, long v)
                {
                    AlignTo(writer, sizeof(long));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, byte v)
                {
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, ushort v)
                {
                    AlignTo(writer, sizeof(ushort));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, uint v)
                {
                    AlignTo(writer, sizeof(uint));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, ulong v)
                {
                    AlignTo(writer, sizeof(ulong));
                    writer.Write(v);
                }

                internal static void Write(IBinaryWriter writer, byte[] data, int index, int count)
                {
                    writer.Write(data, index, count);
                }

                internal static void Write(IBinaryWriter writer, ReadOnlySpan<byte> span)
                {
                    writer.Write(span);
                }

                internal static void WriteBlockReference(IBinaryStreamWriter writer, DataBlock db, StreamReference v)
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

                    writer.Write((long)v.Id);
                }

                internal static void WriteDataUnitReference(IBinaryStreamWriter writer, DataBlock db, StreamReference v)
                {
                    AlignTo(writer, sizeof(ulong));
                    if (db.DataUnitPointers.TryGetValue(v, out var pointers))
                    {
                        pointers.Add(writer.Position - db.Offset);
                    }
                    else
                    {
                        pointers = new List<long>() { writer.Position - db.Offset };
                        db.DataUnitPointers.Add(v, pointers);
                    }

                    writer.Write((long)v.Id); // T*
                    writer.Write((uint)0); // Offset
                    writer.Write((uint)0); // Size
                }

                internal static void Mark(DataBlock db, StreamReference v, long position)
                {
                    db.Markers.Add(v, position - db.Offset);
                }

                internal static void ReplaceReference(IBinaryStreamWriter data, DataBlock db, StreamReference oldRef, StreamReference newRef)
                {
                    // See if we are using this reference (oldRef) in this data block
                    if (!db.BlockPointers.Remove(oldRef, out var oldOffsets)) return;

                    // Update the reference in the stream, replacing oldRef.Id with newRef.Id
                    foreach (var o in oldOffsets)
                    {
                        data.Seek(db.Offset + o); // Seek to the position that has the 'StreamReference'
                        data.Write((long)newRef.Id); // The value we write here is the offset to the data computed in the simulation
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

                internal static void CollectStreamPointers(DataBlock db, ICollection<StreamPointer> streamPointers, IDictionary<StreamReference, long> dataOffsetDataBase)
                {
                    // BlockPointers are references that are written in this Block, each Reference might
                    // have been written more than once so that is why there is a list of Offsets per
                    // StreamReference.
                    foreach (var (sr, offsets) in db.BlockPointers)
                    {
                        // What is the offset of the data block we are pointing to
                        var exists = dataOffsetDataBase.TryGetValue(sr, out var referenceOffset);
                        Debug.Assert(exists);

                        // The offset are relative to the start of the DataBlock
                        foreach (var o in offsets)
                        {
                            // Remember this pointer
                            var sp = new StreamPointer() { Position = db.Offset + o, DataOffset = referenceOffset };
                            streamPointers.Add(sp);
                        }
                    }
                }

                internal static void WriteStreamPointers(IBinaryDataStream data, List<StreamPointer> streamPointers)
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

                internal static void WriteDataBlock(DataBlock db, IBinaryStreamReader data, IBinaryStreamWriter outData, IDictionary<StreamReference, long> dataOffsetDataBase, byte[] readWriteBuffer)
                {
                    // Verify the position of the data stream
                    dataOffsetDataBase.TryGetValue(db.Reference, out var outDataBlockOffset);
                    StreamUtils.Align(outData, db.Alignment);
                    Debug.Assert(outData.Position == outDataBlockOffset);

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
                var offset = mDataWriter.Position;
                if (CMath.TryAlignUp(offset, align, out var alignment)) return;
                mDataWriter.Position = alignment;
            }

            public StreamReference NewBlock(StreamReference reference, int alignment, int size)
            {
                // NOTE the alignment is kinda obsolete, at this moment aligning blocks to 8 bytes is sufficient

                // Always align the size of the block to 8 bytes
                size = CMath.AlignUp32(size, 8);
                mOffset = CMath.AlignUp32(mOffset, alignment);

                mDataBlocks.Add( new DataBlock()
                {
                    DataUnitReference = mDataUnitStack.Peek(),
                    Alignment = alignment,
                    Offset = mOffset,
                    Size = size,
                    Reference = reference
                });

                mReferenceToBlock.Add(reference, mDataBlocks.Count);

                mOffset += size;

                if (mOffset >= mDataWriter.Length)
                {
                    mDataWriter.Length = mOffset + 4 * 1024;
                }

                return mDataUnitStack.Peek();
            }

            public void OpenDataUnit(StreamReference reference)
            {
                mDataUnitStack.Push(reference);
            }

            public void CloseDataUnit()
            {
                Debug.Assert(mDataUnitStack.Count > 0);
                mDataUnitStack.Pop();
            }

            public void OpenBlock(StreamReference r)
            {
                var exists = mReferenceToBlock.TryGetValue(r, out var index);
                Debug.Assert(exists);
                Debug.Assert(mCurrent == -1);

                mCurrent = index;

                // Set the stream position to the start of the block
                var db = mDataBlocks[index];
                db.DataUnitReference = mDataUnitStack.Peek();
                mDataWriter.Position = db.Offset;
            }

            public void Mark(StreamReference reference)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Mark(mDataBlocks[mCurrent], reference, mDataWriter.Position);
            }

            public void CloseBlock()
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.End(mDataBlocks[mCurrent], mDataWriter);

                // Check if the position is within the bounds of this block
                Debug.Assert(mDataWriter.Position >= mDataBlocks[mCurrent].Offset && mDataWriter.Position <= (mDataBlocks[mCurrent].Offset + mDataBlocks[mCurrent].Size));

                mCurrent = -1;
            }

            public void Write(float v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(float v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(double v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(double v)
            {
                Align(8);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(sbyte v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(byte v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(short v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(short v)
            {
                Align(2);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(int v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(int v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(long v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(long v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(ushort v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(ushort v)
            {
                Align(2);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(uint v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(uint v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(ulong v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(ulong v)
            {
                Align(8);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(byte[] data, int index, int count)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, data, index, count);
            }

            public void Write(ReadOnlySpan<byte> span)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, span);
            }

            public void Write(string str)
            {
                Debug.Assert(mCurrent != -1);
                var idx = mStringTable.Add(str);
                var len = mStringTable.ByteCountForIndex(idx);
                var reference = mStringTable.StreamReferenceForIndex(idx);
                Write(reference, len);
            }

            public void WriteFileId(Hash160 signature)
            {
                // todo
            }

            public void WriteBlockReference(StreamReference v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.WriteBlockReference(mDataWriter, mDataBlocks[mCurrent], v);
            }

            public void WriteChunkReference(StreamReference v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.WriteDataUnitReference(mDataWriter, mDataBlocks[mCurrent], v);
            }

            public void Write(StreamReference v, long length)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.WriteBlockReference(mDataWriter, mDataBlocks[mCurrent], v);
                DataBlock.Write(mDataWriter, length);
            }

            public void Final(IBinaryStreamWriter dataWriter)
            {
            }

            private static BinaryMemoryBlock DeduplicateDataBlocks(MemoryStream memoryStream, Dictionary<StreamReference, DataBlock> blockDataBase)
            {
                // For all blocks that are part of this chunk:
                // Collapse identical blocks identified by hash, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the hash of a data block.

                var memoryBytes = memoryStream.ToArray();
                var memoryBlock = new BinaryMemoryBlock();
                memoryBlock.Setup(memoryBytes, 0, memoryBytes.Length);

                var duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                var dataHashDataBase = new Dictionary<Hash160, StreamReference>();

                var dataBlocks = new List<DataBlock>(blockDataBase.Count);
                foreach (var b in blockDataBase)
                    dataBlocks.Add(b.Value);

                while (true)
                {
                    duplicateDataBase.Clear();
                    dataHashDataBase.Clear();

                    foreach (var d in dataBlocks)
                    {
                        var hash = HashUtility.Compute(memoryBytes.AsSpan(d.Offset, d.Size));
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

            public void Finalize(IBinaryStreamWriter dataWriter, List<ulong> dataUnitStreamPositions, List<ulong> dataUnitStreamSizes)
            {
                // NEW, DataUnits!
                // A DataUnit consists of Blocks.
                // For each DataUnit we need to de-duplicate Blocks, and once we have done that we
                // can serialize DataUnits to the dataWriter.
                // Also every serialized DataUnit needs a Header structure, so that when it is loaded in
                // the Runtime we have some information, like where the first Pointer is located to start
                // patching them.
                var blockDataBases = new List<Dictionary<StreamReference, DataBlock>>();
                var dataUnitDataBase = new Dictionary<StreamReference, int>(mDataBlocks.Count);
                foreach (var d in mDataBlocks)
                {
                    Dictionary<StreamReference, DataBlock> blockDatabase;
                    if (!dataUnitDataBase.TryGetValue(d.DataUnitReference, out var blockDatabaseIndex))
                    {
                        blockDatabaseIndex = blockDataBases.Count;
                        blockDataBases.Add(new Dictionary<StreamReference, DataBlock>());
                        dataUnitDataBase.Add(d.DataUnitReference, blockDatabaseIndex);
                    }

                    blockDataBases[blockDatabaseIndex].Add(d.Reference, d);
                }

                var readWriteBuffer = new byte[65536];
                foreach (var cdb in dataUnitDataBase)
                {
                    // Store the start of the DataUnit
                    var dataUnitBeginPos = dataWriter.Position;
                    dataUnitStreamPositions.Add((ulong)dataUnitBeginPos);

                    // Dictionary for mapping a Reference to a DataBlock
                    var blockDataBase = blockDataBases[cdb.Value];

                    var memoryStream = DeduplicateDataBlocks(mMemoryStream, blockDataBase);

                    // Remember the location of the chunk header
                    var headerOffset = dataWriter.Position;
                    dataWriter.Write((uint)0); // Offset to first pointer
                    dataWriter.Write((uint)0); // Number of pointers

                    // Compute stream offset for each data block, do this by simulating the writing process.
                    // All references (pointers) are written in the stream as a 64-bit offset (64-bit pointers)
                    // relative to the start of the data stream.
                    var dataOffsetDataBase = new Dictionary<StreamReference, long>();
                    var offset = dataWriter.Position;
                    foreach (var (dbRef, db) in blockDataBase)
                    {
                        offset = CMath.AlignUp(offset, db.Alignment);
                        dataOffsetDataBase.Add(dbRef, offset);
                        DataBlock.ProcessStreamMarkers(db, offset, dataOffsetDataBase);
                        offset += db.Size;
                    }

                    // Dump all blocks to dataWriter
                    // Dump all reallocation info to relocationData
                    // Patch the location of every reference in the memory stream!
                    var streamPointers = new List<StreamPointer>();
                    foreach (var (_, db) in blockDataBase)
                    {
                        DataBlock.CollectStreamPointers(db, streamPointers, dataOffsetDataBase);
                    }

                    // Connect all StreamPointer into a Singly-LinkedList so that in the Runtime we
                    // only have to walk the list to patch the pointers, we do not need to load an
                    // additional file with pointer locations.
                    DataBlock.WriteStreamPointers(memoryStream, streamPointers);

                    foreach (var (_, db) in blockDataBase)
                    {
                        DataBlock.WriteDataBlock(db, memoryStream, dataWriter, dataOffsetDataBase, readWriteBuffer);
                    }

                    // Remember the current location
                    var dataUnitEndPos = dataWriter.Position;

                    // Change position to chunk header and finalize the header
                    dataWriter.Position = headerOffset;
                    dataWriter.Write((uint)(streamPointers[0].Position / 8));
                    dataWriter.Write((ulong)streamPointers.Count);

                    // Restore the position
                    dataWriter.Position = dataUnitEndPos;

                    // Align to N bytes
                    StreamUtils.Align(dataWriter, 256);

                    // Store the position and size of this DataUnit
                    dataUnitStreamSizes.Add((ulong)dataUnitEndPos - (ulong)dataUnitBeginPos);
                }
            }

            public IArchitecture Architecture => mDataWriter.Architecture;

            public long Position
            {
                get => mDataWriter.Position;
                set => mDataWriter.Position = value;
            }

            public long Length
            {
                get => mDataWriter.Length;
                set => mDataWriter.Length = value;
            }

            public long Seek(long offset)
            {
                return mDataWriter.Seek(offset);
            }

            public void Close()
            {
                mDataWriter.Close();
            }
        }
    }
}
