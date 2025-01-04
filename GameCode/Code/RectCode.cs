using System;

namespace GameData
{
    public readonly struct RectCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();
        public string[] CodeLines => Array.Empty<string>();
    }
}
