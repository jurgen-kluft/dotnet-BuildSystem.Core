using System;

namespace GameData
{
	public class Menu : IDataUnit
	{
        public EDataUnit UnitType { get; } = EDataUnit.Embed;
        public string UnitID { get; } = "Menu-56e889c7-1051-4147-9544-c37ee7bc927e";

		public string descr = "This is menu data";
	}
}
