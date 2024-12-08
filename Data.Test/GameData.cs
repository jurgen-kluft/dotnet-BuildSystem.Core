using System;

namespace GameData
{
	public class GameRoot : IDataRootUnit
	{
        public EDataUnit UnitType { get; } = EDataUnit.Root;
        public string UnitId { get; } = "GameData";

		public AudioFile BootSound = new ("Sound\\BootChime.wav");

		public AI AI = new AI();
		public Fonts Fonts = new Fonts();
		public Menu Menu = new Menu();

		public Cars Cars = new Cars();
		public Tracks Tracks = new Tracks();
	}
}
