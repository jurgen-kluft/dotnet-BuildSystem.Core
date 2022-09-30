using System;

namespace GameData
{
	public class DataRoot : IDataRoot
	{
		public string Name { get { return "Game Name"; } }

		public FileId bootSound = new (new CopyCompiler("Sound/BootChime.wav"));

		public DataUnit ai = new DataUnit("AI", "AI/", EDataUnit.Embed);
		public DataUnit fonts = new DataUnit("Fonts", "Fonts/", EDataUnit.Embed);
		public DataUnit menu = new DataUnit("Menu", "Menu/", EDataUnit.Embed);

		public DataUnit cars = new DataUnit("Cars", "Cars/", EDataUnit.External);
		public DataUnit tracks = new DataUnit("Tracks", "Tracks/", EDataUnit.External);
		public DataUnit tests = new DataUnit("Tests", "Tests/", EDataUnit.External);
	}
}