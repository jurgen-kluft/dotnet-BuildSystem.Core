using System;
using System.Globalization;
using System.IO;
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
        #region Properties

        /// <summary>
        /// The platform this configuration is for
        /// </summary>
        string Platform { get; }

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

        #endregion
    }

    public class BuildSystemLocalizerDefaultConfig : IBuildSystemLocalizerConfig
    {
        #region Properties

        public string Platform => "Default";

        public bool LittleEndian => true;

        public bool Unicode => false;

        public string SubDepFileExtension => ".sdep";

        public string MainDepFileExtension => ".dep";

        public string SubLocFileExtension => ".sloc";

        public string MainLocFileExtension => ".loc";

        #endregion
    }

    public static class LocalizerConfig
    {
        #region Fields

        private static EPlatform _sPlatform = EPlatform.Win64;
        private static EPlatform _sTarget = EPlatform.Win64;
        private static ETerritory _sTerritory = ETerritory.Europe;

        private static string _sName = string.Empty;
        private static Filename _sConfigFilename = Filename.Empty;
        private static Dirname _sSrcPath;
        private static string _sExcel0 = string.Empty;
        private static Dirname _sDstPath;
        private static Dirname _sSubPath;
        private static Dirname _sDepPath;
        private static Dirname _sPublishPath;
        private static Dirname _sToolPath;

        private static IBuildSystemLocalizerConfig _sConfig = new BuildSystemLocalizerDefaultConfig();

        #endregion

        #region Properties

        public static bool PlatformPc => _sPlatform == EPlatform.Win64;

        public static bool PlatformXboxOne => _sPlatform == EPlatform.XboxOne;

        public static bool PlatformXboxOneX => _sPlatform == EPlatform.XboxOneX;

        public static bool PlatformPs4 => _sPlatform == EPlatform.PS4;

        public static bool PlatformPs4Pro => _sPlatform == EPlatform.PS4Pro;

        public static bool TargetPc => _sTarget == EPlatform.Win64;

        public static bool TargetXboxOne => _sTarget == EPlatform.XboxOne;

        public static bool TargetXboxOneX => _sTarget == EPlatform.XboxOneX;

        public static bool TargetPs4 => _sTarget == EPlatform.PS4;

        public static bool TargetPs4Pro => _sTarget == EPlatform.PS4Pro;

        public static string Name => _sName;

        public static bool LittleEndian => _sConfig.LittleEndian;

        public static EPlatform Platform => _sPlatform;

        public static Filename ConfigFilename => _sConfigFilename;

        public static string PlatformName => _sPlatform.ToString();

        public static EPlatform Target => _sTarget;

        public static string TargetName => _sTarget.ToString();

        public static ETerritory Territory => _sTerritory;

        public static string TerritoryName => _sTerritory.ToString();

        public static Dirname SrcPath => _sSrcPath;

        public static string Excel0 => _sExcel0;

        public static Dirname DstPath => _sDstPath;

        public static Dirname SubPath => _sSubPath;

        public static Dirname DepPath => _sDepPath;

        public static Dirname PublishPath => _sPublishPath;

        public static Dirname ToolPath => _sToolPath;

        public static string SubDepFileExtension => _sConfig.SubDepFileExtension;

        public static string MainDepFileExtension => _sConfig.MainDepFileExtension;

        public static string SubLocFileExtension => _sConfig.SubLocFileExtension;

        public static string MainLocFileExtension => _sConfig.MainLocFileExtension;

        #endregion

        #region Methods

        public static bool FolderFilter(string folder)
        {
            if (folder.StartsWith("bin.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.StartsWith("publish.", true, CultureInfo.InvariantCulture))
                return true;
            if (folder.EndsWith(".svn", true, CultureInfo.InvariantCulture))
                return true;
            if (String.Compare(folder, PlatformName, true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        public static bool FileFilter(string file)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            string platformStr = Path.GetExtension(filename).TrimStart('.');

            // Name.%PLATFORM%.cs
            EPlatform platform = FromString(platformStr, Platform);
            if (platform != Platform)
                return true;

            return false;
        }

        public static bool Init(string name, string platform, string territory, string configFilename, string srcPath, string excel0, string dstPath, string depPath, string publishPath, string toolPath)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(platform) || String.IsNullOrEmpty(territory) || String.IsNullOrEmpty(configFilename) || String.IsNullOrEmpty(srcPath) || String.IsNullOrEmpty(excel0) || String.IsNullOrEmpty(dstPath) ||
                String.IsNullOrEmpty(depPath))
                return false;

            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", platform);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("SRCPATH", GameCore.Environment.expandVariables(srcPath));

            _sName = name;
            _sPlatform = FromString(platform, EPlatform.Win64);
            _sTarget = FromString(platform, EPlatform.Win32);
            _sTerritory = FromString(territory, ETerritory.Europe);

            _sConfigFilename = new Filename(GameCore.Environment.expandVariables(configFilename));

            _sExcel0 = new Filename(GameCore.Environment.expandVariables(excel0));

            _sSrcPath = new Dirname(GameCore.Environment.expandVariables(srcPath));
            _sSubPath = Dirname.Empty;
            _sDstPath = new Dirname(GameCore.Environment.expandVariables(dstPath));
            _sDepPath = new Dirname(GameCore.Environment.expandVariables(depPath));
            _sToolPath = new Dirname(GameCore.Environment.expandVariables(toolPath));
            _sPublishPath = new Dirname(GameCore.Environment.expandVariables(publishPath));

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
                if (string.Compare(p, @string, true) == 0)
                {
                    name = p;
                    break;
                }
            }

            if (string.IsNullOrEmpty(name))
                return @default;

            return (T)Enum.Parse(typeof(T), name);
        }

        #endregion
    }
}
