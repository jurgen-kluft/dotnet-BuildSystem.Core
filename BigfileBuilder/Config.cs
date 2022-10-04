using System;

namespace DataBuildSystem
{
    public sealed class BigfileDefaultConfig : IBigfileConfig
    {
        #region Methods & Properties

        public string Platform => "Default";
        public string BigfileName => "Game";
        public string BigFileExtension => ".gda";
        public string BigFileTocExtension => ".gdt";
        public string BigFileFdbExtension => ".gdf";
        public string BigFileHdbExtension => ".gdh";
        public bool LittleEndian => true;
        public Int64 FileAlignment => 256;
        public bool AllowDuplicateFiles => false;
        public UInt32 ReadBufferSize => 8*1024*1024;
        public UInt32 WriteBufferSize => 48*1024*1024;

        #endregion
    }


    public static class BigfileConfig
    {
        #region Fields

        private static IBigfileConfig _sConfig = new BigfileDefaultConfig();

        #endregion
        #region Init

        public static void Init(IBigfileConfig config)
        {
            _sConfig = config;
        }

        #endregion
        #region Methods & Properties

        public static string BigfileName => _sConfig.BigfileName;
        public static string BigFileExtension => _sConfig.BigFileExtension;
        public static string BigFileTocExtension => _sConfig.BigFileTocExtension;
        public static string BigFileFdbExtension => _sConfig.BigFileFdbExtension;
        public static string BigFileHdbExtension => _sConfig.BigFileHdbExtension;
        public static bool LittleEndian => _sConfig.LittleEndian;
        public static Int64 FileAlignment => _sConfig.FileAlignment;
        public static bool AllowDuplicateFiles => _sConfig.AllowDuplicateFiles;
        public static UInt32 ReadBufferSize => _sConfig.ReadBufferSize;
        public static UInt32 WriteBufferSize => _sConfig.WriteBufferSize;

        #endregion
    }
}
