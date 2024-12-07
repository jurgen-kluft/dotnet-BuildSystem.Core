using System;

namespace GameData
{
	public class GameRoot : IDataUnit
	{
        public EDataUnit UnitType { get; } = EDataUnit.External;
        public string UnitID { get; } = "Root-56e889c7-1051-4147-9544-c37ee7bc927e";

		public AudioFile BootSound = new ("Sound\\BootChime.wav");

		public AI AI = new AI();
		public Fonts Fonts = new Fonts();
		public Menu Menu = new Menu();

		public Cars Cars = new Cars();
		public Tracks Tracks = new Tracks();
	}
}
