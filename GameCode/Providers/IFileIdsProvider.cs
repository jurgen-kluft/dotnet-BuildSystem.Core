using System;
using GameCore;

namespace GameData
{
    public interface IFileIdsProvider
    {
        FileId[] FileIds { get; }
    }
}
