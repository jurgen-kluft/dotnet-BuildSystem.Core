
namespace GameData
{
    public enum EDataUnit
    {
        External,
        Embed,
    }

    public interface IDataUnit
    {
        EDataUnit UnitType { get; }
    }

}
