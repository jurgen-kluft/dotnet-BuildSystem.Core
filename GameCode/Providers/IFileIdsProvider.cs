using System;
using GameCore;

namespace GameData
{
    public interface IFileIdsProvider
    {
        void registerAt(IFileRegistrar registrar);
        FileId[] fileIds { get; }
    }
}
