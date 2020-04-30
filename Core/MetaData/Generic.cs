using System;
using System.Collections.Generic;

namespace Core
{
    using MetaCode;

    /// <summary>
    /// StdStream representing a standard object data tree
    /// </summary>
    public class StdDataStream
    {
        #region Fields

        private static int sSizeOfBool = 1;
        private EEndian mEndian = EEndian.LITTLE;

        #endregion
        #region Constructor

        public StdDataStream(EEndian endian)
        {
            mEndian = endian;
        }

        #endregion
        #region Properties

        public static int SizeOfBool
        {
            set
            {
                sSizeOfBool = value;
            }
            get
            {
                return sSizeOfBool;
            }
        }

        #endregion

        #region Unresolved References

        public class UnresolvedReferencesLogger : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly Dictionary<StreamReference, StreamContext> mUnresolvedReferences;
            private readonly List<Member> mHierarchy = new List<Member>();

            public UnresolvedReferencesLogger(StringTable stringTable, Dictionary<StreamReference, StreamContext> unresolvedReferences)
            {
                mStringTable = stringTable; 
                mUnresolvedReferences = unresolvedReferences;
            }

            public bool open()
            {
                return true;
            }

            public bool close()
            {
                return true;
            }

            public void log(string memberName, string typeName)
            {
                string parentStr = string.Empty;

                foreach (Member m in mHierarchy)
                    parentStr += m.type.typeName + "{" + m.name + "}.";

                Console.WriteLine(parentStr + typeName + "{" + memberName + "} which was not resolved");
            }

            public bool writeNullMember(NullMember c)
            {
                // Has no reference
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                // Has no reference
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                // Has no reference
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                // Has no reference
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(mStringTable.ReferenceOf(c.str), out ctx))
                    log(c.name, c.type.typeName);
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                // Has no reference
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.reference, out ctx))
                    log(c.name, c.type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.reference, out ctx))
                    log(c.name, c.type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                // Has no reference, is a system type (bool, int, float)
                c.member.write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.reference, out ctx))
                    log(c.name, c.type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
        }

        #endregion
        #region Object member data validation

        public class MemberDataValidator : IMemberWriter
        {
            private readonly List<Member> mHierarchy = new List<Member>();

            public bool open()
            {
                return true;
            }

            public bool close()
            {
                return true;
            }

            public bool logOutOfRange(string memberName, object v, string errorStr)
            {
                string parentStr = string.Empty;
                foreach (Member m in mHierarchy)
                    parentStr += "[" + m.name + "].";

                Console.WriteLine(parentStr + "[" + memberName + "] " + errorStr, v);
                return false;
            }

            public bool writeNullMember(NullMember c)
            {
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int8, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int16, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int32, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int64, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint8, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint16, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint32, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint64, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.real, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                mHierarchy.Add(c);
                foreach (Member m in c.members)
                {
                    //m.range = c.range;
                    m.write(this);
                }
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                //c.member.range = c.range;
                c.member.write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
        }

        #endregion

        #region StringTable writer

        public class StringTableWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;

            private bool mNullMemberDone;
            private bool mBool8MemberDone;
            private bool mInt8MemberDone;
            private bool mInt16MemberDone;
            private bool mInt32MemberDone;
            private bool mInt64MemberDone;
            private bool mUInt8MemberDone;
            private bool mUInt16MemberDone;
            private bool mUInt32MemberDone;
            private bool mUInt64MemberDone;
            private bool mFloatMemberDone;
            private bool mStringMemberDone;
            private bool mFileIdMemberDone;

            public StringTableWriter(StringTable strTable)
            {
                mStringTable = strTable;
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
                if (!mNullMemberDone)
                {
                    mNullMemberDone = true;
                    mStringTable.Add("void");
                    mStringTable.Add("void[]");
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                if (!mBool8MemberDone)
                {
                    mBool8MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                if (!mInt8MemberDone)
                {
                    mInt8MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                if (!mInt16MemberDone)
                {
                    mInt16MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                if (!mInt32MemberDone)
                {
                    mInt32MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                if (!mInt64MemberDone)
                {
                    mInt64MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                if (!mUInt8MemberDone)
                {
                    mUInt8MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                if (!mUInt16MemberDone)
                {
                    mUInt16MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                if (!mUInt32MemberDone)
                {
                    mUInt32MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                if (!mUInt64MemberDone)
                {
                    mUInt64MemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                if (!mFloatMemberDone)
                {
                    mFloatMemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                if (!mStringMemberDone)
                {
                    mStringMemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                mStringTable.Add(c.str);
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                if (!mFileIdMemberDone)
                {
                    mFileIdMemberDone = true;
                    mStringTable.Add(c.type.typeName);
                }
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                foreach (Member m in c.members)
                    m.write(this);
                mStringTable.Add(c.type.typeName);
                mStringTable.Add(c.name.ToLower());
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                foreach (Member m in c.members)
                    m.write(this);

                mStringTable.Add(c.type.typeName);
                mStringTable.Add(c.name.ToLower());

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                mStringTable.Add(c.type.typeName);
                mStringTable.Add(c.name.ToLower());
                c.member.write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                mStringTable.Add(c.type.typeName);
                mStringTable.Add(c.name.ToLower());

                foreach (Member m in c.members)
                    m.write(this);

                return true;
            }
        }

        #endregion

        #region Object member data writer

        public class ObjectMemberDataWriter : IMemberWriter
        {
            private bool mAlignMembers;
            private StringTable mStringTable;
            private FileIdTable mFileIdTable;
            private IDataWriter mWriter;

            public ObjectMemberDataWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;
                mFileIdTable = fileIdTable;
                mWriter = writer;
            }

            public bool alignMembers
            {
                get
                {
                    return mAlignMembers;
                }
                set
                {
                    mAlignMembers = value;
                }
            }

            public bool open() 
            {
                return mStringTable != null && mWriter != null;
            }
            
            public bool close()
            {
                mStringTable = null;
                mWriter = null;
                return true; 
            }

            private void writeReference(StreamReference reference)
            {
                if (mAlignMembers) align(EStreamAlignment.ALIGN_32);
                mWriter.Write(reference);
            }

            private void align(EStreamAlignment alignment)
            {
                StreamUtils.align(mWriter, alignment);
            }

            public bool writeNullMember(NullMember c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write((int)0);
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write((int)c.boolean);
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.int8);
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.int16);
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.int32);
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.int64);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.uint8);
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.uint16);
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.uint32);
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.uint64);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.real);
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                if (mAlignMembers) align(c.alignment);
                writeReference(mStringTable.ReferenceOf(c.str));
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                if (mAlignMembers) align(c.alignment);
                StreamReference reference = mFileIdTable.Add(c.reference, c.id);
                mWriter.Write(reference);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                if (mAlignMembers) align(c.alignment);
                mWriter.Write(c.members.Count);
                mWriter.Write(c.reference);
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                writeReference(c.reference);
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
                    writeReference(c.reference);
                }
                else
                {
                    if (mAlignMembers) align(c.alignment);
                    foreach (Member m in c.members)
                        m.write(this);
                }
                return true;

            }
        }

        #endregion
        #region Object member reference data writer

        /// <summary>
        /// This writer is responsible for writing referenced data like:
        /// 
        /// - Array
        /// - Class
        /// - Compound
        /// - String
        /// - Int64, UInt64
        /// 
        /// </summary>
        public class ObjectMemberReferenceDataWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
            private readonly IDataWriter mWriter;
            private readonly ObjectMemberDataWriter mObjectMemberDataWriter;

            public ObjectMemberReferenceDataWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;
                mFileIdTable = fileIdTable;

                mWriter = writer;
                mObjectMemberDataWriter = new ObjectMemberDataWriter(mStringTable, mFileIdTable, mWriter);
                mObjectMemberDataWriter.alignMembers = true;
            }

            public bool open()
            {
                return mStringTable!=null && mWriter!=null;
            }

            public bool close()
            {
                return true;
            }

            public bool writeNullMember(NullMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                // Written by the StringTable
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                // The reference of this member can be null!
                if (c.reference!=StreamReference.Empty && mWriter.BeginBlock(c.reference, c.alignment))
                {
                    foreach (Member m in c.members)
                    {
                        m.write(mObjectMemberDataWriter);
                    }
                    mWriter.EndBlock();
                }

                // Write the elements of the array again for type 'Reference'
                foreach (Member m in c.members)
                    m.write(this);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                foreach (Member m in c.members)
                    m.write(this);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                if (c.reference != StreamReference.Empty && mWriter.BeginBlock(c.reference, EStreamAlignment.ALIGN_32))
                {
                    foreach (Member m in c.members)
                        m.write(mObjectMemberDataWriter);
                    mWriter.EndBlock();
                }

                // Let the 'Reference' types write them selfs
                foreach (Member m in c.members)
                    m.write(this);

                return true;
            }
        }

        #endregion

        #region Object member offset or value writer

        public class ObjectMemberOffsetOrValueWriter : IMemberWriter
        {
            private StringTable mStringTable;
            private FileIdTable mFileIdTable;
            private IDataWriter mWriter;

            public ObjectMemberOffsetOrValueWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;
                mFileIdTable = fileIdTable;
                mWriter = writer;
            }

            public bool open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool close()
            {
                mStringTable = null;
                mWriter = null;
                return true;
            }

            /// Member ->
            ///     short mValueType;
            ///     short mStringIndex;
            ///     int mOffsetOrValue;

            private bool writeReferenceMember(StreamReference reference)
            {
                mWriter.Write(reference);
                return true;
            }

            public bool writeInt32(int v)
            {
                mWriter.Write(v);
                return true;
            }

            public bool writeNullMember(NullMember c)
            {
                return writeInt32(0);
            }
            public bool writeBool8Member(BoolMember c)
            {
                return writeInt32(c.boolean);
            }
            public bool writeInt8Member(Int8Member c)
            {
                return writeInt32(c.int8);
            }
            public bool writeInt16Member(Int16Member c)
            {
                return writeInt32(c.int16);
            }
            public bool writeInt32Member(Int32Member c)
            {
                return writeInt32(c.int32);
            }
            public bool writeInt64Member(Int64Member c)
            {
                mWriter.Write(c.int64);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                return writeInt32(c.uint8);
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                return writeInt32(c.uint16);
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                return writeInt32((int)c.uint32);
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                mWriter.Write(c.uint64);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                mWriter.Write(c.real);
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                return writeReferenceMember(mStringTable.ReferenceOf(c.str));
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                StreamReference reference = mFileIdTable.Add(c.reference, c.id);
                mWriter.Write(reference);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                mWriter.Write(c.members.Count);
                mWriter.Write(c.reference);
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                return writeReferenceMember(c.reference);
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                return writeReferenceMember(c.reference);
            }
        }

        #endregion
        #region Object member writer

        public class ObjectMemberWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
            private readonly IDataWriter mWriter;

            private readonly ObjectMemberOffsetOrValueWriter mOffsetOrValueWriter;

            public ObjectMemberWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;
                mFileIdTable = fileIdTable;
                mWriter = writer;
                mOffsetOrValueWriter = new ObjectMemberOffsetOrValueWriter(stringTable, fileIdTable, writer);
            }

            public bool open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool close()
            {
                return true;
            }

            /// short[] MemberOffset
            /// Member ->
            ///     int   mTypeHash;                    // Hash of type name (bool, bool[], int[], float, LString)
            ///     int   mNameHash;                    // Hash of member name
            ///     var   mValue or mOffset;

            private bool writeMember(string typeName, string memberName, Member m)
            {
                UInt32 typeNameHash = mStringTable.HashOf(typeName);
                UInt32 memberNameHash = mStringTable.HashOf(memberName.ToLower());
                if (typeNameHash == UInt32.MaxValue || memberNameHash == UInt32.MaxValue)
                {
                    Console.WriteLine("Error: StringTable is missing some strings!");
                }

                mWriter.Write(typeNameHash);
                mWriter.Write(memberNameHash);
                return m.write(mOffsetOrValueWriter);
            }

            public bool writeNullMember(NullMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeBool8Member(BoolMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeInt8Member(Int8Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeInt16Member(Int16Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeInt32Member(Int32Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeInt64Member(Int64Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeFloatMember(FloatMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeStringMember(StringMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeArrayMember(ArrayMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeObjectMember(ObjectMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeAtomMember(AtomMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                return writeMember(c.type.typeName, c.name, c);
            }
        }

        #endregion
        #region Object writer

        public class ObjectWriter
        {
            private readonly EGenericFormat mDataFormat;
            private readonly StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
            private readonly IDataWriter mWriter;

            private readonly ObjectMemberWriter mObjectMemberWriter;

            public ObjectWriter(EGenericFormat inDataFormat, StringTable inStringTable, FileIdTable inFileIdTable, IDataWriter inWriter)
            {
                mDataFormat = inDataFormat;
                mStringTable = inStringTable;
                mFileIdTable = inFileIdTable;
                mWriter = inWriter;
                mObjectMemberWriter = new ObjectMemberWriter(mStringTable, mFileIdTable, mWriter);
            }

            public bool write(ObjectMember c)
            {
                /// Class -> 
                ///     {SResource*  base;}
                ///     int         count;          // (=members.Count)
                ///     short[]     memberOffsets;
                ///     Member[]    members;        // <- base of the offset is the start of the first member

                /// Write every 'SResource' in a new DataBlock so that
                /// duplicate classes can be collapsed. 
                if (c.reference!=StreamReference.Empty && mWriter.BeginBlock(c.reference, c.alignment))
                {
                    if (mDataFormat != EGenericFormat.STD_FLAT)
                    {
                        if (c.baseClass != null)
                            mWriter.Write(c.baseClass.reference);
                        else
                            mWriter.Write(StreamReference.Empty);
                    }

                    // Here: Sort members by hash of their name, so that in C++ we can do a 
                    //       binary search to find the member by name/hash.
                    c.sortMembers(new MemberNameHashComparer(mStringTable));

                    mWriter.Write(c.members.Count);

                    // Offset to members array
                    Int64 offsetsPos = mWriter.Position;
                    foreach (Member m in c.members)
                        mWriter.Write((short)0);
                    StreamUtils.align(mWriter, EStreamAlignment.ALIGN_64);

                    List<short> memberOffsets = new List<short>(c.members.Count);
                    foreach (Member m in c.members)
                    {
                        StreamUtils.align(mWriter, m.alignment);
                        memberOffsets.Add((short)(mWriter.Position - offsetsPos));
                        m.write(mObjectMemberWriter);
                    }

                    // Seek back to the member offset array and write in the obtained
                    // member offsets and when done seek back to the end position.
                    Int64 endPos = mWriter.Position;
                    {
                        mWriter.Seek(new StreamOffset(offsetsPos));
                        foreach (short offset in memberOffsets)
                            mWriter.Write(offset);
                    }
                    mWriter.Seek(new StreamOffset(endPos));

                    mWriter.EndBlock();
                }

                if (c.baseClass != null)
                    write(c.baseClass);

                return true;
            }

        }

        #endregion

        #region MemberBook

        class MyMemberBook : MemberBook
        {
            public void HandoutReferences()
            {
                // Handout StreamReferences to int64s, uint64s classes, compounds and arrays taking care of equality of these objects.
                // Note: Strings are a bit of a special case since we also will collect the names of members and classes.
                Dictionary<Int64, StreamReference> referencesForInt64Dict = new Dictionary<Int64, StreamReference>();
                foreach (Int64Member i in int64s)
                {
                    StreamReference reference;
                    if (referencesForInt64Dict.TryGetValue(i.int64, out reference))
                    {
                        i.reference = reference;
                    }
                    else
                    {
                        i.reference = StreamReference.Instance;
                        referencesForInt64Dict.Add(i.int64, i.reference);
                    }
                }

                Dictionary<UInt64, StreamReference> referencesForUInt64Dict = new Dictionary<UInt64, StreamReference>();
                foreach (UInt64Member i in uint64s)
                {
                    StreamReference reference;
                    if (referencesForUInt64Dict.TryGetValue(i.uint64, out reference))
                    {
                        i.reference = reference;
                    }
                    else
                    {
                        i.reference = StreamReference.Instance;
                        referencesForUInt64Dict.Add(i.uint64, i.reference);
                    }
                }

                Dictionary<object, ObjectMember> referencesForClassesDict = new Dictionary<object, ObjectMember>();
                foreach (ObjectMember c in classes)
                {
                    ObjectMember i = c;

                    if (i.IsDefault || i.value == null)
                    {
                        i.reference = StreamReference.Empty;

                        i = i.baseClass;
                        while (i != null)
                        {
                            i.reference = StreamReference.Empty;
                            i = i.baseClass;
                        }
                    }
                    else
                    {
                        ObjectMember r;
                        if (referencesForClassesDict.TryGetValue(i.value, out r))
                        {
                            i.reference = r.reference;
                        }
                        else
                        {
                            i.reference = StreamReference.Instance;
                            referencesForClassesDict.Add(i.value, c);
                        }
                        // Do base classes
                        r = (r != null) ? r.baseClass : null;
                        i = i.baseClass;
                        while (i != null)
                        {
                            i.reference = (r != null) ? r.reference : StreamReference.Instance;
                            i = i.baseClass;
                            r = (r != null) ? r.baseClass : null;
                        }
                    }
                }

                Dictionary<object, StreamReference> referencesForCompoundsDict = new Dictionary<object, StreamReference>();
                foreach (CompoundMember c in compounds)
                {
                    if (c.isNullType)
                    {
                        if (c.value != null)
                        {
                            StreamReference reference;
                            if (referencesForCompoundsDict.TryGetValue(c.value, out reference))
                            {
                                c.reference = reference;
                            }
                            else
                            {
                                c.reference = StreamReference.Instance;
                                referencesForCompoundsDict.Add(c.value, c.reference);
                            }
                        }
                        else
                        {
                            c.reference = StreamReference.Empty;
                        }
                    }
                    else
                    {
                        c.reference = StreamReference.Instance;
                    }
                }

                foreach (AtomMember c in atoms)
                {
                }

                Dictionary<Hash128, StreamReference> referencesForFileIdDict = new Dictionary<Hash128, StreamReference>();
                foreach (FileIdMember c in fileids)
                {
                    StreamReference reference;
                    if (referencesForFileIdDict.TryGetValue(c.id, out reference))
                    {
                        c.reference = reference;
                    }
                    else
                    {
                        c.reference = StreamReference.Instance;
                        referencesForFileIdDict.Add(c.id, c.reference);
                    }
                }

                Dictionary<object, StreamReference> referencesForArraysDict = new Dictionary<object, StreamReference>();
                foreach (ArrayMember a in arrays)
                {
                    if (a.IsDefault || a.value == null)
                    {
                        a.reference = StreamReference.Empty;
                    }
                    else
                    {
                        StreamReference reference;
                        if (referencesForArraysDict.TryGetValue(a.value, out reference))
                        {
                            a.reference = reference;
                        }
                        else
                        {
                            a.reference = StreamReference.Instance;
                            referencesForArraysDict.Add(a.value, a.reference);
                        }
                    }
                }
            }
        }

        #endregion

        #region Generic MemberGenerator

        public class GenericMemberGenerator : IMemberGenerator
        {
            #region Fields

            private EGenericFormat mDataFormat;

            #endregion
            #region Constructor

            public GenericMemberGenerator(EGenericFormat inDataFormat)
            {
                mDataFormat = inDataFormat;
            }

            #endregion
            #region Private Methods

            public bool isNull(Type t) { return (t == null); }
            public bool isBool(Type t) { return t == typeof(bool); }
            public bool isInt8(Type t) { return t == typeof(SByte); }
            public bool isUInt8(Type t) { return t == typeof(Byte); }
            public bool isInt16(Type t) { return t == typeof(Int16); }
            public bool isUInt16(Type t) { return t == typeof(UInt16); }
            public bool isInt32(Type t) { return t == typeof(Int32); }
            public bool isUInt32(Type t) { return t == typeof(UInt32); }
            public bool isInt64(Type t) { return t == typeof(Int64); }
            public bool isUInt64(Type t) { return t == typeof(UInt64); }
            public bool isFloat(Type t) { return t == typeof(float); }
            public bool isString(Type t) { return t == typeof(string); }
            public bool isEnum(Type t) { return t.IsEnum; }
            public bool isArray(Type t) { return !t.IsPrimitive && t.IsArray; }
            public bool isObject(Type t) { return t.IsClass && !t.IsArray && !isString(t); }
            public bool isAtom(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(Game.Data.IAtom)); }
            public bool isFileId(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(Game.Data.IFileId)); }
            public bool isCompound(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(Game.Data.ICompound)); }
            public bool isDynamicMember(Type t) { return !t.IsPrimitive && Reflector.HasOrIsGenericInterface(t, typeof(Game.Data.IDynamicMember)); }

            public Member newNullMember(string memberName) { return new NullMember(memberName); }
            public Member newBoolMember(bool o, string memberName) { return new BoolMember(memberName, o); }
            public Member newInt8Member(SByte o, string memberName) { return new Int8Member(memberName, o); }
            public Member newUInt8Member(Byte o, string memberName) { return new UInt8Member(memberName, o); }
            public Member newInt16Member(Int16 o, string memberName) { return new Int16Member(memberName, o); }
            public Member newUInt16Member(UInt16 o, string memberName) { return new UInt16Member(memberName, o); }
            public Member newInt32Member(Int32 o, string memberName) { return new Int32Member(memberName, o); }
            public Member newUInt32Member(UInt32 o, string memberName) { return new UInt32Member(memberName, o); }
            public Member newInt64Member(Int64 o, string memberName) { return new Int64Member(memberName, o); }
            public Member newUInt64Member(UInt64 o, string memberName) { return new UInt64Member(memberName, o); }
            public Member newFloatMember(float o, string memberName) { return new FloatMember(memberName, o); }
            public Member newStringMember(string o, string memberName) { return new StringMember(memberName, o); }
            public Member newFileIdMember(Hash128 o, string memberName) { return new FileIdMember(memberName, o); }
            public Member newEnumMember(object o, string memberName) { return new Int32Member(memberName, (Int32)o); }

            #endregion
            #region IMemberGenerator methods

            public MetaType newMemberType(Type type)
            {
                if (isNull(type))
                {
                    return new NullType();
                }
                
                if (isAtom(type))
                {
                    return new AtomType(type, type.Name);
                }

                if (isFileId(type))
                {
                    return new FileIdType(type, type.Name);
                }

                if (isCompound(type))
                {
                    return new CompoundType(type, type.Name);
                }

                if (isBool(type)) return BoolMember.sType;
                if (isInt8(type)) return Int8Member.sType;
                if (isUInt8(type)) return UInt8Member.sType;
                if (isInt16(type)) return Int16Member.sType;
                if (isUInt16(type)) return UInt16Member.sType;
                if (isInt32(type)) return Int32Member.sType;
                if (isUInt32(type)) return UInt32Member.sType;
                if (isInt64(type)) return Int64Member.sType;
                if (isUInt64(type)) return UInt64Member.sType;
                if (isFloat(type)) return FloatMember.sType;
                if (isString(type)) return StringMember.sType;
                if (isEnum(type)) return Int32Member.sType;
                if (isObject(type)) return newObjectType(type);
                throw new NotImplementedException();
            }

            public MetaType newObjectType(Type type)
            {
                return new ObjectType(type, "Object");
            }

            public MetaType newAtomType(Type type)
            {
                return new AtomType(type, type.Name);
            }

            public MetaType newFileIdType(Type type)
            {
                return new FileIdType(type, type.Name);
            }

            public MetaType newCompoundType(Type type)
            {
                return new CompoundType(type, type.Name);
            }

            public ObjectMember newObjectMember(Type classType, object content, string memberName)
            {
                ObjectMember classMember;
                if (mDataFormat == EGenericFormat.STD_FLAT)
                {
                    classMember = new ObjectMember(content, newObjectType(classType), memberName);
                }
                else
                {
                    classMember = new ObjectMember(content, newObjectType(classType), memberName);

                    ObjectMember c = classMember;
                    Type baseType = classType.BaseType;
                    while (baseType!=null && baseType != typeof(object))
                    {
                        c.baseClass = new ObjectMember(content, newObjectType(baseType), "");
                        c = c.baseClass;

                        // Next base class
                        baseType = baseType.BaseType;
                    }
                }
                return classMember;
            }

            public ArrayMember newArrayMember(Type arrayType, object content, Member elementMember, string memberName)
            {
                ArrayMember arrayMember = new ArrayMember(arrayType, content, elementMember, memberName);
                return arrayMember;
            }

            public AtomMember newAtomMember(Type atomType, Member atomContentMember, string memberName)
            {
                AtomMember atom = new AtomMember(memberName, newAtomType(atomType), atomContentMember);
                return atom;
            }
            public FileIdMember newFileIdMember(Type fileidType, Hash128 content, string memberName)
            {
                FileIdMember fileid = new FileIdMember(memberName, content);
                return fileid;
            }
            public CompoundMember newCompoundMember(Type compoundType, object compoundObject, string memberName)
            {
                CompoundMember compoundMember = new CompoundMember(compoundObject, newCompoundType(compoundType), memberName);
                return compoundMember;
            }

            #endregion
        }

        #endregion
        #region Generic writer

        public bool write(EGenericFormat inDataFormat, object inData, IBinaryWriter resourceDataWriter, IBinaryWriter resourceDataReallocTableWriter)
        {
            try
            {
                IMemberGenerator genericMemberGenerator = new GenericMemberGenerator(inDataFormat);

                // Analyze Root and generate a list of 'Code.Class' objects from this.
                Reflector reflector = new Reflector(genericMemberGenerator);

                MyMemberBook book = new MyMemberBook();
                reflector.Analyze(inData, book);
                book.HandoutReferences();

                // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
                StringTable stringTable = new StringTable();
                stringTable.reference = StreamReference.Instance;
                stringTable.forceLowerCase = false;
                
                // The FileIdTable to collect (and collapse duplicate) all FileIds
                FileIdTable fileIdTable = new FileIdTable();
                fileIdTable.reference = StreamReference.Instance;

                // Database of offsets of references written in the stream as well as the offsets of references to those references
                IDataWriter dataWriter = EndianUtils.CreateDataWriter(mEndian);

                ObjectMember rootClass = book.classes[0];

                dataWriter.Begin();
                {
                    // Header=
                    //     FileIdTable , void*, 32 bit
                    //     StringTable , void*, 32 bit
                    //     ResourceRoot, void*, 32 bit

                    dataWriter.Write(fileIdTable.reference);
                    dataWriter.Write(stringTable.reference);
                    dataWriter.Write(rootClass.reference);      // The root class

                    // Collect all strings (class names + member names)
                    // Collect all strings from member content
                    StringTableWriter strTableWriter = new StringTableWriter(stringTable);
                    rootClass.write(strTableWriter);
                    stringTable.SortByHash();

                    // Write 'SResource' array
                    ObjectWriter genericObjectTreeWriter = new ObjectWriter(inDataFormat, stringTable, fileIdTable, dataWriter);
                    for (int i = 0; i < book.classes.Count; i++)
                        genericObjectTreeWriter.write(book.classes[i]);

                    // Write 'SResource' member data
                    ObjectMemberReferenceDataWriter objectMemberDataWriter = new ObjectMemberReferenceDataWriter(stringTable, fileIdTable, dataWriter);
                    rootClass.write(objectMemberDataWriter);

                    // Write StringTable and FileIdTable
                    stringTable.Write(dataWriter);
                    fileIdTable.Write(dataWriter);
                }
                dataWriter.End();

                // Validate member data
                MemberDataValidator memberDataValidator = new MemberDataValidator();
                rootClass.write(memberDataValidator);

                // Finalize the dataWriter, this will write the data to @name resourceDataWriter and the
                // relocation table to @name resourceDataReallocTableWriter.
                // Note: Before writing it will collapse all identical data blocks.
                Dictionary<StreamReference, StreamContext> referenceDatabase;
                Dictionary<StreamReference, StreamContext> unresolvedReferences;
                Dictionary<StreamReference, StreamContext> markerDatabase;
                dataWriter.Final(resourceDataWriter, out referenceDatabase, out unresolvedReferences, out markerDatabase);

                // Write the reference dictionary (relocation table)
                int numReferencesToReferencesWritten = 0;
                foreach (KeyValuePair<StreamReference, StreamContext> p in referenceDatabase)
                {
                    // Null references are not emitted in the relocation table!
                    if (p.Key != StreamReference.Empty)
                        numReferencesToReferencesWritten += p.Value.Count;
                }

                resourceDataReallocTableWriter.Write(numReferencesToReferencesWritten);
                foreach (KeyValuePair<StreamReference, StreamContext> p in referenceDatabase)
                {
                    // Null references are not emitted in the relocation table!
                    if (p.Key != StreamReference.Empty)
                    {
                        for (int i = 0; i < p.Value.Count; i++)
                            resourceDataReallocTableWriter.Write(p.Value[i].value32);
                    }
                }

                if (unresolvedReferences.Count > 0)
                {
                    UnresolvedReferencesLogger logger = new UnresolvedReferencesLogger(stringTable, unresolvedReferences);
                    foreach (ObjectMember c in book.classes)
                        c.write(logger);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[GenericDataStream:EXCEPTION] {0}", e.Message);
            }

            return true;
        }

        #endregion
    }
}
