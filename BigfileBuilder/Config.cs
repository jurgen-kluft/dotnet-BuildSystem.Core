using System;
using Core;

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
        /// The extension of the Bigfile Node  (.gdn)
        /// </summary>
        string BigFileNodeExtension { get; }

        /// <summary>
        /// The endian of the Toc and Fdb
        /// </summary>
        bool LittleEndian { get; }

        /// <summary>
        /// The alignment of every file in the bigfile
        /// </summary>
        UInt32 FileAlignment { get; }

        /// <summary>
        /// True = Allow binary duplicate files
        /// False = All binary identical files are collapsed
        /// </summary>
        bool AllowDuplicateFiles { get; }

        /// <summary>
        /// Build the Bigfile using asynchronous writing
        /// Note: this will double the size of the write buffer!
        /// </summary>
        bool WriteAsync { get; }

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

    public class BigfileDefaultConfig : IBigfileConfig
    {
        #region Methods & Properties

        public string BigfileName { get { return "Game"; } }
        public string BigFileExtension { get { return ".gda"; } }
        public string BigFileTocExtension { get { return ".gdt"; } }
        public string BigFileFdbExtension { get { return ".gdf"; } }
        public string BigFileHdbExtension { get { return ".gdh"; } }
        public string BigFileNodeExtension { get { return ".gdn"; } }
        public bool LittleEndian { get { return true; } }
        public UInt32 FileAlignment { get { return 2048; } }
        public bool AllowDuplicateFiles { get { return false; } }
        public bool WriteAsync { get { return true; } }
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
        public static string BigFileNodeExtension { get { return sConfig.BigFileNodeExtension; } }
        public static bool LittleEndian { get { return sConfig.LittleEndian; } }
        public static UInt32 FileAlignment { get { return sConfig.FileAlignment; } }
        public static bool AllowDuplicateFiles { get { return sConfig.AllowDuplicateFiles; } }
        public static bool WriteAsync { get { return sConfig.WriteAsync; } }
        public static UInt32 ReadBufferSize { get { return sConfig.ReadBufferSize; } }
        public static UInt32 WriteBufferSize { get { return sConfig.WriteBufferSize; } }

        #endregion
    }
}
