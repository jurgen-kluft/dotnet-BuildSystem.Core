using System;

namespace GameData
{
    public interface ICode
    {
        ICode[] CodeDependency { get; }
        string[] CodeLines { get; }
    }
}
