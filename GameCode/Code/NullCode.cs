namespace GameData
{
    public readonly struct NullCode : ICode
    {
        public ICode[] CodeDependency => Array.Empty<ICode>();
        public string[] CodeLines => Array.Empty<string>();
    }
}
