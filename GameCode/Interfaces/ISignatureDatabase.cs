using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface IReadOnlySignatureDataBase
    {
        (uint primary, uint secondary) GetEntry(Hash160 signature);
    }

    public interface ISignatureDataBase : IReadOnlySignatureDataBase
    {
        bool Register(Hash160 signature, uint primary, uint secondary);

        void RemovePrimary(uint index);

        bool Load(string filepath);
        bool Save(string filepath);
    }
}
