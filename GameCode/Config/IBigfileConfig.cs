using System;
using GameCore;

namespace DataBuildSystem
{
    public interface IBigfileConfig
    {
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
        /// False = All binary identical files are deduplicated
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
    }

    public sealed class BigfileDefaultConfig : IBigfileConfig
    {
        public string BigfileName => "Game";
        public string BigFileExtension => ".gda";
        public string BigFileTocExtension => ".gdt";
        public string BigFileFdbExtension => ".gdf";
        public string BigFileHdbExtension => ".gdh";
        public EPlatform Platform => EPlatform.Win64;
        public Int64 FileAlignment => 256;
        public bool AllowDuplicateFiles => false;
        public UInt32 ReadBufferSize => 1 * 1024 * 1024;
        public UInt32 WriteBufferSize => 1 * 1024 * 1024;
    }


    public static class BigfileConfig
    {
        private static IBigfileConfig _Config = new BigfileDefaultConfig();

        public static void Init(IBigfileConfig config)
        {
            _Config = config;
        }

        public static string BigfileName => _Config.BigfileName;
        public static string BigFileExtension => _Config.BigFileExtension;
        public static string BigFileTocExtension => _Config.BigFileTocExtension;
        public static string BigFileFdbExtension => _Config.BigFileFdbExtension;
        public static string BigFileHdbExtension => _Config.BigFileHdbExtension;
        public static EPlatform Platform => _Config.Platform;
        public static Int64 FileAlignment => _Config.FileAlignment;
        public static bool AllowDuplicateFiles => _Config.AllowDuplicateFiles;
        public static UInt32 ReadBufferSize => _Config.ReadBufferSize;
        public static UInt32 WriteBufferSize => _Config.WriteBufferSize;
    }
}
