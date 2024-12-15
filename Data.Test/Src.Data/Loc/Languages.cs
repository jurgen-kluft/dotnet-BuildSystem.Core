namespace GameData
{
    public class Languages : IDataUnit
    {
        public string Signature => "ae5b7036-88b1-4864-98e2-e4cd567634ed";
        public LocalizationCompiler localization = new("Loc\\Localization.dat");
    }
}
