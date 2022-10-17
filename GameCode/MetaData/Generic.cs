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
            private readonly List<IClassMember> mHierarchy = new List<IClassMember>();

            public UnresolvedReferencesLogger(StringTable stringTable, Dictionary<StreamReference, StreamContext> unresolvedReferences)
            {
                mStringTable = stringTable;
                mUnresolvedReferences = unresolvedReferences;
            }

            public bool Open()
            {
                return true;
            }

            public bool Close()
            {
                return true;
            }

            private void Log(string memberName, string typeName)
            {
                string parentStr = string.Empty;

                foreach (IClassMember m in mHierarchy)
                    parentStr += m.TypeName + "{" + m.MemberName + "}.";

                Console.WriteLine(parentStr + typeName + "{" + memberName + "} which was not resolved");
            }

            public void WriteNullMember(NullMember c)
            {
                // Has no reference
            }
            public void WriteBool8Member(BoolMember c)
            {
                // Has no reference
            }
            public void WriteInt8Member(Int8Member c)
            {
                // Has no reference
            }
            public void WriteInt16Member(Int16Member c)
            {
                // Has no reference
            }
            public void WriteInt32Member(Int32Member c)
            {
                // Has no reference
            }
            public void WriteInt64Member(Int64Member c)
            {
                // Has no reference
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                // Has no reference
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                // Has no reference
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                // Has no reference
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                // Has no reference
            }
            public void WriteEnumMember(EnumMember c)
            {
                // Has no reference
            }
            public void WriteFloatMember(FloatMember c)
            {
                // Has no reference
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                // Has no reference
            }
            public void WriteStringMember(StringMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(mStringTable.ReferenceOf(c.InternalValue), out ctx))
                    Log(c.MemberName, c.TypeName);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                // Has no reference
            }
            public void WriteArrayMember(ArrayMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.MemberName, c.TypeName);

                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
            }
            public void WriteObjectMember(ClassObject c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.MemberName, c.TypeName);

                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
            }
            public void WriteAtomMember(AtomMember c)
            {
                // Has no reference, is a system type (bool, int, float)
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                StreamContext ctx;
                if (mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.MemberName, c.TypeName);

                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
            }
        }

        #endregion
        #region Object member data validation

        private sealed class MemberDataValidator : IMemberWriter
        {
            private readonly List<IClassMember> mHierarchy = new List<IClassMember>();

            public bool Open()
            {
                return true;
            }

            public bool Close()
            {
                return true;
            }

            public bool LogOutOfRange(string memberName, object v, string errorStr)
            {
                string parentStr = string.Empty;
                foreach (IClassMember m in mHierarchy)
                    parentStr += "[" + m.MemberName + "].";

                Console.WriteLine(parentStr + "[" + memberName + "] " + errorStr, v);
                return false;
            }

            public void WriteNullMember(NullMember c)
            {
            }
            public void WriteBool8Member(BoolMember c)
            {
            }
            public void WriteInt8Member(Int8Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int8, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteInt16Member(Int16Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int16, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteInt32Member(Int32Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int32, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteInt64Member(Int64Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.int64, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint8, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint16, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint32, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint64, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteEnumMember(EnumMember c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.uint64, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteFloatMember(FloatMember c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.real, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                //                string errorStr;
                //                if (c.range != null && !c.range.check(c.real, out errorStr))
                //                    return logOutOfRange(c.name, c.value, errorStr);
            }
            public void WriteStringMember(StringMember c)
            {
            }
            public void WriteFileIdMember(FileIdMember c)
            {
            }
            public void WriteArrayMember(ArrayMember c)
            {
                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                {
                    //m.range = c.range;
                    m.Write(this);
                }
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
            }
            public void WriteObjectMember(ClassObject c)
            {
                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
            }
            public void WriteAtomMember(AtomMember c)
            {
                //c.member.range = c.range;
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mHierarchy.RemoveAt(mHierarchy.Count - 1);
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
                EnumMemberDone = 1 << 11,
                FloatMemberDone = 1 << 12,
                DoubleMemberDone = 1 << 13,
                StringMemberDone = 1 << 14,
                FileIdMemberDone = 1 << 15,
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

            public bool Open()
            {
                return true;
            }

            public bool Close()
            {
                return true;
            }

            public void WriteNullMember(NullMember c)
            {
                if (!HasFlag(EDone.NullMemberDone))
                {
                    SetFlag(EDone.NullMemberDone);
                    mStringTable.Add("void");
                    mStringTable.Add("void[]");
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteBool8Member(BoolMember c)
            {
                if (!HasFlag(EDone.Bool8MemberDone))
                {
                    SetFlag(EDone.Bool8MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt8Member(Int8Member c)
            {
                if (!HasFlag(EDone.Int8MemberDone))
                {
                    SetFlag(EDone.Int8MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt16Member(Int16Member c)
            {
                if (!HasFlag(EDone.Int16MemberDone))
                {
                    SetFlag(EDone.Int16MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt32Member(Int32Member c)
            {
                if (!HasFlag(EDone.Int32MemberDone))
                {
                    SetFlag(EDone.Int32MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt64Member(Int64Member c)
            {
                if (!HasFlag(EDone.Int64MemberDone))
                {
                    SetFlag(EDone.Int64MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                if (!HasFlag(EDone.UInt8MemberDone))
                {
                    SetFlag(EDone.UInt8MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                if (!HasFlag(EDone.UInt16MemberDone))
                {
                    SetFlag(EDone.UInt16MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                if (!HasFlag(EDone.UInt32MemberDone))
                {
                    SetFlag(EDone.UInt32MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                if (!HasFlag(EDone.UInt64MemberDone))
                {
                    SetFlag(EDone.UInt64MemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteEnumMember(EnumMember c)
            {
                if (!HasFlag(EDone.EnumMemberDone))
                {
                    SetFlag(EDone.EnumMemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteFloatMember(FloatMember c)
            {
                if (!HasFlag(EDone.FloatMemberDone))
                {
                    SetFlag(EDone.FloatMemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                if (!HasFlag(EDone.DoubleMemberDone))
                {
                    SetFlag(EDone.DoubleMemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteStringMember(StringMember c)
            {
                if (!HasFlag(EDone.StringMemberDone))
                {
                    SetFlag(EDone.StringMemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
                mStringTable.Add(c.InternalValue);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                if (!HasFlag(EDone.FileIdMemberDone))
                {
                    SetFlag(EDone.FileIdMemberDone);
                    mStringTable.Add(c.TypeName);
                }
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteArrayMember(ArrayMember c)
            {
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                mStringTable.Add(c.TypeName);
                mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteObjectMember(ClassObject c)
            {
                foreach (IClassMember m in c.Members)
                    m.Write(this);

                mStringTable.Add(c.TypeName);
                mStringTable.Add(c.MemberName.ToLower());

            }
            public void WriteAtomMember(AtomMember c)
            {
                mStringTable.Add(c.TypeName);
                mStringTable.Add(c.MemberName.ToLower());
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                mStringTable.Add(c.TypeName);
                mStringTable.Add(c.MemberName.ToLower());

                foreach (IClassMember m in c.Members)
                    m.Write(this);

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
                get => mAlignMembers;
                set => mAlignMembers = value;
            }

            public bool Open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool Close()
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

            public void WriteNullMember(NullMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write((int)0);
            }
            public void WriteBool8Member(BoolMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write((int)(c.InternalValue ? 1 : 0));
            }
            public void WriteInt8Member(Int8Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteInt16Member(Int16Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteInt32Member(Int32Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteInt64Member(Int64Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteEnumMember(EnumMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                ulong e = c.InternalValue;
                switch (c.Alignment)
                {
                    case 1:  mWriter.Write((byte)e); break;
                    case 2:  mWriter.Write((ushort)e); break;
                    case 4:  mWriter.Write((uint)e); break;
                    case 8:  mWriter.Write(e); break;
                }
            }
            public void WriteFloatMember(FloatMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteStringMember(StringMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                WriteReference(mStringTable.ReferenceOf(c.InternalValue));
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.InternalValue);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                if (mAlignMembers) Align(c.Alignment);
                mWriter.Write(c.Members.Count);
                mWriter.Write(c.Reference);
            }
            public void WriteObjectMember(ClassObject c)
            {
                WriteReference(c.Reference);
            }
            public void WriteAtomMember(AtomMember c)
            {
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                if (c.IsNullType)
                {
                    WriteReference(c.Reference);
                }
                else
                {
                    if (mAlignMembers) Align(c.Alignment);
                    foreach (IClassMember m in c.Members)
                        m.Write(this);
                }

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

            public bool Open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool Close()
            {
                return true;
            }

            public void WriteNullMember(NullMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteBool8Member(BoolMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteInt8Member(Int8Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteInt16Member(Int16Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteInt32Member(Int32Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteInt64Member(Int64Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteEnumMember(EnumMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteFloatMember(FloatMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteStringMember(StringMember c)
            {
                // Written by the StringTable
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteArrayMember(ArrayMember c)
            {
                // The reference of this member can be null!
                if (c.Reference != StreamReference.Empty && mWriter.BeginBlock(c.Reference, c.Alignment))
                {
                    foreach (IClassMember m in c.Members)
                    {
                        m.Write(mObjectMemberDataWriter);
                    }
                    mWriter.EndBlock();
                }

                // Write the elements of the array again for type 'Reference'
                foreach (IClassMember m in c.Members)
                    m.Write(this);

            }
            public void WriteObjectMember(ClassObject c)
            {
                foreach (IClassMember m in c.Members)
                    m.Write(this);
            }
            public void WriteAtomMember(AtomMember c)
            {
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                if (c.Reference != StreamReference.Empty && mWriter.BeginBlock(c.Reference, sizeof(UInt32)))
                {
                    foreach (IClassMember m in c.Members)
                        m.Write(mObjectMemberDataWriter);
                    mWriter.EndBlock();
                }

                // Let the 'Reference' types write them selfs
                foreach (IClassMember m in c.Members)
                    m.Write(this);
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

            public bool Open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool Close()
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

            public void WriteNullMember(NullMember c)
            {
                WriteInt32(0);
            }
            public void WriteBool8Member(BoolMember c)
            {
                WriteInt32(c.InternalValue ? 0 : 1);
            }
            public void WriteInt8Member(Int8Member c)
            {
                WriteInt32(c.InternalValue);
            }
            public void WriteInt16Member(Int16Member c)
            {
                WriteInt32(c.InternalValue);
            }
            public void WriteInt32Member(Int32Member c)
            {
                WriteInt32(c.InternalValue);
            }
            public void WriteInt64Member(Int64Member c)
            {
                mWriter.Write(c.InternalValue);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                WriteInt32(c.InternalValue);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                WriteInt32(c.InternalValue);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                WriteInt32((int)c.InternalValue);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                mWriter.Write(c.InternalValue);
            }
            public void WriteEnumMember(EnumMember c)
            {
                ulong e = c.InternalValue;
                switch (c.Alignment)
                {
                    case 1:  mWriter.Write((byte)e); break;
                    case 2:  mWriter.Write((ushort)e); break;
                    case 4:  mWriter.Write((uint)e); break;
                    case 8:  mWriter.Write(e); break;
                }
            }
            public void WriteFloatMember(FloatMember c)
            {
                mWriter.Write(c.InternalValue);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                mWriter.Write(c.InternalValue);
            }
            public void WriteStringMember(StringMember c)
            {
                WriteReferenceMember(mStringTable.ReferenceOf(c.InternalValue));
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                mWriter.Write(c.InternalValue);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                mWriter.Write(c.Members.Count);
                mWriter.Write(c.Reference);
            }
            public void WriteObjectMember(ClassObject c)
            {
                WriteReferenceMember(c.Reference);
            }
            public void WriteAtomMember(AtomMember c)
            {
                c.Member.Write(this);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                WriteReferenceMember(c.Reference);
            }
        }

        #endregion
        #region Object member writer

        private sealed class ObjectMemberWriter : IMemberWriter
        {
            private readonly StringTable mStringTable;
            private readonly IDataWriter mWriter;

            private readonly ObjectMemberOffsetOrValueWriter mOffsetOrValueWriter;

            public ObjectMemberWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                mStringTable = stringTable;
                mWriter = writer;
                mOffsetOrValueWriter = new ObjectMemberOffsetOrValueWriter(stringTable, fileIdTable, writer);
            }

            public bool Open()
            {
                return mStringTable != null && mWriter != null;
            }

            public bool Close()
            {
                return true;
            }

            /// short[] MemberOffset
            /// Member ->
            ///     int   mTypeHash;                    // Hash of type name (bool, bool[], int[], float, LString)
            ///     int   mNameHash;                    // Hash of member name
            ///     var   mValue or mOffset;

            private void WriteMember(string typeName, string memberName, IClassMember m)
            {
                UInt32 typeNameHash = mStringTable.HashOf(typeName);
                UInt32 memberNameHash = mStringTable.HashOf(memberName.ToLower());
                if (typeNameHash == UInt32.MaxValue || memberNameHash == UInt32.MaxValue)
                {
                    Console.WriteLine("Error: StringTable is missing some strings!");
                }

                mWriter.Write(typeNameHash);
                mWriter.Write(memberNameHash);
                m.Write(mOffsetOrValueWriter);
            }

            public void WriteNullMember(NullMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteBool8Member(BoolMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteInt8Member(Int8Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteInt16Member(Int16Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteInt32Member(Int32Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteInt64Member(Int64Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteEnumMember(EnumMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteFloatMember(FloatMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteStringMember(StringMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteObjectMember(ClassObject c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteAtomMember(AtomMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteCompoundMember(CompoundMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
        }

        #endregion
        #region Object writer

        private sealed class ObjectWriter
        {
            private readonly StringTable mStringTable;
            private readonly IDataWriter mWriter;

            private readonly ObjectMemberWriter mObjectMemberWriter;

            public ObjectWriter(StringTable inStringTable, FileIdTable inFileIdTable, IDataWriter inWriter)
            {
                mStringTable = inStringTable;
                mWriter = inWriter;
                mObjectMemberWriter = new ObjectMemberWriter(mStringTable, inFileIdTable, mWriter);
            }

            public bool Write(ClassObject c)
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
                    // Here: Sort members by hash of their name, so that in C++ we can do a
                    //       binary search to find the member by name/hash.
                    c.SortMembers(new MemberNameHashComparer(mStringTable));

                    mWriter.Write(c.Members.Count);

                    // Offset to members array
                    Int64 offsetsPos = mWriter.Position;
                    foreach (IClassMember m in c.Members)
                        mWriter.Write((short)0);
                    StreamUtils.Align(mWriter, sizeof(short));

                    List<short> memberOffsets = new (c.Members.Count);
                    foreach (IClassMember m in c.Members)
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

                return true;
            }

        }

        #endregion

        #region MemberBook

        sealed class MyMemberBook : MemberBook
        {
            public void HandoutReferences()
            {
                // Handout StreamReferences to classes, compounds and arrays taking care of equality of these objects.
                // Note: Strings are a bit of a special case since we also will collect the names of members and classes.

                Dictionary<object, ClassObject> referencesForClassesDict = new ();
                foreach (ClassObject c in Classes)
                {
                    ClassObject i = c;

                    if (i.IsDefault || i.Value == null)
                    {
                        i.Reference = StreamReference.Empty;
                    }
                    else
                    {
                        if (referencesForClassesDict.TryGetValue(i.Value, out ClassObject r))
                        {
                            i.Reference = r.Reference;
                        }
                        else
                        {
                            i.Reference = StreamReference.NewReference;
                            referencesForClassesDict.Add(i.Value, c);
                        }
                    }
                }

                Dictionary<object, StreamReference> referencesForCompoundsDict = new Dictionary<object, StreamReference>();
                foreach (CompoundMember c in Compounds)
                {
                    if (c.IsNullType)
                    {
                        if (c.Value != null)
                        {
                            if (referencesForCompoundsDict.TryGetValue(c.Value, out StreamReference reference))
                            {
                                c.Reference = reference;
                            }
                            else
                            {
                                c.Reference = StreamReference.NewReference;
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
                        c.Reference = StreamReference.NewReference;
                    }
                }

                foreach (AtomMember c in Atoms)
                {
                }

                //Dictionary<Int64, StreamReference> referencesForFileIdDict = new ();
                //foreach (FileIdMember c in fileids)
                //{
                //    StreamReference reference;
                //    if (referencesForFileIdDict.TryGetValue(c.ID, out reference))
                //    {
                //        c.Reference = reference;
                //    }
                //    else
                //    {
                //        c.Reference = StreamReference.Instance;
                //        referencesForFileIdDict.Add(c.ID, c.Reference);
                //    }
                //}

                Dictionary<object, StreamReference> referencesForArraysDict = new ();
                foreach (ArrayMember a in Arrays)
                {
                    if (a.IsDefault || a.Value == null)
                    {
                        a.Reference = StreamReference.Empty;
                    }
                    else
                    {
                        if (referencesForArraysDict.TryGetValue(a.Value, out StreamReference reference))
                        {
                            a.Reference = reference;
                        }
                        else
                        {
                            a.Reference = StreamReference.NewReference;
                            referencesForArraysDict.Add(a.Value, a.Reference);
                        }
                    }
                }
            }
        }

        #endregion

        #region Generic writer

        public bool Write(EEndian endian, object inData, string dataFilename, string relocFilename)
        {
            FileInfo dataFileInfo = new(dataFilename);
            FileStream dataStream = new(dataFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            IBinaryWriter resourceDataWriter = EndianUtils.CreateBinaryWriter(dataStream, endian);

            FileInfo reallocTableFileInfo = new(relocFilename);
            FileStream reallocTableStream = new(reallocTableFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 2 * 1024 * 1024);
            IBinaryWriter resourceDataReallocTableWriter = EndianUtils.CreateBinaryWriter(reallocTableStream, endian);

            try
            {
                IMemberGenerator genericMemberGenerator = new GenericMemberGenerator();

                // Analyze Root and generate a list of 'Code.Class' objects from this.
                Reflector reflector = new(genericMemberGenerator);

                MyMemberBook book = new();
                reflector.Analyze(inData, book);
                book.HandoutReferences();

                // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
                StringTable stringTable = new();
                stringTable.Reference = StreamReference.NewReference;

                // The FileIdTable to collect (and collapse duplicate) all FileIds
                FileIdTable fileIdTable = new();
                fileIdTable.Reference = StreamReference.NewReference;

                // Database of offsets of references written in the stream as well as the offsets of references to those references
                IDataWriter dataWriter = EndianUtils.CreateDataWriter(mEndian);

                ClassObject rootClass = book.Classes[0];

                dataWriter.Begin();
                {
                    // Header=
                    //     FileIdTable , void*, 32 bit
                    //     StringTable , void*, 32 bit
                    //     ResourceRoot, void*, 32 bit

                    dataWriter.Write(fileIdTable.Reference);
                    dataWriter.Write(stringTable.Reference);
                    dataWriter.Write(rootClass.Reference); // The root class

                    // Collect all strings (class names + member names)
                    // Collect all strings from member content
                    StringTableWriter strTableWriter = new(stringTable);
                    rootClass.Write(strTableWriter);
                    stringTable.SortByHash();

                    // Write 'SResource' array
                    ObjectWriter genericObjectTreeWriter = new(stringTable, fileIdTable, dataWriter);
                    for (int i = 0; i < book.Classes.Count; i++)
                        genericObjectTreeWriter.Write(book.Classes[i]);

                    // Write 'SResource' member data
                    ObjectMemberReferenceDataWriter objectMemberDataWriter = new ObjectMemberReferenceDataWriter(stringTable, fileIdTable, dataWriter);
                    rootClass.Write(objectMemberDataWriter);

                    // Write StringTable and FileIdTable
                    stringTable.Write(dataWriter);
                    fileIdTable.Write(dataWriter);
                }
                dataWriter.End();

                // Validate member data
                MemberDataValidator memberDataValidator = new();
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
                            resourceDataReallocTableWriter.Write(p.Value[i].Offset);
                    }
                }

                if (unresolvedReferences.Count > 0)
                {
                    UnresolvedReferencesLogger logger = new(stringTable, unresolvedReferences);
                    foreach (ClassObject c in book.Classes)
                        c.Write(logger);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[GenericDataStream:EXCEPTION] {0}", e.Message);
            }
            finally
            {
                reallocTableStream.Flush();
                resourceDataReallocTableWriter.Close();
                reallocTableStream.Close();

                dataStream.Flush();
                resourceDataWriter.Close();
                dataStream.Close();
            }

            return true;
        }

        #endregion
    }

    #region Generic MemberGenerator

    public sealed class GenericMemberGenerator : IMemberGenerator
    {
        #region Fields


        #endregion
        #region Constructor

        public GenericMemberGenerator()
        {
        }

        #endregion
        #region Private Methods

        public bool IsNull(Type t) { return (t == null); }
        public bool IsBool(Type t) { return t == typeof(bool); }
        public bool IsInt8(Type t) { return t == typeof(SByte); }
        public bool IsUInt8(Type t) { return t == typeof(Byte); }
        public bool IsInt16(Type t) { return t == typeof(Int16); }
        public bool IsUInt16(Type t) { return t == typeof(UInt16); }
        public bool IsInt32(Type t) { return t == typeof(Int32); }
        public bool IsUInt32(Type t) { return t == typeof(UInt32); }
        public bool IsInt64(Type t) { return t == typeof(Int64); }
        public bool IsUInt64(Type t) { return t == typeof(UInt64); }
        public bool IsFloat(Type t) { return t == typeof(float); }
        public bool IsDouble(Type t) { return t == typeof(double); }
        public bool IsString(Type t) { return t == typeof(string); }
        public bool IsEnum(Type t) { return t.IsEnum; }
        public bool IsArray(Type t)
        {
            return (!t.IsPrimitive && t.IsArray);
        }
        public bool IsGenericList(Type t)
        {
            return (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>)));
        }
        public bool IsObject(Type t) { return t.IsClass && !t.IsArray && !IsString(t); }
        public bool IsAtom(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.IAtom)); }
        public bool IsFileId(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.IFileId)); }
        public bool IsCompound(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.ICompound)); }

        public IClassMember NewNullMember(string memberName) { return new NullMember(memberName); }
        public IClassMember NewBoolMember(bool o, string memberName) { return new BoolMember(memberName, o); }
        public IClassMember NewInt8Member(SByte o, string memberName) { return new Int8Member(memberName, o); }
        public IClassMember NewUInt8Member(Byte o, string memberName) { return new UInt8Member(memberName, o); }
        public IClassMember NewInt16Member(Int16 o, string memberName) { return new Int16Member(memberName, o); }
        public IClassMember NewUInt16Member(UInt16 o, string memberName) { return new UInt16Member(memberName, o); }
        public IClassMember NewInt32Member(Int32 o, string memberName) { return new Int32Member(memberName, o); }
        public IClassMember NewUInt32Member(UInt32 o, string memberName) { return new UInt32Member(memberName, o); }
        public IClassMember NewInt64Member(Int64 o, string memberName) { return new Int64Member(memberName, o); }
        public IClassMember NewUInt64Member(UInt64 o, string memberName) { return new UInt64Member(memberName, o); }
        public IClassMember NewFloatMember(float o, string memberName) { return new FloatMember(memberName, o); }
        public IClassMember NewDoubleMember(double o, string memberName) { return new DoubleMember(memberName, o); }
        public IClassMember NewStringMember(string o, string memberName) { return new StringMember(memberName, o); }
        public IClassMember NewFileIdMember(Int64 o, string memberName) { return new FileIdMember(memberName, o); }
        public IClassMember NewEnumMember(object o, string memberName) { return new EnumMember(memberName, o.GetType(), o); }

        #endregion
        #region IMemberGenerator methods

        public ClassObject NewObjectMember(Type classType, object content, string memberName)
        {
            string className = classType.Name;
            ClassObject classMember = new (classType, content, className, memberName);
            return classMember;
        }

        public ArrayMember NewArrayMember(Type arrayType, object content, string memberName)
        {
            ArrayMember arrayMember = new(arrayType, content, memberName);
            return arrayMember;
        }

        public AtomMember NewAtomMember(Type atomType, IClassMember atomContentMember, string memberName)
        {
            AtomMember atom = new(memberName, atomType, atomContentMember);
            return atom;
        }
        public FileIdMember NewFileIdMember(Type fileidType, Int64 content, string memberName)
        {
            FileIdMember fileid = new(memberName, content);
            return fileid;
        }
        public CompoundMember NewCompoundMember(Type compoundType, object compoundObject, string memberName)
        {
            CompoundMember compoundMember = new(compoundObject, compoundType, memberName);
            return compoundMember;
        }

        #endregion
    }

    #endregion
}
