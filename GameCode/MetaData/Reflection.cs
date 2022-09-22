using System;
using System.Reflection;
using System.Collections.Generic;

using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;

namespace GameData
{
    #region MemberBook

    public class MemberBook
    {
        protected List<MetaCode.Int64Member> mInt64s;
        protected List<MetaCode.UInt64Member> mUInt64s;
        protected List<MetaCode.ObjectMember> mClasses;
        protected List<MetaCode.CompoundMember> mCompounds;
        protected List<MetaCode.AtomMember> mAtoms;
        protected List<MetaCode.FileIdMember> mFileIds;
        protected List<MetaCode.ArrayMember> mArrays;
        protected List<MetaCode.StringMember> mStrings;

        public List<MetaCode.Int64Member> int64s
        {
            get { return mInt64s; }
            set { mInt64s = value; }
        }

        public List<MetaCode.UInt64Member> uint64s
        {
            get { return mUInt64s; }
            set { mUInt64s = value; }
        }

        public List<MetaCode.ObjectMember> classes
        {
            get { return mClasses; }
            set { mClasses = value; }
        }

        public List<MetaCode.CompoundMember> compounds
        {
            get { return mCompounds; }
            set { mCompounds = value; }
        }

        public List<MetaCode.AtomMember> atoms
        {
            get { return mAtoms; }
            set { mAtoms = value; }
        }

        public List<MetaCode.FileIdMember> fileids
        {
            get { return mFileIds; }
            set { mFileIds = value; }
        }

        public List<MetaCode.ArrayMember> arrays
        {
            get { return mArrays; }
            set { mArrays = value; }
        }

        public List<MetaCode.StringMember> strings
        {
            get { return mStrings; }
            set { mStrings = value; }
        }
    }

    #endregion

    public class Reflector
    {
        #region Fields

        private readonly MetaCode.IMemberGenerator mMemberGenerator;
        private readonly List<MetaCode.Int64Member> mInt64Database;
        private readonly List<MetaCode.UInt64Member> mUInt64Database;
        private readonly List<MetaCode.ObjectMember> mClassDatabase;
        private readonly List<MetaCode.StringMember> mStringDatabase;
        private readonly List<MetaCode.ArrayMember> mArrayDatabase;
        private readonly List<MetaCode.AtomMember> mAtomDatabase;
        private readonly List<MetaCode.FileIdMember> mFileIdDatabase;
        private readonly List<MetaCode.CompoundMember> mCompoundDatabase;

        private readonly Stack<KeyValuePair<object, MetaCode.ObjectMember>> mStack;

        #endregion
        #region Constructor

        public Reflector(MetaCode.IMemberGenerator memberGenerator)
        {
            mMemberGenerator = memberGenerator;

            mInt64Database = new List<MetaCode.Int64Member>();
            mUInt64Database = new List<MetaCode.UInt64Member>();
            mClassDatabase = new List<MetaCode.ObjectMember>();
            mStringDatabase = new List<MetaCode.StringMember>();
            mArrayDatabase = new List<MetaCode.ArrayMember>();
            mAtomDatabase = new List<MetaCode.AtomMember>();
            mFileIdDatabase = new List<MetaCode.FileIdMember>();
            mCompoundDatabase = new List<MetaCode.CompoundMember>();

            mStack = new Stack<KeyValuePair<object, MetaCode.ObjectMember>>();
        }

        #endregion
        #region Methods

        #region addMember

        private MetaCode.Member createMember(object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName)
        {
            MetaCode.Member member = null;

            string memberName = dataObjectFieldName;

            // Check if the object implements the IAtom, ICompound or IObject interface
            // If not then check if it's an Array, Class or other reference type objects
            // Lastly handle all system type objects.
            if (mMemberGenerator.isAtom(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // An Atom is holding a primitive type like Int, Float etc.. we treat this as a MetaCode.Member
                PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                object atomContentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                MetaCode.Member atomContentMember = createMember(atomContentObject, atomContentObject.GetType(), string.Empty);

                MetaCode.AtomMember atomMember = mMemberGenerator.newAtomMember(dataObjectFieldValue.GetType(), atomContentMember, memberName);
                member = atomMember;
            }
            else if (mMemberGenerator.isFileId(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // A FileId is holding a Hash128 object
                PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                object contentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                Hash160 hash = (Hash160)contentObject;

                MetaCode.FileIdMember fileIdMember = mMemberGenerator.newFileIdMember(dataObjectFieldValue.GetType(), hash, memberName);
                member = fileIdMember;
            }
            else if (mMemberGenerator.isCompound(dataObjectFieldType))
            {
                // Create default compound of the given type
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                MetaCode.CompoundMember compoundMember = mMemberGenerator.newCompoundMember(dataObjectFieldValue.GetType(), dataObjectFieldValue, memberName);
                member = compoundMember;
            }
            else if (mMemberGenerator.isArray(dataObjectFieldType))
            {
                Type arrayElementType = dataObjectFieldType.GetElementType();

                Type arrayType;
                if (dataObjectFieldValue != null)
                    arrayType = dataObjectFieldValue.GetType();
                else
                    arrayType = dataObjectFieldType;

                // Recursively create the element member
                MetaCode.Member elementMember = createMember(null, arrayElementType, string.Empty);

                MetaCode.ArrayMember arrayMember = mMemberGenerator.newArrayMember(arrayType, dataObjectFieldValue, elementMember, memberName);
                member = arrayMember;
            }
            else if (mMemberGenerator.isString(dataObjectFieldType))
            {
                // Create default empty string
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = string.Empty;

                MetaCode.StringMember m = mMemberGenerator.newStringMember((string)dataObjectFieldValue, memberName) as MetaCode.StringMember;
                member = m;
            }
            else if (mMemberGenerator.isObject(dataObjectFieldType))
            {
                Type classType;
                if (dataObjectFieldValue!=null)
                    classType = dataObjectFieldValue.GetType();
                else
                    classType = dataObjectFieldType;

                MetaCode.ObjectMember m = mMemberGenerator.newObjectMember(classType, dataObjectFieldValue, memberName);
                member = m;
            }
            else if (mMemberGenerator.isBool(dataObjectFieldType))
            {
                // Create default bool
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new bool();

                MetaCode.BoolMember m = mMemberGenerator.newBoolMember((bool)dataObjectFieldValue, memberName) as MetaCode.BoolMember;
                member = m;
            }
            else if (mMemberGenerator.isInt8(dataObjectFieldType))
            {
                // Create default Int8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new SByte();

                MetaCode.Int8Member m = mMemberGenerator.newInt8Member((Int8)dataObjectFieldValue, memberName) as MetaCode.Int8Member;
                member = m;
            }
            else if (mMemberGenerator.isUInt8(dataObjectFieldType))
            {
                // Create default UInt8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt8();

                MetaCode.UInt8Member m = mMemberGenerator.newUInt8Member((UInt8)dataObjectFieldValue, memberName) as MetaCode.UInt8Member;
                member = m;
            }
            else if (mMemberGenerator.isInt16(dataObjectFieldType))
            {
                // Create default Int16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int16();

                MetaCode.Int16Member m = mMemberGenerator.newInt16Member((Int16)dataObjectFieldValue, memberName) as MetaCode.Int16Member;
                member = m;
            }
            else if (mMemberGenerator.isUInt16(dataObjectFieldType))
            {
                // Create default UInt16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt16();

                MetaCode.UInt16Member m = mMemberGenerator.newUInt16Member((UInt16)dataObjectFieldValue, memberName) as MetaCode.UInt16Member;
                member = m;
            }
            else if (mMemberGenerator.isInt32(dataObjectFieldType))
            {
                // Create default Int32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int32();

                MetaCode.Int32Member m = mMemberGenerator.newInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
                member = m;
            }
            else if (mMemberGenerator.isUInt32(dataObjectFieldType))
            {
                // Create default UInt32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt32();

                MetaCode.UInt32Member m = mMemberGenerator.newUInt32Member((UInt32)dataObjectFieldValue, memberName) as MetaCode.UInt32Member;
                member = m;
            }
            else if (mMemberGenerator.isInt64(dataObjectFieldType))
            {
                // Create default Int64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int64();

                MetaCode.Int64Member m = mMemberGenerator.newInt64Member((Int64)dataObjectFieldValue, memberName) as MetaCode.Int64Member;
                member = m;
            }
            else if (mMemberGenerator.isUInt64(dataObjectFieldType))
            {
                // Create default UInt64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt64();

                MetaCode.UInt64Member m = mMemberGenerator.newUInt64Member((UInt64)dataObjectFieldValue, memberName) as MetaCode.UInt64Member;
                member = m;
            }
            else if (mMemberGenerator.isFloat(dataObjectFieldType))
            {
                // Create default Float
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                MetaCode.FloatMember m = mMemberGenerator.newFloatMember((float)dataObjectFieldValue, memberName) as MetaCode.FloatMember;
                member = m;
            }
            else if (mMemberGenerator.isEnum(dataObjectFieldType))
            {
                // Create default enum
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                MetaCode.Int32Member m = mMemberGenerator.newInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
                member = m;
            }
            else if (mMemberGenerator.isDynamicMember(dataObjectFieldType))
            {
                if (dataObjectFieldValue != null)
                {
                    GameData.IDynamicMember dynamicMember = dataObjectFieldValue as GameData.IDynamicMember;
                    if (dynamicMember.name != string.Empty)
                    {
                        string fieldName = dynamicMember.name;

                        object fieldValue = dynamicMember.value;
                        if (fieldValue != null)
                        {
                            Type fieldType = fieldValue.GetType();
                            member = createMember(fieldValue, fieldType, fieldName);
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return member;
        }

        private MetaCode.Member addMember(MetaCode.CompoundMemberBase inCompound, object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName)
        {
            MetaCode.Member member = createMember(dataObjectFieldValue, dataObjectFieldType, dataObjectFieldName);
            if (member == null)
                return null;

            inCompound.addMember(member);

            if (mMemberGenerator.isAtom(dataObjectFieldType))
            {
                MetaCode.AtomMember atomMember = member as MetaCode.AtomMember;
                mAtomDatabase.Add(atomMember);
            }
            else if (mMemberGenerator.isFileId(dataObjectFieldType))
            {
                MetaCode.FileIdMember fileIdMember = member as MetaCode.FileIdMember;
                mFileIdDatabase.Add(fileIdMember);
            }
            else if (mMemberGenerator.isCompound(dataObjectFieldType))
            {
                MetaCode.CompoundMember compoundMember = member as MetaCode.CompoundMember;
                // Is ICompound a struct or class, which means to say is it a value or ref type?
                compoundMember.isNullType = (dataObjectFieldValue == null) || (dataObjectFieldValue.GetType().IsClass);
                if (!compoundMember.isNullType || dataObjectFieldValue != null)
                {
                    PropertyInfo valuePropertyInfo = dataObjectFieldType.GetProperty("Values");
                    Array objectArray = valuePropertyInfo.GetValue(dataObjectFieldValue, null) as Array;
                    if (objectArray != null)
                    {
                        foreach (object o in objectArray)
                        {
                            if (o != null)
                                addMember(compoundMember, o, o.GetType(), string.Empty);
                            else
                                addMember(compoundMember, null, typeof(object), string.Empty);
                        }
                    }
                }
                mCompoundDatabase.Add(compoundMember);
            }
            else if (mMemberGenerator.isArray(dataObjectFieldType))
            {
                MetaCode.ArrayMember arrayMember = member as MetaCode.ArrayMember;
                Type fieldElementType = dataObjectFieldType.GetElementType();
                Array array = dataObjectFieldValue as Array;
                if (array != null)
                {
                    foreach (object b in array)
                    {
                        if (b != null)
                            addMember(arrayMember, b, b.GetType(), string.Empty);
                        else
                            addMember(arrayMember, null, fieldElementType, string.Empty);
                    }
                }
                mArrayDatabase.Add(arrayMember);
            }
            else if (mMemberGenerator.isObject(dataObjectFieldType))
            {
                MetaCode.ObjectMember classMember = member as MetaCode.ObjectMember;
                mClassDatabase.Add(classMember);
                mStack.Push(new KeyValuePair<object, MetaCode.ObjectMember>(dataObjectFieldValue, classMember));
            }
            else if (mMemberGenerator.isInt64(dataObjectFieldType))
            {
                MetaCode.Int64Member m = member as MetaCode.Int64Member;
                mInt64Database.Add(m);
            }
            else if (mMemberGenerator.isUInt64(dataObjectFieldType))
            {
                MetaCode.UInt64Member m = member as MetaCode.UInt64Member;
                mUInt64Database.Add(m);
            }
            else if (mMemberGenerator.isString(dataObjectFieldType))
            {
                MetaCode.StringMember stringMember = member as MetaCode.StringMember;
                mStringDatabase.Add(stringMember);
            }

            return member;
        }

        public void addMembers(MetaCode.ObjectMember inClass, object inClassObject)
        {
            if (inClassObject != null)
            {
                // TODO: Check for IDynamicClass

                List<FieldInfo> dataObjectFields = getFieldInfoList(inClassObject);
                foreach (FieldInfo dataObjectFieldInfo in dataObjectFields)
                {
                    string fieldName = dataObjectFieldInfo.Name;
                    Type fieldType = dataObjectFieldInfo.FieldType;
                    object fieldValue = dataObjectFieldInfo.GetValue(inClassObject);

                    if (fieldType==typeof(GameData.IDynamicMember) || HasGenericInterface(fieldType, typeof(GameData.IDynamicMember)))
                    {
                        if (fieldValue!=null)
                        {
                            GameData.IDynamicMember dynamicMember = fieldValue as GameData.IDynamicMember;
                            if (dynamicMember.name != string.Empty)
                                fieldName = dynamicMember.name;

                            fieldValue = dynamicMember.value;
                            if (fieldValue != null)
                            {
                                fieldType = dynamicMember.value.GetType();
                                MetaCode.Member member = addMember(inClass, fieldValue, fieldType, fieldName);
                            }
                            else
                            {
                                Console.WriteLine("A dynamic member ({0})with a null value!", fieldName);
                            }
                        }
                    }
                    else
                    {
                        MetaCode.Member member = addMember(inClass, fieldValue, fieldType, fieldName);
                    }
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
        public List<FieldInfo> getFieldInfoList(object o)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            List<FieldInfo> sortedFields = new List<FieldInfo>();
            int insertPos = 0;
            Type declaringType = null;
            foreach (FieldInfo f in fields)
            {
                if (f.DeclaringType != declaringType)
                {
                    insertPos = 0;
                    declaringType = f.DeclaringType;
                }
                sortedFields.Insert(insertPos, f);
                insertPos++;
            }
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
            MetaCode.ObjectMember dataClass = mMemberGenerator.newObjectMember(dataObjectType, data, dataObjectType.Name);
            mClassDatabase.Add(dataClass);
            mStack.Push(new KeyValuePair<object, MetaCode.ObjectMember>(data, dataClass));

            while (mStack.Count > 0)
            {
                KeyValuePair<object, MetaCode.ObjectMember> p = mStack.Pop();
                addMembers(p.Value, p.Key);
            }

            book.int64s = new List<MetaCode.Int64Member>();
            foreach (MetaCode.Int64Member i in mInt64Database)
                book.int64s.Add(i);

            book.uint64s = new List<MetaCode.UInt64Member>();
            foreach (MetaCode.UInt64Member i in mUInt64Database)
                book.uint64s.Add(i);

            book.classes = new List<MetaCode.ObjectMember>();
            foreach (MetaCode.ObjectMember c in mClassDatabase)
                book.classes.Add(c);

            book.compounds = new List<MetaCode.CompoundMember>();
            foreach (MetaCode.CompoundMember c in mCompoundDatabase)
                book.compounds.Add(c);

            book.atoms = new List<MetaCode.AtomMember>();
            foreach (MetaCode.AtomMember c in mAtomDatabase)
                book.atoms.Add(c);

            book.fileids = new List<MetaCode.FileIdMember>();
            foreach (MetaCode.FileIdMember c in mFileIdDatabase)
                book.fileids.Add(c);

            book.arrays = new List<MetaCode.ArrayMember>();
            foreach (MetaCode.ArrayMember a in mArrayDatabase)
                book.arrays.Add(a);

            book.strings = new List<MetaCode.StringMember>();
            foreach (MetaCode.StringMember s in mStringDatabase)
                book.strings.Add(s);

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
