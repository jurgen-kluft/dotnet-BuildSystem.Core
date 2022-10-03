using System;

namespace DataBuildSystem
{
    public sealed class BigfileDefaultConfig : IBigfileConfig
    {
        #region Methods & Properties

        public string Platform { get { return "Default"; } }
        public string BigfileName { get { return "Game"; } }
        public string BigFileExtension { get { return ".gda"; } }
        public string BigFileTocExtension { get { return ".gdt"; } }
        public string BigFileFdbExtension { get { return ".gdf"; } }
        public string BigFileHdbExtension { get { return ".gdh"; } }
        public bool LittleEndian { get { return true; } }
        public Int64 FileAlignment { get { return 256; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public UInt32 ReadBufferSize { get { return 8*1024*1024; } }
        public UInt32 WriteBufferSize { get { return 48*1024*1024; } }

        #endregion
    }


    public static class BigfileConfig
    {
        #region Fields

        private static IBigfileConfig sConfig = new BigfileDefaultConfig();

        #endregion
        #region Init

        public static void Init(IBigfileConfig config)
        {
            sConfig = config;
        }

        #endregion
        #region Methods & Properties

        public static string BigfileName { get { return sConfig.BigfileName; } }
        public static string BigFileExtension { get { return sConfig.BigFileExtension; } }
        public static string BigFileTocExtension { get { return sConfig.BigFileTocExtension; } }
        public static string BigFileFdbExtension { get { return sConfig.BigFileFdbExtension; } }
        public static string BigFileHdbExtension { get { return sConfig.BigFileHdbExtension; } }
        public static bool LittleEndian { get { return sConfig.LittleEndian; } }
        public static Int64 FileAlignment { get { return sConfig.FileAlignment; } }
        public static bool AllowDuplicateFiles { get { return sConfig.AllowDuplicateFiles; } }
        public static UInt32 ReadBufferSize { get { return sConfig.ReadBufferSize; } }
        public static UInt32 WriteBufferSize { get { return sConfig.WriteBufferSize; } }

        #endregion
    }
}
