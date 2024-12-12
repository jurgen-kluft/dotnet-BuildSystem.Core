using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public interface ISignatureDataBase
    {
        (uint bigfileIndex, uint fileIndex) GetFileId(Hash160 signature);
        bool Register(Hash160 signature, uint bigfileIndex, uint fileIndex);

        void RemoveBigfile(uint index);

        bool Load(string filepath);
        bool Save(string filepath);
    }
}
