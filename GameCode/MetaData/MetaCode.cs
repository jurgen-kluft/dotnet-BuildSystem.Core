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
            private Member Element { get; set; }

            public ArrayType(Type arrayObjectType, Member elementMember)
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
            bool isString(Type t);
            bool isEnum(Type t);
            bool isArray(Type t);
            bool isObject(Type t);
            bool isAtom(Type t);
            bool isFileId(Type t);
            bool isCompound(Type t);
            bool isDynamicMember(Type t);

            Member newNullMember(string memberName);
            Member newBoolMember(bool content, string memberName);
            Member newInt8Member(SByte content, string memberName);
            Member newUInt8Member(Byte content, string memberName);
            Member newInt16Member(Int16 content, string memberName);
            Member newUInt16Member(UInt16 content, string memberName);
            Member newInt32Member(Int32 content, string memberName);
            Member newUInt32Member(UInt32 content, string memberName);
            Member newInt64Member(Int64 content, string memberName);
            Member newUInt64Member(UInt64 content, string memberName);
            Member newFloatMember(float content, string memberName);
            Member newStringMember(string content, string memberName);
            Member newFileIdMember(Int64 content, string memberName);
            Member newEnumMember(object content, string memberName);
            ObjectMember newObjectMember(Type objectType, object content, string memberName);
            ArrayMember newArrayMember(Type arrayType, object content, Member elementMember, string memberName);
            AtomMember newAtomMember(Type atomType, Member atomContentMember, string memberName);
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
            bool writeStringMember(StringMember c);
            bool writeFileIdMember(FileIdMember c);
            bool writeArrayMember(ArrayMember c);
            bool writeObjectMember(ObjectMember c);
            bool writeAtomMember(AtomMember c);
            bool writeCompoundMember(CompoundMember c);
        }

        #endregion

        #region MemberComparer (size and hash-of-name)

        public class MemberSizeComparer : IComparer<Member>
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
            public int Compare(Member x, Member y)
            {
                if (x.Alignment == y.Alignment) return 0;
                else if (x.Alignment < y.Alignment) return 1;
                else return -1;
            }
        }

        public class MemberNameHashComparer : IComparer<Member>
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

            public int Compare(Member x, Member y)
            {
                uint hx = mStringTable.HashOf(x.Name);
                uint hy = mStringTable.HashOf(y.Name);
                if (hx == hy) return 0;
                else if (hx < hy) return 1;
                else return -1;
            }
        }

        #endregion

        #region Member

        public abstract class Member
        {
            private readonly string mName = string.Empty;
            private readonly IMetaType mType = null;
            private readonly int mDataSize = 0;
            protected Int64 mAlignment = sizeof(UInt8);
            
            public string Name { get { return mName; } }
            public IMetaType Type { get { return mType; } }
            public int Size { get { return mDataSize; } }
            public Int64 Alignment { get { return mAlignment; } }
            public abstract object Value { get; }

            public abstract bool IsDefault { get; }
            public abstract Member Default();

            public Member(IMetaType type, string name, int dataSize)
            {
                mType = type;
                mName = name;
                mDataSize = dataSize;
            }

            public abstract bool Write(IMemberWriter writer);
        }

        #endregion
        #region NullMember

        public sealed class NullMember : Member
        {
            public static readonly IMetaType sNullType = new NullType();
            
            public NullMember(string name)
                : base(sNullType, name, 4)
            {
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return null;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return true;
                }
            }

            public override Member Default()
            {
                return new NullMember(Name);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeNullMember(this);
                return true;
            }
        }

        #endregion
        #region BoolMember

        public sealed class BoolMember : Member
        {
            public static readonly BoolType sType = new() { type = typeof(bool) };

            private readonly bool mValue;

            public BoolMember(string name, bool value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return mValue ? Byte.MaxValue : Byte.MinValue;
                }
            }


            public byte boolean
            {
                get
                {
                    return mValue ? Byte.MaxValue : Byte.MinValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == false;
                }
            }

            public override Member Default()
            {
                return new BoolMember(Name, false);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeBool8Member(this);
                return true;
            }
        }

        #endregion
        #region Int8Member

        public sealed class Int8Member : Member
        {
            public static readonly Int8Type sType = new() { type = typeof(sbyte) };

            private readonly Int8 mValue;

            public Int8Member(string name, Int8 value)
                : base(sType, name, 1)
            {
                mValue = value;
                mAlignment = sizeof(UInt8);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public Int8 int8
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new Int8Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeInt8Member(this);
                return true;
            }
        }

        #endregion
        #region Int16Member

        public sealed class Int16Member : Member
        {
            public static readonly Int16Type sType = new () { type=typeof(short) };

            private readonly Int16 mValue;

            public Int16Member(string name, Int16 value)
                : base(sType, name, 2)
            {
                mValue = value;
                mAlignment = sizeof(Int16);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public Int16 int16
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new Int16Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeInt16Member(this);
                return true;
            }
        }

        #endregion
        #region Int32Member

        public sealed class Int32Member : Member
        {
            public static readonly Int32Type sType = new() { type = typeof(int) };

            private readonly Int32 mValue;

            public Int32Member(string name, Int32 value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public Int32 int32
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new Int32Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeInt32Member(this);
                return true;
            }
        }

        #endregion
        #region Int64Member

        public sealed class Int64Member : Member
        {
            public static readonly Int64Type sType = new() { type = typeof(Int64) };

            private StreamReference mReference = StreamReference.Empty; 
            private readonly Int64 mValue;

            public Int64Member(string name, Int64 value)
                : base(sType, name, 8)
            {
                mValue = value;
                mAlignment = sizeof(Int64);
            }

            public StreamReference Reference
            {
                get
                {
                    return mReference;
                }
                set
                {
                    mReference = value;
                }
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public Int64 int64
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new Int64Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeInt64Member(this);
                return true;
            }
        }

        #endregion
        #region UInt8Member

        public sealed class UInt8Member : Member
        {
            public static readonly UInt8Type sType = new() { type = typeof(byte) };

            private readonly UInt8 mValue;

            public UInt8Member(string name, UInt8 value)
                : base(sType, name, 1)
            {
                mValue = value;
                mAlignment = sizeof(UInt8);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public UInt8 uint8
            {
                get
                {
                    return mValue;
                }
            }

            public override Member Default()
            {
                return new UInt8Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeUInt8Member(this);
                return true;
            }
        }

        #endregion
        #region UInt16Member

        public sealed class UInt16Member : Member
        {
            public static readonly UInt16Type sType = new() { type = typeof(ushort) };

            private readonly UInt16 mValue;

            public UInt16Member(string name, UInt16 value)
                : base(sType, name, 2)
            {
                mValue = value;
                mAlignment = sizeof(Int16);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public UInt16 uint16
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new UInt16Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeUInt16Member(this);
                return true;
            }
        }

        #endregion
        #region UInt32Member

        public sealed class UInt32Member : Member
        {
            public static readonly UInt32Type sType = new() { type = typeof(uint) };

            private readonly UInt32 mValue;

            public UInt32Member(string name, UInt32 value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public UInt32 uint32
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == 0;
                }
            }

            public override Member Default()
            {
                return new UInt32Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeUInt32Member(this);
                return true;
            }
        }

        #endregion
        #region UInt64Member

        public sealed class UInt64Member : Member
        {
            public static readonly UInt64Type sType = new() { type = typeof(UInt64) };

            public UInt64Member(string name, UInt64 value)
                : base(sType, name, 8)
            {
                uint64 = value;
                mAlignment = sizeof(Int64);
            }

            public StreamReference Reference { get; set; } = StreamReference.Empty;
            public override object Value { get { return uint64; } }
            public UInt64 uint64 { get; }
            public override bool IsDefault{ get { return uint64 == 0; } }

            public override Member Default()
            {
                return new UInt64Member(Name, 0);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeUInt64Member(this);
                return true;
            }
        }

        #endregion
        #region FloatMember

        public sealed class FloatMember : Member
        {
            public static readonly FloatType sType = new() { type = typeof(float) };

            public FloatMember(string name, float value)
                : base(sType, name, 4)
            {
                real = value;
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return real;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return real == 0.0f;
                }
            }

            public float real { get; }

            public override Member Default()
            {
                return new FloatMember(Name, 0.0f);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeFloatMember(this);
                return true;
            }
        }

        #endregion
        #region StringMember

        public sealed class StringMember : Member
        {
            public static readonly StringType sType = new() { type = typeof(string), typeName = "const char*" };

            private readonly string mValue;

            public StringMember(string name, string value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public string StringValue
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue.Length == 0;
                }
            }

            public override Member Default()
            {
                return new StringMember(Name, string.Empty);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeStringMember(this);
                return true;
            }
        }

        #endregion
        #region FileIdMember

        public sealed class FileIdMember : Member
        {
            public static readonly FileIdType sType = new() { type = typeof(GameData.FileId), typeName = "fileid_t" };

            private StreamReference mStreamReference = StreamReference.Empty;
            private readonly Int64 mValue;

            public FileIdMember(string name, Int64 value)
                : base(sType, name, 8)
            {
                mValue = value;
                mAlignment = sizeof(Int64);
            }

            public Int64 ID { get { return mValue; } }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == -1;
                }
            }

            public StreamReference Reference
            {
                get
                {
                    return mStreamReference;
                }
                set
                {
                    mStreamReference = value;
                }
            }

            public override Member Default()
            {
                return new FileIdMember(Name, -1);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeFileIdMember(this);
                return true;
            }
        }

        #endregion

        #region AtomMember

        public sealed class AtomMember : Member
        {
            private readonly Member mValue;

            public AtomMember(string name, IMetaType type, Member member)
                : base(type, name, member.Size)
            {
                mValue = member;
                mAlignment = member.Alignment;
            }

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public Member member
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue.IsDefault;
                }
            }

            public override Member Default()
            {
                return mValue.Default();
            }

            public override bool Write(IMemberWriter writer)
            {
                return writer.writeAtomMember(this);
            }
        }

        #endregion

        #region ReferenceableMember

        public abstract class ReferenceableMember : Member
        {
            #region Fields

            private StreamReference mReference = StreamReference.Empty;

            #endregion
            #region Constructor

            public ReferenceableMember(IMetaType type, string name, int size)
                : base(type, name, size)
            {
            }

            #endregion
            #region Properties

            public StreamReference Reference
            {
                get
                {
                    return mReference;
                }
                set
                {
                    mReference = value;
                }
            }
            #endregion
        }

        #endregion
        #region CompoundMemberBase

        public abstract class CompoundMemberBase : ReferenceableMember
        {
            #region Constructor

            public CompoundMemberBase(IMetaType type, string name, int size)
                : base(type, name, size)
            {
            }

            #endregion
            #region Public Methods

            public abstract void AddMember(Member m);

            #endregion
        }

        #endregion

        #region ArrayMember

        public class ArrayMember : CompoundMemberBase
        {
            #region Fields

            private readonly object mValue;
            private readonly List<Member> mMembers;

            #endregion
            #region Constructor

            public ArrayMember(Type arrayObjectType, object value, Member elementMember, string memberName)
                : base(new ArrayType(arrayObjectType, elementMember), memberName, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
                mMembers = new List<Member>();
            }

            public ArrayMember(IMetaType arrayType, object value, string memberName)
                : base(arrayType, memberName, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == null;
                }
            }

            public List<Member> Members
            {
                get
                {
                    return mMembers;
                }
            }

            #endregion  
            #region Member methods

            public override Member Default()
            {
                return new ArrayMember(Type, null, Name);
            }

            public override void AddMember(Member m)
            {
                mMembers.Add(m);
            }

            public override bool Write(IMemberWriter writer)
            {
                writer.writeArrayMember(this);
                return true;
            }

            #endregion
        }

        #endregion
        #region ObjectMember

        public sealed class ObjectMember : CompoundMemberBase
        {
            #region Fields

            private readonly object mValue;
            private readonly List<Member> mMembers;

            private ObjectMember mBaseClass;

            #endregion
            #region Constructors

            public ObjectMember(object value, string className, string memberName)
                : base(new ObjectType(value.GetType(), className), memberName, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
                mMembers = new List<Member>();
            }

            public ObjectMember(object value, IMetaType type, string memberName)
                : base(type, memberName, 4)
            {
                mValue = value;
                mAlignment = sizeof(Int32);
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == null;
                }
            }

            public List<Member> Members
            {
                get
                {
                    return mMembers;
                }
            }

            public ObjectMember BaseClass
            {
                get
                {
                    return mBaseClass;
                }
                set
                {
                    mBaseClass = value;
                }
            }

            #endregion            
            #region Methods

            public override Member Default()
            {
                ObjectMember c = new ObjectMember(null, Type, Name);
                return c;
            }

            public override void AddMember(Member m)
            {
                if (m.Alignment > mAlignment)
                    mAlignment = m.Alignment;

                bool mitigate = true;
                if (BaseClass == null)
                {
                    mMembers.Add(m);
                }
                else if (!mitigate)
                {
                    Type classType = Type.type;
                    FieldInfo[] fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (FieldInfo fi in fields)
                    {
                        if (fi.Name == m.Name)
                        {
                            mMembers.Add(m);
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
                        mMembers.Add(m);

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
                            mMembers.Add(m);
                        }
                        else
                        {
                            if (BaseClass != null)
                                BaseClass.AddMember(m);
                        }
                    }
                }
            }

            public void sortMembers(IComparer<Member> c)
            {
                // Sort the members on their data size, from big to small
                mMembers.Sort(c);

                if (BaseClass != null)
                    BaseClass.sortMembers(c);
            }

            #endregion
            #region Member methods

            public override bool Write(IMemberWriter writer)
            {
                return (writer.writeObjectMember(this));
            }

            #endregion
        }

        #endregion
        #region CompoundMember

        public sealed class CompoundMember : CompoundMemberBase
        {
            #region Fields

            private bool mIsNullType = false;
            private readonly object mValue;
            private readonly List<Member> mMembers;

            #endregion
            #region Constructor

            public CompoundMember(object value, string typeName, string memberName)
                : base(new CompoundType(value.GetType(), typeName), memberName, 0)
            {
                mValue = value;
                mMembers = new List<Member>();
            }

            public CompoundMember(object value, IMetaType type, string name)
                : base(type, name, 0)
            {
                mValue = value;
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object Value
            {
                get
                {
                    return mValue;
                }
            }

            public bool isNullType
            {
                get
                {
                    return mIsNullType;
                }
                set
                {
                    mIsNullType = value;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return mValue == null;
                }
            }

            public List<Member> members
            {
                get
                {
                    return mMembers;
                }
            }

            #endregion
            #region Member Methods

            public override Member Default()
            {
                CompoundMember cm = new CompoundMember(null, Type, Name);
                foreach (Member m in mMembers)
                    cm.AddMember(m.Default());
                return cm;
            }

            public override void AddMember(Member m)
            {
                if (m.Alignment > mAlignment)
                    mAlignment = m.Alignment;

                mMembers.Add(m);
            }

            public override bool Write(IMemberWriter writer)
            {
                return writer.writeCompoundMember(this);
            }

            #endregion
        }

        #endregion
    }
}
