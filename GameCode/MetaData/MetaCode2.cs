using System.Diagnostics;
using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;
using StreamWriter = System.IO.StreamWriter;

namespace GameData
{
    namespace MetaCode
    {
        public interface IMemberFactory2
        {
            void NewBoolMember(bool content, string memberName);
            void NewInt8Member(sbyte content, string memberName);
            void NewUInt8Member(byte content, string memberName);
            void NewInt16Member(short content, string memberName);
            void NewUInt16Member(ushort content, string memberName);
            void NewInt32Member(int content, string memberName);
            void NewUInt32Member(uint content, string memberName);
            void NewInt64Member(long content, string memberName);
            void NewUInt64Member(ulong content, string memberName);
            void NewFloatMember(float content, string memberName);
            void NewDoubleMember(double content, string memberName);
            void NewStringMember(string content, string memberName);
            void NewEnumMember(Type type, object content, string memberName);
            int NewArrayMember(Type type, object content, string memberName);
            int NewDictionaryMember(Type type, object content, string memberName);
            int NewClassMember(Type type, object content, string memberName);
            void NewStructMember(Type type, object content, string memberName);
        }

        public struct MetaInfo
        {
            public const UInt8 Unknown = 0;
            public const UInt8 Bool = 1;
            public const UInt8 BitSet = 2;
            private const UInt8 Int8 = 3;
            private const UInt8 UInt8 = 4;
            private const UInt8 Int16 = 5;
            private const UInt8 UInt16 = 6;
            private const UInt8 Int32 = 7;
            private const UInt8 UInt32 = 8;
            private const UInt8 Int64 = 9;
            private const UInt8 UInt64 = 10;
            private const UInt8 Float = 11;
            private const UInt8 Double = 12;
            private const UInt8 String = 13;
            public const UInt8 Enum = 14;
            public const UInt8 Struct = 15; // Value Type
            public const UInt8 Class = 16; // Reference Type
            private const UInt8 Array = 17; // Array (offset + size)
            private const UInt8 Dictionary = 18; // Dictionary (offset + size)
            public const UInt8 Count = 19;

            public static MetaInfo AsBool => new() { Index = Bool };
            public static MetaInfo AsBitSet => new() { Index = BitSet };
            public static MetaInfo AsInt8 => new() { Index = Int8 };
            public static MetaInfo AsUInt8 => new() { Index = UInt8 };
            public static MetaInfo AsInt16 => new() { Index = Int16 };
            public static MetaInfo AsUInt16 => new() { Index = UInt16 };
            public static MetaInfo AsInt32 => new() { Index = Int32 };
            public static MetaInfo AsUInt32 => new() { Index = UInt32 };
            public static MetaInfo AsInt64 => new() { Index = Int64 };
            public static MetaInfo AsUInt64 => new() { Index = UInt64 };
            public static MetaInfo AsFloat => new() { Index = Float };
            public static MetaInfo AsDouble => new() { Index = Double };
            public static MetaInfo AsString => new() { Index = String };
            public static MetaInfo AsEnum => new() { Index = Enum };
            public static MetaInfo AsStruct => new() { Index = Struct };
            public static MetaInfo AsClass => new() { Index = Class };
            public static MetaInfo AsArray => new() { Index = Array };
            public static MetaInfo AsDictionary => new() { Index = Dictionary };

            private static readonly int[] sSizeInBits = new int[Count]
            {
                0, // Unknown
                1, // Bool
                32, // BitSet
                8, // Int8
                8, // UInt8
                16, // Int16
                16, // UInt16
                32, // Int32
                32, // UInt32
                64, // Int64
                64, // UInt64
                32, // Float
                64, // Double
                64 + 64, // String (pointer, length)
                32, // Enum
                0, // Struct (unknown)
                64, // Class (pointer)
                64 + 64, // Array (offset, length)
                64 + 64, // Dictionary (offset, length)
            };

            private static readonly int[] sMemberAlignment = new int[Count]
            {
                0, // Unknown
                1, // Bool
                4, // BitSet
                1, // Int8
                1, // UInt8
                2, // Int16
                2, // UInt16
                4, // Int32
                4, // UInt32
                8, // Int64
                8, // UInt64
                4, // Float
                8, // Double
                8, // String (pointer, length)
                4, // Enum
                -1, // Struct (unknown)
                8, // Class (offset)
                8, // Array (offset, length)
                8, // Dictionary (offset, length)
            };


            public int SizeInBits => sSizeInBits[Index];
            public int SizeInBytes => (SizeInBits + 7) >> 3;

            private uint Value { get; set; }

            public UInt8 Index
            {
                get => (UInt8)(Value & 0xFF);
                private init => Value = (Value & 0xFFFFFF00) | value;
            }

            public int MemberAlignment => sMemberAlignment[Index];

            private const uint
                FlagInPlace =
                    0x200000; // For Arrays and Dictionaries, the elements are written in place as values not as references

            public bool IsStruct => Index == Struct;
            public bool IsClass => Index == Class;
            public bool IsArray => Index == Array;
            public bool IsDictionary => Index == Dictionary;

            public bool InPlace
            {
                get => (Value & FlagInPlace) != 0;
                set => Value = ((Value & ~FlagInPlace) | (value ? FlagInPlace : 0));
            }
        }


        // MetaCode is a thought experiment on how to optimize the building of the classes and members that we encounter
        // when we go through the C# code using reflection. Currently we are building this purely using custom C# code
        // and we could do this a lot more efficient.

        public class MetaCode
        {
            public readonly List<string> CodeStrings;
            public readonly StringTable DataStrings;

            public readonly List<MetaInfo> MembersType;
            public readonly List<int> MembersName;
            public readonly List<int> MembersStart; // If we are a Struct the members start here

            public readonly List<int>
                MembersCount = new(); // If we are an Array/List/Dict/Struct we hold many elements/members

            public readonly List<object> MembersObject = new(); // This is to know the type of the class

            public MetaCode(StringTable dataStrings, int estimatedCount)
            {
                CodeStrings = new List<string>(estimatedCount);
                DataStrings = dataStrings;
                MembersType = new List<MetaInfo>(estimatedCount);
                MembersName = new List<int>(estimatedCount);
                MembersStart = new List<int>(estimatedCount);
            }

            public int AddMember(MetaInfo info, int name, int startIndex, int count, object o)
            {
                var index = MembersType.Count;
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

            public void DuplicateMember(int memberIndex)
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
                var alignment = mt.MemberAlignment;
                if (mt.IsStruct)
                {
                    if (MembersObject[memberIndex] is IStruct ios)
                    {
                        alignment = ios.StructAlign;
                    }
                }

                return alignment;
            }

            public int GetDataAlignment(int memberIndex)
            {
                var mt = MembersType[memberIndex];

                // Structs have unknown alignment, we need to get it by using IStruct
                var alignment = MembersType[memberIndex].MemberAlignment;
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

            public class SortMembersBySize : IComparer<int>
            {
                private MetaCode _metaCode;

                public SortMembersBySize(MetaCode metaCode)
                {
                    _metaCode = metaCode;
                }

                public int Compare(int x, int y)
                {
                    var xt = _metaCode.MembersType[x];
                    var yt = _metaCode.MembersType[y];
                    var xs = xt.SizeInBits;
                    var ys = yt.SizeInBits;
                    if (xt.IsStruct)
                    {
                        xs = 4 * 8;
                        if (_metaCode.MembersObject[x] is IStruct xi)
                        {
                            xs = xi.StructSize * 8;
                        }
                    }
                    else if (xt.IsClass)
                    {
                        xs = 4 * 8;
                    }

                    if (yt.IsStruct)
                    {
                        // figure out if it is an IStruct or a Class since IStruct has defined it's own size
                        ys = 4 * 8;
                        if (_metaCode.MembersObject[y] is IStruct yi)
                        {
                            ys = yi.StructSize * 8;
                        }
                    }
                    else if (yt.IsClass)
                    {
                        ys = 4 * 8;
                    }

                    var c = xs.CompareTo(ys);

                    // If the sizes are the same, sort by type, then by member name
                    if (c == 0)
                    {
                        c = xt.Index.CompareTo(yt.Index);
                        if (c == 0)
                        {
                            c = _metaCode.MembersName[x].CompareTo(_metaCode.MembersName[y]);
                        }
                    }

                    return c;
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
                    if (cmt.Index == MetaInfo.Bool)
                    {
                        end -= 1;
                        SwapMembers(mi, end);
                    }
                    else
                    {
                        ++mi;
                    }
                }

                // We can combine 32 booleans into a uint32
                // Number of booleans at the end of the member list
                var n = MembersCount[classIndex] - (end - MembersStart[classIndex]);
                var i = 0;
                while (n > 0)
                {
                    // Duplicate all the boolean members to make room for this bitset member but also to have the bitset member be able to
                    // have its own range of MetaMembers.
                    var numBits = (n <= 32) ? n : 32;
                    var startIndex = MembersCount.Count;
                    mi = end + (i * 32);
                    var mie = mi + numBits;
                    var value = (uint)0;
                    var bit = (uint)1;
                    while (mi < mie)
                    {
                        DuplicateMember(mi);
                        // Depending on the boolean value we set the bit in the bitset
                        if ((bool)MembersObject[mi])
                        {
                            value |= bit;
                        }

                        bit <<= 1;
                        ++mi;
                    }

                    var mni = MembersName[end + i];
                    SetMember(end + i, MetaInfo.AsBitSet, mni, startIndex, numBits, value);

                    i += 1;
                    n -= 32;
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

            public void Write(CppDataStream2 dataStream)
            {
                // Write the instantiated code as binary data matching the class layout

                // Every reference type (class, array, dictionary) will be written as a pointer to the data

                // We need to keep track of where the data is written so we can write the pointers to the data, this
                // is done by keeping StreamReferences for each reference type.
            }
        }

        #region C++ Code writer

        public class CppCodeWriter2
        {
            private readonly MetaCode _metaCode;

            private static readonly string[] sTypeNames = new string[(int)MetaInfo.Count]
            {
                "unknown",
                "bool",
                "u32",
                "s8",
                "u8",
                "s16",
                "u16",
                "s32",
                "u32",
                "s64",
                "u64",
                "f32",
                "f64",
                "string_t",
                "enum",
                "struct",
                "class",
                "array_t",
                "dict_t"
            };

            private static string GetMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var typeIndex = mt.Index;
                return sTypeNames[typeIndex];
            }

            private static string GetEnumMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var enumInstance = metaCode.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                var memberTypeString = sTypeNames[mt.Index];
                return string.Concat(memberTypeString, "<", enumTypeName, ",", "u32", ">");
            }

            private static string GetStructMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mi = metaCode.MembersStart[memberIndex];
                var ios = metaCode.MembersObject[mi] as IStruct;
                return ios.StructName;
            }

            private static string GetClassMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var className = metaCode.MembersObject[memberIndex].GetType().Name;
                return className + "*";
            }

            private static string GetArrayMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var fet = metaCode.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[fet.Index](msi, metaCode, option); // recursive call
                var memberTypeString = sTypeNames[mt.Index];
                return string.Concat(memberTypeString, "<", elementTypeString, ">");
            }

            private static string GetDictMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var fk = metaCode.MembersType[msi]; // first key element
                var fv = metaCode.MembersType[msi + count]; // first value element
                var keyTypeString = sGetMemberTypeString[fk.Index](msi, metaCode, option); // recursive call
                var valueTypeString = sGetMemberTypeString[fv.Index](msi + count, metaCode, option); // recursive call
                var memberTypeString = sTypeNames[mt.Index];
                return string.Concat(memberTypeString, "<", keyTypeString, ",", valueTypeString, ">");
            }

            [Flags]
            private enum EOption
            {
                None = 0,
                InPlace = 1,
            }

            private delegate string GetMemberTypeStringDelegate(int memberIndex, MetaCode metaCode, EOption option);

            private static readonly GetMemberTypeStringDelegate[] sGetMemberTypeString =
                new GetMemberTypeStringDelegate[(int)MetaInfo.Count]
                {
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
                    GetMemberTypeName, // f64
                    GetMemberTypeName, // raw_string_t
                    GetEnumMemberTypeName,
                    GetStructMemberTypeName, // struct
                    GetClassMemberTypeName, // class
                    GetArrayMemberTypeName,
                    GetDictMemberTypeName,
                };


            private static string GetReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var typeIndex = mt.Index;
                return sTypeNames[typeIndex];
            }

            private static string GetEnumReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var enumInstance = metaCode.MembersObject[memberIndex];
                return enumInstance.GetType().Name;
            }

            private static string GetStructReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mi = metaCode.MembersStart[memberIndex];
                var ios = metaCode.MembersObject[mi] as IStruct;
                return ios.StructName;
            }

            private static string GetClassReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var className = metaCode.MembersObject[memberIndex].GetType().Name;
                return className + " const *";
            }

            private static string GetArrayReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msi = metaCode.MembersStart[memberIndex];
                var fet = metaCode.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[fet.Index](msi, metaCode, option); // recursive call
                return $"raw_array_t<{elementTypeString}>";
            }

            private static string GetDictReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msi = metaCode.MembersStart[memberIndex];
                var count = metaCode.MembersCount[memberIndex];
                var fkt = metaCode.MembersType[msi]; // first key element
                var fvt = metaCode.MembersType[msi + count]; // first value element
                var keyTypeString = sGetMemberTypeString[fkt.Index](msi, metaCode, option); // recursive call
                var valueTypeString = sGetMemberTypeString[fvt.Index](msi + count, metaCode, option); // recursive call
                return $"raw_dict_t<{keyTypeString}, {valueTypeString}>";
            }

            private delegate string GetReturnTypeStringDelegate(int memberIndex, MetaCode metaCode, EOption option);

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

            private delegate void WriteGetterDelegate(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option);

            private static readonly WriteGetterDelegate[] sWriteGetterDelegates =
                new WriteGetterDelegate[(int)MetaInfo.Count]
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

            private delegate void WriteMemberDelegate(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option);

            private static readonly WriteMemberDelegate[] sWriteMemberDelegates =
                new WriteMemberDelegate[(int)MetaInfo.Count]
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

            private static void WriteBitsetGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var count = metaCode.MembersCount[memberIndex];

                // Emit a get function for each boolean that the bitset represents
                for (var i = 0; i < count; ++i)
                {
                    var booleanMemberIndex = msi + i;
                    var booleanMemberName = metaCode.MembersName[booleanMemberIndex];
                    var booleanName = metaCode.CodeStrings[booleanMemberName];
                    writer.WriteLine(
                        $"\tinline bool get{booleanName}() const {{ return (m_{memberName} & (1 << {i})) != 0; }}");
                }
            }

            private static void WriteBitsetMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                writer.WriteLine($"\tu32 m_{memberName};");
            }

            private static void WritePrimitiveGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var typeIndex = mt.Index;
                writer.WriteLine(
                    $"\tinline {sTypeNames[typeIndex]} get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WritePrimitiveMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var typeIndex = mt.Index;
                writer.WriteLine($"\t{sTypeNames[typeIndex]} m_{memberName};");
            }

            private static void WriteStringGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                writer.WriteLine($"\tinline string_t const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteEnumGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var enumInstance = metaCode.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                writer.WriteLine($"\tinline {enumTypeName} get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteEnumMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var enumInstance = metaCode.MembersObject[memberIndex];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                writer.WriteLine($"\tenum_t<{enumTypeName}, u32> m_{memberName};");
            }

            private static void WriteStructGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];

                var ios = metaCode.MembersObject[memberIndex] as IStruct;
                var className = ios.StructName;

                writer.WriteLine($"\tinline {className} const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteStructMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var ios = metaCode.MembersObject[memberIndex] as IStruct;
                var className = ios.StructName;
                writer.WriteLine($"\t{className} m_{memberName};");
            }

            private static void WriteClassGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var className = metaCode.MembersObject[memberIndex].GetType().Name;
                writer.WriteLine($"\tinline {className} const* get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteClassMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var className = metaCode.MembersObject[memberIndex].GetType().Name;
                writer.WriteLine("\t" + className + "* m_" + memberName + ";");
            }

            private static void WriteArrayGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var msi = metaCode.MembersStart[memberIndex];
                var fet = metaCode.MembersType[msi];

                // figure out the element type of the array, could be a primitive, struct, class, enum or even another array
                var returnTypeString = sGetReturnTypeString[fet.Index](msi, metaCode, option);

                writer.WriteLine(
                    $"\tinline array_t<{returnTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteArrayMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var memberName = metaCode.CodeStrings[mni];
                var fet = metaCode.MembersType[msi];
                var elementName = sGetMemberTypeString[fet.Index](msi, metaCode, option);
                writer.WriteLine($"\tarray_t<{elementName}> m_{memberName};");
            }

            private static void WriteDictionaryGetter(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var memberName = metaCode.CodeStrings[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode.MembersType[msi];
                var valueElement = metaCode.MembersType[msi + count];
                var keyTypeString = sGetReturnTypeString[keyElement.Index](msi, metaCode, option);
                var valueTypeString = sGetReturnTypeString[valueElement.Index](msi + count, metaCode, option);

                writer.WriteLine(
                    $"\tinline dict_t<{keyTypeString}, {valueTypeString}> const& get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteDictionaryMember(int memberIndex, StreamWriter writer, MetaCode metaCode,
                EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStart[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var memberName = metaCode.CodeStrings[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode.MembersType[msi];
                var valueElement = metaCode.MembersType[msi + count];
                var keyTypeString = sGetMemberTypeString[keyElement.Index](msi, metaCode, option);
                var valueTypeString = sGetMemberTypeString[valueElement.Index](msi + count, metaCode, option);

                writer.WriteLine($"\tdict_t<{keyTypeString}, {valueTypeString}> m_{memberName};");
            }


            public CppCodeWriter2(MetaCode metaCode)
            {
                _metaCode = metaCode;
            }

            private void WriteEnum(Type e, StreamWriter writer)
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

            public void WriteEnums(StreamWriter writer)
            {
                HashSet<string> writtenEnums = new();
                for (var i = 0; i < _metaCode.MembersType.Count; ++i)
                {
                    var mt = _metaCode.MembersType[i];
                    if (mt.Index != MetaInfo.Enum) continue;
                    var enumInstance = _metaCode.MembersObject[i];
                    if (writtenEnums.Contains(enumInstance.GetType().Name)) continue;
                    WriteEnum(enumInstance.GetType(), writer);
                    writtenEnums.Add(enumInstance.GetType().Name);
                }

                writer.WriteLine();
            }

            private void WriteStruct(int memberIndex, StreamWriter writer, EOption option)
            {
            }

            private void WriteClass(int memberIndex, StreamWriter writer, EOption option)
            {
                var msi = _metaCode.MembersStart[memberIndex];
                var count = _metaCode.MembersCount[memberIndex];
                var mt = _metaCode.MembersType[memberIndex];

                var className = _metaCode.MembersObject[memberIndex].GetType().Name;

                writer.WriteLine($"class {className}");
                writer.WriteLine("{");
                writer.WriteLine("public:");

                // write public member getter functions
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = _metaCode.MembersType[i];
                    sWriteGetterDelegates[mmt.Index](i, writer, _metaCode, option);
                }

                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = _metaCode.MembersType[i];
                    sWriteMemberDelegates[mmt.Index](i, writer, _metaCode, EOption.None);
                }

                writer.WriteLine("};");
                writer.WriteLine();
            }

            public void WriteClasses(StreamWriter writer)
            {
                // Forward declares ?
                writer.WriteLine("// Forward declares");
                HashSet<string> writtenClasses = new();
                for (var i = 0; i < _metaCode.MembersType.Count; ++i)
                {
                    var mt = _metaCode.MembersType[i];
                    if (!mt.IsClass) continue;
                    var cn = _metaCode.MembersObject[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    writer.WriteLine($"class {cn};");
                    writtenClasses.Add(cn);
                }

                writer.WriteLine();

                writtenClasses.Clear();
                for (var i = _metaCode.MembersType.Count - 1; i >= 0; --i)
                {
                    var mt = _metaCode.MembersType[i];
                    if (!mt.IsClass) continue;
                    var cn = _metaCode.MembersObject[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    WriteClass(i, writer, EOption.None);
                    writtenClasses.Add(cn);
                }

                writer.WriteLine();
            }
        }

        #endregion

        #region Meta Member Factory

        public class MetaMemberFactory : IMemberFactory2
        {
            public MetaMemberFactory(MetaCode metaCodeCode)
            {
                _metaCode = metaCodeCode;
            }

            private readonly MetaCode _metaCode;

            private readonly Dictionary<string, int> _codeStringMap = new();

            private int RegisterCodeString(string memberName)
            {
                if (_codeStringMap.TryGetValue(memberName, out var index)) return index;
                index = _metaCode.CodeStrings.Count;
                _metaCode.CodeStrings.Add(memberName);
                _codeStringMap.Add(memberName, index);
                return index;
            }

            private int RegisterDataString(string str)
            {
                return _metaCode.DataStrings.Add(str);
            }

            public void NewBoolMember(bool content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsBool, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt8Member(sbyte content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsInt8, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt8Member(byte content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsUInt8, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt16Member(short content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsInt16, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt16Member(ushort content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsUInt16, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt32Member(int content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsInt32, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt32Member(uint content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsUInt32, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewInt64Member(long content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsInt64, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewUInt64Member(ulong content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsUInt64, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewFloatMember(float content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsFloat, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewDoubleMember(double content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsDouble, RegisterCodeString(memberName), -1, 1, content);
            }

            public void NewStringMember(string content, string memberName)
            {
                _metaCode.AddMember(MetaInfo.AsString, RegisterCodeString(memberName), RegisterDataString(content), 1, content);
            }

            public void NewEnumMember(Type type, object content, string memberName)
            {
                if (content is not Enum e)
                    return;

                _metaCode.AddMember(MetaInfo.AsEnum, RegisterCodeString(memberName), -1, 1, e);
            }

            public int NewArrayMember(Type type, object content, string memberName)
            {
                return _metaCode.AddMember(MetaInfo.AsArray, RegisterCodeString(memberName), -1, 0, content);
            }

            public int NewDictionaryMember(Type type, object content, string memberName)
            {
                return _metaCode.AddMember(MetaInfo.AsDictionary, RegisterCodeString(memberName), -1, 0, content);
            }

            public void NewStructMember(Type type, object content, string memberName)
            {
                // An IStruct is either a value type or a reference type, the C++ code is already 'known' in the codebase.
                // Also the data of the 'members' of this struct are written by using the IStruct interface, so we do not
                // need to 'parse' the members of this struct, we will just write the data as is.
                if (content is not IStruct o)
                    return;

                var metaType = MetaInfo.AsStruct;
                _metaCode.AddMember(metaType, RegisterCodeString(memberName), -1, 1, content);
            }

            public int NewClassMember(Type type, object content, string memberName)
            {
                return _metaCode.AddMember(MetaInfo.AsClass, RegisterCodeString(memberName), -1, 0, content);
            }
        }

        #endregion

        #region Class and ClassMember data writer

        public static class DataStreamWriter2
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
                public MetaCode MetaCode { get; init; }
                public StringTable StringTable { get; init; }
                public CppDataStream2 DataStream { get; init; }
                public CalcSizeOfTypeDelegate[] CalcSizeOfTypeDelegates { get; init; }
                public CalcSizeOfTypeDelegate[] CalcDataSizeOfTypeDelegates { get; init; }
                public WriteMemberDelegate[] WriteMemberDelegates { get; init; }
                public Queue<WriteProcess> WriteProcessQueue { get; init; }
            }

            public static void Write(MetaCode metaCode, StringTable stringTable, CppDataStream2 dataStream)
            {
                var ctx = new WriteContext
                {
                    MetaCode = metaCode,
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

                WriteClass(0, ctx);
                while (ctx.WriteProcessQueue.Count > 0)
                {
                    var wp = ctx.WriteProcessQueue.Dequeue();
                    wp.Process(wp.MemberIndex, wp.Reference, ctx);
                }
            }

            private static void WriteBool(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Write((bool)member ? (Int8)1 : (Int8)0);
            }

            private static void WriteBitset(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(4);
                ctx.DataStream.Write((uint)member);
            }

            private static void WriteInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Write((Int8)member);
            }

            private static void WriteUInt8(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Write((UInt8)member);
            }

            private static void WriteInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(2);
                ctx.DataStream.Write((short)member);
            }

            private static void WriteUInt16(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(2);
                ctx.DataStream.Write((ushort)member);
            }

            private static void WriteInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(4);
                ctx.DataStream.Write((int)member);
            }

            private static void WriteUInt32(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(4);
                ctx.DataStream.Write((uint)member);
            }

            private static void WriteInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(8);
                ctx.DataStream.Write((long)member);
            }

            private static void WriteUInt64(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(8);
                ctx.DataStream.Write((ulong)member);
            }

            private static void WriteFloat(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(4);
                ctx.DataStream.Write((float)member);
            }

            private static void WriteDouble(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(8);
                ctx.DataStream.Write((double)member);
            }

            private static void WriteString(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode.MembersType[memberIndex];
                var si = ctx.StringTable.Add(ctx.MetaCode.MembersObject[memberIndex] as string);
                var mr = ctx.StringTable.ReferenceOfByIndex(si);
                var ms = ctx.StringTable.LengthOfByIndex(si);
                ctx.DataStream.Align(4);
                ctx.DataStream.Write(mr, ms);

                // We do not need to schedule the string content to be written since all strings
                // are part of the string table and are written in one go.
            }

            private static void WriteEnum(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersObject[memberIndex];
                ctx.DataStream.Align(4);
                ctx.DataStream.Write((uint)member);
            }

            private static void WriteStruct(int memberIndex, WriteContext ctx)
            {
                var mx = ctx.MetaCode.MembersObject[memberIndex] as IStruct;
                ctx.DataStream.Align(mx.StructAlign);
                mx.StructWrite(ctx.DataStream);
            }

            private static void WriteClassDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // A class is written as a collection of members
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode.MembersStart[memberIndex];
                    var count = ctx.MetaCode.MembersCount[memberIndex];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        var et = ctx.MetaCode.MembersType[mi];
                        ctx.WriteMemberDelegates[et.Index](mi, ctx);
                    }
                }
                ctx.DataStream.CloseBlock();
            }

            private static void WriteClass(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var mr = ctx.DataStream.NewBlock(ctx.MetaCode.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr);

                // We need to schedule the content of this class to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteClassDataProcess, Reference = mr });
            }


            private static void WriteArrayDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // An Array<T> is written as an array of elements
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode.MembersStart[memberIndex];
                    var count = ctx.MetaCode.MembersCount[memberIndex];
                    var et = ctx.MetaCode.MembersType[msi];
                    for (var mi = msi; mi < msi + count; ++mi)
                    {
                        ctx.WriteMemberDelegates[et.Index](mi, ctx);
                    }
                }
                ctx.DataStream.CloseBlock();
            }

            private static void WriteArray(int memberIndex, WriteContext ctx)
            {
                var mt = ctx.MetaCode.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode.MembersCount[memberIndex];
                var mr = ctx.DataStream.NewBlock(ctx.MetaCode.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteArrayDataProcess, Reference = mr });
            }

            private static void WriteDictionaryDataProcess(int memberIndex, StreamReference r, WriteContext ctx)
            {
                // A Dictionary<key,value> is written as an array of keys followed by an array of values
                ctx.DataStream.OpenBlock(r);
                {
                    var msi = ctx.MetaCode.MembersStart[memberIndex];
                    var count = ctx.MetaCode.MembersCount[memberIndex];
                    var kt = ctx.MetaCode.MembersType[msi];
                    var vt = ctx.MetaCode.MembersType[msi + count];
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
                var mt = ctx.MetaCode.MembersType[memberIndex];
                var ms = ctx.CalcDataSizeOfTypeDelegates[mt.Index](memberIndex, ctx);
                var count = ctx.MetaCode.MembersCount[memberIndex];
                var mr = ctx.DataStream.NewBlock(ctx.MetaCode.GetDataAlignment(memberIndex), ms);
                ctx.DataStream.Write(mr, count);

                // We need to schedule this array to be written
                ctx.WriteProcessQueue.Enqueue(new WriteProcess() { MemberIndex = memberIndex, Process = WriteDictionaryDataProcess, Reference = mr });
            }

            private static int GetMemberSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode.MembersType[memberIndex].SizeInBytes;
            }

            private static int GetMemberSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }


            private static int GetDataSizeOfType(int memberIndex, WriteContext ctx)
            {
                return ctx.MetaCode.MembersType[memberIndex].SizeInBytes;
            }

            private static int CalcDataSizeOfString(int memberIndex, WriteContext ctx)
            {
                var member = ctx.MetaCode.MembersStart[memberIndex];
                return ctx.StringTable.LengthOfByIndex(member);
            }

            private static int CalcDataSizeOfStruct(int memberIndex, WriteContext ctx)
            {
                var xi = ctx.MetaCode.MembersObject[memberIndex] as IStruct;
                return xi.StructSize;
            }

            private static int CalcDataSizeOfClass(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode.MembersStart[memberIndex];
                var count = ctx.MetaCode.MembersCount[memberIndex];

                // Alignment of this class is determined by the first member
                var classAlign = ctx.MetaCode.GetMemberAlignment(msi);

                var size = 0;
                for (var mi = msi; mi < msi + count; ++mi)
                {
                    // Obtain the size of this member
                    var ms = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode.MembersType[mi].Index](mi, ctx);

                    // Align the size based on the member type alignment
                    size = CMath.Align32(size, ctx.MetaCode.GetMemberAlignment(mi));

                    size += ms;
                }

                size = CMath.Align32(size, classAlign);

                return size;
            }

            private static int CalcDataSizeOfArray(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode.MembersStart[memberIndex];
                var count = ctx.MetaCode.MembersCount[memberIndex];

                // Determine the alignment of the element and see if we need to align the size
                var elementAlign = ctx.MetaCode.GetMemberAlignment(msi);
                var elementSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode.MembersType[msi].Index](msi, ctx);
                elementSize = CMath.Align32(elementSize, elementAlign);

                var size = count * elementSize;
                return size;
            }

            private static int CalcDataSizeOfDictionary(int memberIndex, WriteContext ctx)
            {
                var msi = ctx.MetaCode.MembersStart[memberIndex];
                var count = ctx.MetaCode.MembersCount[memberIndex];

                var keyIndex = msi;
                var keyAlign = ctx.MetaCode.GetMemberAlignment(keyIndex);
                var keySize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode.MembersType[keyIndex].Index](keyIndex, ctx);
                keySize = CMath.Align32(keySize, keyAlign);

                var valueIndex = msi + count;
                var valueAlign = ctx.MetaCode.GetMemberAlignment(valueIndex);
                var valueSize = ctx.CalcSizeOfTypeDelegates[ctx.MetaCode.MembersType[valueIndex].Index](valueIndex, ctx);
                valueSize = CMath.Align32(valueSize, valueAlign);

                var size = CMath.Align32(count * keySize, valueAlign) + count * valueSize;
                return size;
            }
        }

        #endregion


        /// <summary>
        /// A CppDataStream is used to write DataBlocks, DataBlocks are stored and when
        /// the final data is written identical (Hash) DataBlocks are collapsed to one.
        /// All references (pointers to blocks) are also resolved at the final stage.
        ///
        /// Output: a database of the offset of every reference (DataBlock)
        ///
        /// </summary>
        ///
        public class CppDataStream2 : IBinaryWriter
        {
            #region Fields

            private readonly List<DataBlock> _dataBlocks = new();
            private readonly Dictionary<StreamReference, int> _referenceToBlock = new();
            private readonly StringTable _stringTable;
            private readonly MemoryStream _memoryStream = new();
            private readonly IBinaryStreamWriter _dataWriter;
            private int _current = 0;

            private EPlatform Platform { get; set; }

            #endregion

            #region Constructor

            public CppDataStream2(EPlatform platform, StringTable strTable)
            {
                Platform = platform;
                _dataWriter = EndianUtils.CreateBinaryWriter(_memoryStream, Platform);
                _dataBlocks.Add(new DataBlock() { Offset = 0, Size = 0, Reference = StreamReference.NewReference });
                _stringTable = strTable;
            }

            #endregion

            #region DataBlock

            private class DataBlock
            {
                private readonly Dictionary<StreamReference, List<long>> _pointers = new();

                internal DataBlock()
                {
                    Alignment = 8;
                    Reference = StreamReference.NewReference;
                }

                public int Alignment { get; init; }
                public int Offset { get; init; }
                public int Size { get; init; }

                public StreamReference Reference { get; set; }

                internal void End(IBinaryWriter data)
                {
                    var gap = CMath.Align(Size, Alignment) - Size;

                    // Write actual data to reach the size alignment requirement
                    const byte zero = 0;
                    for (var i = 0; i < gap; ++i)
                        data.Write(zero);
                }


                internal void AlignTo(IBinaryStream writer, int alignment)
                {
                    writer.Position = CMath.Align(writer.Position, alignment);
                }

                private bool IsAligned(IBinaryStream writer, int alignment)
                {
                    return CMath.IsAligned(writer.Position, alignment);
                }

                internal void Write(IBinaryStreamWriter writer, float v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(float)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, double v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(double)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, sbyte v)
                {
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, short v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(short)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, int v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(int)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, long v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(long)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, byte v)
                {
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, ushort v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(ushort)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, uint v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(uint)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, ulong v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(ulong)));
                    writer.Write(v);
                }

                internal void Write(IBinaryStreamWriter writer, byte[] data, int index, int count)
                {
                    writer.Write(data, index, count);
                }

                internal void Write(IBinaryStreamWriter writer, StreamReference v)
                {
                    Debug.Assert(IsAligned(writer, sizeof(uint)));
                    if (_pointers.TryGetValue(v, out var pointers))
                    {
                        pointers.Add(writer.Position);
                    }
                    else
                    {
                        pointers = new List<long>() { writer.Position };
                        _pointers.Add(v, pointers);
                    }

                    writer.Write(v.Id);
                }


                internal void ReplaceReference(IBinaryStreamWriter writer, StreamReference oldRef, StreamReference newRef)
                {
                    if (Reference == oldRef)
                        Reference = newRef;

                    if (!_pointers.Remove(oldRef, out var oldOffsets)) return;

                    // Modify data
                    var currentPos = (writer.Position);
                    foreach (var o in oldOffsets)
                    {
                        writer.Seek(o);
                        writer.Write(0);
                    }

                    writer.Seek(currentPos);

                    // Update pointer and offsets
                    if (_pointers.TryGetValue(newRef, out var newOffsets))
                    {
                        foreach (var o in oldOffsets)
                            newOffsets.Add(o);
                    }
                    else
                    {
                        _pointers.Add(newRef, oldOffsets);
                    }
                }

                internal void WriteTo(IBinaryDataStream data, IBinaryStreamWriter outData, IDictionary<StreamReference, long> dataOffsetDataBase)
                {
                    StreamUtils.Align(outData, Alignment);

                    var currentPos = data.Position;
                    foreach (var k in _pointers)
                    {
                        if (!dataOffsetDataBase.TryGetValue(k.Key, out var outDataOffset)) continue;

                        foreach (var o in k.Value)
                        {
                            // Seek to the position that has the 'StreamReference'
                            data.Seek(o);

                            // Write the relative (signed) offset
                            var offset = (outDataOffset - o);

                            // Assert when the offset is out of bounds (-2GB < offset < 2GB)
                            Debug.Assert(offset is >= int.MinValue and <= int.MaxValue);
                            data.Write(offset);
                        }
                    }

                    data.Seek(currentPos);

                    // Write the data of this block to the output
                    var length = Size;
                    var buffer = new byte[4096];
                    while (length > 0)
                    {
                        var read = data.Read(buffer, 0, 4096);
                        outData.Write(buffer, 0, read);
                        length -= read;
                    }
                }
            }

            #endregion

            #region Methods

            public StreamReference NewBlock(int alignment, int size)
            {
                var i = _dataBlocks.Count - 1;
                var pdb = _dataBlocks[i];
                var cdb = new DataBlock()
                {
                    Alignment = alignment,
                    Offset = CMath.Align32(pdb.Offset + pdb.Size, alignment),
                    Size = size,
                    Reference = StreamReference.NewReference
                };

                _dataBlocks.Add(cdb);
                _referenceToBlock.Add(cdb.Reference, i + 1);
                return cdb.Reference;
            }

            private long OffsetOf(StreamReference r)
            {
                if (_referenceToBlock.TryGetValue(r, out var index))
                {
                    return _dataBlocks[index].Offset;
                }

                return -1;
            }

            public void Align(int align)
            {
                var offset = _dataWriter.Position;
                if (CMath.TryAlign(offset, align, out var alignment)) return;
                _dataWriter.Position = alignment;
            }

            public bool OpenBlock(StreamReference r)
            {
                if (!_referenceToBlock.TryGetValue(r, out var index)) return false;

                _current = index;

                // See if we have to 'grow' the memory stream
                var db = _dataBlocks[index];
                if (db.Offset + db.Size > _memoryStream.Length)
                {
                    _dataWriter.Length = db.Offset + db.Size;
                }

                // Set the stream position to the start of the block
                _dataWriter.Position = db.Offset;

                return true;
            }

            public void CloseBlock()
            {
                Debug.Assert(_current != -1);

                _dataBlocks[_current].End(_dataWriter);

                // Check if the position is within the bounds of this block
                Debug.Assert(_dataWriter.Position >= _dataBlocks[_current].Offset && _dataWriter.Position <= (_dataBlocks[_current].Offset + _dataBlocks[_current].Size));

                _current = -1;
            }

            public void Write(float v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(double v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(sbyte v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(short v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(int v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(long v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(byte v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(ushort v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(uint v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(ulong v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(byte[] data)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, data, 0, data.Length);
            }

            public void Write(byte[] data, int index, int count)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, data, index, count);
            }

            public void Write(string str)
            {
                Debug.Assert(_current != -1);
                var idx = _stringTable.Add(str);
                var len = _stringTable.LengthOfByIndex(idx);
                var reference = _stringTable.ReferenceOfByIndex(idx);
                Write(reference, len);
            }

            public void Write(StreamReference v)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
            }

            public void Write(StreamReference v, long length)
            {
                Debug.Assert(_current != -1);
                _dataBlocks[_current].Write(_dataWriter, v);
                _dataBlocks[_current].Write(_dataWriter, length);
            }

            public void Finalize(IBinaryStreamWriter dataWriter)
            {
                // Dictionary for mapping a Reference to a DataBlock
                var finalDataDataBase = new Dictionary<StreamReference, DataBlock>(_dataBlocks.Count);
                foreach (var d in _dataBlocks)
                    finalDataDataBase.Add(d.Reference, d);

                // For all blocks:
                // Collapse identical blocks identified by hash, and when a collapse has occurred we have
                // to re-iterate again since a collapse changes the hash of a data block.

                var memoryBytes = _memoryStream.ToArray();
                var memoryStream = new BinaryMemoryBlock();
                memoryStream.Setup(memoryBytes, 0, memoryBytes.Length);

                var duplicateDataBase = new Dictionary<StreamReference, List<StreamReference>>();
                var dataHashDataBase = new Dictionary<Hash160, StreamReference>();

                while (true)
                {
                    duplicateDataBase.Clear();
                    dataHashDataBase.Clear();

                    foreach (var d in _dataBlocks)
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

                    foreach (var (sr, refs) in duplicateDataBase)
                    {
                        foreach (var r in refs)
                        {
                            foreach (var d in _dataBlocks)
                            {
                                d.ReplaceReference(memoryStream, r, sr);
                            }
                        }
                    }

                    // Did we find any duplicates, if so then we also replaced references
                    // and by doing so hashes have changed.
                    // Some blocks now might have an identical hash due to this.
                    if (duplicateDataBase.Count == 0) break;
                }

                // Resolve block references again
                var dataOffsetDataBase = new Dictionary<StreamReference, long>();

                // Write the string table first to the output and for each string remember the stream offset
                _stringTable.Write(dataWriter, dataOffsetDataBase);

                var offset = dataWriter.Position;
                foreach (var (sr, db) in finalDataDataBase)
                {
                    offset = CMath.Align(offset, db.Alignment);
                    dataOffsetDataBase.Add(sr, offset);
                    offset += db.Size;
                }

                // Dump all blocks to outData
                // Dump all reallocation info to outReallocationTable
                // Patch the location of every reference in the memory stream!
                foreach (var (_, db) in finalDataDataBase)
                {
                    db.WriteTo(memoryStream, dataWriter, dataOffsetDataBase);
                }
            }

            #endregion
        }
    }
}
