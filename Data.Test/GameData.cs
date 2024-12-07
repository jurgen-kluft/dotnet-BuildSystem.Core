using System;

namespace GameData
{
	public class GameRoot : IDataRoot
	{
		public string Name { get { return "Game Name"; } }

		public AudioFile BootSound = new ("Sound\\BootChime.wav");

		public AI AI = new AI();
		public Fonts Fonts = new Fonts();
		public Menu Menu = new Menu();

		public Cars Cars = new Cars();
		public Tracks Tracks = new Tracks();
		public Tests Tests = new Tests();
	}
}
