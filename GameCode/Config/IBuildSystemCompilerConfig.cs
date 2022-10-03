using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
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
        public string Platform { get { return "Default"; } }
        public string DataFilename(string name) { return "Game" + "." + name; }
        public string DataFileExtension { get { return ".gdd"; } }
        public string DataRelocFileExtension { get { return ".gdr"; } }
        public bool LittleEndian { get { return true; } }
        public bool EnumIsInt32 { get { return false; } }
        public int SizeOfBool { get { return 1; } }

        #endregion
    }

    public static class BuildSystemCompilerConfig
    {
        #region Fields

        private static EPlatform sPlatform = EPlatform.PC;
        private static EPlatform sTarget = EPlatform.PC;
        private static ETerritory sTerritory = ETerritory.USA;

        private static List<Filename> sReferencedAssemblies = new List<Filename>();

        //private static bool sBuildBigfile = false;

        private static IBuildSystemCompilerConfig sConfig = new BuildSystemCompilerConfigDefault();

        #endregion
        #region Properties

        public static bool PlatformPC { get { return sPlatform == EPlatform.PC; } }
        public static bool PlatformXboxOne { get { return sPlatform == EPlatform.XBOX_ONE; } }
        public static bool PlatformXboxOneX { get { return sPlatform == EPlatform.XBOX_ONE_X; } }
        public static bool PlatformPS4 { get { return sPlatform == EPlatform.PS4; } }
        public static bool PlatformPS4Pro { get { return sPlatform == EPlatform.PS4_PRO; } }

        public static bool TargetPC { get { return sTarget == EPlatform.PC; } }
        public static bool TargetXboxOne { get { return sTarget == EPlatform.XBOX_ONE; } }
        public static bool TargetXboxOneX { get { return sTarget == EPlatform.XBOX_ONE_X; } }
        public static bool TargetPS4 { get { return sTarget == EPlatform.PS4; } }
        public static bool TargetPS4Pro { get { return sTarget == EPlatform.PS4_PRO; } }

        public static EEndian Endian { get { return sConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG; } }
        public static string Name { get; private set; }
        public static EPlatform Platform { get { return sPlatform; } }
        public static string PlatformName { get { return sPlatform.ToString(); } }
        public static EPlatform Target { get { return sTarget; } }
        public static string TargetName { get { return sTarget.ToString(); } }
        public static ETerritory Territory { get { return sTerritory; } }
        public static string TerritoryName { get { return sTerritory.ToString(); } }
        public static bool EnumIsInt32 { get { return sConfig.EnumIsInt32; } }
        public static int SizeOfBool { get { return sConfig.SizeOfBool; } }
        public static string BasePath { get; private set;}
        public static string SrcPath { get; private set;}
        public static string GddPath { get; private set;}
        public static string SubPath { get; private set;}
        public static string DstPath { get; private set;}
        public static string PubPath { get; private set;}
        public static string ToolPath{ get; private set;}
        public static string DataFileExtension { get { return sConfig.DataFileExtension; } }
        public static string DataRelocFileExtension { get { return sConfig.DataRelocFileExtension; } }

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

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(platform) || String.IsNullOrEmpty(territory))
                return false;
            if (String.IsNullOrEmpty(srcPath))
                return false;
            if (String.IsNullOrEmpty(gddPath))
                return false;
            if (String.IsNullOrEmpty(dstPath))
                return false;
            if (String.IsNullOrEmpty(pubPath))
                return false;
            if (String.IsNullOrEmpty(toolPath))
                return false;

            if (String.IsNullOrEmpty(target))
                target = platform;


            GameCore.Environment.addVariable("NAME", name);
            GameCore.Environment.addVariable("PLATFORM", platform);
            GameCore.Environment.addVariable("TARGET", target);
            GameCore.Environment.addVariable("TERRITORY", territory);
            GameCore.Environment.addVariable("BASEPATH", GameCore.Environment.expandVariables(basePath));

            sPlatform = FromString(platform, EPlatform.PC);
            sTarget = FromString(target, EPlatform.PC);
            sTerritory = FromString(territory, ETerritory.USA);

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

        public static void AddReferencedAssembly(Filename referencedAssembly)
        {
            if (!sReferencedAssemblies.Contains(referencedAssembly))
                sReferencedAssemblies.Add(referencedAssembly);
        }

        public static void AddReferencedAssemblies(List<Filename> referencedAssemblies)
        {
            foreach (Filename a in referencedAssemblies)
                AddReferencedAssembly(a);
        }

        public static void Init(IBuildSystemCompilerConfig config)
        {
            if (config != null)
                sConfig = config;
            else
                sConfig = new BuildSystemCompilerConfigDefault();
        }

        public static T FromString<T>(string _string, T _default)
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

        public static string DataFilename(string name)
        {
            return sConfig.DataFilename(name);
        }

        #endregion
    }
}
