using System.Reflection;
using TextStreamWriter = System.IO.StreamWriter;
using GameCore;

namespace GameData
{
    namespace MetaCode
    {
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
                var alignment = mt.Alignment;
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
    }
}
