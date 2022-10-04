using System;
using System.Collections.Generic;

namespace GameData
{
    using GameCore;
    using MetaCode;

    /// <summary>
    /// StdStream representing a standard object data tree
    /// </summary>
    public sealed class StdDataStream
    {
        #region Fields

        private readonly EEndian mEndian = EEndian.LITTLE;

        #endregion
        #region Constructor

        public StdDataStream(EEndian endian)
        {
            mEndian = endian;
        }

        #endregion
        #region Properties

        public static int SizeOfBool { get; set; }

        #endregion

        #region Unresolved References

        private sealed class UnresolvedReferencesLogger : IMemberWriter
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

            private void Log(string memberName, string typeName)
            {
                string parentStr = string.Empty;

                foreach (Member m in mHierarchy)
                    parentStr += m.Type.typeName + "{" + m.Name + "}.";

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
                    Log(c.Name, c.Type.typeName);
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
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.Name, c.Type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.Name, c.Type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                // Has no reference, is a system type (bool, int, float)
                c.member.Write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.Name, c.Type.typeName);

                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
        }

        #endregion
        #region Object member data validation

        private sealed class MemberDataValidator : IMemberWriter
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

            public bool LogOutOfRange(string memberName, object v, string errorStr)
            {
                string parentStr = string.Empty;
                foreach (Member m in mHierarchy)
                    parentStr += "[" + m.Name + "].";

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
                foreach (Member m in c.Members)
                {
                    //m.range = c.range;
                    m.Write(this);
                }
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                //c.member.range = c.range;
                c.member.Write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                mHierarchy.Add(c);
                foreach (Member m in c.members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);

                return true;
            }
        }

        #endregion

        #region StringTable writer

        public sealed class StringTableWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;

            private UInt32 mDone = 0;
            private enum EDone : UInt32
            {
                None = 0,
                NullMemberDone = 1 << 1,
                Bool8MemberDone = 1 << 2,
                Int8MemberDone = 1 << 3,
                Int16MemberDone = 1 << 4,
                Int32MemberDone = 1 << 5,
                Int64MemberDone = 1 << 6,
                UInt8MemberDone = 1 << 7,
                UInt16MemberDone = 1 << 8,
                UInt32MemberDone = 1 << 9,
                UInt64MemberDone = 1 << 10,
                FloatMemberDone = 1 << 11,
                StringMemberDone = 1 << 12,
                FileIdMemberDone = 1 << 13,
            };

            private bool HasFlag(EDone d)
            {
                return (mDone & (UInt32)d) == (UInt32)d;
            }

            private void SetFlag(EDone d)
            {
                mDone = mDone | (UInt32)d;
            }

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
                if (!HasFlag(EDone.NullMemberDone))
                {
                    SetFlag(EDone.NullMemberDone);
                    mStringTable.Add("void");
                    mStringTable.Add("void[]");
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                if (!HasFlag(EDone.Bool8MemberDone))
                {
                    SetFlag(EDone.Bool8MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                if (!HasFlag(EDone.Int8MemberDone))
                {
                    SetFlag(EDone.Int8MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                if (!HasFlag(EDone.Int16MemberDone))
                {
                    SetFlag(EDone.Int16MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                if (!HasFlag(EDone.Int32MemberDone))
                {
                    SetFlag(EDone.Int32MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                if (!HasFlag(EDone.Int64MemberDone))
                {
                    SetFlag(EDone.Int64MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                if (!HasFlag(EDone.UInt8MemberDone))
                {
                    SetFlag(EDone.UInt8MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                if (!HasFlag(EDone.UInt16MemberDone))
                {
                    SetFlag(EDone.UInt16MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                if (!HasFlag(EDone.UInt32MemberDone))
                {
                    SetFlag(EDone.UInt32MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                if (!HasFlag(EDone.UInt64MemberDone))
                {
                    SetFlag(EDone.UInt64MemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                if (!HasFlag(EDone.FloatMemberDone))
                {
                    SetFlag(EDone.FloatMemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                if (!HasFlag(EDone.StringMemberDone))
                {
                    SetFlag(EDone.StringMemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                mStringTable.Add(c.str);
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                if (!HasFlag(EDone.FileIdMemberDone))
                {
                    SetFlag(EDone.FileIdMemberDone);
                    mStringTable.Add(c.Type.typeName);
                }
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                foreach (Member m in c.Members)
                    m.Write(this);
                mStringTable.Add(c.Type.typeName);
                mStringTable.Add(c.Name.ToLower());
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                foreach (Member m in c.members)
                    m.Write(this);

                mStringTable.Add(c.Type.typeName);
                mStringTable.Add(c.Name.ToLower());

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                mStringTable.Add(c.Type.typeName);
                mStringTable.Add(c.Name.ToLower());
                c.member.Write(this);
                return true;
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                mStringTable.Add(c.Type.typeName);
                mStringTable.Add(c.Name.ToLower());

                foreach (Member m in c.members)
                    m.Write(this);

                return true;
            }
        }

        #endregion

        #region Object member data writer

        public sealed class ObjectMemberDataWriter : IMemberWriter
        {
            private bool mAlignMembers;
            private StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
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

            private void WriteReference(StreamReference reference)
            {
                if (mAlignMembers) Align(4);
                mWriter.Write(reference);
            }

            private void Align(Int64 alignment)
            {
                StreamUtils.Align(mWriter, alignment);
            }

            public bool writeNullMember(NullMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write((int)0);
                return true;
            }
            public bool writeBool8Member(BoolMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write((int)c.boolean);
                return true;
            }
            public bool writeInt8Member(Int8Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.int8);
                return true;
            }
            public bool writeInt16Member(Int16Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.int16);
                return true;
            }
            public bool writeInt32Member(Int32Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.int32);
                return true;
            }
            public bool writeInt64Member(Int64Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.int64);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.uint8);
                return true;
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.uint16);
                return true;
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.uint32);
                return true;
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.uint64);
                return true;
            }
            public bool writeFloatMember(FloatMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.real);
                return true;
            }
            public bool writeStringMember(StringMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                WriteReference(mStringTable.ReferenceOf(c.str));
                return true;
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                StreamReference reference = mFileIdTable.Add(c.Reference, c.ID);
                mWriter.Write(reference);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.Members.Count);
                mWriter.Write(c.Reference);
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                WriteReference(c.Reference);
                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.Write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                if (c.isNullType)
                {
                    WriteReference(c.Reference);
                }
                else
                {
                    if (mAlignMembers) Align(c.Alignment);
                    foreach (Member m in c.members)
                        m.Write(this);
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
        public sealed class ObjectMemberReferenceDataWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly IDataWriter mWriter;
            private readonly ObjectMemberDataWriter mObjectMemberDataWriter;

            public ObjectMemberReferenceDataWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;

                mWriter = writer;
                mObjectMemberDataWriter = new (mStringTable, fileIdTable, mWriter)
                {
                    alignMembers = true
                };
            }

            public bool open()
            {
                return mStringTable != null && mWriter != null;
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
                if (c.Reference != StreamReference.Empty && mWriter.BeginBlock(c.Reference, c.Alignment))
                {
                    foreach (Member m in c.Members)
                    {
                        m.Write(mObjectMemberDataWriter);
                    }
                    mWriter.EndBlock();
                }

                // Write the elements of the array again for type 'Reference'
                foreach (Member m in c.Members)
                    m.Write(this);

                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                foreach (Member m in c.members)
                    m.Write(this);

                if (c.baseClass != null)
                    writeObjectMember(c.baseClass);

                return true;
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.Write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                if (c.Reference != StreamReference.Empty && mWriter.BeginBlock(c.Reference, sizeof(UInt32)))
                {
                    foreach (Member m in c.members)
                        m.Write(mObjectMemberDataWriter);
                    mWriter.EndBlock();
                }

                // Let the 'Reference' types write them selfs
                foreach (Member m in c.members)
                    m.Write(this);

                return true;
            }
        }

        #endregion

        #region Object member offset or value writer

        public sealed class ObjectMemberOffsetOrValueWriter : IMemberWriter
        {
            private StringTable mStringTable;
            private readonly FileIdTable mFileIdTable;
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

            private bool WriteReferenceMember(StreamReference reference)
            {
                mWriter.Write(reference);
                return true;
            }

            public bool WriteInt32(int v)
            {
                mWriter.Write(v);
                return true;
            }

            public bool writeNullMember(NullMember c)
            {
                return WriteInt32(0);
            }
            public bool writeBool8Member(BoolMember c)
            {
                return WriteInt32(c.boolean);
            }
            public bool writeInt8Member(Int8Member c)
            {
                return WriteInt32(c.int8);
            }
            public bool writeInt16Member(Int16Member c)
            {
                return WriteInt32(c.int16);
            }
            public bool writeInt32Member(Int32Member c)
            {
                return WriteInt32(c.int32);
            }
            public bool writeInt64Member(Int64Member c)
            {
                mWriter.Write(c.int64);
                return true;
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                return WriteInt32(c.uint8);
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                return WriteInt32(c.uint16);
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                return WriteInt32((int)c.uint32);
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
                return WriteReferenceMember(mStringTable.ReferenceOf(c.str));
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                StreamReference reference = mFileIdTable.Add(c.Reference, c.ID);
                mWriter.Write(reference);
                return true;
            }
            public bool writeArrayMember(ArrayMember c)
            {
                mWriter.Write(c.Members.Count);
                mWriter.Write(c.Reference);
                return true;
            }
            public bool writeObjectMember(ObjectMember c)
            {
                return WriteReferenceMember(c.Reference);
            }
            public bool writeAtomMember(AtomMember c)
            {
                return c.member.Write(this);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                return WriteReferenceMember(c.Reference);
            }
        }

        #endregion
        #region Object member writer

        private sealed class ObjectMemberWriter : IMemberWriter
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

            private bool WriteMember(string typeName, string memberName, Member m)
            {
                UInt32 typeNameHash = mStringTable.HashOf(typeName);
                UInt32 memberNameHash = mStringTable.HashOf(memberName.ToLower());
                if (typeNameHash == UInt32.MaxValue || memberNameHash == UInt32.MaxValue)
                {
                    Console.WriteLine("Error: StringTable is missing some strings!");
                }

                mWriter.Write(typeNameHash);
                mWriter.Write(memberNameHash);
                return m.Write(mOffsetOrValueWriter);
            }

            public bool writeNullMember(NullMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeBool8Member(BoolMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeInt8Member(Int8Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeInt16Member(Int16Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeInt32Member(Int32Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeInt64Member(Int64Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeUInt8Member(UInt8Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeUInt16Member(UInt16Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeUInt32Member(UInt32Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeUInt64Member(UInt64Member c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeFloatMember(FloatMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeStringMember(StringMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeFileIdMember(FileIdMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeArrayMember(ArrayMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeObjectMember(ObjectMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeAtomMember(AtomMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
            public bool writeCompoundMember(CompoundMember c)
            {
                return WriteMember(c.Type.typeName, c.Name, c);
            }
        }

        #endregion
        #region Object writer

        private sealed class ObjectWriter
        {
            private readonly EGenericFormat mDataFormat;
            private readonly StringTable mStringTable;
            private readonly IDataWriter mWriter;

            private readonly ObjectMemberWriter mObjectMemberWriter;

            public ObjectWriter(EGenericFormat inDataFormat, StringTable inStringTable, FileIdTable inFileIdTable, IDataWriter inWriter)
            {
                mDataFormat = inDataFormat;
                mStringTable = inStringTable;
                mWriter = inWriter;
                mObjectMemberWriter = new ObjectMemberWriter(mStringTable, inFileIdTable, mWriter);
            }

            public bool Write(ObjectMember c)
            {
                // Class ->
                //     {SResource*  base;}
                //     int         count;          // (=members.Count)
                //     short[]     memberOffsets;
                //     Member[]    members;        // <- base of the offset is the start of the first member

                // Write every 'SResource' in a new DataBlock so that
                // duplicate classes can be collapsed.
                if (c.Reference != StreamReference.Empty && mWriter.BeginBlock(c.Reference, c.Alignment))
                {
                    if (mDataFormat != EGenericFormat.STD_FLAT)
                    {
                        if (c.baseClass != null)
                            mWriter.Write(c.baseClass.Reference);
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
                    StreamUtils.Align(mWriter, sizeof(short));

                    List<short> memberOffsets = new (c.members.Count);
                    foreach (Member m in c.members)
                    {
                        StreamUtils.Align(mWriter, m.Alignment);
                        memberOffsets.Add((short)(mWriter.Position - offsetsPos));
                        m.Write(mObjectMemberWriter);
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
                    Write(c.baseClass);

                return true;
            }

        }

        #endregion

        #region MemberBook

        sealed class MyMemberBook : MemberBook
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
                        i.Reference = reference;
                    }
                    else
                    {
                        i.Reference = StreamReference.Instance;
                        referencesForInt64Dict.Add(i.int64, i.Reference);
                    }
                }

                Dictionary<UInt64, StreamReference> referencesForUInt64Dict = new Dictionary<UInt64, StreamReference>();
                foreach (UInt64Member i in uint64s)
                {
                    StreamReference reference;
                    if (referencesForUInt64Dict.TryGetValue(i.uint64, out reference))
                    {
                        i.Reference = reference;
                    }
                    else
                    {
                        i.Reference = StreamReference.Instance;
                        referencesForUInt64Dict.Add(i.uint64, i.Reference);
                    }
                }

                Dictionary<object, ObjectMember> referencesForClassesDict = new Dictionary<object, ObjectMember>();
                foreach (ObjectMember c in classes)
                {
                    ObjectMember i = c;

                    if (i.IsDefault || i.Value == null)
                    {
                        i.Reference = StreamReference.Empty;

                        i = i.baseClass;
                        while (i != null)
                        {
                            i.Reference = StreamReference.Empty;
                            i = i.baseClass;
                        }
                    }
                    else
                    {
                        ObjectMember r;
                        if (referencesForClassesDict.TryGetValue(i.Value, out r))
                        {
                            i.Reference = r.Reference;
                        }
                        else
                        {
                            i.Reference = StreamReference.Instance;
                            referencesForClassesDict.Add(i.Value, c);
                        }
                        // Do base classes
                        r = (r != null) ? r.baseClass : null;
                        i = i.baseClass;
                        while (i != null)
                        {
                            i.Reference = (r != null) ? r.Reference : StreamReference.Instance;
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
                        if (c.Value != null)
                        {
                            StreamReference reference;
                            if (referencesForCompoundsDict.TryGetValue(c.Value, out reference))
                            {
                                c.Reference = reference;
                            }
                            else
                            {
                                c.Reference = StreamReference.Instance;
                                referencesForCompoundsDict.Add(c.Value, c.Reference);
                            }
                        }
                        else
                        {
                            c.Reference = StreamReference.Empty;
                        }
                    }
                    else
                    {
                        c.Reference = StreamReference.Instance;
                    }
                }

                foreach (AtomMember c in atoms)
                {
                }

                Dictionary<UInt64, StreamReference> referencesForFileIdDict = new ();
                foreach (FileIdMember c in fileids)
                {
                    StreamReference reference;
                    if (referencesForFileIdDict.TryGetValue(c.ID, out reference))
                    {
                        c.Reference = reference;
                    }
                    else
                    {
                        c.Reference = StreamReference.Instance;
                        referencesForFileIdDict.Add(c.ID, c.Reference);
                    }
                }

                Dictionary<object, StreamReference> referencesForArraysDict = new Dictionary<object, StreamReference>();
                foreach (ArrayMember a in arrays)
                {
                    if (a.IsDefault || a.Value == null)
                    {
                        a.Reference = StreamReference.Empty;
                    }
                    else
                    {
                        StreamReference reference;
                        if (referencesForArraysDict.TryGetValue(a.Value, out reference))
                        {
                            a.Reference = reference;
                        }
                        else
                        {
                            a.Reference = StreamReference.Instance;
                            referencesForArraysDict.Add(a.Value, a.Reference);
                        }
                    }
                }
            }
        }

        #endregion

        #region Generic MemberGenerator

        public sealed class GenericMemberGenerator : IMemberGenerator
        {
            #region Fields

            private readonly EGenericFormat mDataFormat;

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
            public bool isAtom(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.IAtom)); }
            public bool isFileId(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.IFileId)); }
            public bool isCompound(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.ICompound)); }
            public bool isDynamicMember(Type t) { return !t.IsPrimitive && Reflector.HasOrIsGenericInterface(t, typeof(GameData.IDynamicMember)); }

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
            public Member newFileIdMember(UInt64 o, string memberName) { return new FileIdMember(memberName, o); }
            public Member newEnumMember(object o, string memberName) { return new Int32Member(memberName, (Int32)o); }

            #endregion
            #region IMemberGenerator methods

            public MetaType NewMemberType(Type type)
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
                if (isObject(type)) return NewObjectType(type);
                throw new NotImplementedException();
            }

            public MetaType NewObjectType(Type type)
            {
                return new ObjectType(type, "Object");
            }

            public MetaType NewAtomType(Type type)
            {
                return new AtomType(type, type.Name);
            }

            public MetaType NewFileIdType(Type type)
            {
                return new FileIdType(type, type.Name);
            }

            public MetaType NewCompoundType(Type type)
            {
                return new CompoundType(type, type.Name);
            }

            public ObjectMember newObjectMember(Type classType, object content, string memberName)
            {
                ObjectMember classMember;
                if (mDataFormat == EGenericFormat.STD_FLAT)
                {
                    classMember = new ObjectMember(content, NewObjectType(classType), memberName);
                }
                else
                {
                    classMember = new ObjectMember(content, NewObjectType(classType), memberName);

                    ObjectMember c = classMember;
                    Type baseType = classType.BaseType;
                    while (baseType != null && baseType != typeof(object))
                    {
                        c.baseClass = new ObjectMember(content, NewObjectType(baseType), "");
                        c = c.baseClass;

                        // Next base class
                        baseType = baseType.BaseType;
                    }
                }
                return classMember;
            }

            public ArrayMember newArrayMember(Type arrayType, object content, Member elementMember, string memberName)
            {
                ArrayMember arrayMember = new (arrayType, content, elementMember, memberName);
                return arrayMember;
            }

            public AtomMember newAtomMember(Type atomType, Member atomContentMember, string memberName)
            {
                AtomMember atom = new (memberName, NewAtomType(atomType), atomContentMember);
                return atom;
            }
            public FileIdMember newFileIdMember(Type fileidType, UInt64 content, string memberName)
            {
                FileIdMember fileid = new (memberName, content);
                return fileid;
            }
            public CompoundMember newCompoundMember(Type compoundType, object compoundObject, string memberName)
            {
                CompoundMember compoundMember = new (compoundObject, NewCompoundType(compoundType), memberName);
                return compoundMember;
            }

            #endregion
        }

        #endregion
        #region Generic writer

        public bool Write(EGenericFormat inDataFormat, object inData, IBinaryWriter resourceDataWriter, IBinaryWriter resourceDataReallocTableWriter)
        {
            try
            {
                IMemberGenerator genericMemberGenerator = new GenericMemberGenerator(inDataFormat);

                // Analyze Root and generate a list of 'Code.Class' objects from this.
                Reflector reflector = new (genericMemberGenerator);

                MyMemberBook book = new ();
                reflector.Analyze(inData, book);
                book.HandoutReferences();

                // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
                StringTable stringTable = new ();
                stringTable.Reference = StreamReference.Instance;

                // The FileIdTable to collect (and collapse duplicate) all FileIds
                FileIdTable fileIdTable = new ();
                fileIdTable.Reference = StreamReference.Instance;

                // Database of offsets of references written in the stream as well as the offsets of references to those references
                IDataWriter dataWriter = EndianUtils.CreateDataWriter(mEndian);

                ObjectMember rootClass = book.classes[0];

                dataWriter.Begin();
                {
                    // Header=
                    //     FileIdTable , void*, 32 bit
                    //     StringTable , void*, 32 bit
                    //     ResourceRoot, void*, 32 bit

                    dataWriter.Write(fileIdTable.Reference);
                    dataWriter.Write(stringTable.Reference);
                    dataWriter.Write(rootClass.Reference);      // The root class

                    // Collect all strings (class names + member names)
                    // Collect all strings from member content
                    StringTableWriter strTableWriter = new (stringTable);
                    rootClass.Write(strTableWriter);
                    stringTable.SortByHash();

                    // Write 'SResource' array
                    ObjectWriter genericObjectTreeWriter = new (inDataFormat, stringTable, fileIdTable, dataWriter);
                    for (int i = 0; i < book.classes.Count; i++)
                        genericObjectTreeWriter.Write(book.classes[i]);

                    // Write 'SResource' member data
                    ObjectMemberReferenceDataWriter objectMemberDataWriter = new ObjectMemberReferenceDataWriter(stringTable, fileIdTable, dataWriter);
                    rootClass.Write(objectMemberDataWriter);

                    // Write StringTable and FileIdTable
                    stringTable.Write(dataWriter);
                    fileIdTable.Write(dataWriter);
                }
                dataWriter.End();

                // Validate member data
                MemberDataValidator memberDataValidator = new ();
                rootClass.Write(memberDataValidator);

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
                            resourceDataReallocTableWriter.Write(p.Value[i].value);
                    }
                }

                if (unresolvedReferences.Count > 0)
                {
                    UnresolvedReferencesLogger logger = new (stringTable, unresolvedReferences);
                    foreach (ObjectMember c in book.classes)
                        c.Write(logger);
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
