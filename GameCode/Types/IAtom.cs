
namespace GameData
{
    /// <summary>
    /// The purpose of these classes is to provide an interface for the ResourceCompiler
    /// of user defined types that need to be collapsed to one of the supported types.
    ///
    /// So if you have a class that you want to collapse to a double then you need to
    /// derive it like this
    ///
    ///         class YourClass : IAtom
    ///
    /// </summary>
    public interface IAtom
    {
        object Value { get; }
    }

}
