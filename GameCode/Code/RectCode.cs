using System;

namespace GameData
{
    public struct RectCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();
        public string[] CodeLines => Array.Empty<string>();
    }
}
