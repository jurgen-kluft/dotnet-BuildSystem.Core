using System;

namespace GameData
{
    namespace Fonts
    {
        public class Fonts : IDataUnit
        {
            public string Name { get { return "Fonts"; } }

            public string Description = "This is fonts data";
            public FontFile Font = new("Fonts\\ARCADECLASSIC.TTF");
        }
    }
}
