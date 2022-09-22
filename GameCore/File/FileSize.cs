using System;

namespace GameCore
{
    public static class FileSize
    {
        public static Int32 Kb(int n)
        {
            return n * 1024;
        }
        public static Int32 Mb(int n)
        {
            return Kb(n) * 1024;
        }
    }
}
