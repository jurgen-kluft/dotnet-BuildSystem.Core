using System;
using GameCore;
using DataBuildSystem;

namespace BigfileFileReorder
{
    public static class Config
    {
        #region Fields

        private static EPlatform sPlatform = EPlatform.PC;

        private static string sName;
        private static Dirname sSrcPath;
        private static Dirname sDstPath;
        private static Dirname sToolPath;
        private static Dirname sPublishPath;

        #endregion
        #region Properties

        public static bool isPC { get { return sPlatform == EPlatform.PC; } }
		public static bool isXboxOne { get { return sPlatform == EPlatform.XBOX_ONE; } }
		public static bool isXboxOneX { get { return sPlatform == EPlatform.XBOX_ONE_X; } }
		public static bool isPS4 { get { return sPlatform == EPlatform.PS4; } }
		public static bool isPS4Pro { get { return sPlatform == EPlatform.PS4_PRO; } }

		public static EEndian Endian
        {
            get
            {
                return EndianUtils.GetPlatformEndian(sPlatform);
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

        public static string Name
        {
            get
            {
                return sName;
            }
        }

		public static Filename FullFileName
		{
			get
			{
				return new Filename(Name + "_" + PlatformName);
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
        
        #endregion
        #region Methods

        public static bool init(string name, string srcPath, string dstPath, string toolPath, string publishPath, string platform)
        {
            if (name == string.Empty || srcPath == string.Empty || dstPath == string.Empty || toolPath == string.Empty || publishPath == string.Empty || platform == string.Empty)
                return false;

            sPlatform = fromString(platform, EPlatform.PC);
            string platformName = sPlatform.ToString();

            sName = name;
            sSrcPath = new Dirname(srcPath);
            sDstPath = new Dirname(dstPath);
            sToolPath = new Dirname(toolPath);
            sPublishPath = new Dirname(publishPath);
            return true;
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
