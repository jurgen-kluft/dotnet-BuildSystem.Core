using System;
using System.IO;

namespace GameCore
{
    public static class DirUtils
    {
        public static bool Create(string dir)
        {
            Directory.CreateDirectory(dir.ToString());
            return true;
        }
    }
}
