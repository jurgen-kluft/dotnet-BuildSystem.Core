using System.Collections.Generic;

namespace GameData
{
    public class Languages : IDataUnit
    {
        public string UnitId { get; } = "Loc-56e889c7-1051-4147-9544-c37ee7bc927e";
        public LocalizationFile localization = new("Loc\\Localization.dat");
    }
}
