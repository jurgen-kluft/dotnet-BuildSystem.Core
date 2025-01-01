
using GameCore;

namespace GameData
{
    public interface IDataUnit
    {
        string Signature { get; }
    }

    public class GameDataFile
    {
        public string BigfileData;
        public string BigfileToc;
        public string BigfileFdb;
        public string BigfileHdb;
    }

    public interface IRootDataUnit : IDataUnit
    {
        List<ICode> CodeDependency { get; }
        List<GameDataFile> GameDataFiles { set; }
    }

}
