using System.Reflection;
using TextStreamWriter = System.IO.StreamWriter;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        public interface IMemberFactory2
        {
            void NewBoolMember(object content, string memberName);
            void NewInt8Member(object content, string memberName);
            void NewUInt8Member(object content, string memberName);
            void NewInt16Member(object content, string memberName);
            void NewUInt16Member(object content, string memberName);
            void NewInt32Member(object content, string memberName);
            void NewUInt32Member(object content, string memberName);
            void NewInt64Member(object content, string memberName);
            void NewUInt64Member(object content, string memberName);
            void NewFloatMember(object content, string memberName);
            void NewDoubleMember(object content, string memberName);
            void NewStringMember(object content, string memberName);
            int NewEnumMember(Type type, object content, string memberName);
            int NewArrayMember(Type type, object content, string memberName);
            int NewDictionaryMember(Type type, object content, string memberName);
            int NewClassMember(Type type, object content, string memberName);
            void NewStructMember(Type type, object content, string memberName);
            int NewDataUnitMember(Type type, object content, string memberName);
        }

        public class MetaMemberFactory2 : IMemberFactory2
        {
            public MetaMemberFactory2(MetaCode2 metaCode2Code2)
            {
                _metaCode2 = metaCode2Code2;
            }

            private readonly MetaCode2 _metaCode2;
            private readonly Dictionary<string, int> _codeStringMap = new();

            private int RegisterCodeString(string memberName)
            {
                if (_codeStringMap.TryGetValue(memberName, out var index)) return index;
                index = _metaCode2.MemberStrings.Count;
                _codeStringMap.Add(memberName, index);
                _metaCode2.MemberStrings.Add(memberName);
                return index;
            }

            public void NewBoolMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_bool, RegisterCodeString(memberName), -1, 1, content, "bool");
            }

            public void NewInt8Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_int8, RegisterCodeString(memberName), -1, 1, content, "s8");
            }

            public void NewUInt8Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_uint8, RegisterCodeString(memberName), -1, 1, content, "u8");
            }

            public void NewInt16Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_int16, RegisterCodeString(memberName), -1, 1, content, "s16");
            }

            public void NewUInt16Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_uint16, RegisterCodeString(memberName), -1, 1, content, "u16");
            }

            public void NewInt32Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_int32, RegisterCodeString(memberName), -1, 1, content, "s32");
            }

            public void NewUInt32Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_uint32, RegisterCodeString(memberName), -1, 1, content, "u32");
            }

            public void NewInt64Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_int64, RegisterCodeString(memberName), -1, 1, content, "s64");
            }

            public void NewUInt64Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_uint64, RegisterCodeString(memberName), -1, 1, content, "u64");
            }

            public void NewFloatMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_float, RegisterCodeString(memberName), -1, 1, content, "f32");
            }

            public void NewDoubleMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_double, RegisterCodeString(memberName), -1, 1, content, "f64");
            }

            public void NewStringMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.s_string, RegisterCodeString(memberName), -1, 1, content, "string_t");
            }

            public int NewEnumMember(Type type, object content, string memberName)
            {
                if (content is not System.Enum e)
                    return -1;
                return _metaCode2.AddMember(MetaInfo.s_enum, RegisterCodeString(memberName), -1, 0, e, e.GetType().Name);
            }

            public int NewArrayMember(Type type, object content, string memberName)
            {
                return _metaCode2.AddMember(MetaInfo.s_array, RegisterCodeString(memberName), -1, 0, content, "array_t");
            }

            public int NewDictionaryMember(Type type, object content, string memberName)
            {
                return _metaCode2.AddMember(MetaInfo.s_dictionary, RegisterCodeString(memberName), -1, 0, content, "dict_t");
            }

            public void NewStructMember(Type type, object content, string memberName)
            {
                // An IStruct is either a value type or a reference type, the C++ code is already 'known' in the codebase.
                // The data of the 'members' of this struct are written by using the IStruct interface, so we do not
                // need to 'parse' the members of this struct, we will just write the data as is.
                if (content is not IStruct o)
                    return;

                var metaType = MetaInfo.s_struct;
                _metaCode2.AddMember(metaType, RegisterCodeString(memberName), -1, 1, content, o.StructMember);
            }

            public int NewClassMember(Type type, object content, string memberName)
            {
                // Should we check for Name override attribute here?
                var className = type.Name;
                if (type.GetCustomAttribute<NameAttribute>() is { } nameAttribute)
                {
                    className = nameAttribute.Name;
                }

                // If content == null, change the type to 'void'
                className = className.ToLower() + "_t";

                return _metaCode2.AddMember(MetaInfo.s_class, RegisterCodeString(memberName), -1, 0, content, className);
            }

            public int NewDataUnitMember(Type type, object content, string memberName)
            {
                // Should we check for Name override attribute here?
                var className = type.Name;
                if (type.GetCustomAttribute<NameAttribute>() is { } nameAttribute)
                {
                    className = nameAttribute.Name;
                }

                className = className.ToLower() + "_t";
                return _metaCode2.AddMember(MetaInfo.s_dataUnit, RegisterCodeString(memberName), -1, 0, content, className);
            }
        }
    }
}
