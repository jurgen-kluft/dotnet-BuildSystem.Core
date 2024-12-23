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

    public struct NullSignature : ISignature
    {
        public Hash160 Signature => Hash160.Null;
    }

}
