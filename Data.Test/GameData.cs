using System;

namespace GameData
{
	public class GameRoot : IDataRoot
	{
		public string Name { get { return "Game Name"; } }

		public FileId BootSound = new (new CopyCompiler("Sound\\BootChime.wav"));

		public DataUnit AI = new ("AI", "AI\\", EDataUnit.Embed);
		public DataUnit Fonts = new ("Fonts", "Fonts\\", EDataUnit.Embed);
		public DataUnit Menu = new ("Menu", "Menu\\", EDataUnit.Embed);

		public DataUnit Cars = new ("Cars", "Cars\\", EDataUnit.External);
		public DataUnit Tracks = new ("Tracks", "Tracks\\", EDataUnit.External);
		public DataUnit Tests = new ("Tests", "Tests\\", EDataUnit.External);
	}
}