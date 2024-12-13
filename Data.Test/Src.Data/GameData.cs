using System;

namespace GameData
{
	public class GameRoot : IRootDataUnit
	{
		public SoundDataFile BootSound = new ("Sound/BootChime.wav");

		public AI AI = new AI();
		public Fonts Fonts = new Fonts();
		public Menu Menu = new Menu();

		public Cars Cars = new Cars();
		public Tracks Tracks = new Tracks();
	}
}
