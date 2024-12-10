using System.Globalization;
using GameCore;

namespace DataBuildSystem
{
    public interface IExcelCharConversion
    {
        char From { get; }
        char To { get; }
    }

    public interface IExcelSheetColumn
    {
        string Name { get; }
        int Column { get; }
        int BeginRow { get; }
        int EndRow { get; }
        byte[] AsciiSet { get; }
    }

    public interface IExcelSheetConfig
    {
        int BeginRow { get; }
        int EndRow { get; }
        IExcelSheetColumn[] Columns { get; }
    }

    public interface IBuildSystemLocalizerConfig
    {
        /// <summary>
        /// The platform this configuration is for
        /// </summary>
        EPlatform Platform { get; }

        /// <summary>
        /// Endianness of the build
        /// </summary>
        bool LittleEndian { get; }

        /// <summary>
        /// Write the .loc file strings in unicode instead of ascii
        /// </summary>
        bool Unicode { get; }

        string SubDepFileExtension { get; }
        string MainDepFileExtension { get; }
        string SubLocFileExtension { get; }
        string MainLocFileExtension { get; }
    }

    public class BuildSystemLocalizerDefaultConfig : IBuildSystemLocalizerConfig
    {
        public EPlatform Platform => EPlatform.Win64;

        public bool LittleEndian => true;

        public bool Unicode => false;

        public string SubDepFileExtension => ".sdep";

        public string MainDepFileExtension => ".dep";

        public string SubLocFileExtension => ".sloc";

        public string MainLocFileExtension => ".loc";
    }

    public static class LocalizerConfig
    {
        private static IBuildSystemLocalizerConfig _sConfig = new BuildSystemLocalizerDefaultConfig();

        public static bool PlatformPc => Platform == EPlatform.Win64;

        public static bool PlatformXboxOne => Platform == EPlatform.XboxOne;

        public static bool PlatformXboxOneX => Platform == EPlatform.XboxOneX;

        public static bool PlatformPlaystation4 => Platform == EPlatform.Playstation4;

        public static bool PlatformPlaystation4Pro => Platform == EPlatform.Playstation4Pro;

        public static bool TargetPc => Target == EPlatform.Win64;

        public static bool TargetXboxOne => Target == EPlatform.XboxOne;

        public static bool TargetXboxOneX => Target == EPlatform.XboxOneX;

        public static bool TargetPlaystation4 => Target == EPlatform.Playstation4;

        public static bool TargetPlaystation4Pro => Target == EPlatform.Playstation4Pro;

        public static string Name { get; private set; } = string.Empty;

        public static bool LittleEndian => _sConfig.LittleEndian;

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

        public static string SubDepFileExtension => _sConfig.SubDepFileExtension;

        public static string MainDepFileExtension => _sConfig.MainDepFileExtension;

        public static string SubLocFileExtension => _sConfig.SubLocFileExtension;

        public static string MainLocFileExtension => _sConfig.MainLocFileExtension;

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

            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", platform);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("SRCPATH", GameCore.Environment.expandVariables(srcPath));

            Name = name;
            Platform = FromString(platform, EPlatform.Win64);
            Target = FromString(platform, EPlatform.Win32);
            Territory = FromString(territory, ETerritory.Europe);

            ConfigFilename = new Filename(GameCore.Environment.expandVariables(configFilename));

            Excel0 = new Filename(GameCore.Environment.expandVariables(excel0));

            SrcPath = new Dirname(GameCore.Environment.expandVariables(srcPath));
            SubPath = Dirname.Empty;
            DstPath = new Dirname(GameCore.Environment.expandVariables(dstPath));
            DepPath = new Dirname(GameCore.Environment.expandVariables(depPath));
            ToolPath = new Dirname(GameCore.Environment.expandVariables(toolPath));
            PublishPath = new Dirname(GameCore.Environment.expandVariables(publishPath));

            return true;
        }

        public static void SetConfig(IBuildSystemLocalizerConfig config)
        {
            if (config != null)
                _sConfig = config;
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
