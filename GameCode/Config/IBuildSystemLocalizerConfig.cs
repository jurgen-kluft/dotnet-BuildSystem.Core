using System;
using System.Globalization;
using System.IO;
using GameCore;

namespace DataBuildSystem
{
    public interface IExcelCharConversion
    {
        char from { get; }
        char to { get; }
    }

    public interface IExcelSheetColumn
    {
        string name { get; }
        int column { get; }
        int beginRow { get; }
        int endRow { get; }
        byte[] asciiset { get; }
    }

    public interface IExcelSheetConfig
    {
        int beginRow { get; }
        int endRow { get; }
        IExcelSheetColumn[] columns { get; }
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

        public string Platform { get { return "Default"; } }
        public bool LittleEndian { get { return true; } }
        public bool Unicode { get { return false; } }
        public string SubDepFileExtension { get { return ".sdep"; } }
        public string MainDepFileExtension { get { return ".dep"; } }
        public string SubLocFileExtension { get { return ".sloc"; } }
        public string MainLocFileExtension { get { return ".loc"; } }

        #endregion
    }

    public static class LocalizerConfig
    {
        #region Fields

        private static EPlatform _sPlatform = EPlatform.PC;
        private static EPlatform _sTarget = EPlatform.PC;
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

        public static bool PlatformPc { get { return _sPlatform == EPlatform.PC; } }
		public static bool platformXboxOne { get { return _sPlatform == EPlatform.XBOX_ONE; } }
		public static bool platformXboxOneX { get { return _sPlatform == EPlatform.XBOX_ONE_X; } }
		public static bool PlatformPs4 { get { return _sPlatform == EPlatform.PS4; } }
		public static bool PlatformPs4Pro { get { return _sPlatform == EPlatform.PS4_PRO; } }

		public static bool TargetPc { get { return _sTarget == EPlatform.PC; } }
		public static bool targetXboxOne { get { return _sTarget == EPlatform.XBOX_ONE; } }
		public static bool targetXboxOneX { get { return _sTarget == EPlatform.XBOX_ONE_X; } }
		public static bool TargetPs4 { get { return _sTarget == EPlatform.PS4; } }
		public static bool TargetPs4Pro { get { return _sTarget == EPlatform.PS4_PRO; } }

		public static string Name { get { return _sName; } }
        public static EEndian Endian { get { return _sConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG; } }
        public static EPlatform Platform { get { return _sPlatform; } }
        public static Filename ConfigFilename { get { return _sConfigFilename; } }
        public static string PlatformName { get { return _sPlatform.ToString(); } }
        public static EPlatform Target { get { return _sTarget; } }
        public static string TargetName { get { return _sTarget.ToString(); } }
        public static ETerritory Territory { get { return _sTerritory; } }
        public static string TerritoryName { get { return _sTerritory.ToString(); } }
        public static Dirname SrcPath { get { return _sSrcPath; } }
        public static string Excel0 { get { return _sExcel0; } }
        public static Dirname DstPath { get { return _sDstPath; } }
        public static Dirname SubPath { get { return _sSubPath; } }
        public static Dirname DepPath { get { return _sDepPath; } }
        public static Dirname PublishPath { get { return _sPublishPath; } }
        public static Dirname ToolPath { get { return _sToolPath; } }
        public static string SubDepFileExtension { get { return _sConfig.SubDepFileExtension; } }
        public static string MainDepFileExtension { get { return _sConfig.MainDepFileExtension; } }
        public static string SubLocFileExtension { get { return _sConfig.SubLocFileExtension; } }
        public static string MainLocFileExtension { get { return _sConfig.MainLocFileExtension; } }

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
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(platform) || String.IsNullOrEmpty(territory) || String.IsNullOrEmpty(configFilename) || String.IsNullOrEmpty(srcPath) || String.IsNullOrEmpty(excel0) || String.IsNullOrEmpty(dstPath) || String.IsNullOrEmpty(depPath))
                return false;

            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", platform);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("SRCPATH", GameCore.Environment.expandVariables(srcPath));

            _sName = name;
            _sPlatform = FromString(platform, EPlatform.PC);
            _sTarget = FromString(platform, EPlatform.PC);
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
            string[] names = Enum.GetNames(typeof(T));
            string name = string.Empty;
            foreach (string p in names)
            {
                if (String.Compare(p, @string, true) == 0)
                {
                    name = p;
                    break;
                }
            }
            if (String.IsNullOrEmpty(name))
                return @default;

            return (T)Enum.Parse(typeof(T), name);
        }

        #endregion
    }
}
