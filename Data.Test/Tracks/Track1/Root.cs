using System;

namespace GameData
{
	public class Test : BaseClass
	{
		public Test()
		{
			name = "test";
		}
	}

	public class Root : IDataUnit
	{
		public string name { get { return "Track1"; } }
		public object test = new Test();
		public string descr = "This is data of Track1";
		public TextureFile road = new ("Tracks\\Track1\\Road.png");
	}
}
