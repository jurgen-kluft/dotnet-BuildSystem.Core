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

        public static ReadOnlyMemory<char> RelativePath(this ReadOnlyMemory<char> path,int relativePathStart)
        {
            return path.Slice(relativePathStart, path.Length - relativePathStart);
        }

        public static IEnumerable<ReadOnlyMemory<char>> EnumerateFiles(string path,string pattern,SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, pattern, searchOption).Select(name => new ReadOnlyMemory<char>(name.ToArray()));
        }

        public static void EnumeratePathExample()
        {
            var rootPath = @"C:\foo\bar";
            foreach (var path in EnumerateFiles(rootPath, "*.txt", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine(path.RelativePath(rootPath.Length).Span.ToString());
            }
        }
		public static IEnumerable<ReadOnlyMemory<char>> EnumerateDirs(string path, string pattern, SearchOption searchOption)
		{
			return Directory.EnumerateDirectories(path, pattern, searchOption).Select(name => new ReadOnlyMemory<char>(name.ToArray()));
		}

		public static void DuplicateFolderStructure(string srcPath, string dstPath)
		{
			List<string> dirs = new List<string>();
			foreach (var path in EnumerateDirs(srcPath, "**", SearchOption.AllDirectories))
			{
				var srcRelativePath = path.Slice(srcPath.Length);
				string dstDir = Path.Join(dstPath, srcRelativePath.ToString());
				dirs.Add(dstDir);
			}
			CreateDirectories(dirs);
		}

		public static void CreateDirectories(List<string> directories)
		{
			// Create the directories first, smartly
			int max_depth = 0;
			Dictionary<int, HashSet<string>> DirectoriesPerDepth = new ();
			foreach (var path in directories)
			{
				int depth = 0;
				
				ReadOnlySpan<char> dir = path.AsSpan();
				while (!dir.IsEmpty)
				{
					int seperator_pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
					if (seperator_pos == -1)
						break;
					++depth;
					dir = dir.Slice(0, seperator_pos);
				}

				if (!DirectoriesPerDepth.TryGetValue(depth, out HashSet<string> dirs))
				{
					if (depth > max_depth)
						max_depth = depth;
					dirs = new ();
					DirectoriesPerDepth.Add(depth, dirs);
				}
				dirs.Add(path);
			}

			HashSet<string> CreatedDirectories = new ();
			int actual_created_directories = 0;
			for (int depth = max_depth; depth >= 0; --depth)
			{
				if (DirectoriesPerDepth.TryGetValue(depth, out HashSet<string> dirs))
				{
					foreach (string path in dirs)
					{
						if (!CreatedDirectories.Contains(path))
						{
							Directory.CreateDirectory(path);
							++actual_created_directories;

							ReadOnlySpan<char> dir = path.AsSpan();
							while (!dir.IsEmpty)
							{
								int seperator_pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
								if (seperator_pos == -1)
									break;
								dir = dir.Slice(0, seperator_pos);
								if (!CreatedDirectories.Add(dir.ToString()))
									break;
							}
						}
					}
				}
			}
		}
	}
}
