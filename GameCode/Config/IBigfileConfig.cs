using System;

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
}
