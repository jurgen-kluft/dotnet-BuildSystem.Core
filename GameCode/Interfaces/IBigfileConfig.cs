using GameCore;

namespace BigfileBuilder
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
        uint FileAlignment { get; }

        /// <summary>
        /// True = Allow binary duplicate files
        /// False = All binary identical files are deduplicated
        /// </summary>
        bool AllowDuplicateFiles { get; }

        /// <summary>
        /// The size of the read buffer when reading files to copy to the Bigfile
        /// </summary>
        uint ReadBufferSize { get; }

        /// <summary>
        /// The size of the write buffer (cache) for writing to the Bigfile
        /// </summary>
        uint WriteBufferSize { get; }
    }

    public sealed class BigfileDefaultConfig : IBigfileConfig
    {
        public string BigfileName => "Game";
        public string BigFileExtension => ".gda";
        public string BigFileTocExtension => ".gdt";
        public string BigFileFdbExtension => ".gdf";
        public string BigFileHdbExtension => ".gdh";
        public EPlatform Platform => EPlatform.Win64;
        public uint FileAlignment => 256;
        public bool AllowDuplicateFiles => false;
        public uint ReadBufferSize => 1 * 1024 * 1024;
        public uint WriteBufferSize => 1 * 1024 * 1024;
    }


    public static class BigfileConfig
    {
        private static IBigfileConfig Config { get; set; } = new BigfileDefaultConfig();

        public static void Init(IBigfileConfig config)
        {
            Config = config;
        }

        public static string BigfileName => Config.BigfileName;
        public static string BigFileExtension => Config.BigFileExtension;
        public static string BigFileTocExtension => Config.BigFileTocExtension;
        public static string BigFileFdbExtension => Config.BigFileFdbExtension;
        public static string BigFileHdbExtension => Config.BigFileHdbExtension;
        public static EPlatform Platform => Config.Platform;
        public static uint FileAlignment => Config.FileAlignment;
        public static bool AllowDuplicateFiles => Config.AllowDuplicateFiles;
        public static uint ReadBufferSize => Config.ReadBufferSize;
        public static uint WriteBufferSize => Config.WriteBufferSize;
    }
}
