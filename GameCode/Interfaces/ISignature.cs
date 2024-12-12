using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface ISignature
    {
        public Hash160 Signature { get; }
    }

}
