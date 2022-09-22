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

        private static EPlatform sPlatform = EPlatform.PC;
        private static EPlatform sTarget = EPlatform.PC;
        private static ETerritory sTerritory = ETerritory.Europe;

        private static string sName = string.Empty;
        private static Filename sConfigFilename = Filename.Empty;
        private static Dirname sSrcPath;
        private static string sExcel0 = string.Empty;
        private static Dirname sDstPath;
        private static Dirname sSubPath;
        private static Dirname sDepPath;
        private static Dirname sPublishPath;
        private static Dirname sToolPath;

        private static IBuildSystemLocalizerConfig sConfig = new BuildSystemLocalizerDefaultConfig();

        #endregion
        #region Properties

        public static bool platformPC { get { return sPlatform == EPlatform.PC; } }
		public static bool platformXboxOne { get { return sPlatform == EPlatform.XBOX_ONE; } }
		public static bool platformXboxOneX { get { return sPlatform == EPlatform.XBOX_ONE_X; } }
		public static bool platformPS4 { get { return sPlatform == EPlatform.PS4; } }
		public static bool platformPS4Pro { get { return sPlatform == EPlatform.PS4_PRO; } }

		public static bool targetPC { get { return sTarget == EPlatform.PC; } }
		public static bool targetXboxOne { get { return sTarget == EPlatform.XBOX_ONE; } }
		public static bool targetXboxOneX { get { return sTarget == EPlatform.XBOX_ONE_X; } }
		public static bool targetPS4 { get { return sTarget == EPlatform.PS4; } }
		public static bool targetPS4Pro { get { return sTarget == EPlatform.PS4_PRO; } }

		public static string Name { get { return sName; } }
        public static EEndian Endian { get { return sConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG; } }
        public static EPlatform Platform { get { return sPlatform; } }
        public static Filename ConfigFilename { get { return sConfigFilename; } }
        public static string PlatformName { get { return sPlatform.ToString(); } }
        public static EPlatform Target { get { return sTarget; } }
        public static string TargetName { get { return sTarget.ToString(); } }
        public static ETerritory Territory { get { return sTerritory; } }
        public static string TerritoryName { get { return sTerritory.ToString(); } }
        public static Dirname SrcPath { get { return sSrcPath; } }
        public static string Excel0 { get { return sExcel0; } }
        public static Dirname DstPath { get { return sDstPath; } }
        public static Dirname SubPath { get { return sSubPath; } }
        public static Dirname DepPath { get { return sDepPath; } }
        public static Dirname PublishPath { get { return sPublishPath; } }
        public static Dirname ToolPath { get { return sToolPath; } }
        public static string SubDepFileExtension { get { return sConfig.SubDepFileExtension; } }
        public static string MainDepFileExtension { get { return sConfig.MainDepFileExtension; } }
        public static string SubLocFileExtension { get { return sConfig.SubLocFileExtension; } }
        public static string MainLocFileExtension { get { return sConfig.MainLocFileExtension; } }

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
            EPlatform platform = fromString(platformStr, Platform);
            if (platform != Platform)
                return true;

            return false;
        }

        public static bool init(string name, string platform, string territory, string configFilename, string srcPath, string excel0, string dstPath, string depPath, string publishPath, string toolPath)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(platform) || String.IsNullOrEmpty(territory) || String.IsNullOrEmpty(configFilename) || String.IsNullOrEmpty(srcPath) || String.IsNullOrEmpty(excel0) || String.IsNullOrEmpty(dstPath) || String.IsNullOrEmpty(depPath))
                return false;

            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", platform);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("SRCPATH", GameCore.Environment.expandVariables(srcPath));

            sName = name;
            sPlatform = fromString(platform, EPlatform.PC);
            sTarget = fromString(platform, EPlatform.PC);
            sTerritory = fromString(territory, ETerritory.Europe);

			sConfigFilename = new Filename(GameCore.Environment.expandVariables(configFilename));

			sExcel0 = new Filename(GameCore.Environment.expandVariables(excel0));

			sSrcPath = new Dirname(GameCore.Environment.expandVariables(srcPath));
            sSubPath = Dirname.Empty;
			sDstPath = new Dirname(GameCore.Environment.expandVariables(dstPath));
			sDepPath = new Dirname(GameCore.Environment.expandVariables(depPath));
			sToolPath = new Dirname(GameCore.Environment.expandVariables(toolPath));
			sPublishPath = new Dirname(GameCore.Environment.expandVariables(publishPath));

            return true;
        }

        public static void SetConfig(IBuildSystemLocalizerConfig config)
        {
            if (config != null)
                sConfig = config;
        }

        public static T fromString<T>(string _string, T _default)
        {
            string[] names = Enum.GetNames(typeof(T));
            string name = string.Empty;
            foreach (string p in names)
            {
                if (String.Compare(p, _string, true) == 0)
                {
                    name = p;
                    break;
                }
            }
            if (String.IsNullOrEmpty(name))
                return _default;

            return (T)Enum.Parse(typeof(T), name);
        }

        #endregion
    }
}
