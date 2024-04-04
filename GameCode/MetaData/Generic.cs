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

        private EPlatform Platform { get; set; }

        #endregion
        #region Constructor

        public StdDataStream(EPlatform platform)
        {
            Platform = platform;
        }

        #endregion
        #region Properties

        public static int SizeOfBool { get; set; }

        #endregion

        #region Unresolved References

        private sealed class UnresolvedReferencesLogger : IMemberWriter
        {
            private readonly StringTable _mStringTable;
            private readonly Dictionary<StreamReference, StreamContext> _mUnresolvedReferences;
            private readonly List<IClassMember> _mHierarchy = new List<IClassMember>();

            public UnresolvedReferencesLogger(StringTable stringTable, Dictionary<StreamReference, StreamContext> unresolvedReferences)
            {
                _mStringTable = stringTable;
                _mUnresolvedReferences = unresolvedReferences;
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

                foreach (IClassMember m in _mHierarchy)
                    parentStr += m.TypeName + "{" + m.MemberName + "}.";

                Console.WriteLine(parentStr + typeName + "{" + memberName + "} which was not resolved");
            }

            public void WriteNullMember(NullMember c)
            {
                // Has no reference
            }
            public void WriteBoolMember(BoolMember c)
            {
                // Has no reference
            }
            public void WriteBitSetMember(BitSetMember c)
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
                if (_mUnresolvedReferences.TryGetValue(_mStringTable.ReferenceOf(c.InternalValue), out ctx))
                    Log(c.MemberName, c.TypeName);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                // Has no reference
            }
            public void WriteArrayMember(ArrayMember c)
            {
                StreamContext ctx;
                if (_mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.MemberName, c.TypeName);

                _mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                _mHierarchy.RemoveAt(_mHierarchy.Count - 1);
            }
            public void WriteObjectMember(ClassObject c)
            {
                StreamContext ctx;
                if (_mUnresolvedReferences.TryGetValue(c.Reference, out ctx))
                    Log(c.MemberName, c.TypeName);

                _mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                _mHierarchy.RemoveAt(_mHierarchy.Count - 1);
            }
            public void WriteStructMember(StructMember c)
            {
                // Has no reference
            }
        }

        #endregion
        #region Object member data validation

        private sealed class MemberDataValidator : IMemberWriter
        {
            private readonly List<IClassMember> _mHierarchy = new List<IClassMember>();

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
                foreach (IClassMember m in _mHierarchy)
                    parentStr += "[" + m.MemberName + "].";

                Console.WriteLine(parentStr + "[" + memberName + "] " + errorStr, v);
                return false;
            }

            public void WriteNullMember(NullMember c)
            {
            }
            public void WriteBoolMember(BoolMember c)
            {
            }
            public void WriteBitSetMember(BitSetMember c)
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
                _mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                {
                    //m.range = c.range;
                    m.Write(this);
                }
                _mHierarchy.RemoveAt(_mHierarchy.Count - 1);
            }
            public void WriteObjectMember(ClassObject c)
            {
                _mHierarchy.Add(c);
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                _mHierarchy.RemoveAt(_mHierarchy.Count - 1);
            }
            public void WriteStructMember(StructMember c)
            {
                //c.member.range = c.range;
            }
        }

        #endregion

        #region StringTable writer

        public sealed class StringTableWriter : IMemberWriter
        {
            private readonly StringTable _mStringTable;

            private UInt32 _mDone = 0;
            private enum EDone : UInt32
            {
                None = 0,
                NullMemberDone = 1 << 1,
                Bool8MemberDone = 1 << 2,
                BitSetMemberDone = 1 << 3,
                Int8MemberDone = 1 << 4,
                Int16MemberDone = 1 << 5,
                Int32MemberDone = 1 << 6,
                Int64MemberDone = 1 << 7,
                UInt8MemberDone = 1 << 8,
                UInt16MemberDone = 1 << 9,
                UInt32MemberDone = 1 << 10,
                UInt64MemberDone = 1 << 11,
                EnumMemberDone = 1 << 12,
                FloatMemberDone = 1 << 13,
                DoubleMemberDone = 1 << 14,
                StringMemberDone = 1 << 15,
                FileIdMemberDone = 1 << 16,
            };

            private bool HasFlag(EDone d)
            {
                return (_mDone & (UInt32)d) == (UInt32)d;
            }

            private void SetFlag(EDone d)
            {
                _mDone = _mDone | (UInt32)d;
            }

            public StringTableWriter(StringTable strTable)
            {
                _mStringTable = strTable;
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
                    _mStringTable.Add("void");
                    _mStringTable.Add("void[]");
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteBoolMember(BoolMember c)
            {
                if (!HasFlag(EDone.Bool8MemberDone))
                {
                    SetFlag(EDone.Bool8MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteBitSetMember(BitSetMember c)
            {
                if (!HasFlag(EDone.BitSetMemberDone))
                {
                    SetFlag(EDone.BitSetMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt8Member(Int8Member c)
            {
                if (!HasFlag(EDone.Int8MemberDone))
                {
                    SetFlag(EDone.Int8MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt16Member(Int16Member c)
            {
                if (!HasFlag(EDone.Int16MemberDone))
                {
                    SetFlag(EDone.Int16MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt32Member(Int32Member c)
            {
                if (!HasFlag(EDone.Int32MemberDone))
                {
                    SetFlag(EDone.Int32MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteInt64Member(Int64Member c)
            {
                if (!HasFlag(EDone.Int64MemberDone))
                {
                    SetFlag(EDone.Int64MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                if (!HasFlag(EDone.UInt8MemberDone))
                {
                    SetFlag(EDone.UInt8MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                if (!HasFlag(EDone.UInt16MemberDone))
                {
                    SetFlag(EDone.UInt16MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                if (!HasFlag(EDone.UInt32MemberDone))
                {
                    SetFlag(EDone.UInt32MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                if (!HasFlag(EDone.UInt64MemberDone))
                {
                    SetFlag(EDone.UInt64MemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteEnumMember(EnumMember c)
            {
                if (!HasFlag(EDone.EnumMemberDone))
                {
                    SetFlag(EDone.EnumMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteFloatMember(FloatMember c)
            {
                if (!HasFlag(EDone.FloatMemberDone))
                {
                    SetFlag(EDone.FloatMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                if (!HasFlag(EDone.DoubleMemberDone))
                {
                    SetFlag(EDone.DoubleMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteStringMember(StringMember c)
            {
                if (!HasFlag(EDone.StringMemberDone))
                {
                    SetFlag(EDone.StringMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
                _mStringTable.Add(c.InternalValue);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                if (!HasFlag(EDone.FileIdMemberDone))
                {
                    SetFlag(EDone.FileIdMemberDone);
                    _mStringTable.Add(c.TypeName);
                }
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteArrayMember(ArrayMember c)
            {
                foreach (IClassMember m in c.Members)
                    m.Write(this);
                _mStringTable.Add(c.TypeName);
                _mStringTable.Add(c.MemberName.ToLower());
            }
            public void WriteObjectMember(ClassObject c)
            {
                foreach (IClassMember m in c.Members)
                    m.Write(this);

                _mStringTable.Add(c.TypeName);
                _mStringTable.Add(c.MemberName.ToLower());

            }
            public void WriteStructMember(StructMember c)
            {
                _mStringTable.Add(c.TypeName);
            }
        }

        #endregion

        #region Object member data writer

        public sealed class ObjectMemberDataWriter : IMemberWriter
        {
            private bool _mAlignMembers;
            private StringTable _mStringTable;
            private readonly FileIdTable _mFileIdTable;
            private IDataWriter _mWriter;

            public ObjectMemberDataWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                _mStringTable = stringTable;
                _mFileIdTable = fileIdTable;
                _mWriter = writer;
            }

            public bool AlignMembers
            {
                get => _mAlignMembers;
                set => _mAlignMembers = value;
            }

            public bool Open()
            {
                return _mStringTable != null && _mWriter != null;
            }

            public bool Close()
            {
                _mStringTable = null;
                _mWriter = null;
                return true;
            }

            private void WriteReference(StreamReference reference)
            {
                if (_mAlignMembers) Align(4);
                _mWriter.Write(reference);
            }

            private void Align(Int64 alignment)
            {
                StreamUtils.Align(_mWriter, alignment);
            }

            public void WriteNullMember(NullMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write((int)0);
            }
            public void WriteBoolMember(BoolMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write((int)(c.InternalValue ? 1 : 0));
            }
            public void WriteBitSetMember(BitSetMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write((int)(c.InternalValue));
            }
            public void WriteInt8Member(Int8Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteInt16Member(Int16Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteInt32Member(Int32Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteInt64Member(Int64Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteEnumMember(EnumMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                ulong e = c.InternalValue;
                switch (c.Alignment)
                {
                    case 1:  _mWriter.Write((byte)e); break;
                    case 2:  _mWriter.Write((ushort)e); break;
                    case 4:  _mWriter.Write((uint)e); break;
                    case 8:  _mWriter.Write(e); break;
                }
            }
            public void WriteFloatMember(FloatMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteStringMember(StringMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                WriteReference(_mStringTable.ReferenceOf(c.InternalValue));
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.InternalValue);
            }
            public void WriteStructMember(StructMember c)
            {
                c.Internal.StructWrite(_mWriter);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                if (_mAlignMembers) Align(c.Alignment);
                _mWriter.Write(c.Members.Count);
                _mWriter.Write(c.Reference);
            }
            public void WriteObjectMember(ClassObject c)
            {
                WriteReference(c.Reference);
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
            private readonly StringTable _mStringTable;
            private readonly IDataWriter _mWriter;
            private readonly ObjectMemberDataWriter _mObjectMemberDataWriter;

            public ObjectMemberReferenceDataWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                _mStringTable = stringTable;

                _mWriter = writer;
                _mObjectMemberDataWriter = new (_mStringTable, fileIdTable, _mWriter)
                {
                    AlignMembers = true
                };
            }

            public bool Open()
            {
                return _mStringTable != null && _mWriter != null;
            }

            public bool Close()
            {
                return true;
            }

            public void WriteNullMember(NullMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteBoolMember(BoolMember c)
            {
                // Embedded in the Member.OffsetOrValue as value
            }
            public void WriteBitSetMember(BitSetMember c)
            {
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
            public void WriteStructMember(StructMember c)
            {
            }
            public void WriteArrayMember(ArrayMember c)
            {
                // The reference of this member can be null!
                if (c.Reference != StreamReference.Empty && _mWriter.BeginBlock(c.Reference, c.Alignment))
                {
                    foreach (IClassMember m in c.Members)
                    {
                        m.Write(_mObjectMemberDataWriter);
                    }
                    _mWriter.EndBlock();
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
        }

        #endregion

        #region Object member offset or value writer

        public sealed class ObjectMemberOffsetOrValueWriter : IMemberWriter
        {
            private StringTable _mStringTable;
            private readonly FileIdTable _mFileIdTable;
            private IDataWriter _mWriter;

            public ObjectMemberOffsetOrValueWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                _mStringTable = stringTable;
                _mFileIdTable = fileIdTable;
                _mWriter = writer;
            }

            public bool Open()
            {
                return _mStringTable != null && _mWriter != null;
            }

            public bool Close()
            {
                _mStringTable = null;
                _mWriter = null;
                return true;
            }

            /// Member ->
            ///     short mValueType;
            ///     short mStringIndex;
            ///     int mOffsetOrValue;

            private bool WriteReferenceMember(StreamReference reference)
            {
                _mWriter.Write(reference);
                return true;
            }

            public bool WriteInt32(int v)
            {
                _mWriter.Write(v);
                return true;
            }

            public void WriteNullMember(NullMember c)
            {
                WriteInt32(0);
            }
            public void WriteBoolMember(BoolMember c)
            {
                WriteInt32(c.InternalValue ? 0 : 1);
            }
            public void WriteBitSetMember(BitSetMember c)
            {
                _mWriter.Write(c.InternalValue);
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
                _mWriter.Write(c.InternalValue);
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
                _mWriter.Write(c.InternalValue);
            }
            public void WriteEnumMember(EnumMember c)
            {
                ulong e = c.InternalValue;
                switch (c.Alignment)
                {
                    case 1:  _mWriter.Write((byte)e); break;
                    case 2:  _mWriter.Write((ushort)e); break;
                    case 4:  _mWriter.Write((uint)e); break;
                    case 8:  _mWriter.Write(e); break;
                }
            }
            public void WriteFloatMember(FloatMember c)
            {
                _mWriter.Write(c.InternalValue);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                _mWriter.Write(c.InternalValue);
            }
            public void WriteStringMember(StringMember c)
            {
                WriteReferenceMember(_mStringTable.ReferenceOf(c.InternalValue));
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                _mWriter.Write(c.InternalValue);
            }
            public void WriteStructMember(StructMember c)
            {
                c.Internal.StructWrite(_mWriter);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                _mWriter.Write(c.Members.Count);
                _mWriter.Write(c.Reference);
            }
            public void WriteObjectMember(ClassObject c)
            {
                WriteReferenceMember(c.Reference);
            }
        }

        #endregion
        #region Object member writer

        private sealed class ObjectMemberWriter : IMemberWriter
        {
            private readonly StringTable _mStringTable;
            private readonly IDataWriter _mWriter;

            private readonly ObjectMemberOffsetOrValueWriter _mOffsetOrValueWriter;

            public ObjectMemberWriter(StringTable stringTable, FileIdTable fileIdTable, IDataWriter writer)
            {
                _mStringTable = stringTable;
                _mWriter = writer;
                _mOffsetOrValueWriter = new ObjectMemberOffsetOrValueWriter(stringTable, fileIdTable, writer);
            }

            public bool Open()
            {
                return _mStringTable != null && _mWriter != null;
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
                UInt32 typeNameHash = _mStringTable.HashOf(typeName);
                UInt32 memberNameHash = _mStringTable.HashOf(memberName.ToLower());
                if (typeNameHash == UInt32.MaxValue || memberNameHash == UInt32.MaxValue)
                {
                    Console.WriteLine("Error: StringTable is missing some strings!");
                }

                _mWriter.Write(typeNameHash);
                _mWriter.Write(memberNameHash);
                m.Write(_mOffsetOrValueWriter);
            }

            public void WriteNullMember(NullMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteBoolMember(BoolMember c)
            {
                WriteMember(c.TypeName, c.MemberName, c);
            }
            public void WriteBitSetMember(BitSetMember c)
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
            public void WriteStructMember(StructMember c)
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
        }

        #endregion
        #region Object writer

        private sealed class ObjectWriter
        {
            private readonly StringTable _mStringTable;
            private readonly IDataWriter _mWriter;

            private readonly ObjectMemberWriter _mObjectMemberWriter;

            public ObjectWriter(StringTable inStringTable, FileIdTable inFileIdTable, IDataWriter inWriter)
            {
                _mStringTable = inStringTable;
                _mWriter = inWriter;
                _mObjectMemberWriter = new ObjectMemberWriter(_mStringTable, inFileIdTable, _mWriter);
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
                if (c.Reference != StreamReference.Empty && _mWriter.BeginBlock(c.Reference, c.Alignment))
                {
                    // Here: Sort members by hash of their name, so that in C++ we can do a
                    //       binary search to find the member by name/hash.
                    c.SortMembers(new MemberNameHashComparer(_mStringTable));

                    _mWriter.Write(c.Members.Count);

                    // Offset to members array
                    var offsetsPos = _mWriter.Position;
                    foreach (var m in c.Members)
                        _mWriter.Write((short)0);
                    StreamUtils.Align(_mWriter, sizeof(short));

                    int alignment = 4;

                    List<short> memberOffsets = new (c.Members.Count);
                    foreach (var m in c.Members)
                    {
                        StreamUtils.Align(_mWriter, alignment);
                        memberOffsets.Add((short)(_mWriter.Position - offsetsPos));
                        m.Write(_mObjectMemberWriter);
                    }

                    // Seek back to the member offset array and write in the obtained
                    // member offsets and when done seek back to the end position.
                    var endPos = _mWriter.Position;
                    {
                        _mWriter.Seek(new StreamOffset(offsetsPos));
                        foreach (short offset in memberOffsets)
                            _mWriter.Write(offset);
                    }
                    _mWriter.Seek(new StreamOffset(endPos));

                    _mWriter.EndBlock();
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

                    if (i.Value == null)
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

                Dictionary<object, StreamReference> referencesForArraysDict = new ();
                foreach (ArrayMember a in Arrays)
                {
                    if (a.Value == null)
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

        public bool Write(EPlatform platform, object inData, string dataFilename, string relocFilename)
        {
            FileInfo dataFileInfo = new(dataFilename);
            FileStream dataStream = new(dataFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            IBinaryStream resourceDataWriter = EndianUtils.CreateBinaryStream(dataStream, platform);

            FileInfo reallocTableFileInfo = new(relocFilename);
            FileStream reallocTableStream = new(reallocTableFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 2 * 1024 * 1024);
            IBinaryStream resourceDataReallocTableWriter = EndianUtils.CreateBinaryStream(reallocTableStream, platform);

            try
            {
                IMemberFactory genericMemberFactory = new GenericMemberFactory();
                ITypeInformation typeInformation = new GenericTypeInformation();

                // Analyze Root and generate a list of 'Code.Class' objects from this.
                Reflector reflector = new(genericMemberFactory, typeInformation);

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
                IDataWriter dataWriter = EndianUtils.CreateDataWriter(platform);

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

        public sealed class GenericTypeInformation : ITypeInformation
        {
        #region Constructor

        public GenericTypeInformation()
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
        public bool IsGenericDictionary(Type t)
        {
            return (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }
        public bool IsClass(Type t) { return (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum)) && (!t.IsArray && !IsString(t)); }
        public bool IsStruct(Type t) { return (t.IsValueType && !t.IsPrimitive && !t.IsEnum) && (!t.IsArray && !IsString(t) && (t != typeof(decimal))&& (t != typeof(DateTime))); }
        public bool IsFileId(Type t) { return !t.IsPrimitive && Reflector.HasGenericInterface(t, typeof(GameData.IFileId)); }
        public bool IsIStruct(Type t) { return Reflector.HasGenericInterface(t, typeof(GameData.IStruct)); }


        #endregion
    }

    public sealed class GenericMemberFactory : IMemberFactory
    {
        #region Fields


        #endregion
        #region Constructor

        public GenericMemberFactory()
        {
        }

        #endregion
        #region IMemberGenerator methods

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
        public IClassMember NewEnumMember(object o, string memberName) { return new EnumMember(memberName, o.GetType(), o); }

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

        public StructMember NewStructMember(Type type, object content, string memberName)
        {
            StructMember atom = new(content as IStruct, memberName);
            return atom;
        }
        public FileIdMember NewFileIdMember(Type type, object content, string memberName)
        {
            FileIdMember fileid = new(memberName, (long)content);
            return fileid;
        }

        #endregion
    }

    #endregion
}
