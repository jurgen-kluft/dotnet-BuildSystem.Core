using System;
using System.Reflection;
using System.Collections.Generic;

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
            bool IsAtom(Type t);
            bool IsFileId(Type t);
            bool IsCompound(Type t);

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
            IClassMember NewFileIdMember(Int64 content, string memberName);
            ClassObject NewObjectMember(Type objectType, object content, string memberName);
            ArrayMember NewArrayMember(Type arrayType, object content, string memberName);
            AtomMember NewAtomMember(Type atomType, IClassMember atomContentMember, string memberName);
            FileIdMember NewFileIdMember(Type atomType, Int64 content, string memberName);
            CompoundMember NewCompoundMember(Type compoundType, object content, string memberName);
        }

        #endregion
        #region IMemberWriter

        public interface IMemberWriter
        {
            bool Open();
            bool Close();

            void WriteNullMember(NullMember c);
            void WriteBool8Member(BoolMember c);
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
            void WriteArrayMember(ArrayMember c);
            void WriteObjectMember(ClassObject c);
            void WriteAtomMember(AtomMember c);
            void WriteCompoundMember(CompoundMember c);
        }

        #endregion

        #region MemberComparer (size and hash-of-name)

        public class SortByMemberAlignment : IComparer<IClassMember>
        {
            public int Compare(IClassMember x, IClassMember y)
            {
                if (x.Alignment == y.Alignment) return 0;
                else if (x.Alignment < y.Alignment) return 1;
                else return -1;
            }
        }

        public class MemberNameHashComparer : IComparer<IClassMember>
        {
            // Summary:
            //     Compares two objects and returns a offset indicating whether one is less than,
            //     equal to, or greater than the other.
            //
            // Parameters:
            //   x:
            //     The first object to compare.
            //
            //   y:
            //     The second object to compare.
            //
            // Returns:
            //     Value Condition Less than zero x is less than y. Zero x equals y. Greater
            //     than zero x is greater than y.
            //
            // Exceptions:
            //   None
            //
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
            string MemberName { get;  }
            Type MemberType { get;  }
            string TypeName { get; }
            int Size { get; }
            int Alignment { get; }
            object Value { get; }

            bool IsDefault { get; }
            IClassMember Default();

            void Write(IMemberWriter writer);
        }

        #endregion

        public struct MetaType
		{
            public string Name { get; private set; }
            public int Alignment { get; private set; }
            public int Size { get; private set; }

            public static MetaType TypeInfo(Type type)
			{
                string name = type.Name;
                int alignment;
                int size;
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
                else if (type == typeof(Enum))
				{
                    return TypeInfo(Enum.GetUnderlyingType(type));
                }
                else
				{
                    name = String.Empty;
                    alignment = 0;
                    size = 0;
				}

                return new MetaType() { Name = name, Alignment = alignment, Size = size };
			}
		}

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

            public bool IsDefault => true;

            public IClassMember Default()
            {
                return new NullMember(MemberName);
            }

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
            public bool InternalValue { get; private set; }

            public bool IsDefault => InternalValue == false;

            public IClassMember Default()
            {
                return new BoolMember(MemberName, false);
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteBool8Member(this);
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
            public Int8 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int8Member(MemberName, 0);
            }

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
            public Int16 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int16Member(MemberName, 0);
            }

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
            public Int32 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int32Member(MemberName, 0);
            }

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
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int64Member(MemberName, 0);
            }

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
            public UInt8 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt8Member(MemberName, 0);
            }

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
            public UInt16 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt16Member(MemberName, 0);
            }

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
            public UInt32 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt32Member(MemberName, 0);
            }

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
            public UInt64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt64Member(MemberName, 0);
            }

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

                // Determine minimum size of the Enum
                EnumValueType = System.Enum.GetUnderlyingType(enumType);
                
                var metaType = MetaType.TypeInfo(EnumValueType);
                Size = metaType.Size;
                Alignment = metaType.Alignment;
                Value = System.Convert.ChangeType(value, EnumValueType);
                EnumValueTypeName = metaType.Name;
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
            public UInt64 InternalValue { get; private set; }

            public bool IsDefault => false;

            public IClassMember Default()
            {
                return new EnumMember(MemberName, null, default);
            }

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
            public float InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0.0f;

            public IClassMember Default()
            {
                return new FloatMember(MemberName, 0);
            }

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
            public double InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0.0;

            public IClassMember Default()
            {
                return new DoubleMember(MemberName, 0);
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteDoubleMember(this);
            }
        }

        #endregion
        #region StringMember

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
            public string InternalValue { get; private set; }

            public bool IsDefault => InternalValue == String.Empty;

            public IClassMember Default()
            {
                return new StringMember(MemberName, String.Empty);
            }

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
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == -1;

            public IClassMember Default()
            {
                return new FileIdMember(MemberName, 0);
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteFileIdMember(this);
            }
        }

        #endregion

        #region AtomMember

        public sealed class AtomMember : IClassMember
        {
            public AtomMember(string name, Type type, IClassMember member)
            {
                MemberName = name;
                MemberType = type;
                Value = member;
                Size = member.Size;
                Alignment = member.Alignment;
            }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }
            public string TypeName => Member.TypeName;
            public int Size { get; private set; }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }
            public IClassMember InternalValue { get; private set; }
            public IClassMember Member { get; private set; }

            public bool IsDefault => InternalValue.IsDefault;

            public  IClassMember Default()
            {
                return InternalValue.Default();
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteAtomMember(this);
            }
        }

        #endregion

        #region ReferenceableMember

        public interface IReferenceableMember
        {
            StreamReference Reference { get; }
        }

        #endregion
        #region CompoundMemberBase

        public interface ICompoundMemberBase
        {
            void AddMember(IClassMember m);
}

        #endregion

        #region ArrayMember

        public class ArrayMember : ICompoundMemberBase, IReferenceableMember, IClassMember
        {
            #region Constructor

            public ArrayMember(Type arrayType, object value, string memberName)
            {
                MemberName = memberName;
                MemberType = arrayType;
                Value = value;
                Alignment = sizeof(Int32) + sizeof(Int32);
                Size = sizeof(Int32) + sizeof(Int32);
                Members = new ();
            }

            #endregion
            #region Properties

            public StreamReference Reference { get; set; }

            public string MemberName { get; private set; }
            public Type MemberType { get; private set; }
            public IClassMember Element { get { return Members[0]; } }
            public string TypeName => "rawarr_t<" + Element.TypeName + ">";
            public int Size { get; private set; }
            public int Alignment { get; private set; }
            public object Value { get; private set; }

            public bool IsDefault => Value == null;

            public List<IClassMember> Members { get; set; }

            #endregion
            #region Member methods

            public  IClassMember Default()
            {
                return new ArrayMember(null, null, String.Empty);
            }

            public  void AddMember(IClassMember m)
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

        public sealed class ClassObject : ICompoundMemberBase, IReferenceableMember, IClassMember
        {
            #region Constructors

            public ClassObject(Type type, object value, string className, string memberName)
            {
                MemberName = memberName;
                MemberType = type;
                TypeName = className;
                Size = 0;
                Alignment = sizeof(Int32); // Will be adjust if we have a member with a larger alignment
                Value = value;
                Members = new ();
            }

            #endregion
            #region Properties

            public string MemberName { get;  }
            public Type MemberType { get;}
            public string TypeName {get;set;}
            public int Size { get; }
            public Int32 Alignment { get; private set; }
            public object Value { get;  }

            public bool IsDefault => Value == null;
            public StreamReference Reference { get; set; }

            public List<IClassMember> Members { get; private set; }

            #endregion
            #region Methods

            public IClassMember Default()
            {
                ClassObject c = new (null, null, "none", "none");
                return c;
            }

            public  void AddMember(IClassMember m)
            {
                if (m.Alignment > Alignment)
                    Alignment = m.Alignment;

                Members.Add(m);
            }

            public void SortMembers(IComparer<IClassMember> c)
            {
                // Sort the members on their data size, from big to small
                Members.Sort(c);
            }

            public void FixMemberAlignment()
			{

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
        #region CompoundMember

        public sealed class CompoundMember : ICompoundMemberBase, IReferenceableMember, IClassMember
        {
            #region Constructor

            public CompoundMember(object value, string typeName, string memberName)
            {
                MemberName = memberName;
                MemberType = value.GetType();
                TypeName = typeName;
                Size = 0;
                Value = value;
                Members = new List<IClassMember>();
            }

            public CompoundMember(object value, Type type, string name)
            {
                MemberName = name;
                MemberType = type;
                Value = value;
                Members = new List<IClassMember>();
            }

            #endregion
            #region Properties

            public StreamReference Reference { get; set; }


            public string MemberName { get;  }
            public Type MemberType { get; private set; }
            public string TypeName {get;set;}

            public int Size { get;  }
            public Int32 Alignment { get; private set; }
            public object Value { get; private set; }

            public bool IsNullType { get; set; }

            public bool IsDefault => Value == null;

            public List<IClassMember> Members { get; set; }

            #endregion
            #region Member Methods

            public IClassMember Default()
            {
                CompoundMember cm = new CompoundMember(null, MemberType, MemberName);
                foreach (IClassMember m in Members)
                    cm.AddMember(m.Default());
                return cm;
            }

            public void AddMember(IClassMember m)
            {
                if (m.Alignment > Alignment)
                    Alignment = m.Alignment;

                Members.Add(m);
            }

            public void Write(IMemberWriter writer)
            {
                writer.WriteCompoundMember(this);
            }

            #endregion
        }

        #endregion
    }
}
