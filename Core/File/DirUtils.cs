using System;
using System.IO;

namespace Core
{
    public static class DirUtils
    {
        public static bool Create(Dirname dir)
        {
            if (!dir.HasDevice)
                dir = dir.MakeAbsolute();

            Directory.CreateDirectory(dir.ToString());

            return true;
        }
    }
}
