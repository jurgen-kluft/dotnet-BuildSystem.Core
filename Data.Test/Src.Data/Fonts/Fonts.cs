using System;

namespace GameData
{
        public class Fonts : IDataUnit
        {
            public string Description = "This is fonts data";
            public FontDataFile Font = new("Fonts\\ARCADECLASSIC.TTF");
        }
}
