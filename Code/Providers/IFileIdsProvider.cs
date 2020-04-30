using System;
using Core;

namespace Game.Data
{
    public interface IFileIdsProvider
    {
        void registerAt(IFileRegistrar registrar);
        FileId[] fileIds { get; }
    }
}
