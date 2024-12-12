using System.Diagnostics;
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
            void NewEnumMember(Type type, object content, string memberName);
            int NewArrayMember(Type type, object content, string memberName);
            int NewDictionaryMember(Type type, object content, string memberName);
            int NewClassMember(Type type, object content, string memberName);
            void NewStructMember(Type type, object content, string memberName);
            void NewDataUnitMember(Type type, object content, string memberName);
        }

        public struct MetaInfo
        {
            public static MetaInfo AsUnknown => new() { Index = 0 };
            public static MetaInfo AsBool => new() { Index = 1 };
            public static MetaInfo AsBitSet => new() { Index = 2 };
            public static MetaInfo AsInt8 => new() { Index = 3 };
            public static MetaInfo AsUInt8 => new() { Index = 4 };
            public static MetaInfo AsInt16 => new() { Index = 5 };
            public static MetaInfo AsUInt16 => new() { Index = 6 };
            public static MetaInfo AsInt32 => new() { Index = 7 };
            public static MetaInfo AsUInt32 => new() { Index = 8 };
            public static MetaInfo AsInt64 => new() { Index = 9 };
            public static MetaInfo AsUInt64 => new() { Index = 10 };
            public static MetaInfo AsFloat => new() { Index = 11 };
            public static MetaInfo AsDouble => new() { Index = 12 };
            public static MetaInfo AsString => new() { Index = 13 };
            public static MetaInfo AsEnum => new() { Index = 14 };
            public static MetaInfo AsStruct => new() { Index = 15 };
            public static MetaInfo AsClass => new() { Index = 16 };
            public static MetaInfo AsArray => new() { Index = 17 };
            public static MetaInfo AsDictionary => new() { Index = 18 };
            public static MetaInfo AsDataUnit => new() { Index = 19 };

            public const int Count = 20;

            public bool IsBool => Index == 1;
            public bool IsEnum => Index == 14;
            public bool IsStruct => Index == 15;
            public bool IsClass => Index == 16;
            public bool IsArray => Index == 17;
            public bool IsDictionary => Index == 18;
            public bool IsDataUnit => Index == 19;

            private static readonly int[] sSizeInBits = new int[Count]
            {
                0, // Unknown
                1, // Bool
                sizeof(byte) * 8, // BitSet
                sizeof(sbyte) * 8, // Int8
                sizeof(byte) * 8, // UInt8
                sizeof(ushort) * 8, // Int16
                sizeof(short) * 8, // UInt16
                sizeof(int) * 8, // Int32
                sizeof(uint) * 8, // UInt32
                sizeof(long) * 8, // Int64
                sizeof(ulong) * 8, // UInt64
                sizeof(float) * 8, // Float
                sizeof(double) * 8, // Double
                64 + 64, // String (length, pointer)
                32, // Enum
                0, // Struct (unknown)
                64, // Class (pointer)
                64 + 64, // Array (length, pointer)
                64 + 64, // Dictionary (length, pointer)
                64 + 32 + 32, // DataUnit (T*, offset, size)
            };

            private static readonly sbyte[] sAlignment = new sbyte[Count]
            {
                1, // Unknown
                1, // Bool
                sizeof(byte), // BitSet
                sizeof(sbyte), // Int8
                sizeof(byte), // UInt8
                sizeof(ushort), // Int16
                sizeof(short), // UInt16
                sizeof(int), // Int32
                sizeof(uint), // UInt32
                sizeof(long), // Int64
                sizeof(ulong), // UInt64
                sizeof(float), // Float
                sizeof(double), // Double
                16, // String (pointer, byte length, rune length)
                4, // Enum
                0, // Struct (depends on the struct type)
                8, // Class (pointer)
                16, // Array (pointer, byte length, item count)
                16, // Dictionary (pointer, byte length, item count)
                16, // DataUnit (pointer, offset, size)
            };

            private static readonly string[] sTypeNames = new string[Count]
            {
                "unknown", // Unknown
                "bool", // Bool
                "u8", // BitSet
                "s8", // Int8
                "u8", // UInt8
                "s16", // Int16
                "u16", // UInt16
                "s32", // Int32
                "u32", // UInt32
                "s64", // Int64
                "u64", // UInt64
                "f32", // Float
                "f64", // Double
                "string_t", // String (pointer, byte length, rune length)
                "enum_t", // Enum
                "struct", // Struct (unknown)
                "class", // Class (offset)
                "array_t", // Array (pointer, byte length, item count)
                "dict_t", // Dictionary (pointer, byte length, item count)
                "data_unit_t" // DataUnit (pointer, offset, size)
            };

            private uint Value { get; set; }

            public byte Index
            {
                get => (byte)(Value & 0xFF);
                private init => Value = (Value & 0xFFFFFF00) | value;
            }

            public int SizeInBits => sSizeInBits[Index];
            public int SizeInBytes => (SizeInBits + 7) >> 3;
            public int Alignment => sAlignment[Index];
            public string NameOfType => sTypeNames[Index];

            private const uint FlagInPlace = 0x200000;

            public bool InPlace
            {
                get => (Value & FlagInPlace) != 0;
                set => Value = ((Value & ~FlagInPlace) | (value ? FlagInPlace : 0));
            }
        }

        // MetaCode is a thought experiment on how to optimize the building of the classes and members that we encounter
        // when we go through the C# code using reflection. Currently, we are building this purely using custom C# code,
        // and we could do this a lot more efficient.

        public class MetaCode2
        {
            public readonly StringTable DataStrings;
            public readonly List<int> SortedMembersMap; // Sorted member index map
            public readonly List<string> MemberStrings;
            public readonly List<MetaInfo> MembersType;
            public readonly List<int> MembersName;
            public readonly List<int> MembersStart; // If we are a Struct the members start here
            public readonly List<int> MembersCount; // If we are an Array/List/Dict/Struct we hold many elements/members
            public readonly List<object> MembersObject; // This is to know the type of the class

            public MetaCode2(StringTable dataStrings, int estimatedCount)
            {
                DataStrings = dataStrings;
                SortedMembersMap = new List<int>(estimatedCount);
                MemberStrings = new List<string>(estimatedCount);
                MembersType = new List<MetaInfo>(estimatedCount);
                MembersName = new List<int>(estimatedCount);
                MembersStart = new List<int>(estimatedCount);
                MembersCount = new List<int>(estimatedCount);
                MembersObject = new List<object>(estimatedCount);
            }

            public int Count => MembersType.Count;

            public int AddMember(MetaInfo info, int name, int startIndex, int count, object o)
            {
                var index = Count;
                SortedMembersMap.Add(index);
                MembersType.Add(info);
                MembersName.Add(name);
                MembersStart.Add(startIndex);
                MembersCount.Add(count);
                MembersObject.Add(o);
                return index;
            }

            private void SetMember(int memberIndex, MetaInfo info, int name, int startIndex, int count, object o)
            {
                MembersType[memberIndex] = info;
                MembersName[memberIndex] = name;
                MembersStart[memberIndex] = startIndex;
                MembersCount[memberIndex] = count;
                MembersObject[memberIndex] = o;
            }

            private void DuplicateMember(int memberIndex)
            {
                var type = MembersType[memberIndex];
                var name = MembersName[memberIndex];
                var startIndex = MembersStart[memberIndex];
                var count = MembersCount[memberIndex];
                var obj = MembersObject[memberIndex];
                AddMember(type, name, startIndex, count, obj);
            }

            public void UpdateStartIndexAndCount(int memberIndex, int startIndex, int count)
            {
                MembersStart[memberIndex] = startIndex;
                MembersCount[memberIndex] = count;
            }

            public int GetMemberAlignment(int memberIndex)
            {
                var mt = MembersType[memberIndex];
                var alignment = mt.Alignment;
                if (alignment > 0)
                    return alignment;

                if (MembersObject[memberIndex] is IStruct ios)
                {
                    alignment = ios.StructAlign;
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

                    var c = xs == ys ? 0 : xs < ys ? -1 : 1;

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
                while (mi < end)
                {
                    var cmt = MembersType[mi];
                    if (cmt.IsBool)
                    {
                        end -= 1;
                        SortedMembersMap.Swap(mi, end);
                    }
                    else
                    {
                        ++mi;
                    }
                }

                // We can combine booleans into a byte
                var s = sizeof(byte) * 8;
                var n = MembersCount[classIndex] - (end - MembersStart[classIndex]);
                var i = 0;
                while (n > 0)
                {
                    // Duplicate all the boolean members to make room for this byte member but also
                    // to have the bitset member be able to have its own range of MetaMembers.
                    var numBits = (n <= s) ? n : s;
                    var startIndex = MembersCount.Count;
                    mi = end + (i * s);
                    var mie = mi + numBits;
                    var value = (uint)0;
                    var bit = (uint)1;
                    while (mi < mie)
                    {
                        DuplicateMember(mi);

                        // Depending on the boolean value we set the bit in the bitset
                        value |= (bit * ((bool)MembersObject[mi] ? (uint)1 : (uint)0));

                        bit <<= 1;
                        ++mi;
                    }

                    var mni = MembersName[end + i];
                    SetMember(end + i, MetaInfo.AsBitSet, mni, startIndex, numBits, value);

                    i += 1;
                    n -= s;
                }

                // Update the member count of this class, remove the number of booleans and add the number of bitsets
                MembersCount[classIndex] = (end - MembersStart[classIndex]) + i;
            }

            public void SortMembers(int classIndex, IComparer<int> comparer)
            {
                // Sort members by size/alignment, descending
                // This sort needs to be stable to ensure that other identical classes are sorted in the same way
                var si = MembersStart[classIndex];
                var count = MembersCount[classIndex];
                SortedMembersMap.Sort(si, count, comparer);
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
                var mt = metaCode2.MembersType[memberIndex];
                var enumInstance = metaCode2.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                return $"{mt.NameOfType}<{enumTypeName},u32>";
            }

            private static string GetStructMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mi = metaCode2.MembersStart[memberIndex];
                var os = metaCode2.MembersObject[mi] as IStruct;
                return os.StructName;
            }

            private static string GetClassMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var className = metaCode2.MembersObject[memberIndex].GetType().Name;
                return $"{className}*";
            }

            private static string GetArrayMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[fet.Index](msi, metaCode2, option); // recursive call
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
                var keyTypeString = sGetMemberTypeString[fk.Index](msi, metaCode2, option); // recursive call
                var valueTypeString = sGetMemberTypeString[fv.Index](msi + count, metaCode2, option); // recursive call
                var memberTypeString = mt.NameOfType;
                return $"{memberTypeString}<{keyTypeString},{valueTypeString}>";
            }

            private static string GetDataUnitMemberTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var mt = metaCode2.MembersType[memberIndex];
                var dataUnitInstance = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = dataUnitInstance != null ? dataUnitInstance.GetType().Name : "?";
                return $"data_t<{dataUnitTypeName}>";
            }

            [Flags]
            private enum EOption
            {
                None = 0,
                InPlace = 1,
            }

            private delegate string GetMemberTypeStringDelegate(int memberIndex, MetaCode2 metaCode2, EOption option);

            private static readonly GetMemberTypeStringDelegate[] sGetMemberTypeString = new GetMemberTypeStringDelegate[(int)MetaInfo.Count]
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
                var mi = metaCode2.MembersStart[memberIndex];
                var ios = metaCode2.MembersObject[mi] as IStruct;
                return ios != null ? ios.StructName : "?";
            }

            private static string GetClassReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var className = metaCode2.MembersObject[memberIndex].GetType().Name;
                return $"{className} const *";
            }

            private static string GetArrayReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[fet.Index](msi, metaCode2, option); // recursive call
                return $"raw_array_t<{elementTypeString}>";
            }

            private static string GetDictReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var msi = metaCode2.MembersStart[memberIndex];
                var count = metaCode2.MembersCount[memberIndex];
                var fkt = metaCode2.MembersType[msi]; // first key element
                var fvt = metaCode2.MembersType[msi + count]; // first value element
                var keyTypeString = sGetMemberTypeString[fkt.Index](msi, metaCode2, option); // recursive call
                var valueTypeString = sGetMemberTypeString[fvt.Index](msi + count, metaCode2, option); // recursive call
                return $"raw_dict_t<{keyTypeString}, {valueTypeString}>";
            }

            private static string GetDataUnitReturnTypeName(int memberIndex, MetaCode2 metaCode2, EOption option)
            {
                var dataUnitTypeName = metaCode2.MembersObject[memberIndex];
                return $"data_t<{dataUnitTypeName.GetType().Name}>&";
            }

            private delegate string GetReturnTypeStringDelegate(int memberIndex, MetaCode2 metaCode2, EOption option);

            private static readonly GetReturnTypeStringDelegate[] sGetReturnTypeString =
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

            private static readonly WriteGetterDelegate[] sWriteGetterDelegates = new WriteGetterDelegate[(int)MetaInfo.Count]
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

            private static readonly WriteMemberDelegate[] sWriteMemberDelegates = new WriteMemberDelegate[(int)MetaInfo.Count]
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
                    var booleanMemberIndex = msi + i;
                    var booleanMemberName = metaCode2.MembersName[booleanMemberIndex];
                    var booleanName = metaCode2.MemberStrings[booleanMemberName];
                    writer.WriteLine($"\tinline bool get{booleanName}() const {{ return (m_{memberName} & (1 << {i})) != 0; }}");
                }
            }

            private static void WriteBitsetMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\t{mt.NameOfType} m_{memberName};");
            }

            private static void WritePrimitiveGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\tinline {mt.NameOfType} get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WritePrimitiveMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var mt = metaCode2.MembersType[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\t{mt.NameOfType} m_{memberName};");
            }

            private static void WriteStringGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\tinline string_t const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteEnumGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var enumInstance = metaCode2.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\tinline {enumTypeName} get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteEnumMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var enumInstance = metaCode2.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                writer.WriteLine($"\tenum_t<{enumTypeName}, u32> m_{memberName};");
            }

            private static void WriteStructGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];

                var ios = metaCode2.MembersObject[memberIndex] as IStruct;
                var className = ios.StructName;

                writer.WriteLine($"\tinline {className} const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteStructMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var ios = metaCode2.MembersObject[memberIndex] as IStruct;
                var className = ios.StructName;
                writer.WriteLine($"\t{className} m_{memberName};");
            }

            private static void WriteClassGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var className = metaCode2.MembersObject[memberIndex].GetType().Name;
                writer.WriteLine($"\tinline {className} const* get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteClassMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var className = metaCode2.MembersObject[memberIndex].GetType().Name;
                writer.WriteLine("\t" + className + "* m_" + memberName + ";");
            }

            private static void WriteArrayGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var msi = metaCode2.MembersStart[memberIndex];
                var fet = metaCode2.MembersType[msi];

                // figure out the element type of the array, could be a primitive, struct, class, enum or even another array
                var returnTypeString = sGetReturnTypeString[fet.Index](msi, metaCode2, option);

                writer.WriteLine($"\tinline array_t<{returnTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteArrayMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var msi = metaCode2.MembersStart[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var fet = metaCode2.MembersType[msi];
                var elementName = sGetMemberTypeString[fet.Index](msi, metaCode2, option);
                writer.WriteLine($"\tarray_t<{elementName}> m_{memberName};");
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
                var keyTypeString = sGetReturnTypeString[keyElement.Index](msi, metaCode2, option);
                var valueTypeString = sGetReturnTypeString[valueElement.Index](msi + count, metaCode2, option);

                writer.WriteLine($"\tinline dict_t<{keyTypeString}, {valueTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
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
                var keyTypeString = sGetMemberTypeString[keyElement.Index](msi, metaCode2, option);
                var valueTypeString = sGetMemberTypeString[valueElement.Index](msi + count, metaCode2, option);

                writer.WriteLine($"\tdict_t<{keyTypeString}, {valueTypeString}> m_{memberName};");
            }

            private static void WriteDataUnitGetter(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var dataUnit = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = dataUnit.GetType().Name;
                writer.WriteLine($"\tinline data_t<{dataUnitTypeName}>& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteDataUnitMember(int memberIndex, TextStreamWriter writer, MetaCode2 metaCode2, EOption option)
            {
                var mni = metaCode2.MembersName[memberIndex];
                var memberName = metaCode2.MemberStrings[mni];
                var dataUnit = metaCode2.MembersObject[memberIndex];
                var dataUnitTypeName = dataUnit.GetType().Name;
                writer.WriteLine("\t" + "data_t<" + dataUnitTypeName + "> m_" + memberName + ";");
            }

            private void WriteEnum(Type e, TextStreamWriter writer)
            {
                writer.WriteLine($"enum {e.Name}");
                writer.WriteLine("{");
                foreach (var en in Enum.GetValues(e))
                {
                    var val = System.Convert.ChangeType(en, System.Enum.GetUnderlyingType(e));
                    writer.WriteLine($"\t{Enum.GetName(e, en)} = {val},");
                }

                writer.WriteLine("};");
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

            private void WriteClass(int memberIndex, TextStreamWriter writer, EOption option)
            {
                var msi = MetaCode.MembersStart[memberIndex];
                var count = MetaCode.MembersCount[memberIndex];
                var mt = MetaCode.MembersType[memberIndex];

                var className = MetaCode.MembersObject[memberIndex].GetType().Name;
                // TODO See if this Type has attributes that override the class name

                writer.WriteLine($"class {className}");
                writer.WriteLine("{");
                writer.WriteLine("public:");

                // write public member getter functions
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = MetaCode.MembersType[i];
                    sWriteGetterDelegates[mmt.Index](i, writer, MetaCode, option);
                }

                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = MetaCode.MembersType[i];
                    sWriteMemberDelegates[mmt.Index](i, writer, MetaCode, EOption.None);
                }

                writer.WriteLine("};");
                writer.WriteLine();
            }

            public void WriteClasses(TextStreamWriter writer)
            {
                // Forward declares ?
                writer.WriteLine("// Forward declares");
                HashSet<string> writtenClasses = new();
                for (var i = 0; i < MetaCode.MembersType.Count; ++i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (!mt.IsClass) continue;
                    var cn = MetaCode.MembersObject[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    writer.WriteLine($"class {cn};");
                    writtenClasses.Add(cn);
                }

                writer.WriteLine();

                writtenClasses.Clear();
                for (var i = MetaCode.MembersType.Count - 1; i >= 0; --i)
                {
                    var mt = MetaCode.MembersType[i];
                    if (!mt.IsClass) continue;
                    var cn = MetaCode.MembersObject[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    WriteClass(i, writer, EOption.None);
                    writtenClasses.Add(cn);
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

            private int RegisterDataString(string str)
            {
                return _metaCode2.DataStrings.Add(str);
            }

            public void NewBoolMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsBool, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt8Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsInt8, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt8Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsUInt8, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt16Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsInt16, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt16Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsUInt16, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt32Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsInt32, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt32Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsUInt32, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt64Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsInt64, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt64Member(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsUInt64, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewFloatMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsFloat, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewDoubleMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsDouble, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewStringMember(object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsString, RegisterCodeString(memberName), RegisterDataString(content as string), 1, content);
            }

            public void NewEnumMember(Type type, object content, string memberName)
            {
                if (content is not Enum e)
                    return;

                _metaCode2.AddMember(MetaInfo.AsEnum, RegisterCodeString(memberName), -1, 1, e);
            }

            public int NewArrayMember(Type type, object content, string memberName)
            {
                return _metaCode2.AddMember(MetaInfo.AsArray, RegisterCodeString(memberName), -1, 0, content);
            }

            public int NewDictionaryMember(Type type, object content, string memberName)
            {
                return _metaCode2.AddMember(MetaInfo.AsDictionary, RegisterCodeString(memberName), -1, 0, content);
            }

            public void NewStructMember(Type type, object content, string memberName)
            {
                // An IStruct is either a value type or a reference type, the C++ code is already 'known' in the codebase.
                // Also the data of the 'members' of this struct are written by using the IStruct interface, so we do not
                // need to 'parse' the members of this struct, we will just write the data as is.
                if (content is not IStruct o)
                    return;

                var metaType = MetaInfo.AsStruct;
                _metaCode2.AddMember(metaType, RegisterCodeString(memberName), -1, 1, content);
            }

            public int NewClassMember(Type type, object content, string memberName)
            {
                return _metaCode2.AddMember(MetaInfo.AsClass, RegisterCodeString(memberName), -1, 0, content);
            }

            public void NewDataUnitMember(Type type, object content, string memberName)
            {
                _metaCode2.AddMember(MetaInfo.AsDataUnit, RegisterCodeString(memberName), -1, 0, content);
            }
        }
    }
}
