using System;

namespace GameData
{
	public class Root : IDataUnit
	{
		public string name { get { return "Track3"; } }

		public string descr = "This is data of Track3";
		public TextureFile road = new ("Tracks\\Track3\\Road.png");
	}
}
