using System;

namespace GameData
{
    public class GameRoot : IRootDataUnit
    {
        public string Signature => "cb379735-a255-4b31-95e3-2a24eccbe2d2";
        public List<GameDataFile> GameDataFiles
        {
            set => GameData = value;
        }

        public List<ICode> CodeDependency
        {
            get
            {
                return new List<ICode> { new GameDataCode(), };
            }
        }

        public SoundFileCooker BootSound = new("Sound/BootChime.wav");

        public AI AI = new AI();
        public Fonts Fonts = new Fonts();
        public Menu Menu = new Menu();
        public Localization Localization = new Localization();

        public Cars Cars = new Cars();
        public Tracks Tracks = new Tracks();

        public List<GameDataFile> GameData;
    }
}
