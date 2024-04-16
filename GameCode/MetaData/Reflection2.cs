using System;
using System.Collections;
using System.Reflection;
using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameData.MetaCode;

namespace GameData
{
    public class Reflector2
    {
        #region IsNullable

        // Extension methods on the Type class to determine if a type is nullable
        public static class NullableTypeHelper
        {
            public static bool IsNullable(Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
        }

        #endregion

        #region Fields

        private readonly MetaCode.MetaCode _metaCode;
        private readonly IMemberFactory2 _memberFactory;
        private readonly ITypeInformation _typeInformation;

        #endregion

        #region Constructor

        public Reflector2(MetaCode.MetaCode metaCode, MetaCode.IMemberFactory2 memberFactory, ITypeInformation typeInformation)
        {
            _metaCode = metaCode;
            _memberFactory = memberFactory;
            _typeInformation = typeInformation;
        }

        #endregion

        #region AddMember

        // A delegate that can process a member
        private delegate void ProcessDelegate(MemberProcess m);

        private struct MemberProcess
        {
            public int MemberIndex { get; init; }
            public object Object { get; init; }
            public Type Type { get; init; }
            public ProcessDelegate Process { get; init; }
        }

        private readonly Queue<MemberProcess> _memberProcessQueue = new(256);

        private void ProcessArray(MemberProcess m)
        {
            if (m.Object is not Array array)
                return;

            var startIndex = _metaCode.MembersType.Count;
            var elementType = array.GetType().GetElementType();
            var elementName = string.Empty;
            for (var i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                CreateMember(element, elementType, elementName);
            }

            var endIndex = _metaCode.MembersType.Count;
            var count = endIndex - startIndex;
            if (count == 0)
            {
                // We will still emit an element because we need to know the type
                CreateMember(null, elementType, elementName);
            }

            _metaCode.UpdateStartIndexAndCount(m.MemberIndex, startIndex, count);
        }

        private void ProcessList(MemberProcess m)
        {
            if (m.Object is not IList list)
                return;

            var startIndex = _metaCode.MembersType.Count;
            var elementType = m.Type.GetGenericArguments()[0];
            var elementName = string.Empty;
            foreach (var element in list)
            {
                CreateMember(element, elementType, elementName);
            }

            var endIndex = _metaCode.MembersType.Count;

            _metaCode.UpdateStartIndexAndCount(m.MemberIndex, startIndex, endIndex - startIndex);
        }

        private void ProcessDictionary(MemberProcess m)
        {
            if (m.Object is not IDictionary dictionary)
                return;

            var elementName = string.Empty;
            var startIndex = _metaCode.MembersType.Count;

            var dictionaryGenericTyping = dictionary.GetType().GetGenericArguments();

            var keyType = dictionaryGenericTyping[0];
            foreach (DictionaryEntry element in dictionary)
            {
                CreateMember(element.Key, keyType, elementName);
            }

            var valueType = dictionaryGenericTyping[1];
            foreach (DictionaryEntry element in dictionary)
            {
                CreateMember(element.Value, valueType, elementName);
            }

            var endIndex = _metaCode.MembersType.Count;

            _metaCode.UpdateStartIndexAndCount(m.MemberIndex, startIndex, endIndex - startIndex);
        }

        private void ProcessClass(MemberProcess m)
        {
            var startIndex = _metaCode.MembersType.Count;
            var dataObjectFields = GetFieldInfoList(m.Object);
            foreach (var dataObjectFieldInfo in dataObjectFields)
            {
                var fieldName = dataObjectFieldInfo.Name;
                var fieldType = dataObjectFieldInfo.FieldType;
                var fieldValue = dataObjectFieldInfo.GetValue(m.Object);
                CreateMember(fieldValue, fieldType, fieldName);
            }

            var endIndex = _metaCode.MembersType.Count;
            _metaCode.UpdateStartIndexAndCount(m.MemberIndex, startIndex, endIndex - startIndex);
        }

        private void CreateMember(object dataObjectFieldValue, Type dataObjectFieldType, string dataObjectFieldName)
        {
            // Adjust member name
            var memberName = dataObjectFieldName;
            if (memberName.StartsWith("m_"))
                memberName = memberName[2..];

            // A nullable type, and this should be a class, if it is not then we just handle the underlying type
            if (Nullable.GetUnderlyingType(dataObjectFieldType) != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(dataObjectFieldType);
                if (dataObjectFieldValue == null && underlyingType != null)
                {
                    if (_typeInformation.IsArray(underlyingType) || _typeInformation.IsGenericList(underlyingType))
                    {
                        dataObjectFieldType = underlyingType;
                        dataObjectFieldValue = Activator.CreateInstance(underlyingType);
                    }
                    else if (_typeInformation.IsStruct(underlyingType) || _typeInformation.IsClass(underlyingType))
                    {
                        dataObjectFieldType = underlyingType;
                    }
                    else
                    {
                        dataObjectFieldType = underlyingType;
                        dataObjectFieldValue = Activator.CreateInstance(underlyingType);
                    }
                }
                else if (underlyingType != null)
                {
                    dataObjectFieldType = underlyingType;
                }
                else
                {
                    // This is a nullable type and the underlying type is null
                    return;
                }
            }

            if (_typeInformation.IsIStruct(dataObjectFieldType))
            {
                _memberFactory.NewStructMember(dataObjectFieldType, dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsGenericDictionary(dataObjectFieldType))
            {
                var member = _memberFactory.NewDictionaryMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessDictionary });
            }
            else if (_typeInformation.IsArray(dataObjectFieldType))
            {
                var member = _memberFactory.NewArrayMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessArray });
            }
            else if (_typeInformation.IsGenericList(dataObjectFieldType))
            {
                var member = _memberFactory.NewArrayMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessList });
            }
            else if (_typeInformation.IsStruct(dataObjectFieldType))
            {
                var member = _memberFactory.NewClassMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessClass });
            }
            else if (_typeInformation.IsClass(dataObjectFieldType))
            {
                var member = _memberFactory.NewClassMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessClass });
            }
            else if (_typeInformation.IsString(dataObjectFieldType))
            {
                _memberFactory.NewStringMember(dataObjectFieldValue as string, memberName);
            }
            else if (_typeInformation.IsBool(dataObjectFieldType))
            {
                _memberFactory.NewBoolMember(dataObjectFieldValue != null && (bool)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt8(dataObjectFieldType))
            {
                _memberFactory.NewInt8Member((Int8)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt8(dataObjectFieldType))
            {
                _memberFactory.NewUInt8Member((UInt8)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt16(dataObjectFieldType))
            {
                _memberFactory.NewInt16Member((Int16)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt16(dataObjectFieldType))
            {
                _memberFactory.NewUInt16Member((UInt16)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt32(dataObjectFieldType))
            {
                _memberFactory.NewInt32Member((Int32)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt32(dataObjectFieldType))
            {
                _memberFactory.NewUInt32Member((UInt32)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt64(dataObjectFieldType))
            {
                _memberFactory.NewInt64Member((Int64)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt64(dataObjectFieldType))
            {
                _memberFactory.NewUInt64Member((UInt64)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsFloat(dataObjectFieldType))
            {
                _memberFactory.NewFloatMember((float)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsDouble(dataObjectFieldType))
            {
                _memberFactory.NewDoubleMember((double)dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsEnum(dataObjectFieldType))
            {
                _memberFactory.NewEnumMember(dataObjectFieldType, dataObjectFieldValue, memberName);
            }
        }

        #endregion

        #region GetFieldInfoList

        /// <summary>
        /// Return a 'List<FieldInfo>' of the incoming object that contains the info of
        /// all fields of that object, including base classes. The returned list is
        /// sorted from base-class down to derived classes, members also appear in the
        /// list in the order of how they are defined.
        /// </summary>
        ///
        private static int CompareFieldInfo(FieldInfo x, FieldInfo y)
        {
            return string.CompareOrdinal(x.GetType().Name, y.GetType().Name);
        }

        private static List<FieldInfo> GetFieldInfoList(object o)
        {
            var fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            var sortedFields = new List<FieldInfo>(fields);
            sortedFields.Sort(CompareFieldInfo);
            return sortedFields;
        }

        #endregion

        #region Analyze

        public void Analyze(object root)
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

            if (!_typeInformation.IsClass(root.GetType())) return;

            var rootIndex = _memberFactory.NewClassMember(root.GetType(), root, string.Empty);
            _memberProcessQueue.Enqueue(new MemberProcess { MemberIndex = rootIndex, Object = root, Type = root.GetType(), Process = ProcessClass });
            while (_memberProcessQueue.Count > 0)
            {
                var m = _memberProcessQueue.Dequeue();
                m.Process(m);
            }
        }

        #endregion
    }
}
