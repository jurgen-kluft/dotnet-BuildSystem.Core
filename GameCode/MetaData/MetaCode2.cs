using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using DataBuildSystem;
using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
        public interface IMemberFactory2
        {
            int NewBoolMember(bool content, string memberName);
            int NewInt8Member(sbyte content, string memberName);
            int NewUInt8Member(byte content, string memberName);
            int NewInt16Member(short content, string memberName);
            int NewUInt16Member(ushort content, string memberName);
            int NewInt32Member(int content, string memberName);
            int NewUInt32Member(uint content, string memberName);
            int NewInt64Member(long content, string memberName);
            int NewUInt64Member(UInt64 content, string memberName);
            int NewFloatMember(float content, string memberName);
            int NewDoubleMember(double content, string memberName);
            int NewStringMember(string content, string memberName);
            int NewEnumMember(Type type, object content, string memberName);
            int NewArrayMember(Type type, string memberName);
            int NewDictionaryMember(Type type, string memberName);
            int NewIStructMember(Type type, object content, string memberName);
            int NewStructMember(Type type, object content, string memberName);
            int NewClassMember(Type type, object content, string memberName);
        }

        public enum EMetaType : int
        {
            Unknown = 0,
            Bool = (1 << SizeShift) | 1,
            BitSet = (32 << SizeShift) | 2,
            Int8 = (8 << SizeShift) | 3,
            UInt8 = (8 << SizeShift) | 4,
            Int16 = (16 << SizeShift) | 5,
            UInt16 = (16 << SizeShift) | 6,
            Int32 = (32 << SizeShift) | 7,
            UInt32 = (32 << SizeShift) | 8,
            Int64 = (64 << SizeShift) | 9,
            UInt64 = (64 << SizeShift) | 10,
            Float = (32 << SizeShift) | 11,
            Double = (64 << SizeShift) | 12,
            String = (64 << SizeShift) | 13,
            Enum = (32 << SizeShift) | 14,
            Class = (64 << SizeShift) | 15, // Reference or Value Type
            Array = (64 << SizeShift) | 16, // Array (offset + size)
            Dictionary = (64 << SizeShift) | 17, // Dictionary (offset + size)
            Count = 18,
            Known = 0x8000, // For Class/Struct, the class exists in the code base
            ValueType = 0x4000, // For Classes, the class is marked as a value type, very likely was a struct
            InPlace = 0x2000, // For Arrays and Dictionaries, the elements are written in place as values not as references
            TypeMask = 0x7F0000FF,
            IndexMask = 0x000000FF,
            SizeMask = 0x7F000000,
            SizeShift = 24,
            AlignmentMask = 0x00FF0000,
            AlignmentShift = 16
        }

        public static class MetaTypeExtensions
        {
            public static int GetSize(this EMetaType type)
            {
                return (int)(type & EMetaType.SizeMask) >> (int)EMetaType.SizeShift;
            }
            public static bool IsKnown(this EMetaType type)
            {
                return (type & EMetaType.Known) != 0;
            }
            public static bool IsValueType(this EMetaType type)
            {
                return (type & EMetaType.ValueType) != 0;
            }
        }

        // MetaCode is a thought experiment on how to optimize the building of the classes and members that we encounter
        // when we go through the C# code using reflection. Currently we are building this purely using custom C# code
        // and we could do this a lot more efficient.

        public class MetaCode
        {
            public List<bool> ValuesBool = new() { false, true };
            public readonly List<UInt8> ValuesU8 = new();
            public readonly List<Int8> ValuesS8 = new();
            public readonly List<ushort> ValuesU16 = new();
            public readonly List<short> ValuesS16 = new();
            public readonly List<uint> ValuesU32 = new();
            public readonly List<int> ValuesS32 = new();
            public readonly List<ulong> ValuesU64 = new();
            public readonly List<long> ValuesS64 = new();
            public readonly List<float> ValuesF32 = new();
            public readonly List<double> ValuesF64 = new();
            public readonly List<string> ValuesString = new();
            public readonly List<IStruct> ValuesIStruct = new();

            public readonly List<EMetaType> MembersType = new();
            public readonly List<int> MembersName = new();
            public readonly List<int> MembersStartIndex = new(); // If we are a Struct the members start here
            public readonly List<int> MembersSubIndex = new(); // If we are a class, struct or enum, this is the index in their respective lists
            public readonly List<int> MembersCount = new(); // If we are an Array/List/Dict/Struct we hold many elements/members

            public readonly List<int> Classes = new(); // Indices into ValuesMember of all the members that are classes
            public readonly List<object> ObjectsClass = new(); // This is to know the type of the class
            public readonly List<Enum> ObjectsEnum = new(); // This is to generate C++ code for the enums

            public int AddMember(EMetaType type, int name, int startIndex, int subIndex, int count)
            {
                var index = MembersType.Count;
                MembersType.Add(type);
                MembersName.Add(name);
                MembersStartIndex.Add(startIndex);
                MembersSubIndex.Add(subIndex);
                MembersCount.Add(count);
                return index;
            }

            public void SetMember(int memberIndex, EMetaType type, int startIndex, int subIndex, int count)
            {
                MembersType[memberIndex] = type;
                MembersStartIndex[memberIndex] = startIndex;
                MembersSubIndex[memberIndex] = subIndex;
                MembersCount[memberIndex] = count;
            }

            public void SetMemberName(int memberIndex, string name)
            {
                var index = ValuesString.Count;
                ValuesString.Add(name);
                MembersName[memberIndex] = index;
            }

            private void DuplicateMember(int memberIndex)
            {
                var type = MembersType[memberIndex];
                var name = MembersName[memberIndex];
                var startIndex = MembersStartIndex[memberIndex];
                var subIndex = MembersSubIndex[memberIndex];
                var count = MembersCount[memberIndex];
                AddMember(type, name, startIndex, subIndex, count);

            }

            private void SwapMembers(int i, int j)
            {
                MembersType.Swap(i, j);
                MembersName.Swap(i, j);
                MembersStartIndex.Swap(i, j);
                MembersSubIndex.Swap(i, j);
                MembersCount.Swap(i, j);
            }

            public void UpdateStartIndexAndCount(int memberIndex, int startIndex, int count)
            {
                MembersStartIndex[memberIndex] = startIndex;
                MembersCount[memberIndex] = count;
            }

            private void SetMemberAlignment(int memberIndex, int alignment)
            {
                var type = (int)MembersType[memberIndex];
                type &= ~(int)EMetaType.AlignmentMask;
                type |= ((int)alignment << (int)EMetaType.AlignmentShift) & (int)EMetaType.AlignmentMask;
                MembersType[memberIndex] = (EMetaType)type;
            }

            public class SortMembersBySize : IComparer<EMetaType>
            {
                public int Compare(EMetaType x, EMetaType y)
                {
                    var xs = (uint)(x & EMetaType.SizeMask);
                    var ys = (uint)(y & EMetaType.SizeMask);
                    return xs.CompareTo(ys);
                }
            }

            public void CombineBooleans(int classIndex)
            {
                // Swap all boolean members to the end of the member list
                var end = MembersStartIndex[classIndex] + MembersCount[classIndex];
                for (var mi = MembersStartIndex[classIndex]; mi < end;)
                {
                    var cmt = MembersType[mi];
                    if (cmt == EMetaType.Bool)
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
                var n = MembersCount[classIndex] - (end - MembersStartIndex[classIndex]);
                var i = 0;
                while (n > 0)
                {
                    // Duplicate all the boolean members to make room for this bitset member but also to have the bitset member be able to
                    // have its own range of MetaMembers.
                    var numBits = (n <= 32) ? n : 32;
                    var startIndex = MembersCount.Count;
                    var mi = end + (i * 32);
                    var mie = mi + numBits;
                    while (mi < mie)
                    {
                        DuplicateMember(mi++);
                    }

                    SetMember(end + i, EMetaType.BitSet, startIndex, 0, numBits);

                    i += 1;
                    n -= 32;
                }

                // Update the member count of this class, remove the number of booleans and add the number of bitsets
                n = MembersCount[classIndex] - (end - MembersStartIndex[classIndex]);
                MembersCount[classIndex] = MembersCount[classIndex] - n + i;
            }

            public void DetermineAlignment(int classIndex)
            {
                // We only need to look at the first member of the class to determine the alignment
                var fm = MembersStartIndex[classIndex];
                var alignment = ((int)MembersType[fm] & (int)EMetaType.AlignmentMask) >> (int)EMetaType.AlignmentShift;
                SetMemberAlignment(classIndex, alignment);
            }

            public void SortMembers(int classIndex, IComparer<EMetaType> comparer)
            {
                // Manual Bubble Sort, sort members by size/alignment, descending
                var si = MembersStartIndex[classIndex];
                var count = MembersCount[classIndex];
                var end = si + count;
                for (var i = si; i < end; ++i)
                {
                    var swapped = false;
                    for (var j = si; j < end - 1; ++j)
                    {
                        if (comparer.Compare(MembersType[j], MembersType[j + 1]) <= 0) continue;
                        SwapMembers(j, j + 1);
                        swapped = true;
                    }

                    if (!swapped) break;
                }
            }

            public void Write(StringTable stringTable, CppDataStream dataStream)
            {
                // Write all of the instantiated code as binary data matching the class layout

                // Every reference type (class, array, dictionary) will be written as a pointer to the data

                // We need to keep track of where the data is written so we can write the pointers to the data, this
                // is done by keeping StreamReferences for each reference type.
            }
        }

        #region C++ Code writer

        public class CppCodeWriter
        {
            private readonly MetaCode _metaCode;

            private static readonly string[] sMemberTypeNames = new string[(int)EMetaType.Count]
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
                "raw_string_t",
                "raw_enum_t",
                "class",
                "raw_array_t",
                "raw_dict_t"
            };

            private static readonly string[] sReturnTypeNames = new string[(int)EMetaType.Count]
            {
                "unknown",
                "bool",
                "bitset",
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
                "class",
                "array_t",
                "dict_t"
            };

            private static string GetMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var typeIndex = (int)(mt & EMetaType.IndexMask);
                return sMemberTypeNames[typeIndex];
            }

            private static string GetEnumMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msi = metaCode.MembersSubIndex[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var enumInstance = metaCode.ObjectsEnum[msi];
                var enumTypeName = enumInstance.GetType().Name;
                var memberTypeString = sMemberTypeNames[(int)(mt & EMetaType.IndexMask)];
                return string.Concat(memberTypeString, "<", enumTypeName, ",", "u32", ">");
            }

            private static string GetClassMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var msi = metaCode.MembersSubIndex[memberIndex];

                string className;
                if (mt.IsKnown())
                {
                    className = metaCode.ValuesIStruct[msi].StructName;
                }
                else
                {
                    className = metaCode.ObjectsClass[msi].GetType().Name;
                }

                if (mt.IsValueType())
                    return className;
                return className + "*";
            }

            private static string GetArrayMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var fet = metaCode.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[(int)(fet & EMetaType.IndexMask)](msi, metaCode, option); // recursive call
                var memberTypeString = sMemberTypeNames[(int)(mt & EMetaType.IndexMask)];
                return string.Concat(memberTypeString, "<", elementTypeString, ">");
            }

            private static string GetDictMemberTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var fk = metaCode.MembersType[msi]; // first key element
                var fv = metaCode.MembersType[msi + count]; // first value element
                var keyTypeString = sGetMemberTypeString[(int)(fk & EMetaType.IndexMask)](msi, metaCode, option); // recursive call
                var valueTypeString = sGetMemberTypeString[(int)(fv & EMetaType.IndexMask)](msi + count, metaCode, option); // recursive call
                var memberTypeString = sMemberTypeNames[(int)(mt & EMetaType.IndexMask)];
                return string.Concat(memberTypeString, "<", keyTypeString, ",", valueTypeString, ">");
            }

            [Flags]
            private enum EOption : int
            {
                None = 0,
                InPlace = 1,
            }

            private delegate string GetMemberTypeStringDelegate(int memberIndex, MetaCode metaCode, EOption option);

            private static readonly GetMemberTypeStringDelegate[] sGetMemberTypeString = new GetMemberTypeStringDelegate[(int)EMetaType.Count]
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
                GetClassMemberTypeName, // class
                GetArrayMemberTypeName,
                GetDictMemberTypeName,
            };


            private static string GetReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var mt = metaCode.MembersType[memberIndex];
                var typeIndex = (int)(mt & EMetaType.IndexMask);
                return sReturnTypeNames[typeIndex];
            }

            private static string GetEnumReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msu = metaCode.MembersSubIndex[memberIndex];
                var enumInstance = metaCode.ObjectsEnum[msu];
                return enumInstance.GetType().Name;
            }

            private static string GetClassReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msu = metaCode.MembersSubIndex[memberIndex];
                var mt = metaCode.MembersType[memberIndex];

                string className;
                if (mt.IsKnown())
                {
                    className = metaCode.ValuesIStruct[msu].StructName;
                }
                else
                {
                    className = metaCode.ObjectsClass[msu].GetType().Name;
                }

                if (!mt.IsValueType())
                    className =  className + "*";

                return className;
            }

            private static string GetArrayReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msi = metaCode.MembersStartIndex[memberIndex];
                var fet = metaCode.MembersType[msi];
                var elementTypeString = sGetMemberTypeString[(int)(fet & EMetaType.IndexMask)](msi, metaCode, option); // recursive call
                return $"raw_array_t<{elementTypeString}>";
            }

            private static string GetDictReturnTypeName(int memberIndex, MetaCode metaCode, EOption option)
            {
                var msi = metaCode.MembersStartIndex[memberIndex];
                var count = metaCode.MembersCount[memberIndex];
                var fkt = metaCode.MembersType[msi]; // first key element
                var fvt = metaCode.MembersType[msi + count]; // first value element
                var keyTypeString = sGetMemberTypeString[(int)(fkt & EMetaType.IndexMask)](msi, metaCode, option); // recursive call
                var valueTypeString = sGetMemberTypeString[(int)(fvt & EMetaType.IndexMask)](msi + count, metaCode, option); // recursive call
                return $"raw_dict_t<{keyTypeString}, {valueTypeString}>";
            }

            private delegate string GetReturnTypeStringDelegate(int memberIndex, MetaCode metaCode, EOption option);

            private static readonly GetReturnTypeStringDelegate[] sGetReturnTypeString = new GetReturnTypeStringDelegate[(int)EMetaType.Count]
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
                GetClassReturnTypeName, // class
                GetArrayReturnTypeName,
                GetDictReturnTypeName,
            };

            private delegate void WriteGetterDelegate(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option);

            private static readonly WriteGetterDelegate[] sWriteGetterDelegates = new WriteGetterDelegate[(int)EMetaType.Count]
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
                WriteClassGetter,
                WriteArrayGetter,
                WriteDictionaryGetter
            };

            private delegate void WriteMemberDelegate(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option);

            private static readonly WriteMemberDelegate[] sWriteMemberDelegates = new WriteMemberDelegate[(int)EMetaType.Count]
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
                WriteClassMember,
                WriteArrayMember,
                WriteDictionaryMember
            };

            private static void WriteBitsetGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var count = metaCode.MembersCount[memberIndex];

                // Emit a get function for each boolean that the bitset represents
                for (var i = 0; i < count; ++i)
                {
                    var booleanMemberIndex = msi + i;
                    var booleanMemberName = metaCode.MembersName[booleanMemberIndex];
                    var booleanName = metaCode.ValuesString[booleanMemberName];
                    writer.WriteLine($"\tinline bool get{booleanName}() const {{ return {memberName} & (1 << {i})) != 0; }}");
                }
            }

            private static void WriteBitsetMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                writer.WriteLine($"\tu32 m_{memberName};");
            }

            private static void WritePrimitiveGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var typeIndex = (int)(mt & EMetaType.IndexMask);
                writer.WriteLine($"\tinline {sReturnTypeNames[typeIndex]} get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WritePrimitiveMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var typeIndex = (int)(mt & EMetaType.IndexMask);
                writer.WriteLine($"\t{sMemberTypeNames[typeIndex]} m_{memberName};");
            }

            private static void WriteStringGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                writer.WriteLine($"\tinline string_t get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteEnumGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var msu = metaCode.MembersSubIndex[memberIndex];
                var enumInstance = metaCode.ObjectsEnum[msu];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                writer.WriteLine($"\tinline {enumTypeName} get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteEnumMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var msu = metaCode.MembersSubIndex[memberIndex];
                var enumInstance = metaCode.ObjectsEnum[msu];
                var enumTypeName = enumInstance.GetType().Name;
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                writer.WriteLine($"\traw_enum_t<{enumTypeName}, u32> m_{memberName};");
            }

            private static void WriteClassGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                // NOTE Some classes are not written as pointers, but as values
                var mni = metaCode.MembersName[memberIndex];
                var msu = metaCode.MembersSubIndex[memberIndex];
                var mt = metaCode.MembersType[memberIndex];
                var memberName = metaCode.ValuesString[mni];

                string className;
                if (mt.IsKnown())
                {
                    className = metaCode.ValuesIStruct[msu].StructName;
                }
                else
                {
                    className = metaCode.ObjectsClass[msu].GetType().Name;
                }

                if (!mt.IsValueType())
                    className =  className + "const *";

                writer.WriteLine($"\tinline {className} get{memberName}() const {{ return m_{memberName}; }}");
            }

            private static void WriteClassMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                // NOTE Some classes are not written as pointers, but as values
                var mni = metaCode.MembersName[memberIndex];
                var msu = metaCode.MembersSubIndex[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var mt = metaCode.MembersType[memberIndex];

                string className;
                if (mt.IsKnown())
                {
                    className = metaCode.ValuesIStruct[msu].StructName;
                }
                else
                {
                    className = metaCode.ObjectsClass[msu].GetType().Name;
                }

                if (!mt.IsValueType())
                    className =  className + "*";

                writer.WriteLine($"\t{className} m_{memberName};");
            }

            private static void WriteArrayGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var fet = metaCode.MembersType[msi];

                // figure out the element type of the array, could be a primitive, struct, class, enum or even another array
                var returnTypeString = sGetReturnTypeString[(int)(fet & EMetaType.IndexMask)](msi, metaCode, option);

                writer.WriteLine($"\tinline array_t<{returnTypeString}> get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteArrayMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var memberName = metaCode.ValuesString[mni];
                var fet = metaCode.MembersType[msi];
                var elementName = sGetMemberTypeString[(int)(fet & EMetaType.IndexMask)](msi, metaCode, option);
                writer.WriteLine($"\traw_array_t<{elementName}> m_{memberName};");
            }

            private static void WriteDictionaryGetter(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msu = metaCode.MembersSubIndex[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var memberName = metaCode.ValuesString[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode.MembersType[msi];
                var valueElement = metaCode.MembersType[msi + count];
                var keyTypeString = sGetReturnTypeString[(int)(keyElement & EMetaType.IndexMask)](msi, metaCode, option);
                var valueTypeString = sGetReturnTypeString[(int)(valueElement & EMetaType.IndexMask)](msi + count, metaCode, option);

                writer.WriteLine($"\tinline const dict_t<{keyTypeString}, {valueTypeString}>& get{memberName}() const {{ return m_{memberName}.get(); }}");
            }

            private static void WriteDictionaryMember(int memberIndex, StreamWriter writer, MetaCode metaCode, EOption option)
            {
                var mni = metaCode.MembersName[memberIndex];
                var msu = metaCode.MembersSubIndex[memberIndex];
                var msi = metaCode.MembersStartIndex[memberIndex];
                var count = metaCode.MembersCount[memberIndex];

                var memberName = metaCode.ValuesString[mni];

                // figure out the key and value type of the dictionary, could be a primitive, struct, class, enum or even array
                var keyElement = metaCode.MembersType[msi];
                var valueElement = metaCode.MembersType[msi + count];
                var keyTypeString = sGetMemberTypeString[(int)(keyElement & EMetaType.IndexMask)](msi, metaCode, option);
                var valueTypeString = sGetMemberTypeString[(int)(valueElement & EMetaType.IndexMask)](msi + count, metaCode, option);

                writer.WriteLine($"\traw_dict_t<{keyTypeString}, {valueTypeString}> m_{memberName};");
            }


            public CppCodeWriter(MetaCode metaCode)
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
                foreach (var c in _metaCode.ObjectsEnum)
                {
                    if (writtenEnums.Contains(c.GetType().Name)) continue;
                    WriteEnum(c.GetType(), writer);
                    writtenEnums.Add(c.GetType().Name);
                }

                writer.WriteLine();
            }

            private void WriteClass(int memberIndex, StreamWriter writer, EOption option)
            {
                var mni = _metaCode.MembersName[memberIndex];
                var msu = _metaCode.MembersSubIndex[memberIndex];
                var msi = _metaCode.MembersStartIndex[memberIndex];
                var count = _metaCode.MembersCount[memberIndex];
                var mci = _metaCode.Classes[msu];
                var mt = _metaCode.MembersType[memberIndex];

                string className;
                if (mt.IsKnown())
                {
                    className = _metaCode.ValuesIStruct[msu].StructName;
                }
                else
                {
                    className = _metaCode.ObjectsClass[msu].GetType().Name;
                }


                writer.WriteLine($"class {className}");
                writer.WriteLine("{");
                writer.WriteLine("public:");

                // write public member getter functions
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = _metaCode.MembersType[i];
                    sWriteGetterDelegates[(int)(mmt & EMetaType.IndexMask)](i, writer, _metaCode, option);
                }

                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
                for (var i = msi; i < msi + count; ++i)
                {
                    var mmt = _metaCode.MembersType[i];
                    sWriteMemberDelegates[(int)(mmt & EMetaType.IndexMask)](i, writer, _metaCode, EOption.None);
                }

                writer.WriteLine("};");
                writer.WriteLine();
            }

            public void WriteClasses(StreamWriter writer)
            {
                // Forward declares ?
                writer.WriteLine("// Forward declares");
                HashSet<string> writtenClasses = new();
                for (int i=0; i<_metaCode.Classes.Count; ++i)
                {
                    var cn = _metaCode.ObjectsClass[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    writer.WriteLine($"class {cn};");
                    writtenClasses.Add(cn);
                }

                writer.WriteLine();

                writtenClasses.Clear();
                for (int i=0; i<_metaCode.Classes.Count; ++i)
                {
                    var cn = _metaCode.ObjectsClass[i].GetType().Name;
                    if (writtenClasses.Contains(cn)) continue;
                    var ci = _metaCode.Classes[i];
                    WriteClass(ci, writer, EOption.None);
                    writtenClasses.Add(cn);
                }

                writer.WriteLine();
            }
        }

        #endregion

        public class MetaMemberFactory : IMemberFactory2
        {
            public MetaMemberFactory(MetaCode metaCodeCode)
            {
                _metaCode = metaCodeCode;
            }

            private readonly MetaCode _metaCode;

            private readonly Dictionary<string, int> _stringMap = new();

            // NOTE We could de-duplicate all of the types we encounter here, but we are not doing that yet
            //      For example:
            //              private readonly Dictionary<double, int> _doubleMap = new();
            //              private readonly Dictionary<float, int> _floatMap = new();

            private int RegisterString(string memberName)
            {
                if (_stringMap.TryGetValue(memberName, out var index)) return index;
                index = _metaCode.ValuesString.Count;
                _metaCode.ValuesString.Add(memberName);
                _stringMap.Add(memberName, index);
                return index;
            }

            public int NewBoolMember(bool content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Bool, RegisterString(memberName), (content) ? 1 : 0, -1, 1);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewInt8Member(sbyte content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Int8, RegisterString(memberName), _metaCode.ValuesS8.Count, -1, 1);
                _metaCode.ValuesS8.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewUInt8Member(byte content, string memberName)
            {
                _metaCode.AddMember(EMetaType.UInt8, RegisterString(memberName), _metaCode.ValuesU8.Count, -1, 1);
                _metaCode.ValuesU8.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewInt16Member(short content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Int16, RegisterString(memberName), _metaCode.ValuesS16.Count, -1, 1);
                _metaCode.ValuesS16.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewUInt16Member(ushort content, string memberName)
            {
                _metaCode.AddMember(EMetaType.UInt16, RegisterString(memberName), _metaCode.ValuesU16.Count, -1, 1);
                _metaCode.ValuesU16.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewInt32Member(int content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Int32, RegisterString(memberName), _metaCode.ValuesS32.Count, -1, 1);
                _metaCode.ValuesS32.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewUInt32Member(uint content, string memberName)
            {
                _metaCode.AddMember(EMetaType.UInt32, RegisterString(memberName), _metaCode.ValuesU32.Count, -1, 1);
                _metaCode.ValuesU32.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewInt64Member(long content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Int64, RegisterString(memberName), _metaCode.ValuesS64.Count, -1, 1);
                _metaCode.ValuesS64.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewUInt64Member(ulong content, string memberName)
            {
                _metaCode.AddMember(EMetaType.UInt64, RegisterString(memberName), _metaCode.ValuesU64.Count, -1, 1);
                _metaCode.ValuesU64.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewFloatMember(float content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Float, RegisterString(memberName), _metaCode.ValuesF32.Count, -1, 1);
                _metaCode.ValuesF32.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewDoubleMember(double content, string memberName)
            {
                _metaCode.AddMember(EMetaType.Double, RegisterString(memberName), _metaCode.ValuesF64.Count, -1, 1);
                _metaCode.ValuesF64.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewStringMember(string content, string memberName)
            {
                _metaCode.AddMember(EMetaType.String, RegisterString(memberName), _metaCode.ValuesString.Count, -1, 1);
                _metaCode.ValuesString.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewEnumMember(Type type, object content, string memberName)
            {
                if (content is not Enum e)
                    return -1;

                _metaCode.AddMember(EMetaType.Enum, RegisterString(memberName), _metaCode.ObjectsEnum.Count, _metaCode.ObjectsEnum.Count, 1);
                _metaCode.ObjectsEnum.Add(e); // Also make sure to register this type for the C++ code generation
                return _metaCode.MembersType.Count - 1;
            }

            public int NewArrayMember(Type type, string memberName)
            {
                _metaCode.AddMember(EMetaType.Array, RegisterString(memberName), -1, -1, 0);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewDictionaryMember(Type type, string memberName)
            {
                _metaCode.AddMember(EMetaType.Dictionary, RegisterString(memberName), -1, -1, 0);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewIStructMember(Type type, object content, string memberName)
            {
                // An IStruct is either a value type or a reference type, the C++ code is already 'known' in the codebase.
                // Also the data of the 'members' of this struct are written by using the IStruct interface, so we do not
                // need to 'parse' the members of this struct, we will just write the data as is.
                if (content is not IStruct o)
                    return -1;

                var metaType = (EMetaType)((int)EMetaType.Class | (int)EMetaType.ValueType | (int)EMetaType.Known);
                if (!o.StructIsValueType)
                    metaType &= ~EMetaType.ValueType;

                _metaCode.AddMember(metaType, RegisterString(memberName), _metaCode.ValuesIStruct.Count, _metaCode.ValuesIStruct.Count, 1);
                _metaCode.ValuesIStruct.Add(o);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewStructMember(Type type, object content, string memberName)
            {
                _metaCode.Classes.Add(_metaCode.MembersType.Count);
                var metaType = (EMetaType)((int)EMetaType.Class | (int)EMetaType.ValueType);
                _metaCode.AddMember(metaType, RegisterString(memberName), -1, _metaCode.ObjectsClass.Count, 0);
                _metaCode.ObjectsClass.Add(content);
                return _metaCode.MembersType.Count - 1;
            }

            public int NewClassMember(Type type, object content, string memberName)
            {
                _metaCode.Classes.Add(_metaCode.MembersType.Count);
                _metaCode.AddMember(EMetaType.Class, RegisterString(memberName), -1, _metaCode.ObjectsClass.Count, 0);
                _metaCode.ObjectsClass.Add(content);
                return _metaCode.MembersType.Count - 1;
            }
        }
    }
}
