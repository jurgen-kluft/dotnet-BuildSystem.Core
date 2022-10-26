using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using DataBuildSystem;
using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        #region IMemberGenerator

        public interface IMemberGenerator
        {
            bool IsNull(Type t);
            bool IsBool(Type t);
            bool IsInt8(Type t);
            bool IsUInt8(Type t);
            bool IsInt16(Type t);
            bool IsUInt16(Type t);
            bool IsInt32(Type t);
            bool IsUInt32(Type t);
            bool IsInt64(Type t);
            bool IsUInt64(Type t);
            bool IsFloat(Type t);
            bool IsDouble(Type t);
            bool IsString(Type t);
            bool IsEnum(Type t);
            bool IsArray(Type t);
            bool IsGenericList(Type t);
            bool IsObject(Type t);
            bool IsIStruct(Type t);
            bool IsFileId(Type t);

            IClassMember NewNullMember(string memberName);
            IClassMember NewBoolMember(bool content, string memberName);
            IClassMember NewInt8Member(SByte content, string memberName);
            IClassMember NewUInt8Member(Byte content, string memberName);
            IClassMember NewInt16Member(Int16 content, string memberName);
            IClassMember NewUInt16Member(UInt16 content, string memberName);
            IClassMember NewInt32Member(Int32 content, string memberName);
            IClassMember NewUInt32Member(UInt32 content, string memberName);
            IClassMember NewInt64Member(Int64 content, string memberName);
            IClassMember NewUInt64Member(UInt64 content, string memberName);
            IClassMember NewEnumMember(object content, string memberName);
            IClassMember NewFloatMember(float content, string memberName);
            IClassMember NewDoubleMember(double content, string memberName);
            IClassMember NewStringMember(string content, string memberName);
            FileIdMember NewFileIdMember(Int64 content, string memberName);
            ClassObject NewObjectMember(Type objectType, object content, string memberName);
            ArrayMember NewArrayMember(Type arrayType, object content, string memberName);
            StructMember NewStructMember(IStruct content, string memberName);
        }

        #endregion

        #region IMemberWriter

        public interface IMemberWriter
        {
            bool Open();
            bool Close();

            void WriteNullMember(NullMember c);
            void WriteBoolMember(BoolMember c);
            void WriteBitSetMember(BitSetMember c);
            void WriteInt8Member(Int8Member c);
            void WriteInt16Member(Int16Member c);
            void WriteInt32Member(Int32Member c);
            void WriteInt64Member(Int64Member c);
            void WriteUInt8Member(UInt8Member c);
            void WriteUInt16Member(UInt16Member c);
            void WriteUInt32Member(UInt32Member c);
            void WriteUInt64Member(UInt64Member c);
            void WriteEnumMember(EnumMember c);
            void WriteFloatMember(FloatMember c);
            void WriteDoubleMember(DoubleMember c);
            void WriteStringMember(StringMember c);
            void WriteFileIdMember(FileIdMember c);
            void WriteStructMember(StructMember c);

            void WriteArrayMember(ArrayMember c);
            void WriteObjectMember(ClassObject c);
        }

        #endregion

        #region MemberComparer (size and hash-of-name)

        public class SortByMemberAlignment : IComparer<IClassMember>
        {
            public int Compare(IClassMember x, IClassMember y)
            {
                var xa = x.Alignment;
                if (x.IsPointerTo)
                {
                    xa = EndianUtils.IsPlatform64Bit(BuildSystemCompilerConfig.Platform) ? 8 : 4;
                }

                var ya = y.Alignment;
                if (y.IsPointerTo)
                {
                    ya = EndianUtils.IsPlatform64Bit(BuildSystemCompilerConfig.Platform) ? 8 : 4;
                }

                if (xa == ya) return 0;
                if (xa < ya) return 1;
                return -1;
            }
        }

        public class MemberNameHashComparer : IComparer<IClassMember>
        {
            private StringTable mStringTable;

            public MemberNameHashComparer(StringTable strTable)
            {
                mStringTable = strTable;
            }

            public int Compare(IClassMember x, IClassMember y)
            {
                uint hx = mStringTable.HashOf(x.MemberName);
                uint hy = mStringTable.HashOf(y.MemberName);
                if (hx == hy) return 0;
                else if (hx < hy) return 1;
                else return -1;
            }
        }

        #endregion

        #region IClassMember

        public interface IClassMember
        {
            string MemberName { get; }
            Type MemberType { get; }
            string TypeName { get; }
            int Size { get; }
            int Alignment { get; }
            object Value { get; }
            bool IsPointerTo { get; set; }
            StreamReference Reference { get; }

            void Write(IMemberWriter writer);
        }

        #endregion

        #region MetaType.TypeInfo

        public static class MetaType
        {
            public static bool TypeInfo(Type type, out string name, out int size, out int alignment)
            {
                name = type.Name;
                if (type == typeof(bool))
                {
                    alignment = 4;
                    size = 4;
                    name = "bool_t";
                }
                else if (type == typeof(byte))
                {
                    alignment = 1;
                    size = 1;
                    name = "u8";
                }
                else if (type == typeof(sbyte))
                {
                    alignment = 1;
                    size = 1;
                    name = "s8";
                }
                else if (type == typeof(ushort))
                {
                    alignment = 2;
                    size = 2;
                    name = "u16";
                }
                else if (type == typeof(short))
                {
                    alignment = 2;
                    size = 2;
                    name = "s16";
                }
                else if (type == typeof(uint))
                {
                    alignment = 4;
                    size = 4;
                    name = "u32";
                }
                else if (type == typeof(int))
                {
                    alignment = 4;
                    size = 4;
                    name = "s32";
                }
                else if (type == typeof(ulong))
                {
                    alignment = 8;
                    size = 8;
                    name = "u64";
                }
                else if (type == typeof(long))
                {
                    alignment = 8;
                    size = 8;
                    name = "s64";
                }
                else if (type == typeof(float))
                {
                    alignment = 4;
                    size = 4;
                    name = "f32";
                }
                else if (type == typeof(double))
                {
                    alignment = 8;
                    size = 8;
                    name = "f64";
                }
                else if (type == typeof(string))
                {
                    alignment = 4;
                    size = 0;
                    name = "string_t";
                }
                else if (type == typeof(Enum))
                {
                    return TypeInfo(Enum.GetUnderlyingType(type), out name, out size, out alignment);
                }
                else
                {
                    alignment = 0;
                    size = 0;
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region NullMember

        public sealed class NullMember : IClassMember
        {
            public NullMember(string name)
            {
                MemberName = name;
                MemberType = null;
                Alignment = sizeof(Int32);
            }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }
            public string TypeName => "void";

            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value => null;
            public bool IsPointerTo { get; set; }

            public StreamReference Reference { get; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteNullMember(this);
            }
        }

        #endregion

        #region BoolMember

        public sealed class BoolMember : IClassMember
        {
            public BoolMember(string name, bool value)
            {
                MemberName = name;
                Size = sizeof(Int32);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int32);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(bool);
            public string TypeName => "bool";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public bool InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteBoolMember(this);
            }
        }

        #endregion

        #region BitSetMember

        public sealed class BitSetMember : IClassMember
        {
            public BitSetMember(List<BoolMember> members)
            {
                MemberName = members[0].MemberName;
                Size = sizeof(uint);
                InternalValue = 0;
                uint bit = 1;
                foreach (var b in members)
                {
                    if (b.InternalValue)
                    {
                        InternalValue = (InternalValue | bit);
                    }
                    bit <<= 1;
                }
                Value = InternalValue;
                Alignment = sizeof(uint);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(uint);
            public string TypeName => "u32";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public uint InternalValue { get; private set; }
            public bool IsPointerTo { get; set; } = false;
            public List<BoolMember> Members { get; set; }       

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteBitSetMember(this);
            }
        }

        #endregion
        #region Int8Member

        public sealed class Int8Member : IClassMember
        {
            public Int8Member(string name, Int8 value)
            {
                MemberName = name;
                Size = sizeof(Int8);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int8);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(Int8);
            public string TypeName => "s8";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public Int8 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteInt8Member(this);
            }
        }

        #endregion

        #region Int16Member

        public sealed class Int16Member : IClassMember
        {
            public Int16Member(string name, Int16 value)
            {
                MemberName = name;
                Size = sizeof(Int16);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int16);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(Int16);
            public string TypeName => "s16";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public Int16 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteInt16Member(this);
            }
        }

        #endregion

        #region Int32Member

        public sealed class Int32Member : IClassMember
        {
            public Int32Member(string name, Int32 value)
            {
                MemberName = name;
                Size = sizeof(Int32);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int32);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(Int32);
            public string TypeName => "s32";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public Int32 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteInt32Member(this);
            }
        }

        #endregion

        #region Int64Member

        public sealed class Int64Member : IClassMember
        {
            public Int64Member(string name, Int64 value)
            {
                MemberName = name;
                Size = sizeof(Int64);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int64);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(Int64);
            public string TypeName => "s64";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public Int64 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteInt64Member(this);
            }
        }

        #endregion

        #region UInt8Member

        public sealed class UInt8Member : IClassMember
        {
            public UInt8Member(string name, UInt8 value)
            {
                MemberName = name;
                Size = sizeof(UInt8);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt8);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(UInt8);
            public string TypeName => "u8";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public UInt8 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteUInt8Member(this);
            }
        }

        #endregion

        #region UInt16Member

        public sealed class UInt16Member : IClassMember
        {
            public UInt16Member(string name, UInt16 value)
            {
                MemberName = name;
                Size = sizeof(UInt16);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt16);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(UInt16);
            public string TypeName => "u16";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public UInt16 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteUInt16Member(this);
            }
        }

        #endregion

        #region UInt32Member

        public sealed class UInt32Member : IClassMember
        {
            public UInt32Member(string name, UInt32 value)
            {
                MemberName = name;
                Size = sizeof(UInt32);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt32);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(UInt32);
            public string TypeName => "u32";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public UInt32 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteUInt32Member(this);
            }
        }

        #endregion

        #region UInt64Member

        public sealed class UInt64Member : IClassMember
        {
            public UInt64Member(string name, UInt64 value)
            {
                MemberName = name;
                Size = sizeof(UInt64);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt64);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(UInt64);
            public string TypeName => "u64";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public UInt64 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteUInt64Member(this);
            }
        }

        #endregion

        #region EnumMember

        public sealed class EnumMember : IClassMember
        {
            public EnumMember(string name, Type enumType, object value)
            {
                MemberName = name;
                MemberType = enumType;
                EnumType = enumType;
                EnumValueType = System.Enum.GetUnderlyingType(enumType);
                MetaType.TypeInfo(EnumValueType, out string enumValueTypeName, out int size, out int alignment);
                EnumValueTypeName = enumValueTypeName;
                Size = size;
                Alignment = alignment;
                Value = System.Convert.ChangeType(value, EnumValueType);
            }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }
            public string TypeName => "rawenum_t<" + EnumType.Name + ", " + EnumValueTypeName + ">";
            public Type EnumType { get; private set; }
            public Type EnumValueType { get; set; }
            public string EnumValueTypeName { get; set; }
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; set; }
            public bool IsPointerTo { get; set; }
            public UInt64 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteEnumMember(this);
            }
        }

        #endregion

        #region FloatMember

        public sealed class FloatMember : IClassMember
        {
            public FloatMember(string name, float value)
            {
                MemberName = name;
                Size = sizeof(float);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(float);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(float);
            public string TypeName => "f32";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public float InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteFloatMember(this);
            }
        }

        #endregion

        #region DoubleMember

        public sealed class DoubleMember : IClassMember
        {
            public DoubleMember(string name, double value)
            {
                MemberName = name;
                Size = sizeof(double);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(double);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(Double);
            public string TypeName => "f64";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public double InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteDoubleMember(this);
            }
        }

        #endregion

        #region StringMember

        [DebuggerDisplay("String: \"{InternalValue}\", Name: {MemberName}, Pointer: {IsPointerTo}")]
        public sealed class StringMember : IClassMember
        {
            public StringMember(string name, string value)
            {
                MemberName = name;
                Size = sizeof(Int32) + sizeof(Int32);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int32) + sizeof(Int32);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(String);
            public string TypeName => "rawstr_t";
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public string InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteStringMember(this);
            }
        }

        #endregion

        #region FileIdMember

        public sealed class FileIdMember : IClassMember
        {
            public FileIdMember(string name, Int64 value)
            {
                MemberName = name;
                Size = sizeof(Int64);
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int64);
            }

            public string MemberName { get; private set; }
            public Type MemberType => typeof(FileId);
            public string TypeName => "fileid_t";
            public int Size { get; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public Int64 InternalValue { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteFileIdMember(this);
            }
        }

        #endregion

        #region StructMember

        [DebuggerDisplay("Struct {TypeName}, {MemberName} {IsPointerTo}")]
        public sealed class StructMember : IClassMember
        {
            public StructMember(IStruct content, string memberName)
            {
                MemberName = memberName;
                MemberType = typeof(IStruct);
                Value = content;
                Internal = content;
            }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }
            public string TypeName => Internal.StructName;
            public int Size => Internal.StructSize;
            public Int32 Alignment => Internal.StructAlign;
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }
            public IStruct Internal { get; private set; }

            public StreamReference Reference { get; set; }

            public void Write(IMemberWriter writer)
            {
                writer.WriteStructMember(this);
            }
        }

        #endregion

        #region CompoundMemberBase

        public interface ICompoundMemberBase
        {
            void AddMember(IClassMember m);
        }

        #endregion

        #region ArrayMember

        public class ArrayMember : ICompoundMemberBase, IClassMember
        {
            #region Constructor

            public ArrayMember(Type arrayType, object value, string memberName)
            {
                MemberName = memberName;
                MemberType = arrayType;
                Value = value;
                Alignment = sizeof(Int32) + sizeof(Int32);
                Size = sizeof(Int32) + sizeof(Int32);
                Members = new();
            }

            #endregion

            #region Properties

            public StreamReference Reference { get; set; }

            public StreamReference ArrayDataReference { get; set; }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }

            public IClassMember Element
            {
                get { return Members[0]; }
            }

            public string TypeName => "rawarr_t<" + Element.TypeName + ">";
            public int Size { get; private set; }
            public int Alignment { get; private set; }
            public object Value { get; private set; }
            public bool IsPointerTo { get; set; }

            public List<IClassMember> Members { get; set; }

            #endregion

            #region Member methods

            public void AddMember(IClassMember m)
            {
                Members.Add(m);
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteArrayMember(this);
            }

            #endregion
        }

        #endregion

        #region ClassObject

        [DebuggerDisplay("Class {TypeName}, Pointer: {IsPointerTo}")]
        public sealed class ClassObject : ICompoundMemberBase, IClassMember
        {
            #region Constructors

            public ClassObject(Type type, object value, string className, string memberName)
            {
                MemberName = memberName;
                MemberType = type;
                TypeName = className;
                Size = 0;
                Alignment = 4;
                Value = value;
                IsPointerTo = true;
                Members = new();
            }

            #endregion

            #region Properties

            public string MemberName { get; }
            public Type MemberType { get; }
            public string TypeName { get; init; }
            public int Size { get; }
            public int Alignment { get; private set; }
            public object Value { get; }
            public bool IsPointerTo { get; set; }

            public StreamReference Reference { get; set; }

            public List<IClassMember> Members { get; private set; }

            #endregion

            #region Methods

            public void AddMember(IClassMember m)
            {
                Members.Add(m);
            }

            public void SortMembers(IComparer<IClassMember> c)
            {
                // Sort the members on their data size, from big to small
                Members.Sort(c);
            }
            public void DetermineAlignment()
            {
                // Determine the alignment of this Class
                if (Members.Count > 0)
                {
                    Alignment = Members[0].Alignment;
                }
            }

            public void CombineBooleans()
            {
                // Combine boolean members into max 32 boolean members
                foreach (var m in Members)
                {
                }
            }
            
            #endregion

            #region Member methods

            public void Write(IMemberWriter writer)
            {
                writer.WriteObjectMember(this);
            }

            #endregion
        }

        #endregion
    }
}
