using System;

namespace GameCore
{
    public static class FileSize
    {
        public static int Kb(int n)
        {
            return n * 1024;
        }
        public static int Mb(int n)
        {
            return Kb(n) * 1024;
        }
    }
}
