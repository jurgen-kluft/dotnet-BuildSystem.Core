using System;

namespace Game.Data
{
	public class Root : IDataUnit
	{
		public string name { get { return "Track2"; } }

		public string descr = "This is data of Track2";
		public FileId road = new FileId(new CopyCompiler("Tracks\\Track2\\Road.png"));
	}
}