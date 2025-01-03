using System.Globalization;
using GameCore;

namespace DataBuildSystem
{
    public class BuildSystemDefaultConfig : IBuildSystemConfig
    {
        public EPlatform Platform => EPlatform.Win64;
        public string Name => "Game";
        public bool LittleEndian => true;
        public bool EnumIsInt32 => false;
        public int SizeOfBool => 1;
    }

    public static class BuildSystemConfig
    {
        private static IBuildSystemConfig s_config = new BuildSystemDefaultConfig();
        public static string Name { get; private set; }
        public static EPlatform Platform { get; private set; } = EPlatform.Win64;
        public static EPlatform Target { get; private set; } = EPlatform.Win64;
        public static ETerritory Territory { get; private set; } = ETerritory.Europe;
        public static bool EnumIsInt32 => s_config.EnumIsInt32;
        public static int SizeOfBool => s_config.SizeOfBool;
        public static string BasePath { get; private set;}
        public static string SrcPath { get; private set;}
        public static string GddPath { get; private set;}
        public static string SubPath { get; private set;}
        public static string DstPath { get; private set;}
        public static string PubPath { get; private set;}
        public static string ToolPath{ get; private set;}

        public static bool FolderFilter(string folder)
        {
            if (folder.StartsWith("bin.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.StartsWith("publish.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.EndsWith(".git", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.EndsWith(".svn", true, CultureInfo.InvariantCulture))
                return true;
            if (string.Compare(folder, Platform.ToString(), true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        public static bool FileFilter(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            var platformStr = Path.GetExtension(filename).TrimStart('.');

            // Name.%PLATFORM%.cs
            var platform = EnumFromString(platformStr, Platform);
            if (platform != Platform)
                return true;

            return false;
        }

        public static bool Init(string name, string platform, string target, string territory, string basePath, string srcPath, string gddPath, string subPath, string dstPath, string pubPath, string toolPath)
        {
            Console.WriteLine("Name: " + name);
            Console.WriteLine("Platform: " + platform);
            Console.WriteLine("Target: " + target);
            Console.WriteLine("Territory: " + territory);
            Console.WriteLine("Base Path: " + basePath);
            Console.WriteLine("Src Path: " + srcPath);
            Console.WriteLine("Gdd Path: " + gddPath);
            Console.WriteLine("Sub Path: " + subPath);
            Console.WriteLine("Dst Path: " + dstPath);
            Console.WriteLine("Tool Path: " + toolPath);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(territory))
                return false;
            if (string.IsNullOrEmpty(srcPath))
                return false;
            if (string.IsNullOrEmpty(gddPath))
                return false;
            if (string.IsNullOrEmpty(dstPath))
                return false;
            if (string.IsNullOrEmpty(pubPath))
                return false;
            if (string.IsNullOrEmpty(toolPath))
                return false;

            if (string.IsNullOrEmpty(target))
                target = platform;


            GameCore.Environment.AddVariable("Name", name);
            GameCore.Environment.AddVariable("Platform", platform);
            GameCore.Environment.AddVariable("Target", target);
            GameCore.Environment.AddVariable("Territory", territory);
            GameCore.Environment.AddVariable("BasePath", GameCore.Environment.ExpandVariables(basePath));

            Platform = EnumFromString(platform, EPlatform.Win64);
            Target = EnumFromString(target, EPlatform.Win64);
            Territory = EnumFromString(territory, ETerritory.USA);

            Name = name;
            BasePath = GameCore.Environment.ExpandVariables(basePath);
            SrcPath = GameCore.Environment.ExpandVariables(srcPath);
            GddPath = GameCore.Environment.ExpandVariables(gddPath);
            SubPath = GameCore.Environment.ExpandVariables(string.IsNullOrEmpty(subPath) ? string.Empty : subPath);
            DstPath = GameCore.Environment.ExpandVariables(dstPath);
            PubPath = GameCore.Environment.ExpandVariables(pubPath);
            ToolPath = GameCore.Environment.ExpandVariables(toolPath);

            return true;
        }

        public static void Init(IBuildSystemConfig config)
        {
            s_config = config ?? new BuildSystemDefaultConfig();
        }

        private static T EnumFromString<T>(string @string, T @default)
        {
            var names = Enum.GetNames(typeof(T));
            var name = string.Empty;
            foreach (var p in names)
            {
                if (string.Compare(p, @string, StringComparison.OrdinalIgnoreCase) != 0) continue;
                name = p;
                break;
            }
            if (string.IsNullOrEmpty(name))
                return @default;

            return (T)Enum.Parse(typeof(T), name);
        }
    }
}
