namespace GameData
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct GameDataCode : ICode
    {
        public ICode[] CodeDependency => new ICode[]
            {
                new StringCode(),
                new FileIdCode(),
                new ArrayCode(),
                new ArchiveLoaderCode(),
                new DataUnitCode(),
                new DataFileCode(),
            };

        public string[] CodeLines => Array.Empty<string>();
    }
}
