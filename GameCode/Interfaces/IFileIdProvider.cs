using System;
using GameCore;

namespace GameData
{
    public interface IFileIdProvider
    {
        /// <summary>
        /// The FileId is set by an outside process and this id is used to connect to an instance
        /// of this interface. With that we can thus connect 'FileId' with 'Bigfile+BigfileFile'
        /// </summary>
        uint FileIndex { get; set; }
    }
}
