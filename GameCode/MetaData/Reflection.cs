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
        public List<MetaCode.FileIdMember> FileIds { get; set; }
        public List<MetaCode.ArrayMember> Arrays { get; set; }
        public List<MetaCode.StringMember> Strings { get; set; }
    }

    #endregion

    public class Reflector
    {
        #region IsNullable

        // Extension methods on the Type class to determine if a type is nullable
        public static class NullableTypeHelper
        {
            public static bool IsNullable(Type type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return true;
                return false;
            }
        }

        #endregion

        #region Fields

        private readonly MetaCode.IMemberGenerator _mMemberGenerator;
        private readonly List<MetaCode.ClassObject> _mClassDatabase;
        private readonly List<MetaCode.EnumMember> _mEnumDatabase;
        private readonly List<MetaCode.StringMember> _mStringDatabase;
        private readonly List<MetaCode.ArrayMember> _mArrayDatabase;
        private readonly List<MetaCode.StructMember> _mStructDatabase;
        private readonly List<MetaCode.FileIdMember> _mFileIdDatabase;

        private readonly Stack<KeyValuePair<object, MetaCode.ClassObject>> _mStack;

        #endregion

        #region Constructor

        public Reflector(MetaCode.IMemberGenerator memberGenerator)
        {
            _mMemberGenerator = memberGenerator;

            _mClassDatabase = new();
            _mStringDatabase = new();
            _mArrayDatabase = new();
            _mStructDatabase = new();
            _mFileIdDatabase = new();

            _mEnumDatabase = new();

            _mStack = new();
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

            if (_mMemberGenerator.IsIStruct(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                var contentObject = dataObjectFieldValue;
                var structMember = _mMemberGenerator.NewStructMember(contentObject as IStruct, memberName);
                member = structMember;
            }
            else if (_mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // A FileId is holding a simple index
                var valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                var contentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);
                var id = (Int64)contentObject;

                member = _mMemberGenerator.NewFileIdMember(id, memberName);
            }
            else if (_mMemberGenerator.IsArray(dataObjectFieldType))
            {
                var arrayType = typeof(Array);
                member = _mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (_mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                var arrayType = dataObjectFieldType.GetGenericTypeDefinition();
                member = _mMemberGenerator.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (_mMemberGenerator.IsString(dataObjectFieldType))
            {
                // Create default empty string
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = string.Empty;

                member = _mMemberGenerator.NewStringMember((string)dataObjectFieldValue, memberName) as MetaCode.StringMember;
            }
            else if (_mMemberGenerator.IsObject(dataObjectFieldType))
            {
                Type classType;
                if (dataObjectFieldValue != null)
                    classType = dataObjectFieldValue.GetType();
                else
                    classType = dataObjectFieldType;

                member = _mMemberGenerator.NewObjectMember(classType, dataObjectFieldValue, memberName);
            }
            else if (_mMemberGenerator.IsBool(dataObjectFieldType))
            {
                // Create default bool
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new bool();

                member = _mMemberGenerator.NewBoolMember((bool)dataObjectFieldValue, memberName) as MetaCode.BoolMember;
            }
            else if (_mMemberGenerator.IsInt8(dataObjectFieldType))
            {
                // Create default Int8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new SByte();

                member = _mMemberGenerator.NewInt8Member((Int8)dataObjectFieldValue, memberName) as MetaCode.Int8Member;
            }
            else if (_mMemberGenerator.IsUInt8(dataObjectFieldType))
            {
                // Create default UInt8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt8();

                member = _mMemberGenerator.NewUInt8Member((UInt8)dataObjectFieldValue, memberName) as MetaCode.UInt8Member;
            }
            else if (_mMemberGenerator.IsInt16(dataObjectFieldType))
            {
                // Create default Int16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int16();

                member = _mMemberGenerator.NewInt16Member((Int16)dataObjectFieldValue, memberName) as MetaCode.Int16Member;
            }
            else if (_mMemberGenerator.IsUInt16(dataObjectFieldType))
            {
                // Create default UInt16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt16();

                member = _mMemberGenerator.NewUInt16Member((UInt16)dataObjectFieldValue, memberName) as MetaCode.UInt16Member;
            }
            else if (_mMemberGenerator.IsInt32(dataObjectFieldType))
            {
                // Create default Int32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int32();

                member = _mMemberGenerator.NewInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
            }
            else if (_mMemberGenerator.IsUInt32(dataObjectFieldType))
            {
                // Create default UInt32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt32();

                member = _mMemberGenerator.NewUInt32Member((UInt32)dataObjectFieldValue, memberName) as MetaCode.UInt32Member;
            }
            else if (_mMemberGenerator.IsInt64(dataObjectFieldType))
            {
                // Create default Int64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int64();

                member = _mMemberGenerator.NewInt64Member((Int64)dataObjectFieldValue, memberName) as MetaCode.Int64Member;
            }
            else if (_mMemberGenerator.IsUInt64(dataObjectFieldType))
            {
                // Create default UInt64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt64();

                member = _mMemberGenerator.NewUInt64Member((UInt64)dataObjectFieldValue, memberName) as MetaCode.UInt64Member;
            }
            else if (_mMemberGenerator.IsFloat(dataObjectFieldType))
            {
                // Create default Float
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = _mMemberGenerator.NewFloatMember((float)dataObjectFieldValue, memberName) as MetaCode.FloatMember;
            }
            else if (_mMemberGenerator.IsDouble(dataObjectFieldType))
            {
                // Create default Double
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = _mMemberGenerator.NewDoubleMember((double)dataObjectFieldValue, memberName) as MetaCode.DoubleMember;
            }
            else if (_mMemberGenerator.IsEnum(dataObjectFieldType))
            {
                // Create default enum
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                member = _mMemberGenerator.NewEnumMember(dataObjectFieldValue, memberName) as MetaCode.EnumMember;
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

            if (_mMemberGenerator.IsIStruct(dataObjectFieldType))
            {
                var m = member as StructMember;
                _mStructDatabase.Add(m);
            }
            else if (_mMemberGenerator.IsFileId(dataObjectFieldType))
            {
                var m = member as FileIdMember;
                _mFileIdDatabase.Add(m);
            }
            else if (_mMemberGenerator.IsEnum(dataObjectFieldType))
            {
                var m = member as EnumMember;
                _mEnumDatabase.Add(m);
            }
            else if (_mMemberGenerator.IsArray(dataObjectFieldType))
            {
                var arrayMember = member as ArrayMember;
                var fieldElementType = dataObjectFieldType.GetElementType();
                if (dataObjectFieldValue is Array array)
                {
                    for (var i = 0; i < array.Length; ++i)
                    {
                        var b = array.GetValue(i);
                        var element = AddMember(arrayMember, b, fieldElementType, string.Empty, EOptions.None);
                        if (fieldElementType.IsClass && options.HasFlag(EOptions.ArrayElementsInPlace))
                        {
                            // Class object should be serialized in-place
                            element.IsPointerTo = false;
                        }
                    }
                }

                _mArrayDatabase.Add(arrayMember);
            }
            else if (_mMemberGenerator.IsGenericList(dataObjectFieldType))
            {
                var arrayMember = member as ArrayMember;
                if (dataObjectFieldValue is IList list)
                {
                    var fieldElementType = dataObjectFieldType.GenericTypeArguments[0];
					for (var i = 0; i < list.Count; ++i)
					{
                        var b = list[i];
						var element = AddMember(arrayMember, b, fieldElementType, string.Empty, EOptions.None);
                        if (fieldElementType.IsClass && options.HasFlag(EOptions.ArrayElementsInPlace))
                        {
                            // Class object should be serialized in-place
                            element.IsPointerTo = false;
                        }
                    }
                }

                _mArrayDatabase.Add(arrayMember);
            }
            else if (_mMemberGenerator.IsObject(dataObjectFieldType))
            {
                var c = member as ClassObject;
                _mClassDatabase.Add(c);
                _mStack.Push(new KeyValuePair<object, ClassObject>(dataObjectFieldValue, c));
            }
            else if (_mMemberGenerator.IsString(dataObjectFieldType))
            {
                var stringMember = member as StringMember;
                _mStringDatabase.Add(stringMember);
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
            var dataClass = _mMemberGenerator.NewObjectMember(dataObjectType, data, dataObjectType.Name);
            _mClassDatabase.Add(dataClass);
            _mStack.Push(new(data, dataClass));

            while (_mStack.Count > 0)
            {
                KeyValuePair<object, MetaCode.ClassObject> p = _mStack.Pop();
                AddMembers(p.Value, p.Key);
            }

            book.Classes = new();
            foreach (MetaCode.ClassObject c in _mClassDatabase)
                book.Classes.Add(c);

            book.Enums = new();
            foreach (MetaCode.EnumMember c in _mEnumDatabase)
                book.Enums.Add(c);

            book.Structs = new();
            foreach (MetaCode.StructMember c in _mStructDatabase)
                book.Structs.Add(c);

            book.FileIds = new();
            foreach (MetaCode.FileIdMember c in _mFileIdDatabase)
                book.FileIds.Add(c);

            book.Arrays = new();
            foreach (MetaCode.ArrayMember a in _mArrayDatabase)
                book.Arrays.Add(a);

            book.Strings = new();
            foreach (MetaCode.StringMember s in _mStringDatabase)
                book.Strings.Add(s);

            _mClassDatabase.Clear();
            _mStructDatabase.Clear();
            _mFileIdDatabase.Clear();
            _mArrayDatabase.Clear();
            _mStringDatabase.Clear();

            _mStack.Clear();
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
