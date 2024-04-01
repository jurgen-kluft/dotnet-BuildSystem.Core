using System.Globalization;
using GameCore;

namespace DataBuildSystem
{
    public interface IBuildSystemCompilerConfig
    {
        #region Methods & Properties

        /// <summary>
        /// The platform this configuration is for
        /// </summary>
        string Platform { get; }

        /// <summary>
        /// The resource data file name
        /// </summary>
        string DataFilename(string name);

        /// <summary>
        /// The resource data file extension
        /// </summary>
        string DataFileExtension { get; }

        /// <summary>
        /// The resource data relocation information file extension
        /// </summary>
        string DataRelocFileExtension { get; }

        /// <summary>
        /// Write the BigfileToc and Resource data in which endian
        /// </summary>
        bool LittleEndian { get; }

        /// <summary>
        /// Treat every enum as a 32 bit integer
        /// </summary>
        bool EnumIsInt32 { get; }

        /// <summary>
        /// Treat every bool as a n byte value (1, 2 or 4)
        /// </summary>
        int SizeOfBool { get; }

        #endregion
    }

    public class BuildSystemCompilerConfigDefault : IBuildSystemCompilerConfig
    {
        #region Methods & Properties
        public string Platform => "Default";
        public string DataFilename(string name) { return "Game" + "." + name; }
        public string DataFileExtension => ".gdd";
        public string DataRelocFileExtension => ".gdr";
        public bool LittleEndian => true;
        public bool EnumIsInt32 => false;
        public int SizeOfBool => 1;

        #endregion
    }

    public static class BuildSystemCompilerConfig
    {
        #region Fields

        private static EPlatform _sTarget = EPlatform.Win64;
        private static ETerritory _sTerritory = ETerritory.USA;

        private static IBuildSystemCompilerConfig _sConfig = new BuildSystemCompilerConfigDefault();

        #endregion
        #region Properties

        public static EEndian Endian => _sConfig.LittleEndian ? EEndian.Little : EEndian.Big;
        public static string Name { get; private set; }
        public static EPlatform Platform { get; private set; } = EPlatform.Win64;

        public static string PlatformName => Platform.ToString();
        public static EPlatform Target => _sTarget;
        public static string TargetName => _sTarget.ToString();
        public static ETerritory Territory => _sTerritory;
        public static string TerritoryName => _sTerritory.ToString();
        public static bool EnumIsInt32 => _sConfig.EnumIsInt32;
        public static int SizeOfBool => _sConfig.SizeOfBool;
        public static string BasePath { get; private set;}
        public static string SrcPath { get; private set;}
        public static string GddPath { get; private set;}
        public static string SubPath { get; private set;}
        public static string DstPath { get; private set;}
        public static string PubPath { get; private set;}
        public static string ToolPath{ get; private set;}
        public static string DataFileExtension => _sConfig.DataFileExtension;
        public static string DataRelocFileExtension => _sConfig.DataRelocFileExtension;

        #endregion
        #region Methods

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
            if (string.Compare(folder, PlatformName, true, CultureInfo.InvariantCulture) == 0)
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

        public static bool Init(string name, string platform, string target, string territory, string basePath, string srcPath, string gddPath, string subPath, string dstPath, string pubPath, string toolPath)
        {
            Console.WriteLine("name: " + name);
            Console.WriteLine("platform: " + platform);
            Console.WriteLine("target: " + target);
            Console.WriteLine("territory: " + territory);
            Console.WriteLine("base path: " + basePath);
            Console.WriteLine("src path: " + srcPath);
            Console.WriteLine("gdd path: " + gddPath);
            Console.WriteLine("sub path: " + subPath);
            Console.WriteLine("dst path: " + dstPath);
            Console.WriteLine("tool path: " + toolPath);

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


            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", target);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("BASEPATH", GameCore.Environment.expandVariables(basePath));

            Platform = FromString(platform, EPlatform.Win64);
            _sTarget = FromString(target, EPlatform.Win64);
            _sTerritory = FromString(territory, ETerritory.USA);

            Name = name;
            BasePath = GameCore.Environment.expandVariables(basePath);
            SrcPath = GameCore.Environment.expandVariables(srcPath);
            GddPath = GameCore.Environment.expandVariables(gddPath);
            SubPath = GameCore.Environment.expandVariables(string.IsNullOrEmpty(subPath) ? string.Empty : subPath);
            DstPath = GameCore.Environment.expandVariables(dstPath);
            PubPath = GameCore.Environment.expandVariables(pubPath);
            ToolPath = GameCore.Environment.expandVariables(toolPath);

            return true;
        }

        public static void Init(IBuildSystemCompilerConfig config)
        {
            _sConfig = config ?? new BuildSystemCompilerConfigDefault();
        }

        private static T FromString<T>(string @string, T @default)
        {
            string[] names = Enum.GetNames(typeof(T));
            string name = string.Empty;
            foreach (string p in names)
            {
                if (string.Compare(p, @string, StringComparison.OrdinalIgnoreCase) != 0) continue;
                name = p;
                break;
            }
            if (string.IsNullOrEmpty(name))
                return @default;

            return (T)Enum.Parse(typeof(T), name);
        }

        public static string DataFilename(string name)
        {
            return _sConfig.DataFilename(name);
        }

        #endregion
    }
}
