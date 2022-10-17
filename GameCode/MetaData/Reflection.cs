using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;
using GameData.MetaCode;

namespace GameData
{
    #region MemberBook

    public class MemberBook
    {
        public List<MetaCode.ClassObject> Classes { get; set; }
        public List<MetaCode.EnumMember> Enums { get; set; }
        public List<MetaCode.CompoundMember> Compounds { get; set; }
        public List<MetaCode.AtomMember> Atoms{ get; set; }
        public List<MetaCode.FileIdMember> FileIds{ get; set; }
        public List<MetaCode.ArrayMember> Arrays{ get; set; }
        public List<MetaCode.StringMember> Strings{ get; set; }
    }

    #endregion

    public class Reflector
    {
        #region Fields

        private readonly MetaCode.IMemberGenerator mMemberGenerator;
        private readonly List<MetaCode.ClassObject> mClassDatabase;
        private readonly List<MetaCode.EnumMember> mEnumDatabase;
        private readonly List<MetaCode.StringMember> mStringDatabase;
        private readonly List<MetaCode.ArrayMember> mArrayDatabase;
        private readonly List<MetaCode.AtomMember> mAtomDatabase;
        private readonly List<MetaCode.FileIdMember> mFileIdDatabase;
        private readonly List<MetaCode.CompoundMember> mCompoundDatabase;

        private readonly Stack<KeyValuePair<object, MetaCode.ClassObject>> mStack;

        #endregion
        #region Constructor

        public Reflector(MetaCode.IMemberGenerator memberGenerator)
        {
            mMemberGenerator = memberGenerator;

            mClassDatabase = new ();
            mStringDatabase = new ();
            mArrayDatabase = new ();
            mAtomDatabase = new ();
            mFileIdDatabase = new ();
            mCompoundDatabase = new ();

            mEnumDatabase = new();

            mStack = new ();
        }

        #endregion
        #region Methods

        #region addMember

        private MetaCode.IClassMember CreateMember(object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName)
        {
            MetaCode.IClassMember member = null;

            string memberName = dataObjectFieldName;
            if (memberName.StartsWith("m_"))
                memberName = memberName.Substring(2);

            // Check if the object implements the IAtom, ICompound or IObject interface
            // If not then check if it's an Array, Class or other reference type objects
            // Lastly handle all system type objects.
            if (mMemberGenerator.IsAtom(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // An Atom is holding a primitive type like Int, Float etc.. we treat this as a MetaCode.Member
                PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                object atomContentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                MetaCode.IClassMember atomContentMember = CreateMember(atomContentObject, atomContentObject.GetType(), string.Empty);

                MetaCode.AtomMember atomMember = mMemberGenerator.NewAtomMember(dataObjectFieldValue.GetType(), atomContentMember, memberName);
                member = atomMember;
            }
            else if (mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // A FileId is holding a simple index
                PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                object contentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                Int64 id = (Int64)contentObject;

                MetaCode.FileIdMember fileIdMember = mMemberGenerator.NewFileIdMember(dataObjectFieldValue.GetType(), id, memberName);
                member = fileIdMember;
            }
            else if (mMemberGenerator.IsCompound(dataObjectFieldType))
            {
                // Create default compound of the given type
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                MetaCode.CompoundMember compoundMember = mMemberGenerator.NewCompoundMember(dataObjectFieldValue.GetType(), dataObjectFieldValue, memberName);
                member = compoundMember;
            }
            else if (mMemberGenerator.IsArray(dataObjectFieldType))
            {
                Type arrayType = typeof(Array);
                MetaCode.ArrayMember arrayMember = mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
                member = arrayMember;
            }
            else if (mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                Type arrayType = dataObjectFieldType.GetGenericTypeDefinition();
                MetaCode.ArrayMember arrayMember = mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
                member = arrayMember;
            }
            else if (mMemberGenerator.IsString(dataObjectFieldType))
            {
                // Create default empty string
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = string.Empty;

                MetaCode.StringMember m = mMemberGenerator.NewStringMember((string)dataObjectFieldValue, memberName) as MetaCode.StringMember;
                member = m;
            }
            else if (mMemberGenerator.IsObject(dataObjectFieldType))
            {
                Type classType;
                if (dataObjectFieldValue!=null)
                    classType = dataObjectFieldValue.GetType();
                else
                    classType = dataObjectFieldType;

                MetaCode.ClassObject m = mMemberGenerator.NewObjectMember(classType, dataObjectFieldValue, memberName);
                member = m;
            }
            else if (mMemberGenerator.IsBool(dataObjectFieldType))
            {
                // Create default bool
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new bool();

                MetaCode.BoolMember m = mMemberGenerator.NewBoolMember((bool)dataObjectFieldValue, memberName) as MetaCode.BoolMember;
                member = m;
            }
            else if (mMemberGenerator.IsInt8(dataObjectFieldType))
            {
                // Create default Int8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new SByte();

                MetaCode.Int8Member m = mMemberGenerator.NewInt8Member((Int8)dataObjectFieldValue, memberName) as MetaCode.Int8Member;
                member = m;
            }
            else if (mMemberGenerator.IsUInt8(dataObjectFieldType))
            {
                // Create default UInt8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt8();

                MetaCode.UInt8Member m = mMemberGenerator.NewUInt8Member((UInt8)dataObjectFieldValue, memberName) as MetaCode.UInt8Member;
                member = m;
            }
            else if (mMemberGenerator.IsInt16(dataObjectFieldType))
            {
                // Create default Int16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int16();

                MetaCode.Int16Member m = mMemberGenerator.NewInt16Member((Int16)dataObjectFieldValue, memberName) as MetaCode.Int16Member;
                member = m;
            }
            else if (mMemberGenerator.IsUInt16(dataObjectFieldType))
            {
                // Create default UInt16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt16();

                MetaCode.UInt16Member m = mMemberGenerator.NewUInt16Member((UInt16)dataObjectFieldValue, memberName) as MetaCode.UInt16Member;
                member = m;
            }
            else if (mMemberGenerator.IsInt32(dataObjectFieldType))
            {
                // Create default Int32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int32();

                MetaCode.Int32Member m = mMemberGenerator.NewInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
                member = m;
            }
            else if (mMemberGenerator.IsUInt32(dataObjectFieldType))
            {
                // Create default UInt32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt32();

                MetaCode.UInt32Member m = mMemberGenerator.NewUInt32Member((UInt32)dataObjectFieldValue, memberName) as MetaCode.UInt32Member;
                member = m;
            }
            else if (mMemberGenerator.IsInt64(dataObjectFieldType))
            {
                // Create default Int64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int64();

                MetaCode.Int64Member m = mMemberGenerator.NewInt64Member((Int64)dataObjectFieldValue, memberName) as MetaCode.Int64Member;
                member = m;
            }
            else if (mMemberGenerator.IsUInt64(dataObjectFieldType))
            {
                // Create default UInt64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt64();

                MetaCode.UInt64Member m = mMemberGenerator.NewUInt64Member((UInt64)dataObjectFieldValue, memberName) as MetaCode.UInt64Member;
                member = m;
            }
            else if (mMemberGenerator.IsFloat(dataObjectFieldType))
            {
                // Create default Float
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                MetaCode.FloatMember m = mMemberGenerator.NewFloatMember((float)dataObjectFieldValue, memberName) as MetaCode.FloatMember;
                member = m;
            }
            else if (mMemberGenerator.IsDouble(dataObjectFieldType))
            {
                // Create default Double
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                MetaCode.DoubleMember m = mMemberGenerator.NewDoubleMember((double)dataObjectFieldValue, memberName) as MetaCode.DoubleMember;
                member = m;
            }
            else if (mMemberGenerator.IsEnum(dataObjectFieldType))
            {
                // Create default enum
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                MetaCode.EnumMember m = mMemberGenerator.NewEnumMember(dataObjectFieldValue, memberName) as MetaCode.EnumMember;
                member = m;
            }
            else
            {
                throw new NotImplementedException();
            }

            return member;
        }

        private MetaCode.IClassMember AddMember(MetaCode.ICompoundMemberBase inCompound, object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName)
        {
            MetaCode.IClassMember member = CreateMember(dataObjectFieldValue, dataObjectFieldType, dataObjectFieldName);
            if (member == null)
                return null;

            inCompound.AddMember(member);

            if (mMemberGenerator.IsAtom(dataObjectFieldType))
            {
                MetaCode.AtomMember m= member as MetaCode.AtomMember;
                mAtomDatabase.Add(m);
            }
            else if (mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                MetaCode.FileIdMember m= member as MetaCode.FileIdMember;
                mFileIdDatabase.Add(m);
            }
            else if (mMemberGenerator.IsEnum(dataObjectFieldType))
			{
                MetaCode.EnumMember m = member as MetaCode.EnumMember;
                mEnumDatabase.Add(m);
            }
            else if (mMemberGenerator.IsCompound(dataObjectFieldType))
            {
                MetaCode.CompoundMember compoundMember = member as MetaCode.CompoundMember;
                // Is ICompound a struct or class, which means to say is it a value or ref type?
                compoundMember.IsNullType = (dataObjectFieldValue == null) || (dataObjectFieldValue.GetType().IsClass);
                if (!compoundMember.IsNullType || dataObjectFieldValue != null)
                {
                    PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Values");
                    if (valuePropertyInfo.GetValue(dataObjectFieldValue, null) is Array objectArray)
                    {
                        foreach (object o in objectArray)
                        {
                            if (o != null)
                                AddMember(compoundMember, o, o.GetType(), string.Empty);
                            else
                                AddMember(compoundMember, null, typeof(object), string.Empty);
                        }
                    }
                }
                mCompoundDatabase.Add(compoundMember);
            }
            else if (mMemberGenerator.IsArray(dataObjectFieldType))
            {
                MetaCode.ArrayMember arrayMember = member as MetaCode.ArrayMember;
                Type fieldElementType = dataObjectFieldType.GetElementType();
                if (dataObjectFieldValue is Array array)
                {
                    foreach (object b in array)
                    {
                        if (b != null)
                            AddMember(arrayMember, b, b.GetType(), string.Empty);
                        else
                            AddMember(arrayMember, null, fieldElementType, string.Empty);
                    }
                }
                mArrayDatabase.Add(arrayMember);
            }
            else if (mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                MetaCode.ArrayMember arrayMember = member as MetaCode.ArrayMember;
                if (dataObjectFieldValue is IEnumerable array)
                {
                    foreach (object b in array)
                    {
                        AddMember(arrayMember, b, b.GetType(), string.Empty);
                    }
                }
                mArrayDatabase.Add(arrayMember);
            }
            else if (mMemberGenerator.IsObject(dataObjectFieldType))
            {
                MetaCode.ClassObject c= member as MetaCode.ClassObject;
                mClassDatabase.Add(c);
                mStack.Push(new KeyValuePair<object, MetaCode.ClassObject>(dataObjectFieldValue, c));
            }
            else if (mMemberGenerator.IsString(dataObjectFieldType))
            {
                MetaCode.StringMember stringMember = member as MetaCode.StringMember;
                mStringDatabase.Add(stringMember);
            }

            return member;
        }

        public void AddMembers(MetaCode.ClassObject inClass, object inClassObject)
        {
            if (inClassObject != null)
            {
                List<FieldInfo> dataObjectFields = GetFieldInfoList(inClassObject);
                foreach (FieldInfo dataObjectFieldInfo in dataObjectFields)
                {
                    string fieldName = dataObjectFieldInfo.Name;
                    Type fieldType = dataObjectFieldInfo.FieldType;
                    object fieldValue = dataObjectFieldInfo.GetValue(inClassObject);
                    AddMember(inClass, fieldValue, fieldType, fieldName);
                }
            }
        }

        #endregion
        #region getFieldInfoList

        /// <summary>
        /// Return a List<FieldInfo> of the incoming object that contains the info of
        /// all fields of that object, including base classes. The returned list is
        /// sorted from base-class down to derived classes, members also appear in the
        /// list in the order of how they are defined.
        /// </summary>
        ///
        private static int CompareFieldInfo(FieldInfo x, FieldInfo y)
        {
            return String.CompareOrdinal(x.GetType().Name, y.GetType().Name);
        }
        public List<FieldInfo> GetFieldInfoList(object o)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            List<FieldInfo> sortedFields = new List<FieldInfo>(fields);
            sortedFields.Sort(CompareFieldInfo);
            return sortedFields;
        }

        #endregion

        #endregion
        #region Analyze

        public void Analyze(object data, MemberBook book)
        {
            // Ok, so every class has fields and we can reflect on the type of the field.
            // When the field is a normal primitive we know what to do and when it is a
            // class it's also easy.
            // But when the class is derived from another class than it's a different story.
            //
            // Solution #1:
            //
            //    We treat all data, including those from the base classes, as if it was only
            //    the derived class. This will result in predictable serialization and generated
            //    code classes are not derived in anyway, making it impossible to 'cast'.
            //
            //
            // Solution #2:
            //
            //    The way we save data is such that it becomes very difficult to 'derive' data.
            //    We can of course use pointers to data, but then again we cannot use virtual
            //    functions, since that would add a 'vfptr'.
            //

            Type dataObjectType = data.GetType();
            MetaCode.ClassObject dataClass = mMemberGenerator.NewObjectMember(dataObjectType, data, dataObjectType.Name);
            mClassDatabase.Add(dataClass);
            mStack.Push(new (data, dataClass));

            while (mStack.Count > 0)
            {
                KeyValuePair<object, MetaCode.ClassObject> p = mStack.Pop();
                AddMembers(p.Value, p.Key);
            }

            book.Classes = new();
            foreach (MetaCode.ClassObject c in mClassDatabase)
                book.Classes.Add(c);

            book.Enums= new();
            foreach (MetaCode.EnumMember c in mEnumDatabase)
                book.Enums.Add(c);

            book.Compounds = new ();
            foreach (MetaCode.CompoundMember c in mCompoundDatabase)
                book.Compounds.Add(c);

            book.Atoms = new ();
            foreach (MetaCode.AtomMember c in mAtomDatabase)
                book.Atoms.Add(c);

            book.FileIds = new ();
            foreach (MetaCode.FileIdMember c in mFileIdDatabase)
                book.FileIds.Add(c);

            book.Arrays = new ();
            foreach (MetaCode.ArrayMember a in mArrayDatabase)
                book.Arrays.Add(a);

            book.Strings = new ();
            foreach (MetaCode.StringMember s in mStringDatabase)
                book.Strings.Add(s);

            mClassDatabase.Clear();
            mCompoundDatabase.Clear();
            mAtomDatabase.Clear();
            mFileIdDatabase.Clear();
            mArrayDatabase.Clear();
            mStringDatabase.Clear();

            mStack.Clear();
        }

        #endregion
        #region Static Methods

        public static bool HasGenericInterface(Type objectType, Type interfaceType)
        {
            Type[] baseTypes = objectType.GetInterfaces();
            foreach (Type t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        public static bool HasOrIsGenericInterface(Type objectType, Type interfaceType)
        {
            if (objectType == interfaceType)
                return true;

            Type[] baseTypes = objectType.GetInterfaces();
            foreach (Type t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        #endregion
    }
}
