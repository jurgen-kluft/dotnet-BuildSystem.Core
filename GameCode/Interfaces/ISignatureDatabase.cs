using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface ISignatureDataBase
    {
        (uint primary, uint secondary) GetEntry(Hash160 signature);
        bool Register(Hash160 signature, uint primary, uint secondary);

        void RemovePrimary(uint index);

        bool Load(string filepath);
        bool Save(string filepath);
    }
}
