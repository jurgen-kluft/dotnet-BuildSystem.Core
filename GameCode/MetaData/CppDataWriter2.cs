using System.Diagnostics;
using System.Text;
using GameCore;
using BinaryWriter = GameCore.BinaryWriter;

namespace GameData
{
    namespace MetaCode
    {
        public static class CppDataWriter2
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
                public int DataUnitIndex { get; init; }
            }

            private class WriteContext
            {
                public MetaCode2 MetaCode2 { get; init; }
                public IReadOnlySignatureDataBase SignatureDataBase { get; init; }
                public CppDataStream2 GameDataStream { get; init; }
                public WriteMemberDelegate[] WriteMemberDelegates { get; init; }
                public Queue<ProcessObject> ProcessObjectQueue { get; init; }
                public Queue<ProcessDataUnit> ProcessDataUnitQueue { get; init; }
            }

            public static void Write(MetaCode2 metaCode2, string rootSignature, IReadOnlySignatureDataBase signatureDb, CppDataStream2 dataStream)
            {
                var ctx = new WriteContext
                {
                    MetaCode2 = metaCode2,
                    SignatureDataBase = signatureDb,
                    GameDataStream = dataStream,
                    ProcessObjectQueue = new Queue<ProcessObject>(),
                    ProcessDataUnitQueue = new Queue<ProcessDataUnit>(),
                    WriteMemberDelegates = new WriteMemberDelegate[MetaInfo.Count],
                };

                ctx.WriteMemberDelegates[MetaInfo.s_unknown.Index] = null;
                ctx.WriteMemberDelegates[MetaInfo.s_bool.Index] = WriteBool;
                ctx.WriteMemberDelegates[MetaInfo.s_bitset.Index] = WriteBitset;
                ctx.WriteMemberDelegates[MetaInfo.s_int8.Index] = WriteInt8;
                ctx.WriteMemberDelegates[MetaInfo.s_uint8.Index] = WriteUInt8;
                ctx.WriteMemberDelegates[MetaInfo.s_int16.Index] = WriteInt16;
                ctx.WriteMemberDelegates[MetaInfo.s_uint16.Index] = WriteUInt16;
                ctx.WriteMemberDelegates[MetaInfo.s_int32.Index] = WriteInt32;
                ctx.WriteMemberDelegates[MetaInfo.s_uint32.Index] = WriteUInt32;
                ctx.WriteMemberDelegates[MetaInfo.s_int64.Index] = WriteInt64;
                ctx.WriteMemberDelegates[MetaInfo.s_uint64.Index] = WriteUInt64;
                ctx.WriteMemberDelegates[MetaInfo.s_float.Index] = WriteFloat;
                ctx.WriteMemberDelegates[MetaInfo.s_double.Index] = WriteDouble;
                ctx.WriteMemberDelegates[MetaInfo.s_string.Index] = WriteString;
                ctx.WriteMemberDelegates[MetaInfo.s_enum.Index] = WriteEnum;
                ctx.WriteMemberDelegates[MetaInfo.s_struct.Index] = WriteStruct;
                ctx.WriteMemberDelegates[MetaInfo.s_class.Index] = WriteClass;
                ctx.WriteMemberDelegates[MetaInfo.s_array.Index] = WriteArray;
                ctx.WriteMemberDelegates[MetaInfo.s_dictionary.Index] = WriteDictionary;
                ctx.WriteMemberDelegates[MetaInfo.s_dataUnit.Index] = WriteDataUnit;

                try
                {
                    // Write the data of the MetaCode to the DataStream
                    var rootHashSignature = HashUtility.Compute_ASCII(rootSignature);
                    var rootDataUnitIndex = ctx.GameDataStream.GetDataUnitIndex(rootHashSignature);
                    ctx.ProcessDataUnitQueue.Enqueue(new ProcessDataUnit() { MemberIndex = 0, DataUnitIndex = rootDataUnitIndex });

                    // Whenever we encounter a DataUnit we need to write the data of the DataUnit to the DataStream
                    var writtenReferences = new HashSet<StreamReference>();
                    while (ctx.ProcessDataUnitQueue.Count > 0)
                    {
                        var du = ctx.ProcessDataUnitQueue.Dequeue();

                        ctx.GameDataStream.OpenDataUnit(du.DataUnitIndex);
                        {
                            var mr = StreamReference.NewReference;
                            ctx.GameDataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(du.MemberIndex));
                            ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = du.MemberIndex, Process = WriteClassDataProcess, BlockReference = mr});

                            writtenReferences.Clear();
                            while (ctx.ProcessObjectQueue.Count > 0)
                            {
                                var wp = ctx.ProcessObjectQueue.Dequeue();
                                if (writtenReferences.Add(wp.BlockReference))
                                {
                                    wp.Process(wp.MemberIndex, wp.BlockReference, ctx);
                                }
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
                BinaryWriter.Write(ctx.GameDataStream, (bool)member ? (sbyte)1 : (sbyte)0);
            }

            private static void WriteBitset(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (byte)member);
            }

            private static void WriteInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (sbyte)member);
            }

            private static void WriteUInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (byte)member);
            }

            private static void WriteInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (short)member);
            }

            private static void WriteUInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (ushort)member);
            }

            private static void WriteInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (int)member);
            }

            private static void WriteUInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (uint)member);
            }

            private static void WriteInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (long)member);
            }

            private static void WriteUInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream,(ulong)member);
            }

            private static void WriteFloat(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (float)member);
            }

            private static void WriteDouble(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                BinaryWriter.Write(ctx.GameDataStream, (double)member);
            }

            private static void WriteStringDataProcess(int memberIndex, StreamReference br, WriteContext ctx)
            {
                // A string is written as an array of UTF8 bytes
                if (ctx.MetaCode2.MembersObject[memberIndex] is not string str) return;

                ctx.GameDataStream.OpenBlock(br);
                {
                    var ms = ctx.MetaCode2.MembersCount[memberIndex];
                    var bytes = new byte[ms];
                    var bl = s_utf8Encoding.GetBytes(str, 0, str.Length, bytes, 0);
                    BinaryWriter.Write(ctx.GameDataStream, bytes, 0, bl);
                }
                ctx.GameDataStream.CloseBlock();
            }

            private static readonly UTF8Encoding s_utf8Encoding = new ();

            private static void WriteString(int memberIndex, WriteContext ctx)
            {
                if (ctx.MetaCode2.MembersObject[memberIndex] is not string str) return;

                var bl = s_utf8Encoding.GetByteCount(str);
                var rl = str.Length;
                var ms = CMath.AlignUp32(bl + 1, 8);
                ctx.MetaCode2.MembersCount[memberIndex] = ms;

                var br = StreamReference.NewReference;
                ctx.GameDataStream.NewBlock(br, ctx.MetaCode2.GetDataAlignment(memberIndex));

                BinaryWriter.Write(ctx.GameDataStream, bl); // Length in bytes
                BinaryWriter.Write(ctx.GameDataStream, rl); // Length in runes
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
                                BinaryWriter.Write(ctx.GameDataStream, (sbyte)member);
                                break;
                            case false:
                                BinaryWriter.Write(ctx.GameDataStream, (byte)member);
                                break;
                        }
                        break;
                    case 2:
                        switch (fet.IsSigned)
                        {
                            case true:
                                BinaryWriter.Write(ctx.GameDataStream, (short)member);
                                break;
                            case false:
                                BinaryWriter.Write(ctx.GameDataStream, (ushort)member);
                                break;
                        }
                        break;
                    case 4:
                        switch (fet.IsSigned)
                        {
                            case true:
                                BinaryWriter.Write(ctx.GameDataStream, (int)member);
                                break;
                            case false:
                                BinaryWriter.Write(ctx.GameDataStream, (uint)member);
                                break;
                        }
                        break;
                    case 8:
                        switch (fet.IsSigned)
                        {
                            case true:
                                BinaryWriter.Write(ctx.GameDataStream, (long)member);
                                break;
                            case false:
                                BinaryWriter.Write(ctx.GameDataStream, (ulong)member);
                                break;
                        }
                        break;
                }
            }

            private static void WriteStruct(int memberIndex, WriteContext ctx)
            {
                if (ctx.MetaCode2.MembersObject[memberIndex] is IStruct mx)
                {
                    BinaryWriter.Align(ctx.GameDataStream, mx.StructAlign);
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

                ctx.GameDataStream.WriteDataBlockReference(mr);
                BinaryWriter.Write(ctx.GameDataStream, count);
                BinaryWriter.Write(ctx.GameDataStream, count);

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

                ctx.GameDataStream.WriteDataBlockReference(mr);
                BinaryWriter.Write(ctx.GameDataStream, count);
                BinaryWriter.Write(ctx.GameDataStream, count);

                // We need to schedule this array to be written
                ctx.ProcessObjectQueue.Enqueue(new ProcessObject() { MemberIndex = memberIndex, Process = WriteDictionaryDataProcess, BlockReference = mr});
            }

            private static void WriteDataUnit(int memberIndex, WriteContext ctx)
            {
                if (ctx.MetaCode2.MembersObject[memberIndex] is not IDataUnit dataUnit) return;

                var signature = HashUtility.Compute_ASCII(dataUnit.Signature);
                (uint bigfileIndex, uint fileIndex) = ctx.SignatureDataBase.GetEntry(signature);

                // fileid
                BinaryWriter.Write(ctx.GameDataStream, bigfileIndex); // (4 bytes)
                BinaryWriter.Write(ctx.GameDataStream, fileIndex); // (4 bytes)

                var dataUnitIndex = ctx.GameDataStream.GetDataUnitIndex(signature);

                // The 'class' member of this DataUnit is the (only) member
                var dataUnitClassMemberIndex = ctx.MetaCode2.MembersStart[memberIndex];

                ctx.ProcessDataUnitQueue.Enqueue(new ProcessDataUnit() { MemberIndex = dataUnitClassMemberIndex, DataUnitIndex = dataUnitIndex });
            }
        }
    }
}
