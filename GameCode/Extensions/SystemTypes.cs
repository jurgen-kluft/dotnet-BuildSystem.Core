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
        public static bool IsEven(this int v)
        {
            return (v & 1) == 0;
        }

        public static bool IsOdd(this int v)
        {
            return (v & 1) == 1;
        }

        public static long AlignUp(this long v, int alignment)
        {
            return (v + alignment - 1) & ~(alignment - 1);
        }

        public static long AlignDown(this long v, int alignment)
        {
            return v & ~(alignment - 1);
        }

        public static int Lower32(this long v)
        {
            return (int)(v);
        }

        public static int Upper32(this long v)
        {
            return (int)(v >> 32);
        }
    }
}
