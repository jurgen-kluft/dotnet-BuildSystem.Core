using System;
using System.Globalization;
using System.IO;

using GameCore;
using DataBuildSystem;
namespace BigfilePacker
{
    public interface IResourceCompilerConfig
    {
        #region Methods & Properties

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
        /// Write the BigfileToc and Resource data in which endianness
        /// </summary>
        bool LittleEndian { get; }

        /// <summary>
        /// The class hierarchy as a blob, so collapse all members to one object instead of preserving the hierarchy
        /// </summary>
        bool ClassIsBlob { get; }

        /// <summary>
        /// Treat every enum as a 32 bit integer
        /// </summary>
        bool EnumIsInt32 { get; }

        /// <summary>
        /// Force building the bigfile, even if no assets have been modified
        /// </summary>
        bool ForceBuildingBigfile { get; }

        #endregion
    }

    public class ResourceCompilerDefaultConfig : IResourceCompilerConfig
    {
        #region Methods & Properties

        public string DataFilename(string name)
        {
            return "PMR2" + "." + name;
        }

        public string DataFileExtension
        {
            get
            {
                return ".rdf";
            }
        }

        public string DataRelocFileExtension
        {
            get
            {
                return ".raf";
            }
        }

        public bool LittleEndian
        {
            get
            {
                return true;
            }
        }

        public bool ClassIsBlob
        {
            get
            {
                return true;
            }
        }

        public bool EnumIsInt32
        {
            get
            {
                return false;
            }
        }

        public bool ForceBuildingBigfile
        {
            get
            {
                return false;
            }
        }

        public string DataDllFilename
        {
            get
            {
                return "Name.Data";
            }
        }

        #endregion
    }

    public static class Config
    {
        #region Fields

        private static EPlatform sPlatform = EPlatform.PC;

        private static string sName;
        private static Dirname sSrcPath;
        private static Dirname sDstPath;
        private static Dirname sToolPath;
        private static Dirname sPublishPath;

        private static IResourceCompilerConfig sConfig = new ResourceCompilerDefaultConfig();

        #endregion
        #region Properties

        public static bool isPC { get { return sPlatform == EPlatform.PC; } }

        public static EEndian Endian
        {
            get
            {
                return sConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG;
            }
        }


        public static EPlatform Platform
        {
            get
            {
                return sPlatform;
            }
        }

        public static string PlatformName
        {
            get
            {
                return sPlatform.ToString();
            }
        }

        public static bool ClassIsBlob
        {
            get
            {
                return sConfig.ClassIsBlob;
            }
        }

        public static string Name
        {
            get
            {
                return sName;
            }
        }

        public static bool EnumIsInt32
        {
            get
            {
                return sConfig.EnumIsInt32;
            }
        }

        public static bool ForceBuildingBigfile
        {
            get
            {
                return sConfig.ForceBuildingBigfile;
            }
        }

        public static Filename CsProjFilename
        {
            get
            {
                return new Filename(Name + ".Data." + PlatformName + ".csproj");
            }
        }

        public static Filename CsProjTemplateFilename
        {
            get
            {
                return new Filename(Name + ".Data.Template.csproj");
            }
        }

        public static Dirname SrcPath
        {
            get
            {
                return sSrcPath;
            }
        }

        public static Dirname DstPath
        {
            get
            {
                return sDstPath;
            }
        }

        public static Dirname ToolPath
        {
            get
            {
                return sToolPath;
            }
        }

        public static Dirname PublishPath
        {
            get
            {
                return sPublishPath;
            }
        }

        public static string DataFileExtension
        {
            get
            {
                return sConfig.DataFileExtension;
            }
        }

        public static string DataRelocFileExtension
        {
            get
            {
                return sConfig.DataRelocFileExtension;
            }
        }

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

        public static bool init(string name, string srcPath, string dstPath, string toolPath, string publishPath, string platform)
        {
            if (name == string.Empty || srcPath == string.Empty || dstPath == string.Empty || toolPath == string.Empty || publishPath == string.Empty || platform == string.Empty)
                return false;

            sPlatform = fromString(platform, EPlatform.PC);

            sName = name;
            sSrcPath = new Dirname(srcPath);
            sDstPath = new Dirname(dstPath);
            sToolPath = new Dirname(toolPath);
            sPublishPath = new Dirname(publishPath);
            return true;
        }

        public static void SetConfig(IResourceCompilerConfig config)
        {
            if (config != null)
                sConfig = config;
        }

        public static EPlatform fromString(string platform, EPlatform defaultPlatform)
        {
            string[] platformNames = Enum.GetNames(typeof(EPlatform));
            string platformName = string.Empty;
            foreach (string p in platformNames)
            {
                if (String.Compare(p, platform, true) == 0)
                {
                    platformName = p;
                    break;
                }
            }
            if (platformName == string.Empty)
                return defaultPlatform;

            return (EPlatform)Enum.Parse(typeof(EPlatform), platformName);
        }

        #endregion
    }
}
