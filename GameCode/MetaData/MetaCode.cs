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
        #region IMetaType

        public interface IMetaType
        {
            Type type { get; set; }
            string typeName { get; }
        }

        #endregion
        #region NullType

        public class NullType : IMetaType
        {
            public Type type { get { return null; } set { } }
            public string typeName => "null";
        }

        #endregion
        #region BoolType

        public class BoolType : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "bool";
        }

        #endregion
        #region Int8Type

        public class Int8Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "int8";
        }

        #endregion
        #region Int16Type

        public class Int16Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "int16";
        }

        #endregion
        #region Int32Type

        public class Int32Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "int32";
        }

        #endregion
        #region Int64Type

        public class Int64Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "int64";
        }

        #endregion
        #region UInt8Type

        public class UInt8Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "uint8";
        }

        #endregion
        #region UInt16Type

        public class UInt16Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "uint16";
        }

        #endregion
        #region UInt32Type

        public class UInt32Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "uint32";
        }

        #endregion
        #region UInt64Type

        public class UInt64Type : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "uint64";
        }

        #endregion
        #region FloatType

        public class FloatType : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "float";
        }

        #endregion
        #region DoubleType

        public class DoubleType : IMetaType
        {
            public Type type { get; set; }
            public string typeName => "double";
        }

        #endregion
        #region StringType

        public class StringType : IMetaType
        {
            public Type type { get; set; }
            public string typeName { get; set; }
        }

        #endregion
        #region FileIdType

        public class FileIdType : IMetaType
        {
            public Type type { get; set; }
            public string typeName { get; set; }
        }

        #endregion
        #region ArrayType

        public class ArrayType : IMetaType
        {
            private IClassMember Element { get; set; }

            public ArrayType(Type arrayObjectType, IClassMember elementMember)
            {
                Element = elementMember;
                type = arrayObjectType;
                typeName = elementMember.Type.typeName + "[]";
            }
            public Type type { get; set; }
            public string typeName { get; set; }

        }

        #endregion
        #region ObjectType

        public class ObjectType : IMetaType
        {
            public ObjectType(Type classType, string className)
            {
                type = classType;
                typeName = className;
            }
            public Type type { get; set; }
            public string typeName { get; set; }
        }

        #endregion
        #region AtomType

        public class AtomType : IMetaType
        {
            public AtomType(Type _type, string _typeName)
            {
                type = _type;
                typeName = _typeName;
            }
            public Type type { get; set; }
            public string typeName { get; set; }
        }

        #endregion
        #region CompoundType

        public class CompoundType : IMetaType
        {
            public CompoundType(Type _type, string _typeName)
            {
                type = _type;
                typeName = _typeName;
            }
            public Type type { get; set; }
            public string typeName { get; set; }
        }

        #endregion

        #region IMemberGenerator

        public interface IMemberGenerator
        {
            bool isNull(Type t);
            bool isBool(Type t);
            bool isInt8(Type t);
            bool isUInt8(Type t);
            bool isInt16(Type t);
            bool isUInt16(Type t);
            bool isInt32(Type t);
            bool isUInt32(Type t);
            bool isInt64(Type t);
            bool isUInt64(Type t);
            bool isFloat(Type t);
            bool isDouble(Type t);
            bool isString(Type t);
            bool isEnum(Type t);
            bool isArray(Type t);
            bool isObject(Type t);
            bool isAtom(Type t);
            bool isFileId(Type t);
            bool isCompound(Type t);

            IClassMember newNullMember(string memberName);
            IClassMember newBoolMember(bool content, string memberName);
            IClassMember newInt8Member(SByte content, string memberName);
            IClassMember newUInt8Member(Byte content, string memberName);
            IClassMember newInt16Member(Int16 content, string memberName);
            IClassMember newUInt16Member(UInt16 content, string memberName);
            IClassMember newInt32Member(Int32 content, string memberName);
            IClassMember newUInt32Member(UInt32 content, string memberName);
            IClassMember newInt64Member(Int64 content, string memberName);
            IClassMember newUInt64Member(UInt64 content, string memberName);
            IClassMember newFloatMember(float content, string memberName);
            IClassMember newDoubleMember(double content, string memberName);
            IClassMember newStringMember(string content, string memberName);
            IClassMember newFileIdMember(Int64 content, string memberName);
            IClassMember newEnumMember(object content, string memberName);
            ClassObject newObjectMember(Type objectType, object content, string memberName);
            ArrayMember newArrayMember(Type arrayType, object content, IClassMember elementMember, string memberName);
            AtomMember newAtomMember(Type atomType, IClassMember atomContentMember, string memberName);
            FileIdMember newFileIdMember(Type atomType, Int64 content, string memberName);
            CompoundMember newCompoundMember(Type compoundType, object content, string memberName);
        }

        #endregion
        #region IMemberWriter

        public interface IMemberWriter
        {
            bool open();
            bool close();

            bool writeNullMember(NullMember c);
            bool writeBool8Member(BoolMember c);
            bool writeInt8Member(Int8Member c);
            bool writeInt16Member(Int16Member c);
            bool writeInt32Member(Int32Member c);
            bool writeInt64Member(Int64Member c);
            bool writeUInt8Member(UInt8Member c);
            bool writeUInt16Member(UInt16Member c);
            bool writeUInt32Member(UInt32Member c);
            bool writeUInt64Member(UInt64Member c);
            bool writeFloatMember(FloatMember c);
            bool writeDoubleMember(DoubleMember c);

            bool writeStringMember(StringMember c);
            bool writeFileIdMember(FileIdMember c);
            bool writeArrayMember(ArrayMember c);
            bool writeObjectMember(ClassObject c);
            bool writeAtomMember(AtomMember c);
            bool writeCompoundMember(CompoundMember c);
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
                uint hx = mStringTable.HashOf(x.Name);
                uint hy = mStringTable.HashOf(y.Name);
                if (hx == hy) return 0;
                else if (hx < hy) return 1;
                else return -1;
            }
        }

        #endregion

        #region IClassMember

        public interface IClassMember
        {            
            string Name { get;  }
            IMetaType Type { get;  }
            int Size { get; }
            Int64 Alignment { get; }
            object Value { get; }

            bool IsDefault { get; }
            IClassMember Default();

            bool Write(IMemberWriter writer);
        }

        #endregion

        #region NullMember

        public sealed class NullMember : IClassMember
        {
            public static readonly IMetaType sNullType = new NullType();
            
            public NullMember(string name)
            {
               Name = name;
               Type = sNullType;
               Alignment = sizeof(Int32);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get { return null; } }

            public bool IsDefault => true;

            public IClassMember Default()
            {
                return new NullMember(Name);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeNullMember(this);
                return true;
            }
        }

        #endregion
        #region BoolMember

        public sealed class BoolMember : IClassMember
        {
            public static readonly BoolType sType = new() { type = typeof(bool) };

            private readonly bool mValue;

            public BoolMember(string name, bool value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int32);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public bool InternalValue { get; private set; }

            public bool IsDefault { get { return InternalValue == false; } }

            public IClassMember Default()
            {
                return new BoolMember(Name, false);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeBool8Member(this);
                return true;
            }
        }

        #endregion
        #region Int8Member

        public sealed class Int8Member : IClassMember
        {
            public static readonly Int8Type sType = new() { type = typeof(Int8) };

            public Int8Member(string name, Int8 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int8);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int8 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int8Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeInt8Member(this);
                return true;
            }
        }

        #endregion
        #region Int16Member

        public sealed class Int16Member : IClassMember
        {
            public static readonly Int16Type sType = new() { type = typeof(Int16) };

            public Int16Member(string name, Int16 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int16);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int16 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int16Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeInt16Member(this);
                return true;
            }
        }

        #endregion
        #region Int32Member

        public sealed class Int32Member : IClassMember
        {
            public static readonly Int32Type sType = new() { type = typeof(Int32) };

            public Int32Member(string name, Int32 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int32);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int32 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int32Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeInt32Member(this);
                return true;
            }
        }

        #endregion
        #region Int64Member

        public sealed class Int64Member : IClassMember
        {
            public static readonly Int64Type sType = new() { type = typeof(Int64) };

            public Int64Member(string name, Int64 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int64);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new Int64Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeInt64Member(this);
                return true;
            }
        }

        #endregion
        #region UInt8Member

        public sealed class UInt8Member : IClassMember
        {
            public static readonly UInt8Type sType = new() { type = typeof(UInt8) };

            public UInt8Member(string name, UInt8 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt8);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public UInt8 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt8Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeUInt8Member(this);
                return true;
            }
        }

        #endregion
        #region UInt16Member

        public sealed class UInt16Member : IClassMember
        {
            public static readonly UInt16Type sType = new() { type = typeof(UInt16) };

            public UInt16Member(string name, UInt16 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt16);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public UInt16 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt16Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeUInt16Member(this);
                return true;
            }
        }

        #endregion
        #region UInt32Member

        public sealed class UInt32Member : IClassMember
        {
            public static readonly UInt32Type sType = new() { type = typeof(UInt32) };

            public UInt32Member(string name, UInt32 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt32);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public UInt32 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt32Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeUInt32Member(this);
                return true;
            }
        }

        #endregion
        #region UInt64Member

        public sealed class UInt64Member : IClassMember
        {
            public static readonly UInt64Type sType = new() { type = typeof(UInt64) };

            public UInt64Member(string name, UInt64 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(UInt64);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public UInt64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0;

            public IClassMember Default()
            {
                return new UInt64Member(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeUInt64Member(this);
                return true;
            }
        }

        #endregion
        #region FloatMember

        public sealed class FloatMember : IClassMember
        {
            public static readonly FloatType sType = new() { type = typeof(float) };

            public FloatMember(string name, float value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(float);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public float InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0.0f;

            public IClassMember Default()
            {
                return new FloatMember(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeFloatMember(this);
                return true;
            }
        }

        #endregion
        #region DoubleMember

        public sealed class DoubleMember : IClassMember
        {
            public static readonly DoubleType sType = new() { type = typeof(double) };

            public DoubleMember(string name, double value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(double);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public double InternalValue { get; private set; }

            public bool IsDefault => InternalValue == 0.0;

            public IClassMember Default()
            {
                return new DoubleMember(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeDoubleMember(this);
                return true;
            }
        }

        #endregion
        #region StringMember

        public sealed class StringMember : IClassMember
        {
            public static readonly StringType sType = new() { type = typeof(string) };

            public StringMember(string name, string value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Size = sizeof(Int32) + sizeof(Int32);
                Alignment = sizeof(Int32) + sizeof(Int32);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public string InternalValue { get; private set; }

            public bool IsDefault => InternalValue == String.Empty;

            public IClassMember Default()
            {
                return new StringMember(Name, String.Empty);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeStringMember(this);
                return true;
            }
        }

        #endregion
        #region FileIdMember

        public sealed class FileIdMember : IClassMember
        {
            public static readonly FileIdType sType = new() { type = typeof(Int64) };

            public FileIdMember(string name, Int64 value)
            {
                Name = name;
                Type = sType;
                Value = value;
                InternalValue = value;
                Alignment = sizeof(Int64);
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => InternalValue == -1;

            public IClassMember Default()
            {
                return new FileIdMember(Name, 0);
            }

            public bool Write(IMemberWriter writer)
            {
                writer.writeFileIdMember(this);
                return true;
            }
        }

        #endregion

        #region AtomMember

        public sealed class AtomMember : IClassMember
        {
            public AtomMember(string name, IMetaType type, IClassMember member)
            {
                Name = name;
                Value = member;
                Alignment = member.Alignment;
            }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public IClassMember InternalValue { get; private set; }
            public IClassMember Member { get; private set; }

            public bool IsDefault => InternalValue.IsDefault;

            public  IClassMember Default()
            {
                return InternalValue.Default();
            }

            public bool Write(IMemberWriter writer)
            {
                return writer.writeAtomMember(this);
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

            public ArrayMember(Type arrayObjectType, object value, IClassMember elementMember, string memberName)
            {
                Type = new ArrayType(arrayObjectType, elementMember);
                Value = value;
                Alignment = sizeof(Int32) + sizeof(Int32);
                Size = sizeof(Int32) + sizeof(Int32);
                Members = new List<IClassMember>();
            }

            public ArrayMember(IMetaType arrayType, object value, string memberName)
            {
                Type = arrayType;
                Value = value;
                Alignment = sizeof(Int32);
                Members = new List<IClassMember>();
            }

            #endregion
            #region Properties

            public StreamReference Reference { get; set; }

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => Value == null;

            public List<IClassMember> Members { get; set; }

            #endregion  
            #region Member methods

            public  IClassMember Default()
            {
                return new ArrayMember(Type, null, Name);
            }

            public  void AddMember(IClassMember m)
            {
                Members.Add(m);
            }

            public  bool Write(IMemberWriter writer)
            {
                writer.writeArrayMember(this);
                return true;
            }

            #endregion
        }

        #endregion
        #region ClassObject

        public sealed class ClassObject : ICompoundMemberBase, IReferenceableMember, IClassMember
        {
            #region Constructors

            public ClassObject(object value, string className, string memberName)
            {
                Value = value;
                Alignment = sizeof(Int32);
                Members = new ();
            }

            public ClassObject(object value, IMetaType type, string memberName)
            {
                Value = value;
                Alignment = sizeof(Int32);
                Members = new ();
            }

            #endregion
            #region Properties

            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int64 InternalValue { get; private set; }

            public bool IsDefault => Value == null;
            public StreamReference Reference { get; set; }

            public List<IClassMember> Members { get; private set; }

            public ClassObject BaseClass { get; set; }

            #endregion            
            #region Methods

            public IClassMember Default()
            {
                ClassObject c = new (null, Type, Name);
                return c;
            }

            public  void AddMember(IClassMember m)
            {
                if (m.Alignment > Alignment)
                    Alignment = m.Alignment;

                bool mitigate = true;
                if (BaseClass == null)
                {
                    Members.Add(m);
                }
                else if (!mitigate)
                {
                    Type classType = Type.type;
                    FieldInfo[] fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (FieldInfo fi in fields)
                    {
                        if (fi.Name == m.Name)
                        {
                            Members.Add(m);
                            return;
                        }
                    }

                    BaseClass.AddMember(m);
                }
                else if (mitigate)
                {
                    Type classType = Type.type;
                    FieldInfo[] fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                    bool isThisMember = false;
                    foreach (FieldInfo fi in fields)
                    {
                        if (fi.Name == m.Name)
                        {
                            isThisMember = true;
                            break;
                        }
                    }

                    if (!m.IsDefault)
                    {
                        // A non default member is always added to This
                        Members.Add(m);

                        // Create member with default value to add to the base class if this member does not belong to This
                        if (!isThisMember && BaseClass != null)
                        {
                            m = m.Default();
                            BaseClass.AddMember(m);
                        }
                    }
                    else
                    {
                        if (isThisMember)
                        {
                            Members.Add(m);
                        }
                        else
                        {
                            if (BaseClass != null)
                                BaseClass.AddMember(m);
                        }
                    }
                }
            }

            public void SortMembers(IComparer<IClassMember> c)
            {
                // Sort the members on their data size, from big to small
                Members.Sort(c);

                if (BaseClass != null)
                    BaseClass.SortMembers(c);
            }

            public void FixMemberAlignment()
			{

			}

            #endregion
            #region Member methods

            public bool Write(IMemberWriter writer)
            {
                return (writer.writeObjectMember(this));
            }

            #endregion
        }

        #endregion
        #region CompoundMember

        public sealed class CompoundMember : ICompoundMemberBase, IReferenceableMember, IClassMember
        {
            #region Fields

            private bool mIsNullType = false;

            #endregion
            #region Constructor

            public CompoundMember(object value, string typeName, string memberName)
            {
                Type = new CompoundType(value.GetType(), typeName);
                Value = value;
                Members = new List<IClassMember>();
            }

            public CompoundMember(object value, IMetaType type, string name)
            {
                Type = type;
                Value = value;
                Members = new List<IClassMember>();
            }

            #endregion
            #region Properties

            public StreamReference Reference { get; set; }


            public string Name { get; private set; }
            public IMetaType Type { get; private set; }
            public int Size { get; private set; }
            public Int64 Alignment { get; private set; }
            public object Value { get; private set; }
            public Int64 InternalValue { get; private set; }

            public bool IsNullType { get; set; }

            public bool IsDefault => Value == null;

            public List<IClassMember> Members { get; set; }

            #endregion
            #region Member Methods

            public IClassMember Default()
            {
                CompoundMember cm = new CompoundMember(null, Type, Name);
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

            public bool Write(IMemberWriter writer)
            {
                return writer.writeCompoundMember(this);
            }

            #endregion
        }

        #endregion
    }
}
