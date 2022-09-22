using System;
using Game.Data.AI;

namespace GameData
{
	public class DataUnit : IDataUnit
	{
		public string name { get { return "Game Name"; } }

		public IDynamicMember ai = new Game.Data.AI.DataUnit("AI", "AI/", EDataUnit.Embed);
		public IDynamicMember fonts = new Game.Data.Fonts("Fonts", "Fonts/", EDataUnit.Embed);
		public IDynamicMember menu = new DataUnit("Menu/", EDataUnit.Embed);

		public IDynamicMember cars = new DataUnit("Cars/", EDataUnit.External);
		public IDynamicMember tracks = new DataUnit("Tracks/", EDataUnit.External);
		public IDynamicMember tests = new DataUnit("Tests/", EDataUnit.External);
	}
}