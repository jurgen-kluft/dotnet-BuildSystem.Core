using System;
using GameCore;

namespace DataBuildSystem
{
    public interface IBigfileConfig
    {
        #region Methods & Properties

        /// <summary>
        /// The name of the big file
        /// </summary>
        string BigfileName { get; }

        /// <summary>
        /// The extension of the Bigfile (.gda)
        /// </summary>
        string BigFileExtension { get; }

        /// <summary>
        /// The extension of the Bigfile Toc (Table of Content) (.gdt)
        /// </summary>
        string BigFileTocExtension { get; }

        /// <summary>
        /// The extension of the Bigfile Filename Database (.gdf)
        /// </summary>
        string BigFileFdbExtension { get; }

        /// <summary>
        /// The extension of the Bigfile Hash Database (.gdh)
        /// </summary>
        string BigFileHdbExtension { get; }

        /// <summary>
        /// The platform for the Toc and Fdb
        /// </summary>
        EPlatform Platform { get; }

        /// <summary>
        /// The alignment of every file in the bigfile
        /// </summary>
        Int64 FileAlignment { get; }

        /// <summary>
        /// True = Allow binary duplicate files
        /// False = All binary identical files are collapsed
        /// </summary>
        bool AllowDuplicateFiles { get; }

        /// <summary>
        /// The size of the read buffer when reading files to copy to the Bigfile
        /// </summary>
        UInt32 ReadBufferSize { get; }

        /// <summary>
        /// The size of the write buffer (cache) for writing to the Bigfile
        /// </summary>
        UInt32 WriteBufferSize { get; }

        #endregion
    }

    public sealed class BigfileDefaultConfig : IBigfileConfig
    {
        #region Methods & Properties

        public string BigfileName => "Game";
        public string BigFileExtension => ".gda";
        public string BigFileTocExtension => ".gdt";
        public string BigFileFdbExtension => ".gdf";
        public string BigFileHdbExtension => ".gdh";
        public EPlatform Platform => EPlatform.Win64;
        public Int64 FileAlignment => 256;
        public bool AllowDuplicateFiles => false;
        public UInt32 ReadBufferSize => 8 * 1024 * 1024;
        public UInt32 WriteBufferSize => 48 * 1024 * 1024;

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
        public static EPlatform Platform => _sConfig.Platform;
        public static Int64 FileAlignment => _sConfig.FileAlignment;
        public static bool AllowDuplicateFiles => _sConfig.AllowDuplicateFiles;
        public static UInt32 ReadBufferSize => _sConfig.ReadBufferSize;
        public static UInt32 WriteBufferSize => _sConfig.WriteBufferSize;

        #endregion
    }
}
