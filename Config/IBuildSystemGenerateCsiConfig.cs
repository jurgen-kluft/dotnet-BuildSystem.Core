using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Core;

namespace DataBuildSystem
{
    public static class BuildSystemGenerateCsiConfig
    {
        #region Fields

        private static string sName;

        private static EPlatform sPlatform = EPlatform.PC;
        private static ETerritory sTerritory = ETerritory.USA;

        private static Dirname sSrcPath;
        private static Dirname sSubPath;

        #endregion
        #region Properties

        public static bool platformPC { get { return sPlatform == EPlatform.PC; } }
		public static bool platformXboxOne { get { return sPlatform == EPlatform.XBOX_ONE; } }
		public static bool platformXboxOneX { get { return sPlatform == EPlatform.XBOX_ONE_X; } }
		public static bool platformPS4 { get { return sPlatform == EPlatform.PS4; } }
		public static bool platformPS4Pro { get { return sPlatform == EPlatform.PS4_PRO; } }

		public static string Name { get { return sName; } }
        public static EPlatform Platform { get { return sPlatform; } }
        public static string PlatformName { get { return sPlatform.ToString(); } }
        public static ETerritory Territory { get { return sTerritory; } }
        public static string TerritoryName { get { return sTerritory.ToString(); } }
        public static Dirname SrcPath { get { return sSrcPath; } }
        public static Dirname SubPath { get { return sSubPath; } }

        #endregion
        #region Methods

        public static bool SourceCodeFilterDelegate(DirectoryScanner.FilterEvent e) 
        { 
            return (e.isFolder) ? BuildSystemCompilerConfig.FolderFilter(e.name) : BuildSystemCompilerConfig.FileFilter(e.name); 
        }

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

        public static bool init(string name, string platform, string territory, string srcPath, string subPath)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(platform) || String.IsNullOrEmpty(territory) || String.IsNullOrEmpty(srcPath))
                return false;

			Core.Environment.addVariable("NAME", name);
			Core.Environment.addVariable("PLATFORM", platform);
			Core.Environment.addVariable("TERRITORY", territory);
			Core.Environment.addVariable("SRCPATH", Core.Environment.expandVariables(srcPath));

            sPlatform = fromString(platform, EPlatform.PC);
            sTerritory = fromString(territory, ETerritory.USA);

            sName = name;
			sSrcPath = new Dirname(Core.Environment.expandVariables(srcPath));
			sSubPath = new Dirname(Core.Environment.expandVariables(string.IsNullOrEmpty(subPath) ? string.Empty : subPath));

            return true;
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
