using System;

namespace Game.Data
{
	public class Root : IDataUnit
	{
		public string name { get { return "Track3"; } }

		public string descr = "This is data of Track3";
		public FileId road = new FileId(new CopyCompiler("Tracks\\Track3\\Road.png"));
	}
}