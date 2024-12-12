using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface IFileId
    {
        public Hash160 Signature { get; set; }
    }

}
