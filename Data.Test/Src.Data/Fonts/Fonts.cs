using System;

namespace GameData
{
    public class Fonts : IDataUnit
    {
        public string Signature => "c7b30e37-f9df-43c1-8b26-53d527b4e5a9";
        public string Description = "This is fonts data";
        public FontFileCooker Font = new("Fonts\\ARCADECLASSIC.TTF");
    }
}
