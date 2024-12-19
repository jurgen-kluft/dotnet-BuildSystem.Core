using System.Collections;
using System.Reflection;
using GameData.MetaCode;

namespace GameData
{
    public class Reflector2
    {
        private readonly MetaCode2 _metaCode2;
        private readonly IMemberFactory2 _memberFactory;
        private readonly ITypeInfo2 _typeInformation;

        public Reflector2(MetaCode2 metaCode2, IMemberFactory2 memberFactory, ITypeInfo2 typeInformation)
        {
            _metaCode2 = metaCode2;
            _memberFactory = memberFactory;
            _typeInformation = typeInformation;
        }

        // A delegate that can process a member
        private delegate void ProcessDelegate(MemberProcessor m);

        private struct MemberProcessor
        {
            public int Index { get; init; }
            public object Object { get; init; }
            public Type Type { get; init; }
            public ProcessDelegate Process { get; init; }
        }

        private readonly Queue<MemberProcessor> _memberProcessQueue = new(256);

        private void ProcessArray(MemberProcessor m)
        {
            if (m.Object is not System.Array array)
                return;

            // Even though the array is empty, we still want a way to recognize the type of members in the array.
            // So we put the count of the array to 0, start index to an entry that serves as the type of the array

            var startIndex = _metaCode2.MembersType.Count;
            var elementType = array.GetType().GetElementType();
            var elementName = string.Empty;
            for (var i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                CreateMember(element, elementType, elementName);
            }

            var endIndex = _metaCode2.MembersType.Count;
            var count = endIndex - startIndex;
            if (count == 0)
            {
                // We will still emit an element because we need to know the type
                var instance = Activator.CreateInstance(elementType);
                CreateMember(instance, elementType, elementName);
            }

            _metaCode2.UpdateStartIndexAndCount(m.Index, startIndex, count);
        }

        private void ProcessList(MemberProcessor m)
        {
            if (m.Object is not IList list)
                return;

            // Even though the array is empty, we still want a way to recognize the type of members in the array.
            // So we put the count of the array to 0, start index to an entry that serves as the type of the array

            var startIndex = _metaCode2.MembersType.Count;
            var elementType = m.Type.GetGenericArguments()[0];
            var elementName = string.Empty;
            foreach (var element in list)
            {
                CreateMember(element, elementType, elementName);
            }

            var endIndex = _metaCode2.MembersType.Count;
            var count = endIndex - startIndex;
            if (count == 0)
            {
                // We will still emit an element because we need to know the type
                var instance = Activator.CreateInstance(elementType);
                CreateMember(instance, elementType, elementName);
            }

            _metaCode2.UpdateStartIndexAndCount(m.Index, startIndex, count);
        }

        private void ProcessDictionary(MemberProcessor m)
        {
            if (m.Object is not IDictionary dictionary)
                return;

            var elementName = string.Empty;
            var startIndex = _metaCode2.MembersType.Count;

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

            var endIndex = _metaCode2.MembersType.Count;

            _metaCode2.UpdateStartIndexAndCount(m.Index, startIndex, endIndex - startIndex);
        }

        private void ProcessClass(MemberProcessor m)
        {
            var startIndex = _metaCode2.MembersType.Count;
            var dataObjectFields = GetFieldInfoList(m.Object);
            foreach (var dataObjectFieldInfo in dataObjectFields)
            {
                var fieldName = dataObjectFieldInfo.Name;
                var fieldType = dataObjectFieldInfo.FieldType;
                var fieldValue = dataObjectFieldInfo.GetValue(m.Object);
                CreateMember(fieldValue, fieldType, fieldName);
            }

            var endIndex = _metaCode2.MembersType.Count;
            _metaCode2.UpdateStartIndexAndCount(m.Index, startIndex, endIndex - startIndex);
        }

        private void ProcessEnum(MemberProcessor m)
        {
            var startIndex = _metaCode2.MembersType.Count;

            if (m.Object is System.Enum e)
            {
                var primitiveType = Enum.GetUnderlyingType(e.GetType());
                if (primitiveType == typeof(byte))
                {
                    _memberFactory.NewUInt8Member(e, string.Empty);
                }
                else if (primitiveType == typeof(sbyte))
                {
                    _memberFactory.NewInt8Member(e, string.Empty);
                }
                else if (primitiveType == typeof(short))
                {
                    _memberFactory.NewInt16Member(e, string.Empty);
                }
                else if (primitiveType == typeof(ushort))
                {
                    _memberFactory.NewUInt16Member(e, string.Empty);
                }
                else if (primitiveType == typeof(int))
                {
                    _memberFactory.NewInt32Member(e, string.Empty);
                }
                else if (primitiveType == typeof(uint))
                {
                    _memberFactory.NewUInt32Member(e, string.Empty);
                }
                else if (primitiveType == typeof(long))
                {
                    _memberFactory.NewInt64Member(e, string.Empty);
                }
                else if (primitiveType == typeof(ulong))
                {
                    _memberFactory.NewUInt64Member(e, string.Empty);
                }
                else
                {
                    _memberFactory.NewInt32Member(0, string.Empty);
                }
            }
            else
            {
                _memberFactory.NewInt32Member(0, string.Empty);
            }

            var endIndex = _metaCode2.MembersType.Count;
            _metaCode2.UpdateStartIndexAndCount(m.Index, startIndex, endIndex - startIndex);
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

            if (_typeInformation.IsDataUnit(dataObjectFieldType))
            {
                var member = _memberFactory.NewDataUnitMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessClass });
            }
            else if (_typeInformation.IsDataFile(dataObjectFieldType))
            {
                // The DataFile provides a CookedObject which always has to be a class or an IStruct
                if (dataObjectFieldValue is IDataFile dataFile)
                {
                    var cookedObject = dataFile.CookedObject;
                    var cookedObjectType = cookedObject.GetType();
                    if (_typeInformation.IsIStruct(cookedObjectType))
                    {
                        _memberFactory.NewStructMember(cookedObjectType, cookedObject, memberName);
                    }
                    else
                    {
                        var member = _memberFactory.NewClassMember(cookedObjectType, cookedObject, memberName);
                        _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = cookedObject, Type = cookedObjectType, Process = ProcessClass });
                    }
                }
            }
            else if (_typeInformation.IsIStruct(dataObjectFieldType))
            {
                _memberFactory.NewStructMember(dataObjectFieldType, dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsGenericDictionary(dataObjectFieldType))
            {
                var member = _memberFactory.NewDictionaryMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessDictionary });
            }
            else if (_typeInformation.IsArray(dataObjectFieldType))
            {
                var member = _memberFactory.NewArrayMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessArray });
            }
            else if (_typeInformation.IsGenericList(dataObjectFieldType))
            {
                var member = _memberFactory.NewArrayMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessList });
            }
            else if (_typeInformation.IsStruct(dataObjectFieldType))
            {
                var member = _memberFactory.NewClassMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessClass });
            }
            else if (_typeInformation.IsClass(dataObjectFieldType))
            {
                var member = _memberFactory.NewClassMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessClass });
            }
            else if (_typeInformation.IsString(dataObjectFieldType))
            {
                _memberFactory.NewStringMember(dataObjectFieldValue as string, memberName);
            }
            else if (_typeInformation.IsBool(dataObjectFieldType))
            {
                _memberFactory.NewBoolMember(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt8(dataObjectFieldType))
            {
                _memberFactory.NewInt8Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt8(dataObjectFieldType))
            {
                _memberFactory.NewUInt8Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt16(dataObjectFieldType))
            {
                _memberFactory.NewInt16Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt16(dataObjectFieldType))
            {
                _memberFactory.NewUInt16Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt32(dataObjectFieldType))
            {
                _memberFactory.NewInt32Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt32(dataObjectFieldType))
            {
                _memberFactory.NewUInt32Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsInt64(dataObjectFieldType))
            {
                _memberFactory.NewInt64Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsUInt64(dataObjectFieldType))
            {
                _memberFactory.NewUInt64Member(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsFloat(dataObjectFieldType))
            {
                _memberFactory.NewFloatMember(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsDouble(dataObjectFieldType))
            {
                _memberFactory.NewDoubleMember(dataObjectFieldValue, memberName);
            }
            else if (_typeInformation.IsEnum(dataObjectFieldType))
            {
                var member = _memberFactory.NewEnumMember(dataObjectFieldType, dataObjectFieldValue, memberName);
                _memberProcessQueue.Enqueue(new MemberProcessor { Index = member, Object = dataObjectFieldValue, Type = dataObjectFieldType, Process = ProcessEnum });
            }
        }

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

        public void Analyze(IRootDataUnit root)
        {
            // Ok, so every class has fields, and we can reflect on the type of the field.
            // When the field is a normal primitive we know what to do and when it is a
            // class it's also easy.
            // But when the class is derived from another class than it's a different story.
            //
            // Solution #1:
            //
            //    We treat all data, including those from the base classes, as if it was only
            //    the derived class. This will result in predictable serialization and generated
            //    code classes are not derived in any way, making it impossible to 'cast'.
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
            _memberProcessQueue.Enqueue(new MemberProcessor { Index = rootIndex, Object = root, Type = root.GetType(), Process = ProcessClass });
            while (_memberProcessQueue.Count > 0)
            {
                var m = _memberProcessQueue.Dequeue();
                m.Process(m);
            }
        }
    }
}
