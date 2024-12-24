namespace GameData
{
    public class Localization : IDataUnit
    {
        public string Signature => "ae5b7036-88b1-4864-98e2-e4cd567634ed";
        public LocalizationFileCooker Languages = new("Loc\\Localization.dat");
    }
}
