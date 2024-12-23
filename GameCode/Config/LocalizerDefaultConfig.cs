using System.Globalization;
using GameCore;

namespace DataBuildSystem
{

    public class LocalizerDefaultConfig : ILocalizerConfig
    {
        public EPlatform Platform => EPlatform.Win64;

        public string SubDepFileExtension => ".sdep";

        public string MainDepFileExtension => ".dep";

        public string SubLocFileExtension => ".sloc";

        public string MainLocFileExtension => ".loc";
    }

    public static class LocalizerConfig
    {
        private static ILocalizerConfig s_config = new LocalizerDefaultConfig();

        public static string Name { get; private set; } = string.Empty;

        public static EPlatform Platform { get; private set; } = EPlatform.Win64;

        public static Filename ConfigFilename { get; private set; } = Filename.Empty;

        public static string PlatformName => Platform.ToString();

        public static EPlatform Target { get; private set; } = EPlatform.Win64;

        public static string TargetName => Target.ToString();

        public static ETerritory Territory { get; private set; } = ETerritory.Europe;

        public static string TerritoryName => Territory.ToString();

        public static Dirname SrcPath { get; private set; }

        public static string Excel0 { get; private set; } = string.Empty;

        public static Dirname DstPath { get; private set; }

        public static Dirname SubPath { get; private set; }

        public static Dirname DepPath { get; private set; }

        public static Dirname PublishPath { get; private set; }

        public static Dirname ToolPath { get; private set; }

        public static string SubDepFileExtension => s_config.SubDepFileExtension;

        public static string MainDepFileExtension => s_config.MainDepFileExtension;

        public static string SubLocFileExtension => s_config.SubLocFileExtension;

        public static string MainLocFileExtension => s_config.MainLocFileExtension;

        public static bool FolderFilter(string folder)
        {
            if (folder.StartsWith("bin.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.StartsWith("publish.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.EndsWith(".svn", true, CultureInfo.InvariantCulture))
                return true;
            if (string.Compare(folder, PlatformName, true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        public static bool FileFilter(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            var platformStr = Path.GetExtension(filename).TrimStart('.');

            // Name.%PLATFORM%.cs
            var platform = FromString(platformStr, Platform);
            return platform != Platform;
        }

        public static bool Init(string name, string platform, string territory, string configFilename, string srcPath, string excel0, string dstPath, string depPath, string publishPath, string toolPath)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(territory) || string.IsNullOrEmpty(configFilename) || string.IsNullOrEmpty(srcPath) || string.IsNullOrEmpty(excel0) || string.IsNullOrEmpty(dstPath) ||
                string.IsNullOrEmpty(depPath))
                return false;

            GameCore.Environment.AddVariable("NAME", name);
            GameCore.Environment.AddVariable("PLATFORM", platform);
            GameCore.Environment.AddVariable("TARGET", platform);
            GameCore.Environment.AddVariable("TERRITORY", territory);
            GameCore.Environment.AddVariable("SRCPATH", GameCore.Environment.ExpandVariables(srcPath));

            Name = name;
            Platform = FromString(platform, EPlatform.Win64);
            Target = FromString(platform, EPlatform.Win32);
            Territory = FromString(territory, ETerritory.Europe);

            ConfigFilename = new Filename(GameCore.Environment.ExpandVariables(configFilename));

            Excel0 = new Filename(GameCore.Environment.ExpandVariables(excel0));

            SrcPath = new Dirname(GameCore.Environment.ExpandVariables(srcPath));
            SubPath = Dirname.Empty;
            DstPath = new Dirname(GameCore.Environment.ExpandVariables(dstPath));
            DepPath = new Dirname(GameCore.Environment.ExpandVariables(depPath));
            ToolPath = new Dirname(GameCore.Environment.ExpandVariables(toolPath));
            PublishPath = new Dirname(GameCore.Environment.ExpandVariables(publishPath));

            return true;
        }

        public static void SetConfig(ILocalizerConfig config)
        {
            if (config != null)
                s_config = config;
        }

        public static T FromString<T>(string @string, T @default)
        {
            var names = Enum.GetNames(typeof(T));
            var name = string.Empty;
            foreach (var p in names)
            {
                if (string.Compare(p, @string, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    name = p;
                    break;
                }
            }

            if (string.IsNullOrEmpty(name))
                return @default;

            return (T)Enum.Parse(typeof(T), name);
        }
    }
}
