using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface ISignature
    {
        Hash160 Signature { get; }
    }

}
