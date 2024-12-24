using System.Reflection;
using TextStreamWriter = System.IO.StreamWriter;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        public class CppCodeWriter2
        {
            public MetaCode2 MetaCode { get; init; }

            private static string GetMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                return mt.NameOfType;
            }

            private static string GetEnumMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                return fet.NameOfType;
            }

            private static string GetStructMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var typeName = metaCode2.MemberTypeName[memberIndex];
                return typeName;
            }

            private static string GetClassMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var typeName = metaCode2.MemberTypeName[memberIndex];
                return $"{typeName} *";
            }

            private static string GetArrayMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                var elementTypeString = s_getMemberTypeString[fet.Index](msi, metaCode2, option); // recursive call
                var memberTypeString = mt.NameOfType;
                return $"{memberTypeString}<{elementTypeString}>";
            }

            private static string GetDictMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var count = metaCode2.MembersCount[memberIndex];

                var fk = metaCode2.MembersType[msi]; // first key element
                var fv = metaCode2.MembersType[msi + count]; // first value element
                var keyTypeString = s_getMemberTypeString[fk.Index](msi, metaCode2, option); // recursive call
                var valueTypeString = s_getMemberTypeString[fv.Index](msi + count, metaCode2, option); // recursive call
                var memberTypeString = mt.NameOfType;
                return $"{memberTypeString}<{keyTypeString},{valueTypeString}>";
            }

            private static string GetDataUnitMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var dataUnitTypeName = metaCode2.MemberTypeName[memberIndex];
                return $"datafile_t<{dataUnitTypeName}>";
            }

            [Flags]
            private enum EOption
            {
                None = 0,
                //InPlace = 1,
            }

            private delegate string GetMemberTypeStringDelegate(int memberIndex, MetaCode2 metaCode2, EOption option);

            private static readonly GetMemberTypeStringDelegate[] s_getMemberTypeString = new GetMemberTypeStringDelegate[MetaInfo.Count]
            {
                null,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetMemberTypeName,
                GetEnumMemberTypeName, // enum
                GetStructMemberTypeName, // struct
                GetClassMemberTypeName, // class
                GetArrayMemberTypeName, // array
                GetDictMemberTypeName, // dictionary
                GetDataUnitMemberTypeName, // data unit
            };

            private static string GetReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                return mt.NameOfType;
            }

            private static string GetEnumReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var enumInstance = metaCode2.MembersObject[memberIndex];
                return enumInstance.GetType().Name;
            }

            private static string GetStructReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var typeName = metaCode2.MemberTypeName[memberIndex];
                return typeName;
            }

            private static string GetClassReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var className = metaCode2.MemberTypeName[memberIndex];
                return $"{className}*";
            }

            private static string GetArrayReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                var elementTypeString = s_getMemberTypeString[fet.Index](msi, metaCode2, option); // recursive call
                return $"array_t<{elementTypeString}>";
            }

            private static string GetDictReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var msi = metaCode2.MembersStart[memberIndex];
                var count = metaCode2.MembersCount[memberIndex];
                var fkt = metaCode2.MembersType[msi]; // first key element
                var fvt = metaCode2.MembersType[msi + count]; // first value element
                var keyTypeString = s_getMemberTypeString[fkt.Index](msi, metaCode2, option); // recursive call
                var valueTypeString = s_getMemberTypeString[fvt.Index](msi + count, metaCode2, option); // recursive call
                return $"dict_t<{keyTypeString}, {valueTypeString}>";
            }

            private static string GetDataUnitReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var dataUnitTypeName = metaCode2.MemberTypeName[memberIndex];
                return $"dataunit_t<{dataUnitTypeName}>";
            }

            private delegate string GetReturnTypeStringDelegate(int memberIndex, MetaCode2 metaCode2, EOption option);

            private static readonly GetReturnTypeStringDelegate[] s_getReturnTypeString =
                new GetReturnTypeStringDelegate[(int)MetaInfo.Count]
                {
                    null,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName,
                    GetReturnTypeName, // f64
                    GetReturnTypeName, // string_t
                    GetEnumReturnTypeName,
                    GetStructReturnTypeName, // class
                    GetClassReturnTypeName, // class
                    GetArrayReturnTypeName,
                    GetDictReturnTypeName,
                    GetDataUnitReturnTypeName,
                };

            private delegate void WriteGetterDelegate(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option);

            private static readonly WriteGetterDelegate[] s_writeGetterDelegates = new WriteGetterDelegate[(int)MetaInfo.Count]
            {
                null,
                WritePrimitiveGetter,
                WriteBitsetGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WritePrimitiveGetter,
                WriteStringGetter,
                WriteEnumGetter,
                WriteStructGetter,
                WriteClassGetter,
                WriteArrayGetter,
                WriteDictionaryGetter,
                WriteDataUnitGetter
            };

            private delegate void WriteMemberDelegate(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option);

            private static readonly WriteMemberDelegate[] s_writeMemberDelegates = new WriteMemberDelegate[(int)MetaInfo.Count]
            {
                null,
                WritePrimitiveMember,
                WriteBitsetMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WritePrimitiveMember,
                WriteEnumMember,
                WriteStructMember,
                WriteClassMember,
                WriteArrayMember,
                WriteDictionaryMember,
                WriteDataUnitMember
            };

            private static void WriteBitsetGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var count = metaCode2.MembersCount[memberIndex];

                // Emit a get function for each boolean that the bitset represents
                for (var i = 0; i < count; ++i)
                {
                    var booleanMemberIndex = metaCode2.MemberSorted[msi + i];
                    var booleanMemberName = metaCode2.MembersName[booleanMemberIndex];
                    var booleanName = metaCode2.MemberStrings[booleanMemberName];
                    writer.WriteLine($"    inline bool get{booleanName}() const {{ return (m_{memberName} & (1 << {i})) != 0; }}");
                }
            }

            private static void WriteBitsetMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"    {mt.NameOfType} m_{memberName};");
            }

            private static void WritePrimitiveGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"    inline {mt.NameOfType} get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WritePrimitiveMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"    {mt.NameOfType} m_{memberName};");
            }

            private static void WriteStringGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"    inline string_t const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteEnumGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                //var enumInstance = metaCode2.MembersObject[memberIndex];
                //var enumTypeName = enumInstance.GetType().Name;
                var enumTypeName = metaCode2.MemberTypeName[memberIndex];
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"    inline enums::{enumTypeName} get{memberName}() const {{ return (enums::{enumTypeName})m_{memberName}; }}");
            }

            private static void WriteEnumMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                writer.WriteLine("    " + fet.NameOfType + " m_" + memberName + ";");
            }

            private static void WriteStructGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var className = metaCode2.MemberTypeName[memberIndex];

                writer.WriteLine($"    inline {className} const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteStructMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var className = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine($"    {className} m_{memberName};");
            }

            private static void WriteClassGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var className = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine($"    inline {className} const* get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteClassMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                //var memberObject = metaCode2.MembersObject[memberIndex];
                var className = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine("    " + className + " * m_" + memberName + ";");
            }

            private static void WriteArrayGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                var returnTypeString = s_getReturnTypeString[fet.Index](msi, metaCode2, option);
                writer.WriteLine($"    inline array_t<{returnTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteArrayMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var fet = metaCode2.MembersType[msi];
                var elementName = s_getMemberTypeString[fet.Index](msi, metaCode2, option);
                writer.WriteLine($"    array_t<{elementName}> m_{memberName};");
            }

            private static void WriteDictionaryGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var count = metaCode2.MembersCount[memberIndex];

                var memberName = metaCode2.MemberStrings[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode2.MembersType[msi];
                var valueElement = metaCode2.MembersType[msi + count];
                var keyTypeString = s_getReturnTypeString[keyElement.Index](msi, metaCode2, option);
                var valueTypeString = s_getReturnTypeString[valueElement.Index](msi + count, metaCode2, option);

                writer.WriteLine($"    inline dict_t<{keyTypeString}, {valueTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteDictionaryMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var count = metaCode2.MembersCount[memberIndex];

                var memberName = metaCode2.MemberStrings[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode2.MembersType[msi];
                var valueElement = metaCode2.MembersType[msi + count];
                var keyTypeString = s_getMemberTypeString[keyElement.Index](msi, metaCode2, option);
                var valueTypeString = s_getMemberTypeString[valueElement.Index](msi + count, metaCode2, option);

                writer.WriteLine($"    dict_t<{keyTypeString}, {valueTypeString}> m_{memberName};");
            }

            private static void WriteDataUnitGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var dataUnit = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine($"    inline dataunit_t<{dataUnitTypeName}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteDataUnitMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var dataUnit = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine("    dataunit_t<" + dataUnitTypeName + "> m_" + memberName + ";");
            }

            private static void WriteEnum(Type e, TextStreamWriter writer)
            {
                writer.WriteLine($"namespace enums");
                writer.WriteLine("{");
                writer.WriteLine($"    enum {e.Name}");
                writer.WriteLine("    {");
                foreach (var en in System.Enum.GetValues(e))
                {
                    var val = Convert.ChangeType(en, System.Enum.GetUnderlyingType(e));
                    writer.WriteLine($"        {System.Enum.GetName(e, en)} = {val},");
                }

                writer.WriteLine("    };");
                writer.WriteLine("}");
                writer.WriteLine();
            }

            public void WriteEnums(TextStreamWriter writer)
            {
                HashSet<string> writtenEnums = new();
                for (var i = 0; i < MetaCode.MembersType.Count; ++i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (!mt.IsEnum) continue;
                    var enumInstance = MetaCode.MembersObject[i];
                    if (writtenEnums.Contains(enumInstance.GetType().Name)) continue;
                    WriteEnum(enumInstance.GetType(), writer);
                    writtenEnums.Add(enumInstance.GetType().Name);
                }

                writer.WriteLine();
            }

            private void WriteStruct(int memberIndex, TextStreamWriter writer, EOption option)
            {
                var msi = MetaCode.MembersStart[memberIndex];
                var count = MetaCode.MembersCount[memberIndex];
                //var mt = MetaCode.MembersType[memberIndex];

                //var className = MetaCode.MembersObject[memberIndex].GetType().Name;
                var className = MetaCode.MemberTypeName[memberIndex];

                writer.WriteLine($"struct {className}");
                writer.WriteLine("{");

                // write public member getter functions
                for (var i = msi; i < msi + count; ++i)
                {
                    var si = MetaCode.MemberSorted[i];
                    var mmt = MetaCode.MembersType[si];
                    s_writeGetterDelegates[mmt.Index](si, writer, MetaCode, option);
                }

                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
                for (var i = msi; i < msi + count; ++i)
                {
                    var si = MetaCode.MemberSorted[i];
                    var mmt = MetaCode.MembersType[si];
                    s_writeMemberDelegates[mmt.Index](si, writer, MetaCode, EOption.None);
                }

                writer.WriteLine("};");
                writer.WriteLine();
            }

            public void WriteStructs(TextStreamWriter writer)
            {
                // Forward declares ?
                writer.WriteLine("// Forward declares");
                var writtenClasses = new HashSet<string>();
                for (var i = 0; i < MetaCode.MembersType.Count; ++i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (mt.IsClass || mt.IsDataUnit)
                    {
                        var ct = MetaCode.MemberTypeName[i];
                        if (writtenClasses.Contains(ct)) continue;
                        var className = MetaCode.MemberTypeName[i];
                        writer.WriteLine($"struct {className};");
                        writtenClasses.Add(ct);
                    }
                }
                writer.WriteLine();

                // Emit all the predefined code
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (TypeInfo2.HasGenericInterface(t, typeof(ICode)))
                        {
                            var code = (ICode)Activator.CreateInstance(t);
                            var lines = code.GetCode();
                            foreach (var line in lines)
                            {
                                writer.WriteLine(line);
                            }
                            writer.WriteLine();
                        }
                    }
                }

                // Write out any code for IStruct's that we are using
                var writtenIStructs = new List<Type>();
                for (var i = MetaCode.MembersType.Count - 1; i >= 0; --i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (!mt.IsStruct) continue;
                    var mo = MetaCode.MembersObject[i];
                    if (mo is not IStruct ios) continue;

                    if (writtenIStructs.Contains(ios.GetType())) continue;

                    var lines = ios.StructCode();
                    foreach (var line in lines)
                    {
                        writer.WriteLine(line);
                    }
                    writer.WriteLine();

                    writtenIStructs.Add(ios.GetType());
                }

                // Write out all the structs that are used in the game data
                writtenClasses.Clear();
                for (var i = MetaCode.MembersType.Count - 1; i >= 0; --i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (!mt.IsClass && !mt.IsDataUnit) continue;
                    var ct = MetaCode.MemberTypeName[i];
                    if (writtenClasses.Contains(ct)) continue;
                    WriteStruct(i, writer, EOption.None);
                    writtenClasses.Add(ct);
                }

                writer.WriteLine();
            }
        }
    }
}
