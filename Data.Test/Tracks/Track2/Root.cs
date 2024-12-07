using System;

namespace GameData
{
	public class Root : IDataUnit
	{
		public string name { get { return "Track2"; } }

		public string descr = "This is data of Track2";
		public TextureFile road = new ("Tracks\\Track2\\Road.png");
	}
}
