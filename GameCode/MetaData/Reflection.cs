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
        public List<MetaCode.ArrayMember> Arrays { get; set; }
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

        private readonly MetaCode.IMemberFactory _memberFactory;
        private readonly MetaCode.ITypeInformation _typeInfo;
        private readonly List<MetaCode.ClassObject> _mClassDatabase;
        private readonly List<MetaCode.EnumMember> _mEnumDatabase;
        private readonly List<MetaCode.StringMember> _mStringDatabase;
        private readonly List<MetaCode.ArrayMember> _mArrayDatabase;
        private readonly List<MetaCode.StructMember> _mStructDatabase;
        private readonly List<MetaCode.FileIdMember> _mFileIdDatabase;

        private readonly Stack<KeyValuePair<object, MetaCode.ClassObject>> _mStack;

        #endregion

        #region Constructor

        public Reflector(MetaCode.IMemberFactory memberFactory, ITypeInformation typeInfo)
        {
            _memberFactory = memberFactory;
            _typeInfo = typeInfo;

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

            if (_typeInfo.IsIStruct(dataObjectFieldType))
            {
                var contentObject = dataObjectFieldValue;
                var structMember = _memberFactory.NewStructMember(dataObjectFieldType, contentObject, memberName);
                member = structMember;
            }
            else if (_typeInfo.IsFileId(dataObjectFieldType))
            {
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                // A FileId is holding a simple index
                var valuePropertyInfo = dataObjectFieldType.GetProperty("Value");
                var contentObject = valuePropertyInfo.GetValue(dataObjectFieldValue, null);

                member = _memberFactory.NewFileIdMember(dataObjectFieldType, contentObject, memberName);
            }
            else if (_typeInfo.IsArray(dataObjectFieldType))
            {
                var arrayType = typeof(Array);
                member = _memberFactory.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (_typeInfo.IsGenericList(dataObjectFieldType))
            {
                var arrayType = dataObjectFieldType.GetGenericTypeDefinition();
                member = _memberFactory.NewArrayMember(arrayType, dataObjectFieldValue, memberName);
            }
            else if (_typeInfo.IsString(dataObjectFieldType))
            {
                // Create default empty string
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = string.Empty;

                member = _memberFactory.NewStringMember((string)dataObjectFieldValue, memberName) as MetaCode.StringMember;
            }
            else if (_typeInfo.IsClass(dataObjectFieldType))
            {
                Type classType;
                if (dataObjectFieldValue != null)
                    classType = dataObjectFieldValue.GetType();
                else
                    classType = dataObjectFieldType;

                member = _memberFactory.NewObjectMember(classType, dataObjectFieldValue, memberName);
            }
            else if (_typeInfo.IsBool(dataObjectFieldType))
            {
                // Create default bool
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new bool();

                member = _memberFactory.NewBoolMember((bool)dataObjectFieldValue, memberName) as MetaCode.BoolMember;
            }
            else if (_typeInfo.IsInt8(dataObjectFieldType))
            {
                // Create default Int8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new SByte();

                member = _memberFactory.NewInt8Member((Int8)dataObjectFieldValue, memberName) as MetaCode.Int8Member;
            }
            else if (_typeInfo.IsUInt8(dataObjectFieldType))
            {
                // Create default UInt8
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt8();

                member = _memberFactory.NewUInt8Member((UInt8)dataObjectFieldValue, memberName) as MetaCode.UInt8Member;
            }
            else if (_typeInfo.IsInt16(dataObjectFieldType))
            {
                // Create default Int16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int16();

                member = _memberFactory.NewInt16Member((Int16)dataObjectFieldValue, memberName) as MetaCode.Int16Member;
            }
            else if (_typeInfo.IsUInt16(dataObjectFieldType))
            {
                // Create default UInt16
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt16();

                member = _memberFactory.NewUInt16Member((UInt16)dataObjectFieldValue, memberName) as MetaCode.UInt16Member;
            }
            else if (_typeInfo.IsInt32(dataObjectFieldType))
            {
                // Create default Int32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int32();

                member = _memberFactory.NewInt32Member((Int32)dataObjectFieldValue, memberName) as MetaCode.Int32Member;
            }
            else if (_typeInfo.IsUInt32(dataObjectFieldType))
            {
                // Create default UInt32
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt32();

                member = _memberFactory.NewUInt32Member((UInt32)dataObjectFieldValue, memberName) as MetaCode.UInt32Member;
            }
            else if (_typeInfo.IsInt64(dataObjectFieldType))
            {
                // Create default Int64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new Int64();

                member = _memberFactory.NewInt64Member((Int64)dataObjectFieldValue, memberName) as MetaCode.Int64Member;
            }
            else if (_typeInfo.IsUInt64(dataObjectFieldType))
            {
                // Create default UInt64
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new UInt64();

                member = _memberFactory.NewUInt64Member((UInt64)dataObjectFieldValue, memberName) as MetaCode.UInt64Member;
            }
            else if (_typeInfo.IsFloat(dataObjectFieldType))
            {
                // Create default Float
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = _memberFactory.NewFloatMember((float)dataObjectFieldValue, memberName) as MetaCode.FloatMember;
            }
            else if (_typeInfo.IsDouble(dataObjectFieldType))
            {
                // Create default Double
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = new float();

                member = _memberFactory.NewDoubleMember((double)dataObjectFieldValue, memberName) as MetaCode.DoubleMember;
            }
            else if (_typeInfo.IsEnum(dataObjectFieldType))
            {
                // Create default enum
                if (dataObjectFieldValue == null)
                    dataObjectFieldValue = Activator.CreateInstance(dataObjectFieldType);

                member = _memberFactory.NewEnumMember(dataObjectFieldValue, memberName) as MetaCode.EnumMember;
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

            if (_typeInfo.IsIStruct(dataObjectFieldType))
            {
                var m = member as StructMember;
                _mStructDatabase.Add(m);
            }
            else if (_typeInfo.IsFileId(dataObjectFieldType))
            {
                var m = member as FileIdMember;
                _mFileIdDatabase.Add(m);
            }
            else if (_typeInfo.IsEnum(dataObjectFieldType))
            {
                var m = member as EnumMember;
                _mEnumDatabase.Add(m);
            }
            else if (_typeInfo.IsArray(dataObjectFieldType))
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
            else if (_typeInfo.IsGenericList(dataObjectFieldType))
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
            else if (_typeInfo.IsClass(dataObjectFieldType))
            {
                var c = member as ClassObject;
                _mClassDatabase.Add(c);
                _mStack.Push(new KeyValuePair<object, ClassObject>(dataObjectFieldValue, c));
            }
            else if (_typeInfo.IsString(dataObjectFieldType))
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
            var dataClass = _memberFactory.NewObjectMember(dataObjectType, data, dataObjectType.Name);
            _mClassDatabase.Add(dataClass);
            _mStack.Push(new(data, dataClass));

            while (_mStack.Count > 0)
            {
                var p = _mStack.Pop();
                AddMembers(p.Value, p.Key);
            }

            book.Classes = new();
            foreach (var c in _mClassDatabase)
                book.Classes.Add(c);

            book.Enums = new();
            foreach (var c in _mEnumDatabase)
                book.Enums.Add(c);

            book.Arrays = new();
            foreach (var a in _mArrayDatabase)
                book.Arrays.Add(a);

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
