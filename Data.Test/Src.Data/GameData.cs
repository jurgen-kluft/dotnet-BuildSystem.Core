using System;

namespace GameData
{
	public class GameRoot : IRootDataUnit, IDataUnit
    {
        public string Signature => "cb379735-a255-4b31-95e3-2a24eccbe2d2";
		public SoundDataFile BootSound = new ("Sound/BootChime.wav");

		public AI AI = new AI();
		public Fonts Fonts = new Fonts();
		public Menu Menu = new Menu();
        public Localization Localization = new Localization();

		public Cars Cars = new Cars();
		public Tracks Tracks = new Tracks();
	}
}
