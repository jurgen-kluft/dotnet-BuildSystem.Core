using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    public class GameDataFileDataBase : ISignatureDataBase
    {
        public (uint bigfileIndex, uint fileIndex) GetFileId(Hash160 signature)
        {
            return (0, 0);
        }

        public void Add(Hash160 signature)
        {

        }
    }
}
