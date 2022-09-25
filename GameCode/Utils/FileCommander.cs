using System;
using System.Collections.Generic;
using System.IO;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    public static class FileCommander
    {
        public static bool createDirectoryOnDisk(string inPath)
        {
            try
            {
                string path = inPath;
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (!dirInfo.Exists)
                    dirInfo.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool createDirectoryOnDisk(string basePath, string filename)
        {
            try
            {
                string f = Path.Join(basePath, filename);
                DirUtils.Create(f);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool copyFromTo(string srcPath, string srcFilename, string dstPath, string dstFilename)
        {
            try
            {
                FileInfo srcFileInfo = new FileInfo(Path.Combine(srcPath,srcFilename));
                if (!srcFileInfo.Exists)
                {
                    Console.WriteLine("Error: Cannot copy non existing file {0}.", srcFilename);
                    return false;
                }

                FileInfo dstFileInfo = new FileInfo(Path.Combine(dstPath, dstFilename));

                // Directory exists at destination ?
                DirectoryInfo dstDirInfo = new DirectoryInfo(Path.GetDirectoryName(dstFileInfo.FullName));
                if (!dstDirInfo.Exists)
                    dstDirInfo.Create();

                File.Copy(srcFileInfo.FullName, dstFileInfo.FullName, true);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool copy(string srcPath, string dstPath, List<string> dstFilenames)
        {
            bool result = true;
            foreach(string filename in dstFilenames)
                if (!copyFromTo(srcPath, filename, dstPath, filename))
                    result = false;
            return result;
        }

    }
}
