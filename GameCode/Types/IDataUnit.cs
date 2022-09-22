
namespace GameData
{
    public enum EDataUnit
    {
        External,
        Embed,
    }

    public interface IDataRoot
	{
        string Name { get; }
	}

    public interface IDataUnit
    {
        EDataUnit UnitType { get; }
    }

}
