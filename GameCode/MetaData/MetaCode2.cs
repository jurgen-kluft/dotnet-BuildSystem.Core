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

            public const int Count = 19;

            public bool IsBool => Index == 1;
            public bool IsEnum => Index == 14;
            public bool IsStruct => Index == 15;
            public bool IsClass => Index == 16;
            public bool IsArray => Index == 17;
            public bool IsDictionary => Index == 18;


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
                8, // String (pointer, length)
                4, // Enum
                0, // Struct (unknown)
                8, // Class (offset)
                8, // Array (offset, length)
                8, // Dictionary (offset, length)
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
                "string_t", // String (pointer, length)
                "enum_t", // Enum
                "struct", // Struct (unknown)
                "class", // Class (offset)
                "array_t", // Array (offset, length)
                "dict_t" // Dictionary (offset, length)
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
        // when we go through the C# code using reflection. Currently we are building this purely using custom C# code
        // and we could do this a lot more efficient.

        public class MetaCode2
        {
            public readonly StringTable DataStrings;
            public readonly List<string> MemberStrings;
            public readonly List<MetaInfo> MembersType;
            public readonly List<int> MembersName;
            public readonly List<int> MembersStart; // If we are a Struct the members start here
            public readonly List<int> MembersCount; // If we are an Array/List/Dict/Struct we hold many elements/members
            public readonly List<object> MembersObject; // This is to know the type of the class

            public MetaCode2(StringTable dataStrings, int estimatedCount)
            {
                DataStrings = dataStrings;
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

            private void SwapMembers(int i, int j)
            {
                MembersType.Swap(i, j);
                MembersName.Swap(i, j);
                MembersStart.Swap(i, j);
                MembersCount.Swap(i, j);
                MembersObject.Swap(i, j);
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
                    var yt = _metaCode2.MembersType[y];
                    var xs = xt.SizeInBits;
                    var ys = yt.SizeInBits;
                    if (xt.IsStruct)
                    {
                        xs = sizeof(ulong) * 8;
                        if (_metaCode2.MembersObject[x] is IStruct xi)
                        {
                            xs = xi.StructSize * 8;
                        }
                    }

                    if (yt.IsStruct)
                    {
                        // figure out if it is an IStruct since IStruct has defined it's own size
                        ys = sizeof(ulong) * 8;
                        if (_metaCode2.MembersObject[y] is IStruct yi)
                        {
                            ys = yi.StructSize * 8;
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
                        SwapMembers(mi, end);
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
                // Manual Bubble Sort, sort members by size/alignment, descending
                // This sort needs to be stable to ensure that other identical classes are sorted in the same way
                var si = MembersStart[classIndex];
                var count = MembersCount[classIndex];
                var end = si + count;
                for (var i = si; i < end; ++i)
                {
                    var swapped = false;
                    for (var j = si; j < end - 1; ++j)
                    {
                        if (comparer.Compare(j, j + 1) > 0) continue;
                        SwapMembers(j, j + 1);
                        swapped = true;
                    }

                    if (!swapped) break;
                }
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
                return ios.StructName;
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
                WriteDictionaryGetter
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
                WriteDictionaryMember
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
        }

        public static class CppDataStreamWriter2
        {
            private delegate void WriteMemberDelegate(int memberIndex, WriteContext ctx);

            private delegate int CalcSizeOfTypeDelegate(int memberIndex, WriteContext ctx);

            private delegate void WriteProcessDelegate(int memberIndex, StreamReference r, WriteContext ctx);

            private struct WriteProcess
            {
                public int MemberIndex { get; init; }
                public WriteProcessDelegate Process { get; init; }
                public StreamReference Reference { get; init; }
            }

            private class WriteContext
            {
                public MetaCode2 MetaCode2 { get; init; }
                public StringTable StringTable { get; init; }
                public CppDataStream2 DataStream { get; init; }
                public CalcSizeOfTypeDelegate[] CalcSizeOfTypeDelegates { get; init; }
                public CalcSizeOfTypeDelegate[] CalcDataSizeOfTypeDelegates { get; init; }
                public WriteMemberDelegate[] WriteMemberDelegates { get; init; }
                public Queue<WriteProcess> WriteProcessQueue { get; init; }
            }

            public static void Write(MetaCode2 metaCode2, StringTable stringTable, CppDataStream2 dataStream)
            {
                var ctx = new WriteContext
                {
                    MetaCode2 = metaCode2,
                    StringTable = stringTable,
                    DataStream = dataStream,
                    WriteProcessQueue = new Queue<WriteProcess>(),

                    WriteMemberDelegates = new WriteMemberDelegate[(int)MetaInfo.Count]
                    {
                        null,
                        WriteBool,
                        WriteBitset,
                        WriteInt8,
                        WriteUInt8,
                        WriteInt16,
                        WriteUInt16,
                        WriteInt32,
                        WriteUInt32,
                        WriteInt64,
                        WriteUInt64,
                        WriteFloat,
                        WriteDouble,
                        WriteString,
                        WriteEnum,
                        WriteStruct,
                        WriteClass,
                        WriteArray,
                        WriteDictionary
                    },

                    CalcSizeOfTypeDelegates = new CalcSizeOfTypeDelegate[(int)MetaInfo.Count]
                    {
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfStruct,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType,
                        GetMemberSizeOfType
                    },

                    CalcDataSizeOfTypeDelegates = new CalcSizeOfTypeDelegate[(int)MetaInfo.Count]
                    {
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        GetDataSizeOfType,
                        CalcDataSizeOfString,
                        GetDataSizeOfType,
                        CalcDataSizeOfStruct,
                        CalcDataSizeOfClass,
                        CalcDataSizeOfArray,
                        CalcDataSizeOfDictionary
                    }
                };

                ctx.StringTable.Write(ctx.DataStream);

                var rootRef = StreamReference.NewReference;
                ctx.DataStream.NewBlock(rootRef, 8, 2 * 8);
                ctx.DataStream.OpenBlock(rootRef);
                {
                    ctx.DataStream.Write(stringTable.Reference); // String Table pointer
                    WriteClass(0, ctx); // Root Class pointer
                    ctx.DataStream.CloseBlock();

                    while (ctx.WriteProcessQueue.Count > 0)
                    {
                        var wp = ctx.WriteProcessQueue.Dequeue();
                        wp.Process(wp.MemberIndex, wp.Reference, ctx);
                    }
                }
            }

            private static void WriteBool(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.Write((bool)member ? (sbyte)1 : (sbyte)0);
            }

            private static void WriteBitset(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((uint)member);
            }

            private static void WriteInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.Write((sbyte)member);
            }

            private static void WriteUInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.Write((byte)member);
            }

            private static void WriteInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((short)member);
            }

            private static void WriteUInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((ushort)member);
            }

            private static void WriteInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((int)member);
            }

            private static void WriteUInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((uint)member);
            }

            private static void WriteInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((long)member);
            }

            private static void WriteUInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((ulong)member);
            }

            private static void WriteFloat(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((float)member);
            }

            private static void WriteDouble(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((double)member);
            }

            private static void WriteString(int memberIndex, WriteContext ctx)
            {
                var value = ctx.MetaCode2.MembersObject[memberIndex] as string;
                ctx.DataStream.Write(value);

                // Note: We do not need to schedule the string content to be written since all
                //       strings are part a string table.
            }

            private static void WriteEnum(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersObject[memberIndex];
                ctx.DataStream.AlignWrite((uint)member);
            }

            private static void WriteStruct(int memberIndex, WriteContext ctx)
            {
                var mx = ctx.MetaCode2.MembersObject[memberIndex] as IStruct;
                ctx.DataStream.Align(mx.StructAlign);
                mx.StructWrite(ctx.DataStream);
            }

            private static void WriteClassDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // A class is written as a collection of members
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        var et = ctx.MetaCode2.MembersType[mi];
                        ctx.WriteMemberDelegates[et.Index](mi, ctx);
                    }
                }
                ctx.DataStream.CloseBlock();
            }

            private static void WriteClass(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var mr = StreamReference.NewReference;
                ctx.DataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr);

                // We need to schedule the content of this class to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteClassDataProcess, Reference = mr });
            }


            private static void WriteArrayDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // An Array<T> is written as an array of elements
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    var et = ctx.MetaCode2.MembersType[msi];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[et.Index](mi, ctx);
                    }
                }
                ctx.DataStream.CloseBlock();
            }

            private static void WriteArray(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                ctx.DataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteArrayDataProcess, Reference = mr });
            }

            private static void WriteDictionaryDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // A Dictionary<key,value> is written as an array of keys followed by an array of values
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode2.MembersStart[memberIndex];
                    var count = ctx.MetaCode2.MembersCount[memberIndex];
                    var kt = ctx.MetaCode2.MembersType[msi];
                    var vt = ctx.MetaCode2.MembersType[msi + count];
                    // First write the array of keys
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[kt.Index](mi, ctx);
                    }

                    // NOTE, alignment ?

                    // Second, write the array of values
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[vt.Index](mi, ctx);
                    }
                }
                ctx.DataStream.CloseBlock();
            }

            private static void WriteDictionary(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode2.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode2.MembersCount[memberIndex];
                var mr = StreamReference.NewReference;
                ctx.DataStream.NewBlock(mr, ctx.MetaCode2.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteDictionaryDataProcess, Reference = mr });
            }

            private static int GetMemberSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode2.MembersType[memberIndex].SizeInBytes;
            }

            private static int GetMemberSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode2.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }


            private static int GetDataSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode2.MembersType[memberIndex].SizeInBytes;
            }

            private static int CalcDataSizeOfString(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode2.MembersStart[memberIndex];
                return ctx.StringTable.LengthOfByIndex(member);
            }

            private static int CalcDataSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode2.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }

            private static int CalcDataSizeOfClass(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                // Alignment of this class is determined by the first member
                var classAlign = ctx.MetaCode2.GetMemberAlignment(msi);

                var size = 0;
                for (var mi = msi; mi < msi + count; ++mi)
                {
                    // Obtain the size of this member
                    var ms = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[mi].Index](mi, ctx);

                    // Align the size based on the member type alignment
                    size = CMath.AlignUp32(size, ctx.MetaCode2.GetMemberAlignment(mi));

                    size += ms;
                }

                size = CMath.AlignUp32(size, classAlign);

                return size;
            }

            private static int CalcDataSizeOfArray(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                // Determine the alignment of the element and see if we need to align the size
                var elementAlign = ctx.MetaCode2.GetMemberAlignment(msi);
                var elementSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[msi].Index](msi, ctx);
                elementSize = CMath.AlignUp32(elementSize, elementAlign);

                var size = count * elementSize;
                return size;
            }

            private static int CalcDataSizeOfDictionary(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode2.MembersStart[memberIndex];
                var count = ctx.MetaCode2.MembersCount[memberIndex];

                var keyIndex = msi;
                var keyAlign = ctx.MetaCode2.GetMemberAlignment(keyIndex);
                var keySize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[keyIndex].Index](keyIndex, ctx);
                keySize = CMath.AlignUp32(keySize, keyAlign);

                var valueIndex = msi + count;
                var valueAlign = ctx.MetaCode2.GetMemberAlignment(valueIndex);
                var valueSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode2.MembersType[valueIndex].Index](valueIndex, ctx);
                valueSize = CMath.AlignUp32(valueSize, valueAlign);

                var size = CMath.AlignUp32(count * keySize, valueAlign) + count * valueSize;
                return size;
            }
        }

        public class CppDataStream2 : IDataWriter
        {
            private int mCurrent;
            private int mOffset;
            private readonly EPlatform mPlatform;
            private readonly List<DataBlock> mDataBlocks;
            private readonly Dictionary<StreamReference, int> mReferenceToBlock;
            private readonly Dictionary<StreamReference, long> mMarkers;
            private readonly StringTable mStringTable;
            private readonly MemoryStream mMemoryStream;
            private readonly IBinaryStreamWriter mDataWriter;

            public CppDataStream2(EPlatform platform, StringTable strTable)
            {
                mCurrent = -1;
                mOffset = 0;
                mPlatform = platform;
                mDataBlocks = new();
                mReferenceToBlock = new();
                mMarkers = new();
                mStringTable = strTable;
                mMemoryStream = new();
                mDataWriter = ArchitectureUtils.CreateBinaryWriter(mMemoryStream, mPlatform);
            }

            private class DataBlock
            {
                private Dictionary<StreamReference, List<long>> Pointers { get; } = new();
                private Dictionary<StreamReference, long> Markers { get; } = new();

                public int Alignment { get; init; }
                public int Offset { get; init; }
                public int Size { get; init; }
                public StreamReference Reference { get; init; }

                public static void End(DataBlock db, IBinaryWriter data)
                {
                    var gap = CMath.AlignUp(db.Size, db.Alignment) - db.Size;
                    // Write actual data to reach the size alignment requirement
                    const byte zero = 0;
                    for (var i = 0; i < gap; ++i)
                        data.Write(zero);
                }

                private static void AlignTo(IBinaryStream writer, uint alignment)
                {
                    writer.Position = CMath.AlignUp(writer.Position, alignment);
                }

                private static bool IsAligned(IBinaryStream writer, uint alignment)
                {
                    return CMath.IsAligned(writer.Position, alignment);
                }

                internal static void Write(IBinaryStreamWriter writer, float v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(float)));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, double v)
                {
                    AlignTo(writer, sizeof(double));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, sbyte v)
                {
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, short v)
                {
                    AlignTo(writer, sizeof(short));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, int v)
                {
                    AlignTo(writer, sizeof(int));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, long v)
                {
                    AlignTo(writer, sizeof(long));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, byte v)
                {
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, ushort v)
                {
                    AlignTo(writer, sizeof(ushort));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, uint v)
                {
                    AlignTo(writer, sizeof(uint));
                    writer.Write(v);
                }

                internal static void Write(IBinaryStreamWriter writer, ulong v)
                {
                    AlignTo(writer, sizeof(ulong));
                    writer.Write(v);
                }

                internal static void Write(IBinaryWriter writer, byte[] data, int index, int count)
                {
                    writer.Write(data, index, count);
                }

                internal static void Write(IBinaryStreamWriter writer, DataBlock db, StreamReference v)
                {
                    AlignTo(writer, sizeof(ulong));
                    if (db.Pointers.TryGetValue(v, out var pointers))
                    {
                        pointers.Add(writer.Position - db.Offset);
                    }
                    else
                    {
                        pointers = new List<long>() { writer.Position - db.Offset };
                        db.Pointers.Add(v, pointers);
                    }

                    writer.Write((long)v.Id);
                }

                internal static void Mark(DataBlock db, StreamReference v, long position)
                {
                    db.Markers.Add(v, position - db.Offset);
                }

                internal static void ReplaceReference(IBinaryStreamWriter data, DataBlock db, StreamReference oldRef, StreamReference newRef)
                {
                    // See if we are using this reference (oldRef) in this data block
                    if (!db.Pointers.Remove(oldRef, out var oldOffsets)) return;

                    // Update the reference in the stream, replacing oldRef.Id with newRef.Id
                    foreach (var o in oldOffsets)
                    {
                        data.Seek(db.Offset + o); // Seek to the position that has the 'StreamReference'
                        data.Write((long)newRef.Id); // The value we write here is the offset to the data computed in the simulation
                    }

                    // Update pointer and offsets
                    // It could be that we also are using newRef in this data block
                    if (db.Pointers.TryGetValue(newRef, out var newOffsets))
                    {
                        foreach (var o in oldOffsets)
                            newOffsets.Add(o);
                    }
                    else
                    {
                        db.Pointers.Add(newRef, oldOffsets);
                    }
                }

                internal static void WriteTo(DataBlock db, IBinaryDataStream data, IBinaryStreamWriter outData, IBinaryWriter outRelocationDataWriter, IDictionary<StreamReference, long> dataOffsetDataBase, byte[] readWriteBuffer)
                {
                    StreamUtils.Align(outData, db.Alignment);

                    // Verify the position of the data stream
                    dataOffsetDataBase.TryGetValue(db.Reference, out var outDataBlockOffset);
                    Debug.Assert(outData.Position == outDataBlockOffset);

                    foreach (var (sr, offsets) in db.Pointers)
                    {
                        // What is the offset of the data block we are pointing to
                        var exists = dataOffsetDataBase.TryGetValue(sr, out var referenceOffset);
                        Debug.Assert(exists);

                        // The offset are relative to the start of the DataBlock
                        foreach (var o in offsets)
                        {
                            data.Seek(db.Offset + o); // Seek to the position that has the 'StreamReference'
                            data.Write(referenceOffset); // The value we write here is the offset to the data computed in the simulation
                        }
                    }

                    // Update the dataOffsetDatabase with any markers we have in this data block
                    foreach (var (sr, offset) in db.Markers)
                    {
                        dataOffsetDataBase.Add(sr, outDataBlockOffset + offset);
                    }

                    // Read from the data stream at the start of the block and write the block to the output
                    data.Seek(db.Offset);

                    // Write the data of this block to the output, chunk for chunk
                    var sizeToWrite = db.Size;
                    while (sizeToWrite > 0)
                    {
                        var chunkRead = Math.Min(sizeToWrite, readWriteBuffer.Length);
                        var actualRead = data.Read(readWriteBuffer, 0, chunkRead);
                        Debug.Assert(actualRead > 0);
                        outData.Write(readWriteBuffer, 0, actualRead);
                        sizeToWrite -= actualRead;
                    }

                    // Write relocation info
                    // NOTE: the offsets (_pointers) of this data block are relative to this block
                    foreach (var (_, offsets) in db.Pointers)
                    {
                        foreach (var o in offsets)
                        {
                            outRelocationDataWriter.Write((int)outDataBlockOffset + (int)o);
                        }
                    }
                }
            }

            public void Align(int align)
            {
                var offset = mDataWriter.Position;
                if (CMath.TryAlignUp(offset, align, out var alignment)) return;
                mDataWriter.Position = alignment;
            }

            public void NewBlock(StreamReference reference, int alignment, int size)
            {
                // NOTE the alignment is kinda obsolete, at this moment aligning blocks to 8 bytes is sufficient

                // Always align the size of the block to 8 bytes
                size = CMath.AlignUp32(size, 8);
                mOffset = CMath.AlignUp32(mOffset, alignment);

                mDataBlocks.Add( new DataBlock()
                {
                    Alignment = alignment,
                    Offset = mOffset,
                    Size = size,
                    Reference = reference
                });

                mReferenceToBlock.Add(reference, mDataBlocks.Count);

                mOffset += size;

                if (mOffset >= mDataWriter.Length)
                {
                    mDataWriter.Length = mOffset + 4 * 1024;
                }
            }

            public void OpenBlock(StreamReference r)
            {
                var exists = mReferenceToBlock.TryGetValue(r, out var index);
                Debug.Assert(exists);
                Debug.Assert(mCurrent == -1);

                mCurrent = index;

                // Set the stream position to the start of the block
                var db = mDataBlocks[index];
                mDataWriter.Position = db.Offset;
            }

            public void Mark(StreamReference reference)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Mark(mDataBlocks[mCurrent], reference, mDataWriter.Position);
            }

            public void CloseBlock()
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.End(mDataBlocks[mCurrent], mDataWriter);

                // Check if the position is within the bounds of this block
                Debug.Assert(mDataWriter.Position >= mDataBlocks[mCurrent].Offset && mDataWriter.Position <= (mDataBlocks[mCurrent].Offset + mDataBlocks[mCurrent].Size));

                mCurrent = -1;
            }

            public void Write(float v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(float v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(double v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(double v)
            {
                Align(8);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(sbyte v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(byte v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(short v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(short v)
            {
                Align(2);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(int v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(int v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(long v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(long v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(ushort v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(ushort v)
            {
                Align(2);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(uint v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(uint v)
            {
                Align(4);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(ulong v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void AlignWrite(ulong v)
            {
                Align(8);
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, v);
            }

            public void Write(byte[] data, int index, int count)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, data, index, count);
            }

            public void Write(string str)
            {
                Debug.Assert(mCurrent != -1);
                var idx = mStringTable.Add(str);
                var len = mStringTable.LengthOfByIndex(idx);
                var reference = mStringTable.ReferenceOfByIndex(idx);
                Write(reference, len);
            }

            public void Write(StreamReference v)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, mDataBlocks[mCurrent], v);
            }

            public void Write(StreamReference v, long length)
            {
                Debug.Assert(mCurrent != -1);
                DataBlock.Write(mDataWriter, mDataBlocks[mCurrent], v);
                DataBlock.Write(mDataWriter, length);
            }

            public void Final(IBinaryStreamWriter dataWriter)
            {
            }

            public void Finalize(IBinaryStreamWriter dataWriter, IBinaryStreamWriter relocationDataWriter)
            {
                // Dictionary for mapping a Reference to a DataBlock
                var finalDataDataBase = new Dictionary<StreamReference, DataBlock>(mDataBlocks.Count);
                foreach (var d in mDataBlocks)
                    finalDataDataBase.Add(d.Reference, d);

                // For all blocks:
                // Collapse identical blocks identified by hash, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the hash of a data block.

                var memoryBytes = mMemoryStream.ToArray();
                var memoryStream = new BinaryMemoryBlock();
                memoryStream.Setup(memoryBytes, 0, memoryBytes.Length);

                var duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                var dataHashDataBase = new Dictionary<Hash160, StreamReference>();

                while (true)
                {
                    duplicateDataBase.Clear();
                    dataHashDataBase.Clear();

                    foreach (var d in mDataBlocks)
                    {
                        var hash = HashUtility.Compute(memoryBytes.AsSpan(d.Offset, d.Size));
                        if (dataHashDataBase.TryGetValue(hash, out var newRef))
                        {
                            // Encountering a block of data which has a duplicate.
                            // After the first iteration it might be the case that
                            // they have the same 'Reference' since they are collapsed.
                            if (d.Reference != newRef)
                            {
                                if (!duplicateDataBase.ContainsKey(newRef))
                                {
                                    if (finalDataDataBase.ContainsKey(d.Reference))
                                    {
                                        var duplicateReferences = new List<StreamReference>() { d.Reference };
                                        duplicateDataBase[newRef] = duplicateReferences;
                                    }
                                }
                                else
                                {
                                    if (finalDataDataBase.ContainsKey(d.Reference))
                                        duplicateDataBase[newRef].Add(d.Reference);
                                }

                                finalDataDataBase.Remove(d.Reference);
                            }
                        }
                        else
                        {
                            // This block of data is still unique
                            dataHashDataBase.Add(hash, d.Reference);
                        }
                    }

                    // Rebuild the list of data blocks
                    mDataBlocks.Clear();
                    mDataBlocks.Capacity = finalDataDataBase.Count;
                    foreach (var (_, db) in finalDataDataBase)
                    {
                        mDataBlocks.Add(db);
                    }

                    // For each data block replace any occurence of an old reference with its unique reference
                    foreach (var db in mDataBlocks)
                    {
                        foreach (var (uniqueRef, duplicateRefs) in duplicateDataBase)
                        {
                            foreach (var duplicateRef in duplicateRefs)
                            {
                                DataBlock.ReplaceReference(memoryStream, db, duplicateRef, uniqueRef);
                            }
                        }
                    }

                    // Did we find any duplicates, if so then we also replaced references
                    // and by doing so hashes have changed.
                    // Some blocks now might have an identical hash due to this.
                    if (duplicateDataBase.Count == 0) break;
                }

                // Compute stream offset for each data block, do this by simulating the writing process.
                // All references (pointers) are written in the stream as a 64-bit offset (64-bit pointers)
                // relative to the start of the data stream.
                var dataOffsetDataBase = new Dictionary<StreamReference, long>();
                var offset = dataWriter.Position;
                foreach (var (dbRef, db) in finalDataDataBase)
                {
                    offset = CMath.AlignUp(offset, db.Alignment);
                    dataOffsetDataBase.Add(dbRef, offset);
                    offset += db.Size;
                }

                // Dump all blocks to dataWriter
                // Dump all reallocation info to relocationDataWriter
                // Patch the location of every reference in the memory stream!
                var readWriteBuffer = new byte[8192];
                foreach (var (_, db) in finalDataDataBase)
                {
                    DataBlock.WriteTo(db, memoryStream, dataWriter, relocationDataWriter, dataOffsetDataBase, readWriteBuffer);
                }
            }

            public IArchitecture Architecture => mDataWriter.Architecture;

            public long Position
            {
                get => mDataWriter.Position;
                set => mDataWriter.Position = value;
            }

            public long Length
            {
                get => mDataWriter.Length;
                set => mDataWriter.Length = value;
            }

            public long Seek(long offset)
            {
                return mDataWriter.Seek(offset);
            }

            public void Close()
            {
                mDataWriter.Close();
            }
        }
    }
}
