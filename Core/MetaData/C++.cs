using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace Core
{
    using MetaCode;

    /// <summary>
    /// CodeStream for generating C++ code
    /// </summary>
    public class CppCodeStream
    {
        #region Class and ClassMember data writer

        public class DataStreamWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
            private readonly CppDataStream mDataStream;

            public DataStreamWriter(StringTable stringTable, FileIdTable fileIdTable, CppDataStream dataStream)
            {
                mStringTable = stringTable;
                mFileIdTable = fileIdTable;
                mDataStream = dataStream;
            }

            public bool open()
            {
                mDataStream.beginBlock();
                return true; 
            }
            public bool close() 
            {
                mDataStream.endBlock();
                return true; 
            }

            public bool writeNullMember(NullMember c)
            {
                mDataStream.write(0);
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                mDataStream.write(c.boolean);
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                mDataStream.write(c.int8);
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                mDataStream.write(c.int16);
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                mDataStream.write(c.int32);
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                mDataStream.write(c.int64);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                mDataStream.write(c.uint8);
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                mDataStream.write(c.uint16);
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                mDataStream.write(c.uint32);
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                mDataStream.write(c.uint64);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                mDataStream.write(c.real);
                return true;
            }
            public bool writeStringMember(StringMember s)
            {
                mDataStream.write(mStringTable.ReferenceOf(s.str));
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                mDataStream.write(c.reference);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                mDataStream.write(c.members.Count);
                if (c.reference != StreamReference.Empty)
                {
                    mDataStream.write(c.reference);
                }
                else
                {
                    c.reference = mDataStream.beginBlock();
                    {
                        foreach (Member m in c.members)
                            m.write(this);
                    }
                    mDataStream.endBlock();
                    mDataStream.write(c.reference);
                }
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                if (c.reference != StreamReference.Empty)
                {
                    mDataStream.write(c.reference);
                }
                else
                {
                    c.reference = mDataStream.beginBlock();
                    {
                        foreach (Member m in c.members)
                            m.write(this);
                    }
                    mDataStream.endBlock();
                    mDataStream.write(c.reference);
                }
                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                if (c.isNullType)
                {
                    if (c.reference != StreamReference.Empty)
                    {
                        mDataStream.write(c.reference);
                    }
                    else
                    {
                        c.reference = mDataStream.beginBlock();
                        {
                            foreach (Member m in c.members)
                                m.write(this);
                        }
                        mDataStream.endBlock();
                        mDataStream.write(c.reference);
                    }
                }
                else
                {
                    foreach (Member m in c.members)
                        m.write(this);
                }
                return true;
            }
        }

        #endregion
        #region C++ Class Member Getter writer

        public class MemberGetterWriter : IMemberWriter
        {
            private StreamWriter mWriter;

            public MemberGetterWriter()
            {
                mWriter = null;
            }

            public MemberGetterWriter(StreamWriter writer)
            {
                mWriter = writer;
            }

            public void setWriter(StreamWriter writer)
            {
                mWriter = writer;
            }

            public bool open()
            {
                return true; 
            }

            public bool close() 
            {
                return true; 
            }

            public bool writeNullMember(NullMember c)
            {
                string line = "\tvoid*\tget" + c.name + "() const\t{ return 0; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                string line = "\tconst " + c.type.typeName + "&\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                string line = "\t" + c.type.typeName + "\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                string line = "\tconst " + c.type.typeName + "&\tget" + c.name + "() const\t{ return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                string line = "\tconst " + c.type.typeName + "*\tget" + c.name + "() const { return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                c.member.write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                string line = "\tconst " + c.type.typeName + "*\tget" + c.name + "() const { return m" + c.name + "; }";
                mWriter.WriteLine(line);
                return true;
            }
        }

        #endregion
        #region C++ Class Member writer

        public class MemberWriter : IMemberWriter
        {
            private StreamWriter mWriter;

            public MemberWriter()
            {
                mWriter = null;
            }

            public MemberWriter(StreamWriter writer)
            {
                mWriter = writer;
            }

            public void setWriter(StreamWriter writer)
            {
                mWriter = writer;
            }

            public bool open()
            {
                return true; 
            }

            public bool close() 
            {
                return true; 
            }

            public bool write(string type, string name, StreamWriter writer)
            {
                string line = "\t" + type + " m" + name + ";";
                writer.WriteLine(line);
                return true;
            }

            public bool writeNullMember(NullMember c)
            {
                return write("void*", c.name, mWriter);
            }
            public bool writeBool8Member(BoolMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeInt8Member(Int8Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeInt16Member(Int16Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeInt32Member(Int32Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeInt64Member(Int64Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeFloatMember(FloatMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeStringMember(StringMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeArrayMember(ArrayMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeObjectMember(ObjectMember c)
            {
                return write(c.type.typeName + "*", c.name, mWriter);
            }
            public bool writeAtomMember(AtomMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                return write(c.type.typeName, c.name, mWriter);
            }
        }

        #endregion
        #region C++ Code writer

        public class CppCodeWriter
        {
            private readonly MemberGetterWriter mMemberGetterWriter = new MemberGetterWriter();
            private readonly MemberWriter mMemberWriter = new MemberWriter();

            public bool write(ObjectMember c, StreamWriter writer)
            {
                mMemberGetterWriter.setWriter(writer);
                mMemberWriter.setWriter(writer);

                writer.WriteLine("class {0}", c.type.typeName);
                writer.WriteLine("{");

                // Member getters
                writer.WriteLine("public:");
                foreach (Member m in c.members)
                {
                    m.write(mMemberGetterWriter);
                }

                // Member data
                writer.WriteLine("private:");
                foreach (Member m in c.members)
                {
                    m.write(mMemberWriter);
                }

                writer.WriteLine("};");
                return true;
            }

            public void write(List<ObjectMember> classes, StreamWriter writer)
            {
                Dictionary<string, ObjectMember> writtenClasses = new Dictionary<string, ObjectMember>();

                foreach (ObjectMember c in classes)
                {
                    if (!writtenClasses.ContainsKey(c.type.typeName))
                    {
                        write(c, writer);
                        writtenClasses.Add(c.type.typeName, c);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// A CppDataStream is used to write DataBlocks, DataBlocks are stored and when
    /// the final data is written identical (MD5) DataBlocks are collapsed to one.
    /// All references (pointers to blocks) are also resolved at the final stage.
    /// 
    /// Output: a database of the offset of every reference (DataBlock) 
    /// 
    /// </summary>
    public class CppDataStream
    {
        #region DataBlock

        private class DataBlock
        {
            private static readonly MD5CryptoServiceProvider sMd5Service = new MD5CryptoServiceProvider();

            private StreamReference mReference;
            private readonly MemoryStream mDataStream = new MemoryStream();
            private readonly IBinaryWriter mDataWriter;
            private readonly MemoryStream mTypeInfoStream = new MemoryStream();
            private readonly IBinaryWriter mTypeInfoWriter;

            private readonly Dictionary<StreamReference, List<StreamOffset>> mPointers = new Dictionary<StreamReference, List<StreamOffset>>();

            private enum EDataType : uint
            {
                PRIMITIVE = 0x5052494D,
                REFERENCE = 0x43505452,
                ALIGNMENT = 0x414C4732,
            }

            internal DataBlock(EEndian inEndian)
            {
                mReference = StreamReference.Instance;
                mDataWriter = EndianUtils.CreateBinaryWriter(mDataStream, inEndian);
                mTypeInfoWriter = EndianUtils.CreateBinaryWriter(mTypeInfoStream, inEndian);
            }

            internal StreamReference reference
            {
                get 
                {
                    return mReference; 
                }
            }

            public void align(EStreamAlignment align)
            {
                StreamUtils.align(mDataWriter, align);
            }

            internal int position
            {
                get
                {
                    return (int)mDataStream.Position;
                }
            }

            internal int size
            {
                get
                {
                    return (int)mDataStream.Length;
                }
            }

            internal void write(float v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_32));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(SByte v)
            {
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(Int16 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_16));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(Int32 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_32));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(Int64 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_64));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(Byte v)
            {
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(UInt16 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_16));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(UInt32 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_32));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(UInt64 v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_64));
                mTypeInfoWriter.Write((uint)EDataType.PRIMITIVE);
                mDataWriter.Write(v);
            }
            internal void write(StreamReference v)
            {
                Debug.Assert(StreamUtils.aligned(mDataWriter, EStreamAlignment.ALIGN_32));

                if (mPointers.ContainsKey(v))
                {
                    mPointers[v].Add(new StreamOffset(position));
                }
                else
                {
                    List<StreamOffset> offsets = new List<StreamOffset>();
                    offsets.Add(new StreamOffset(position));
                    mPointers.Add(v, offsets);
                }

                mTypeInfoWriter.Write((int)EDataType.REFERENCE);
                mDataWriter.Write(0);
            }

            internal Hash128 computeMD5()
            {
                Hash128 dataHash = HashUtility.compute(mDataStream);
                return dataHash;
            }

            internal void replaceReference(StreamReference oldRef, StreamReference newRef)
            {
                if (mReference == oldRef)
                    mReference = newRef;

                if (mPointers.ContainsKey(oldRef))
                {
                    List<StreamOffset> oldOffsets = mPointers[oldRef];
                    mPointers.Remove(oldRef);

                    // Modify data
                    StreamOffset currentPos = new StreamOffset(mDataStream.Position);
                    foreach (StreamOffset o in oldOffsets)
                    {
                        mDataWriter.Seek(o);
                        mDataWriter.Write(0);
                    }
                    mDataWriter.Seek(currentPos);

                    // Update pointer and offsets
                    if (mPointers.ContainsKey(newRef))
                    {
                        List<StreamOffset> newOffsets = mPointers[newRef];
                        foreach (StreamOffset o in oldOffsets)
                            newOffsets.Add(o);
                    }
                    else
                    {
                        mPointers.Add(newRef, oldOffsets);
                    }
                }
            }

            internal void writeTo(IBinaryWriter outData, IBinaryWriter outReallocationTable, IDictionary<StreamReference, StreamOffset> dataOffsetDataBase, IDictionary<StreamReference, int> referenceOffsetDataBase)
            {
                StreamOffset currentPos = new StreamOffset(mDataStream.Position);
                foreach (KeyValuePair<StreamReference, List<StreamOffset>> k in mPointers)
                {
                    StreamOffset outDataOffset;
                    if (dataOffsetDataBase.TryGetValue(k.Key, out outDataOffset))
                    {
                        foreach (StreamOffset o in k.Value)
                        {
                            mDataWriter.Seek(o);
                            mDataWriter.Write(outDataOffset.value32);
                        }
                    }
                }
                mDataWriter.Seek(currentPos);

                // Write data
                outData.Write(mDataStream.GetBuffer());

                StreamOffset currentOffset;
                dataOffsetDataBase.TryGetValue(reference, out currentOffset);

                // Write reallocation info
                foreach (KeyValuePair<StreamReference, List<StreamOffset>> k in mPointers)
                {
                    foreach (StreamOffset o in k.Value)
                    {
                        outReallocationTable.Write(currentOffset.value32 + o.value32);
                    }
                }
            }
        }

        #endregion

        #region Fields

        private readonly List<DataBlock> mData = new List<DataBlock>();
        private readonly Stack<DataBlock> mStack = new Stack<DataBlock>();

        private EEndian mEndian = EEndian.LITTLE;
        private DataBlock mCurrent = null;

        #endregion
        #region Constructor

        public CppDataStream(EEndian endian)
        {
            mEndian = endian;
        }

        #endregion
        #region Methods

        // Return data block index
        public StreamReference beginBlock()
        {
            if (mCurrent!=null)
                mStack.Push(mCurrent);

            mCurrent = new DataBlock(mEndian);
            mData.Add(mCurrent);
            return mCurrent.reference;
        }

        public void write(float v)
        {
            mCurrent.write(v);
        }
        public void write(sbyte v)
        {
            mCurrent.write(v);
        }
        public void write(short v)
        {
            mCurrent.write(v);
        }
        public void write(int v)
        {
            mCurrent.write(v);
        }
        public void write(Int64 v)
        {
            mCurrent.write(v);
        }
        public void write(byte v)
        {
            mCurrent.write(v);
        }
        public void write(ushort v)
        {
            mCurrent.write(v);
        }
        public void write(uint v)
        {
            mCurrent.write(v);
        }
        public void write(UInt64 v)
        {
            mCurrent.write(v);
        }
        public void write(StreamReference v)
        {
            mCurrent.write(v);
        }
        public void endBlock()
        {
            if (mStack.Count>0)
                mCurrent = mStack.Pop();
            else
                mCurrent = null;
        }

        public void finalize(IBinaryWriter dataWriter, IBinaryWriter relocWriter, out Dictionary<StreamReference, int> referenceOffsetDatabase)
        {
            // Dictionary for mapping a Reference object to a Data object
            Dictionary<StreamReference, DataBlock> dataDataBase = new Dictionary<StreamReference, DataBlock>();
            foreach (DataBlock d in mData)
                dataDataBase.Add(d.reference, d);

            Dictionary<StreamReference, DataBlock> finalDataDataBase = new Dictionary<StreamReference, DataBlock>();
            foreach (DataBlock d in mData)
                finalDataDataBase.Add(d.reference, d);

            // For all blocks, calculate an MD5
            // Collapse identical blocks, and when a collapse has occurred we have
            // to re-iterate again since a collapse changes the MD5 of a block.
            bool collapse = true;
            while (collapse)
            {
                Dictionary<StreamReference, List<StreamReference>> duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                Dictionary<Hash128, StreamReference> dataMD5DataBase = new Dictionary<Hash128, StreamReference>();

                foreach (DataBlock d in mData)
                {
                    Hash128 md5 = d.computeMD5();
                    if (dataMD5DataBase.ContainsKey(md5))
                    {
                        // Encountering a block of data which has a duplicate.
                        // After the first iteration it might be the case that
                        // they have the same 'Reference' since they are collapsed.
                        StreamReference newRef = dataMD5DataBase[md5];
                        if (d.reference != newRef)
                        {
                            if (!duplicateDataBase.ContainsKey(newRef))
                            {
                                if (finalDataDataBase.ContainsKey(d.reference))
                                {
                                    List<StreamReference> duplicateReferences = new List<StreamReference>();
                                    duplicateReferences.Add(d.reference);
                                    duplicateDataBase[newRef] = duplicateReferences;
                                }
                            }
                            else
                            {
                                if (finalDataDataBase.ContainsKey(d.reference))
                                    duplicateDataBase[newRef].Add(d.reference);
                            }
                            finalDataDataBase.Remove(d.reference);
                        }
                    }
                    else
                    {
                        // This block of data is unique
                        dataMD5DataBase.Add(md5, d.reference);
                    }
                }

                foreach (KeyValuePair<StreamReference, List<StreamReference>> p in duplicateDataBase)
                {
                    foreach (StreamReference r in p.Value)
                    {
                        foreach (DataBlock d in mData)
                        {
                            d.replaceReference(r, p.Key);
                        }
                    }
                }

                // Did we find any duplicates, if so then we also replaced references
                // and by doing so MD5 hashes have changed.
                // Some blocks now might have an identical MD5 due to this.
                collapse = duplicateDataBase.Count > 0;
            }

            // Resolve block references again
            Dictionary<StreamReference, StreamOffset> dataOffsetDataBase = new Dictionary<StreamReference, StreamOffset>();

            int offset = 0;
            foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataDataBase)
            {
                dataOffsetDataBase.Add(k.Key, new StreamOffset(offset));
                offset += k.Value.size;
            }

            // Dump all blocks to outData
            // Dump all reallocation info to outReallocationTable
            // Remember the location of every reference in the memory stream!
            referenceOffsetDatabase = new Dictionary<StreamReference, int>();
            foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataDataBase)
            {
                k.Value.writeTo(dataWriter, relocWriter, dataOffsetDataBase, referenceOffsetDatabase);
            }
        }

        #endregion
    }
	
}
