using System;
using GameCore;

namespace GameData
{
    public interface IFilesProvider
    {
        /// <summary>
        /// The FileIndex is set by and outside process and this index is used to connect to an instance
        /// of this interface. With that we can thus connect 'FileIndex' with 'FileNames'.
        /// </summary>
        UInt64 FilesProviderId { get; set; }

        /// <summary>
        /// The actual Filepaths associated with 'FilesProviderIndex'
        /// </summary>
        string[] FilesProviderFilepaths { get; }
    }
}
