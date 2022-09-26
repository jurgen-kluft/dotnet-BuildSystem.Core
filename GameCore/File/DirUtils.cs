using System;
using System.IO;

//using System.Memory;

namespace GameCore
{
    public static class DirUtils
    {
        public static bool Create(string dir)
        {
            Directory.CreateDirectory(dir.ToString());
            return true;
        }

        public static ReadOnlyMemory<char> RelativePath(
            this ReadOnlyMemory<char> path,
            int relativePathStart
        )
        {
            return path.Slice(relativePathStart, path.Length - relativePathStart);
        }

        public static IEnumerable<ReadOnlyMemory<char>> EnumerateFiles(
            string path,
            string pattern,
            SearchOption searchOption
        )
        {
            return Directory
                .EnumerateFiles(path, pattern, searchOption)
                .Select(name => new ReadOnlyMemory<char>(name.ToArray()));
        }

        public static void EnumeratePathExample()
        {
            var rootPath = @"C:\foo\bar";
            foreach (var path in EnumerateFiles(rootPath, "*.txt", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine(path.RelativePath(rootPath.Length).Span.ToString());
            }
        }
    }
}
