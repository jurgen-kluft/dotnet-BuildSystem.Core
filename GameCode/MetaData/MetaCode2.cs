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

        public readonly struct MetaInfo
        {
            public static readonly MetaInfo s_unknown = new() { Index = 0 };
            public static readonly MetaInfo s_bool = new() { Index = 1 };
            public static readonly MetaInfo s_bitset = new() { Index = 2 };
            public static readonly MetaInfo s_int8 = new() { Index = 3 };
            public static readonly MetaInfo s_uint8 = new() { Index = 4 };
            public static readonly MetaInfo s_int16 = new() { Index = 5 };
            public static readonly MetaInfo s_uint16 = new() { Index = 6 };
            public static readonly MetaInfo s_int32 = new() { Index = 7 };
            public static readonly MetaInfo s_uint32 = new() { Index = 8 };
            public static readonly MetaInfo s_int64 = new() { Index = 9 };
            public static readonly MetaInfo s_uint64 = new() { Index = 10 };
            public static readonly MetaInfo s_float = new() { Index = 11 };
            public static readonly MetaInfo s_double = new() { Index = 12 };
            public static readonly MetaInfo s_string = new() { Index = 13 };
            public static readonly MetaInfo s_enum = new() { Index = 14 };
            public static readonly MetaInfo s_struct = new() { Index = 15 };
            public static readonly MetaInfo s_class = new() { Index = 16 };
            public static readonly MetaInfo s_array = new() { Index = 17 };
            public static readonly MetaInfo s_dictionary = new() { Index = 18 };
            public static readonly MetaInfo s_dataUnit = new() { Index = 19 };

            public const int Count = 20;

            public bool IsBool => Index == 1;
            public bool IsEnum => Index == 14;
            public bool IsStruct => Index == 15;
            public bool IsClass => Index == 16;
            public bool IsArray => Index == 17;
            public bool IsDictionary => Index == 18;
            public bool IsDataUnit => Index == 19;

            private readonly struct Details
            {
                internal byte SizeInBits { get; init; }
                internal byte AlignmentInBytes { get; init; }
                internal bool IsSignedType { get; init; }
                internal string TypeName { get; init; }
            };

            private static readonly Details[] s_details = new Details[Count]
            {
                new() { SizeInBits = 0, AlignmentInBytes = 1, IsSignedType = false, TypeName = "unknown" }, // Unknown
                new() { SizeInBits = 1, AlignmentInBytes = 1, IsSignedType = false, TypeName = "bool" }, // Bool
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = false, TypeName = "u8" }, // BitSet
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = true, TypeName = "s8" }, // Int8
                new() { SizeInBits = 8, AlignmentInBytes = 1, IsSignedType = false, TypeName = "u8" }, // UInt8
                new() { SizeInBits = 16, AlignmentInBytes = 2, IsSignedType = true, TypeName = "s16" }, // Int16
                new() { SizeInBits = 16, AlignmentInBytes = 2, IsSignedType = false, TypeName = "u16" }, // UInt16
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = true, TypeName = "s32" }, // Int32
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = false, TypeName = "u32" }, // UInt32
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = true, TypeName = "s64" }, // Int64
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = false, TypeName = "u64" }, // UInt64
                new() { SizeInBits = 32, AlignmentInBytes = 4, IsSignedType = true, TypeName = "f32" }, // Float
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = true, TypeName = "f64" }, // Double
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "string_t" }, // String
                new() { SizeInBits = 0, AlignmentInBytes = 0, IsSignedType = false, TypeName = "enum_t" }, // Enum
                new() { SizeInBits = 0, AlignmentInBytes = 0, IsSignedType = false, TypeName = "struct" }, // Struct
                new() { SizeInBits = 64, AlignmentInBytes = 8, IsSignedType = false, TypeName = "class" }, // Class
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "array_t" }, // Array
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "dict_t" }, // Dictionary
                new() { SizeInBits = 64 + 32 + 32, AlignmentInBytes = 16, IsSignedType = false, TypeName = "data_unit_t" }, // DataUnit
            };

            public byte Index { get; private init; }

            public int SizeInBits => s_details[Index].SizeInBits;
            public int SizeInBytes => (s_details[Index].SizeInBits + 7) >> 3;
            public bool IsSigned => s_details[Index].IsSignedType;
            public int Alignment => s_details[Index].AlignmentInBytes;
            public string NameOfType => s_details[Index].TypeName;
        }

        // MetaCode is a thought experiment on how to optimize the building of the classes and members that we encounter
        // when we go through the C# code using reflection. Currently, we are building this purely using custom C# code,
        // and we could do this a lot more efficient.

        public class MetaCode2
        {
            public readonly List<string> MemberStrings;
            public readonly List<int> MemberSorted; // Sorted member index map
            public readonly List<MetaInfo> MembersType; // Type of the member (int, float, string, class, struct, array, dictionary)
            public readonly List<int> MembersName;
            public readonly List<int> MembersStart; // If we are a Struct the members start here
            public readonly List<int> MembersCount; // If we are an Array/List/Dict/Struct we hold many elements/members
            public readonly List<string> MemberTypeName; // This is to know the type of the class
            public readonly List<object> MembersObject; // This is to know content

            public MetaCode2(int estimatedCount)
            {
                MemberStrings = new List<string>(estimatedCount);
                MemberSorted = new List<int>(estimatedCount);
                MembersType = new List<MetaInfo>(estimatedCount);
                MembersName = new List<int>(estimatedCount);
                MembersStart = new List<int>(estimatedCount);
                MembersCount = new List<int>(estimatedCount);
                MemberTypeName = new List<string>(estimatedCount);
                MembersObject = new List<object>(estimatedCount);
            }

            public int Count => MembersType.Count;

            public int AddMember(MetaInfo info, int name, int startIndex, int count, object o, string typeName)
            {
                var index = Count;
                MemberSorted.Add(index);
                MembersType.Add(info);
                MembersName.Add(name);
                MembersStart.Add(startIndex);
                MembersCount.Add(count);
                MemberTypeName.Add(typeName);
                MembersObject.Add(o);
                return index;
            }

            private void SetMember(int memberIndex, MetaInfo info, int name, int startIndex, int count, object o, string typeName)
            {
                MembersType[memberIndex] = info;
                MembersName[memberIndex] = name;
                MembersStart[memberIndex] = startIndex;
                MembersCount[memberIndex] = count;
                MemberTypeName[memberIndex] = typeName;
                MembersObject[memberIndex] = o;
            }

            private void DuplicateMember(int memberIndex)
            {
                var type = MembersType[memberIndex];
                var name = MembersName[memberIndex];
                var start = MembersStart[memberIndex];
                var count = MembersCount[memberIndex];
                var typeName = MemberTypeName[memberIndex];
                var obj = MembersObject[memberIndex];
                AddMember(type, name, start, count, obj, typeName);
            }

            public void UpdateStartIndexAndCount(int memberIndex, int startIndex, int count)
            {
                MembersStart[memberIndex] = startIndex;
                MembersCount[memberIndex] = count;
            }

            private int GetMemberAlignment(int memberIndex)
            {
                var mt = MembersType[memberIndex];
                var alignment = mt.Alignment;
                if (alignment > 0)
                    return alignment;

                if (MembersObject[memberIndex] is IStruct ios)
                {
                    alignment = ios.StructAlign;
                }
                else if (mt.IsEnum)
                {
                    // An enum is a special case, it can be any of the primitive types
                    // u8, s8, u16, s16, u32, s32, u64, s64.
                    var msi = MembersStart[memberIndex];
                    var fet = MembersType[msi];
                    alignment = fet.Alignment;
                }

                return alignment;
            }

            public int GetDataAlignment(int memberIndex)
            {
                var mt = MembersType[memberIndex];

                // Structs have unknown alignment, we need to get it by using IStruct
                var alignment = MembersType[memberIndex].Alignment;
                if (mt.IsStruct)
                {
                    if (MembersObject[memberIndex] is IStruct ios)
                    {
                        alignment = ios.StructAlign;
                    }
                }
                else if (mt.IsEnum)
                {
                    // An enum is a special case, it can be any of the primitive types
                    // u8, s8, u16, s16, u32, s32, u64, s64.
                    var msi = MembersStart[memberIndex];
                    var fet = MembersType[msi];
                    alignment = fet.Alignment;
                }
                else if (mt.IsClass || mt.IsArray || mt.IsDictionary)
                {
                    // Get the alignment from the first member of the object
                    var mi = MembersStart[memberIndex];
                    alignment = GetMemberAlignment(mi);
                }

                return alignment;
            }

            public class SortMembersPredicate : IComparer<int>
            {
                private readonly MetaCode2 _metaCode2;

                public SortMembersPredicate(MetaCode2 metaCode2)
                {
                    _metaCode2 = metaCode2;
                }

                public int Compare(int x, int y)
                {
                    var xt = _metaCode2.MembersType[x];
                    var xs = xt.SizeInBits;
                    if (xt.IsStruct)
                    {
                        if (_metaCode2.MembersObject[x] is IStruct xi)
                        {
                            xs = xi.StructSize * 8;
                        }
                        else
                        {
                            xs = sizeof(ulong) * 8;
                        }
                    }
                    if (xt.IsEnum)
                    {
                        var msi = _metaCode2.MembersStart[x];
                        var fet = _metaCode2.MembersType[msi];
                        xs = fet.SizeInBits;
                    }

                    var yt = _metaCode2.MembersType[y];
                    var ys = yt.SizeInBits;
                    if (yt.IsStruct)
                    {
                        // figure out if it is an IStruct since IStruct has defined its own size
                        if (_metaCode2.MembersObject[y] is IStruct yi)
                        {
                            ys = yi.StructSize * 8;
                        }
                        else
                        {
                            ys = sizeof(ulong) * 8;
                        }
                    }
                    else if (yt.IsEnum)
                    {
                        var msi = _metaCode2.MembersStart[y];
                        var fet = _metaCode2.MembersType[msi];
                        ys = fet.SizeInBits;
                    }

                    var c = xs == ys ? 0 : xs > ys ? -1 : 1;

                    // sort by size
                    if (c != 0) return c;
                    // sizes are the same -> sort by type
                    c = xt.Index == yt.Index ? 0 : xt.Index < yt.Index ? -1 : 1;
                    if (c != 0) return c;
                    // size and type are the same -> sort by member name
                    var xn = _metaCode2.MembersName[x];
                    var yn = _metaCode2.MembersName[y];
                    return xn == yn ? 0 : xn < yn ? -1 : 1;
                }
            }

            public void CombineBooleans(int classIndex)
            {
                // Swap all boolean members to the end of the member list
                var mi = MembersStart[classIndex];
                if (mi == -1) return;

                var end = mi + MembersCount[classIndex];
                {
                    // Swap boolean members to the end
                    // Note: After sorting all members, these should end-up at the end, so why are we not
                    //       sorting the members first?
                    var me = end - 1;
                    while (me >= mi) // find the first boolean member from the end
                    {
                        var sme = MemberSorted[me];
                        var cmt = MembersType[sme];
                        if (!cmt.IsBool)
                            break;
                        --me;
                    }

                    end = me + 1;
                    while (me >= mi) // any other interleaved boolean members should be moved to the end
                    {
                        var sme = MemberSorted[me];
                        var cmt = MembersType[sme];
                        if (cmt.IsBool)
                        {
                            end -= 1;
                            MemberSorted.Swap(sme, end);
                        }
                        else
                        {
                            --me;
                        }
                    }
                }

                // Did we find any boolean members?, if not, we are done
                if (end == (mi + MembersCount[classIndex]))
                    return;

                // We can combine 8 booleans into a byte
                const int numBooleansPerBitSet = sizeof(byte) * 8;
                var numberOfBooleans = MembersCount[classIndex] - (end - MembersStart[classIndex]);
                var numberOfBitSets = (numberOfBooleans + numBooleansPerBitSet - 1) / numBooleansPerBitSet;
                var startOfCurrentBooleans = end;
                var startOfDuplicateBooleans = MembersCount.Count;
                var booleansValues = new bool[numberOfBooleans];
                for (int i = 0; i < numberOfBooleans; ++i)
                {
                    var mis = MemberSorted[startOfCurrentBooleans + i];
                    DuplicateMember(mis);
                    booleansValues[i] = (bool)MembersObject[mis];
                }

                var perBitSetValue = new byte[numberOfBitSets];
                var perBitSetNumBools = new short[numberOfBitSets];
                for (int i = 0; i < numberOfBitSets; ++i)
                {
                    var value = (byte)0;
                    perBitSetNumBools[i] = 0;
                    for (int j = 0; j < numBooleansPerBitSet; ++j)
                    {
                        var index = i * numBooleansPerBitSet + j;
                        if (index >= numberOfBooleans)
                            break;
                        value |= (byte)((booleansValues[index] ? 1 : 0) << j);
                        perBitSetNumBools[i] += 1;
                    }
                    perBitSetValue[i] = value;
                }

                // Setup (replace some of) the boolean members with bitset members
                var startOfBitSets = end;
                for (int i = 0; i < numberOfBitSets; ++i)
                {
                    var mis = MemberSorted[startOfBitSets + i];
                    var mni = MemberStrings.Count;
                    MemberStrings.Add($"Booleans{i}");
                    SetMember(mis, MetaInfo.s_bitset, mni, startOfDuplicateBooleans + (i * numBooleansPerBitSet), perBitSetNumBools[i], perBitSetValue[i], "bitset_t");
                }

                // Update the member count of this class, remove the number of booleans and add the number of bitsets
                MembersCount[classIndex] = MembersCount[classIndex] - numberOfBooleans + numberOfBitSets;
            }

            public void SortMembers(int classIndex, IComparer<int> comparer)
            {
                // Sort members by size/alignment, descending
                // This sort needs to be stable to ensure that other identical classes are sorted in the same way
                var si = MembersStart[classIndex];
                var count = MembersCount[classIndex];
                MemberSorted.Sort(si, count, comparer);
            }
        }

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
                return $"datafile_t<{dataUnitTypeName}>";
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
                writer.WriteLine($"    inline datafile_t<{dataUnitTypeName}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteDataUnitMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var dataUnit = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = metaCode2.MemberTypeName[memberIndex];
                writer.WriteLine("    datafile_t<" + dataUnitTypeName + "> m_" + memberName + ";");
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

        public class MetaMemberFactory : IMemberFactory2
        {
            public MetaMemberFactory(MetaCode2 metaCode2Code2)
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
