using System;
using System.Reflection;
using System.Collections.Generic;

using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace Core
{
    namespace MetaCode
    {
        #region MetaType

        public abstract class MetaType
        {
            private Type mType;
            private string mTypeName;

            public MetaType(Type _type, string _typeName)
            { 
                type = type;
                typeName = _typeName;
            }

            public Type type
            {
                get
                {
                    return mType;
                }
                set
                {
                    mType = value;
                }
            }

            public string typeName 
            { 
                get 
                {
                    return mTypeName;
                }
                set
                {
                    mTypeName = value.ToLower();
                }
            }
        }

        #endregion
        #region NullType

        public class NullType : MetaType
        {
            public NullType()
                : base(null, "null")
            {
            }
        }

        #endregion
        #region BoolType

        public class BoolType : MetaType
        {
            public BoolType(Type type)
                : base(type, "bool")
            {
            }
        }

        #endregion
        #region Int8Type

        public class Int8Type : MetaType
        {
            public Int8Type(Type type)
                : base(type, "int8")
            {
            }
        }

        #endregion
        #region Int16Type

        public class Int16Type : MetaType
        {
            public Int16Type(Type type)
                : base(type, "int16")
            {
            }
        }

        #endregion
        #region Int32Type

        public class Int32Type : MetaType
        {
            public Int32Type(Type type)
                : base(type, "int32")
            {
            }
        }

        #endregion
        #region Int64Type

        public class Int64Type : MetaType
        {
            public Int64Type(Type type)
                : base(type, "int64")
            {
            }
        }

        #endregion
        #region UInt8Type

        public class UInt8Type : MetaType
        {
            public UInt8Type(Type type)
                : base(type, "uint8")
            {
            }
        }

        #endregion
        #region UInt16Type

        public class UInt16Type : MetaType
        {
            public UInt16Type(Type type)
                : base(type, "uint16")
            {
            }
        }

        #endregion
        #region UInt32Type

        public class UInt32Type : MetaType
        {
            public UInt32Type(Type type)
                : base(type, "uint32")
            {
            }
        }

        #endregion
        #region UInt64Type

        public class UInt64Type : MetaType
        {
            public UInt64Type(Type type)
                : base(type, "uint64")
            {
            }
        }

        #endregion
        #region FloatType

        public class FloatType : MetaType
        {
            public FloatType(Type type)
                : base(type, "float")
            {
            }
        }

        #endregion
        #region StringType

        public class StringType : MetaType
        {
            public StringType(Type type)
                : base(type, "string")
            {
            }
        }

        #endregion
        #region FileIdType

        public class FileIdType : MetaType
        {
            public FileIdType(Type type, string typeName)
                : base(type, typeName)
            {
            }
        }

        #endregion
        #region ArrayType

        public class ArrayType : MetaType
        {
            private readonly Member mElement;

            public ArrayType(Type arrayObjectType, Member elementMember)
                : base(arrayObjectType, elementMember.type.typeName + "[]")
            {
                mElement = elementMember;
            }
        }

        #endregion
        #region ObjectType

        public class ObjectType : MetaType
        {
            public ObjectType(Type classType, string className)
                : base(classType, className)
            {
            }
        }

        #endregion
        #region AtomType

        public class AtomType : MetaType
        {
            public AtomType(Type type, string typeName)
                : base(type, typeName)
            {
            }
        }

        #endregion
        #region CompoundType

        public class CompoundType : MetaType
        {
            public CompoundType(Type type, string typeName)
                : base(type, typeName)
            {
            }
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
            Member newFileIdMember(Hash128 content, string memberName);
            Member newEnumMember(object content, string memberName);
            ObjectMember newObjectMember(Type objectType, object content, string memberName);
            ArrayMember newArrayMember(Type arrayType, object content, Member elementMember, string memberName);
            AtomMember newAtomMember(Type atomType, Member atomContentMember, string memberName);
            FileIdMember newFileIdMember(Type atomType, Hash128 content, string memberName);
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
                if (x.alignment == y.alignment) return 0;
                else if (x.alignment < y.alignment) return 1;
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
                uint hx = mStringTable.HashOf(x.name);
                uint hy = mStringTable.HashOf(y.name);
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
            private readonly MetaType mType = null;
            private readonly int mDataSize = 0;
            protected EStreamAlignment mAlignment = EStreamAlignment.ALIGN_8;
            
            public string name { get { return mName; } }
            public MetaType type { get { return mType; } }
            public int size { get { return mDataSize; } }
            public EStreamAlignment alignment { get { return mAlignment; } }
            public abstract object value { get; }

            public abstract bool IsDefault { get; }
            public abstract Member Default();

            public Member(MetaType type, string name, int dataSize)
            {
                mType = type;
                mName = name;
                mDataSize = dataSize;
            }

            public abstract bool write(IMemberWriter writer);
        }

        #endregion
        #region NullMember

        public class NullMember : Member
        {
            public static readonly MetaType sNullType = new NullType();
            
            public NullMember(string name)
                : base(sNullType, name, 4)
            {
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                return new NullMember(name);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeNullMember(this);
                return true;
            }
        }

        #endregion
        #region BoolMember

        public class BoolMember : Member
        {
            public static readonly BoolType sType = new BoolType(typeof(bool));

            private readonly bool mValue;

            public BoolMember(string name, bool value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                return new BoolMember(name, false);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeBool8Member(this);
                return true;
            }
        }

        #endregion
        #region Int8Member

        public class Int8Member : Member
        {
            public static readonly Int8Type sType = new Int8Type(typeof(sbyte));

            private readonly Int8 mValue;

            public Int8Member(string name, Int8 value)
                : base(sType, name, 1)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_8;
            }

            public override object value
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
                return new Int8Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeInt8Member(this);
                return true;
            }
        }

        #endregion
        #region Int16Member

        public class Int16Member : Member
        {
            public static readonly Int16Type sType = new Int16Type(typeof(short));

            private readonly Int16 mValue;

            public Int16Member(string name, Int16 value)
                : base(sType, name, 2)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_16;
            }

            public override object value
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
                return new Int16Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeInt16Member(this);
                return true;
            }
        }

        #endregion
        #region Int32Member

        public class Int32Member : Member
        {
            public static readonly Int32Type sType = new Int32Type(typeof(int));

            private readonly Int32 mValue;

            public Int32Member(string name, Int32 value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                return new Int32Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeInt32Member(this);
                return true;
            }
        }

        #endregion
        #region Int64Member

        public class Int64Member : Member
        {
            public static readonly Int64Type sType = new Int64Type(typeof(Int64));

            private StreamReference mReference = StreamReference.Empty; 
            private readonly Int64 mValue;

            public Int64Member(string name, Int64 value)
                : base(sType, name, 8)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_64;
            }

            public StreamReference reference
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

            public override object value
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
                return new Int64Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeInt64Member(this);
                return true;
            }
        }

        #endregion
        #region UInt8Member

        public class UInt8Member : Member
        {
            public static readonly UInt8Type sType = new UInt8Type(typeof(byte));

            private readonly UInt8 mValue;

            public UInt8Member(string name, UInt8 value)
                : base(sType, name, 1)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_8;
            }

            public override object value
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
                return new UInt8Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeUInt8Member(this);
                return true;
            }
        }

        #endregion
        #region UInt16Member

        public class UInt16Member : Member
        {
            public static readonly UInt16Type sType = new UInt16Type(typeof(ushort));

            private readonly UInt16 mValue;

            public UInt16Member(string name, UInt16 value)
                : base(sType, name, 2)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_16;
            }

            public override object value
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
                return new UInt16Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeUInt16Member(this);
                return true;
            }
        }

        #endregion
        #region UInt32Member

        public class UInt32Member : Member
        {
            public static readonly UInt32Type sType = new UInt32Type(typeof(uint));

            private readonly UInt32 mValue;

            public UInt32Member(string name, UInt32 value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                return new UInt32Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeUInt32Member(this);
                return true;
            }
        }

        #endregion
        #region UInt64Member

        public class UInt64Member : Member
        {
            public static readonly UInt64Type sType = new UInt64Type(typeof(UInt64));

            private StreamReference mReference = StreamReference.Empty; 
            private readonly UInt64 mValue;

            public UInt64Member(string name, UInt64 value)
                : base(sType, name, 8)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_64;
            }

            public StreamReference reference
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

            public override object value
            {
                get
                {
                    return mValue;
                }
            }

            public UInt64 uint64
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
                return new UInt64Member(name, 0);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeUInt64Member(this);
                return true;
            }
        }

        #endregion
        #region FloatMember

        public class FloatMember : Member
        {
            public static readonly FloatType sType = new FloatType(typeof(float));

            private readonly float mValue;

            public FloatMember(string name, float value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                    return mValue == 0.0f;
                }
            }

            public float real
            {
                get
                {
                    return mValue;
                }
            }

            public override Member Default()
            {
                return new FloatMember(name, 0.0f);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeFloatMember(this);
                return true;
            }
        }

        #endregion
        #region StringMember

        public class StringMember : Member
        {
            public static readonly StringType sType = new StringType(typeof(string));

            private readonly string mValue;

            public StringMember(string name, string value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
            {
                get
                {
                    return mValue;
                }
            }

            public string str
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
                return new StringMember(name, string.Empty);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeStringMember(this);
                return true;
            }
        }

        #endregion
        #region FileIdMember

        public class FileIdMember : Member
        {
            public static readonly FileIdType sType = new FileIdType(typeof(Game.Data.FileId), typeof(Game.Data.FileId).Name);

            private StreamReference mStreamReference = StreamReference.Empty;
            private readonly Hash128 mValue;

            public FileIdMember(string name, Hash128 value)
                : base(sType, name, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
            }

            public override object value
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
                    return mValue == Hash128.Empty;
                }
            }

            public StreamReference reference
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

            public Hash128 id
            {
                get
                {
                    return mValue;
                }
            }

            public override Member Default()
            {
                return new FileIdMember(name, Hash128.Empty);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeFileIdMember(this);
                return true;
            }
        }

        #endregion

        #region AtomMember

        public class AtomMember : Member
        {
            private readonly Member mValue;

            public AtomMember(string name, MetaType type, Member member)
                : base(type, name, member.size)
            {
                mValue = member;
                mAlignment = member.alignment;
            }

            public override object value
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

            public override bool write(IMemberWriter writer)
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

            public ReferenceableMember(MetaType type, string name, int size)
                : base(type, name, size)
            {
            }

            #endregion
            #region Properties

            public StreamReference reference
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

            public CompoundMemberBase(MetaType type, string name, int size)
                : base(type, name, size)
            {
            }

            #endregion
            #region Public Methods

            public abstract void addMember(Member m);

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
                mAlignment = EStreamAlignment.ALIGN_32;
                mMembers = new List<Member>();
            }

            public ArrayMember(MetaType arrayType, object value, string memberName)
                : base(arrayType, memberName, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object value
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

            public List<Member> members
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
                return new ArrayMember(type, null, name);
            }

            public override void addMember(Member m)
            {
                mMembers.Add(m);
            }

            public override bool write(IMemberWriter writer)
            {
                writer.writeArrayMember(this);
                return true;
            }

            #endregion
        }

        #endregion
        #region ObjectMember

        public class ObjectMember : CompoundMemberBase
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
                mAlignment = EStreamAlignment.ALIGN_32;
                mMembers = new List<Member>();
            }

            public ObjectMember(object value, MetaType type, string memberName)
                : base(type, memberName, 4)
            {
                mValue = value;
                mAlignment = EStreamAlignment.ALIGN_32;
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object value
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

            public List<Member> members
            {
                get
                {
                    return mMembers;
                }
            }

            public ObjectMember baseClass
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
                ObjectMember c = new ObjectMember(null, type, name);
                return c;
            }

            public override void addMember(Member m)
            {
                if (m.alignment > mAlignment)
                    mAlignment = m.alignment;

                bool mitigate = true;
                if (baseClass == null)
                {
                    mMembers.Add(m);
                }
                else if (!mitigate)
                {
                    Type classType = type.type;
                    FieldInfo[] fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (FieldInfo fi in fields)
                    {
                        if (fi.Name == m.name)
                        {
                            mMembers.Add(m);
                            return;
                        }
                    }

                    baseClass.addMember(m);
                }
                else if (mitigate)
                {
                    Type classType = type.type;
                    FieldInfo[] fields = classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    
                    bool isThisMember = false;
                    foreach (FieldInfo fi in fields)
                    {
                        if (fi.Name == m.name)
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
                        if (!isThisMember && baseClass != null)
                        {
                            m = m.Default();
                            baseClass.addMember(m);
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
                            if (baseClass != null)
                                baseClass.addMember(m);
                        }
                    }
                }
            }

            public void sortMembers(IComparer<Member> c)
            {
                // Sort the members on their data size, from big to small
                mMembers.Sort(c);

                if (baseClass != null)
                    baseClass.sortMembers(c);
            }

            #endregion
            #region Member methods

            public override bool write(IMemberWriter writer)
            {
                return (writer.writeObjectMember(this));
            }

            #endregion
        }

        #endregion
        #region CompoundMember

        public class CompoundMember : CompoundMemberBase
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

            public CompoundMember(object value, MetaType type, string name)
                : base(type, name, 0)
            {
                mValue = value;
                mMembers = new List<Member>();
            }

            #endregion
            #region Properties

            public override object value
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
                CompoundMember cm = new CompoundMember(null, type, name);
                foreach (Member m in mMembers)
                    cm.addMember(m.Default());
                return cm;
            }

            public override void addMember(Member m)
            {
                if (m.alignment > mAlignment)
                    mAlignment = m.alignment;

                mMembers.Add(m);
            }

            public override bool write(IMemberWriter writer)
            {
                return writer.writeCompoundMember(this);
            }

            #endregion
        }

        #endregion
    }
}
