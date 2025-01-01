namespace GameData
{
    public struct NullCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();
        public string[] CodeLines => Array.Empty<string>();
    }
}
