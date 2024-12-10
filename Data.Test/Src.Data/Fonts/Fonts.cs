using System;

namespace GameData
{
        public class Fonts : IDataUnit
        {
            public string UnitId { get; } = "Fonts-56e889c7-1051-4147-9544-c37ee7bc927e";

            public string Description = "This is fonts data";
            public FontFile Font = new("Fonts\\ARCADECLASSIC.TTF");
        }
}
