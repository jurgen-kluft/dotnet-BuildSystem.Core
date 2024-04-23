using System;
using System.Diagnostics.Contracts;

namespace GameData
{
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, int firstIndex, int secondIndex)
        {
            if (firstIndex == secondIndex)
                return;
            Contract.Requires(list != null);
            Contract.Requires(firstIndex >= 0 && firstIndex < list.Count);
            Contract.Requires(secondIndex >= 0 && secondIndex < list.Count);
            (list[firstIndex], list[secondIndex]) = (list[secondIndex], list[firstIndex]);
        }
    }

    public static class IntegerExtensions
    {
        public static bool IsEven(this Int32 v)
        {
            return (v & 1) == 0;
        }

        public static bool IsOdd(this Int32 v)
        {
            return (v & 1) == 1;
        }

        public static Int64 AlignUp(this Int64 v, int alignment)
        {
            return (v + alignment - 1) & ~(alignment - 1);
        }

        public static Int64 AlignDown(this Int64 v, int alignment)
        {
            return v & ~(alignment - 1);
        }

        public static Int32 Lower32(this Int64 v)
        {
            return (Int32)(v);
        }

        public static Int32 Upper32(this Int64 v)
        {
            return (Int32)(v >> 32);
        }
    }
}
