using System;
using System.Collections;
using System.Reflection;

using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameData.MetaCode;

namespace GameData
{
    #region MemberBook

    public class MemberBook
    {
        public List<MetaCode.ClassObject> Classes { get; set; }
        public List<MetaCode.EnumMember> Enums { get; set; }
        public List<MetaCode.StructMember> Structs { get; set; }
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
        private readonly List<MetaCode.StructMember> mStructDatabase;
        private readonly List<MetaCode.FileIdMember> mFileIdDatabase;

        private readonly Stack<KeyValuePair<object, MetaCode.ClassObject>> mStack;

        #endregion
        #region Constructor

        public Reflector(MetaCode.IMemberGenerator memberGenerator)
        {
            mMemberGenerator = memberGenerator;

            mClassDatabase = new ();
            mStringDatabase = new ();
            mArrayDatabase = new ();
            mStructDatabase = new ();
            mFileIdDatabase = new ();

            mEnumDatabase = new();

            mStack = new ();
        }

        #endregion

        #region AddMember

        private MetaCode.IClassMember CreateMember(object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName, EOptions options)
        {
            MetaCode.IClassMember member = null;

            // Adjust member name
            var memberName = dataObjectFieldName;
            if (memberName.StartsWith("m_"))
                memberName = memberName[2..];

            // A nullable type
            if (Nullable.GetUnderlyingType(dataObjectFieldType) != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(dataObjectFieldType);
                var nullableMember = CreateMember(dataObjectFieldValue, underlyingType, dataObjectFieldName, EOptions.None);
                nullableMember.IsPointerTo = true;
                return nullableMember;
            }

            if (mMemberGenerator.IsIStruct(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                var contentObject = dataObjectFieldValue;
                var structMember = mMemberGenerator.NewStructMember(contentObject as IStruct, memberName);
                member = structMember;
            }
            else if (mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // A FileId is holding a simple index
                var valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                var contentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                var id = (Int64)contentObject;

                member = mMemberGenerator.NewFileIdMember(id, memberName);
            }
            else if (mMemberGenerator.IsArray(dataObjectFieldType))
            {
                var arrayType = typeof(Array);
                member = mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                var arrayType = dataObjectFieldType.GetGenericTypeDefinition();
                member = mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (mMemberGenerator.IsString(dataObjectFieldType))
            {
                // Create default empty string
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = string.Empty;

                member = mMemberGenerator.NewStringMember((string)dataObjectFieldValue, memberName) as MetaCode.StringMember;
            }
            else if (mMemberGenerator.IsObject(dataObjectFieldType))
            {
                Type classType;
                if (dataObjectFieldValue != null)
                    classType = dataObjectFieldValue.GetType();
                else
                    classType = dataObjectFieldType;

                member = mMemberGenerator.NewObjectMember(classType, dataObjectFieldValue, memberName);
            }
            else if (mMemberGenerator.IsBool(dataObjectFieldType))
            {
                // Create default bool
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new bool();

                member = mMemberGenerator.NewBoolMember((bool)dataObjectFieldValue, memberName) as MetaCode.BoolMember;
            }
            else if (mMemberGenerator.IsInt8(dataObjectFieldType))
            {
                // Create default Int8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new SByte();

                member = mMemberGenerator.NewInt8Member((Int8)dataObjectFieldValue, memberName) as MetaCode.Int8Member;
            }
            else if (mMemberGenerator.IsUInt8(dataObjectFieldType))
            {
                // Create default UInt8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt8();

                member = mMemberGenerator.NewUInt8Member((UInt8)dataObjectFieldValue, memberName) as MetaCode.UInt8Member;
            }
            else if (mMemberGenerator.IsInt16(dataObjectFieldType))
            {
                // Create default Int16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int16();

                member = mMemberGenerator.NewInt16Member((Int16)dataObjectFieldValue, memberName) as MetaCode.Int16Member;
            }
            else if (mMemberGenerator.IsUInt16(dataObjectFieldType))
            {
                // Create default UInt16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt16();

                member = mMemberGenerator.NewUInt16Member((UInt16)dataObjectFieldValue, memberName) as MetaCode.UInt16Member;
            }
            else if (mMemberGenerator.IsInt32(dataObjectFieldType))
            {
                // Create default Int32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int32();

                member = mMemberGenerator.NewInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
            }
            else if (mMemberGenerator.IsUInt32(dataObjectFieldType))
            {
                // Create default UInt32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt32();

                member = mMemberGenerator.NewUInt32Member((UInt32)dataObjectFieldValue, memberName) as MetaCode.UInt32Member;
            }
            else if (mMemberGenerator.IsInt64(dataObjectFieldType))
            {
                // Create default Int64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int64();

                member = mMemberGenerator.NewInt64Member((Int64)dataObjectFieldValue, memberName) as MetaCode.Int64Member;
            }
            else if (mMemberGenerator.IsUInt64(dataObjectFieldType))
            {
                // Create default UInt64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt64();

                member = mMemberGenerator.NewUInt64Member((UInt64)dataObjectFieldValue, memberName) as MetaCode.UInt64Member;
            }
            else if (mMemberGenerator.IsFloat(dataObjectFieldType))
            {
                // Create default Float
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = mMemberGenerator.NewFloatMember((float)dataObjectFieldValue, memberName) as MetaCode.FloatMember;
            }
            else if (mMemberGenerator.IsDouble(dataObjectFieldType))
            {
                // Create default Double
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = mMemberGenerator.NewDoubleMember((double)dataObjectFieldValue, memberName) as MetaCode.DoubleMember;
            }
            else if (mMemberGenerator.IsEnum(dataObjectFieldType))
            {
                // Create default enum
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                member = mMemberGenerator.NewEnumMember(dataObjectFieldValue, memberName) as MetaCode.EnumMember;
            }
            else
            {
                throw new NotImplementedException();
            }

            return member;
        }

        [Flags]
        private enum EOptions : int
        {
            None,
            ArrayElementsInPlace,
        }

        private IClassMember AddMember(ICompoundMemberBase inCompound, object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName, EOptions options)
        {
            var member = CreateMember(dataObjectFieldValue, dataObjectFieldType, dataObjectFieldName, options);
            if (member == null)
                return null;

            inCompound.AddMember(member);

            if (mMemberGenerator.IsIStruct(dataObjectFieldType))
            {
                var m= member as StructMember;
                mStructDatabase.Add(m);
            }
            else if (mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                var m= member as FileIdMember;
                mFileIdDatabase.Add(m);
            }
            else if (mMemberGenerator.IsEnum(dataObjectFieldType))
			{
                var m = member as EnumMember;
                mEnumDatabase.Add(m);
            }
            else if (mMemberGenerator.IsArray(dataObjectFieldType))
            {
                var arrayMember = member as ArrayMember;
                var fieldElementType = dataObjectFieldType.GetElementType();
                if (dataObjectFieldValue is Array array)
                {
                    for (var i=0; i<array.Length; ++i)
                    {
                        var b = array.GetValue(i);
                        if (b == null)
                        {
                            b = Activator.CreateInstance(fieldElementType);
                        }
                        
                        var element = AddMember(arrayMember, b, b.GetType(), string.Empty, EOptions.None);
                        if (fieldElementType.IsClass && options.HasFlag(EOptions.ArrayElementsInPlace))
                        {
                            // Class object should be serialized in-place
                            element.IsPointerTo = false;
                        }
                    }
                }
                mArrayDatabase.Add(arrayMember);
            }
            else if (mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                var arrayMember = member as ArrayMember;
                if (dataObjectFieldValue is IEnumerable array)
                {
                    var fieldElementType = dataObjectFieldType.GenericTypeArguments[0];
                    foreach(var o in array)
                    {
                        var b = o;
                        if (b == null)
                        {
                            b = Activator.CreateInstance(fieldElementType);
                        }

                        var element = AddMember(arrayMember, b, b.GetType(), string.Empty, EOptions.None);
                        if (fieldElementType.IsClass && options.HasFlag(EOptions.ArrayElementsInPlace))
                        {
                            // Class object should be serialized in-place
                            element.IsPointerTo = false;
                        }
                    }
                }
                mArrayDatabase.Add(arrayMember);
            }
            else if (mMemberGenerator.IsObject(dataObjectFieldType))
            {
                var c= member as ClassObject;
                mClassDatabase.Add(c);
                mStack.Push(new KeyValuePair<object, ClassObject>(dataObjectFieldValue, c));
            }
            else if (mMemberGenerator.IsString(dataObjectFieldType))
            {
                var stringMember = member as StringMember;
                mStringDatabase.Add(stringMember);
            }

            return member;
        }

        public void AddMembers(ClassObject inClass, object inClassObject)
        {
            if (inClassObject == null) return;
            var dataObjectFields = GetFieldInfoList(inClassObject);
            foreach (var dataObjectFieldInfo in dataObjectFields)
            {
                var fieldName = dataObjectFieldInfo.Name;
                var fieldType = dataObjectFieldInfo.FieldType;
                var fieldValue = dataObjectFieldInfo.GetValue(inClassObject);
                var options = EOptions.None;

                foreach (var attribute in dataObjectFieldInfo.CustomAttributes)
                {
                    if (attribute.AttributeType == typeof(ArrayElementsInPlace))
                    {
                        options |= EOptions.ArrayElementsInPlace;
                    }
                }
                AddMember(inClass, fieldValue, fieldType, fieldName, options);
            }
        }

        #endregion
        #region GetFieldInfoList

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

            var dataObjectType = data.GetType();
            var dataClass = mMemberGenerator.NewObjectMember(dataObjectType, data, dataObjectType.Name);
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

            book.Structs = new ();
            foreach (MetaCode.StructMember c in mStructDatabase)
                book.Structs.Add(c);

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
            mStructDatabase.Clear();
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
