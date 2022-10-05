using System;

namespace GameData
{
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
