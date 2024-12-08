
namespace GameData
{
    public enum EDataUnit
    {
        External,
        Embed,
        Root,
    }

    public interface IDataUnit
    {
        EDataUnit UnitType { get; }
        string UnitId { get; }
    }

    public interface IDataRootUnit : IDataUnit
    {

    }

}
