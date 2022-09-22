using System;

namespace GameData
{
	namespace AI
	{
		public class DataRoot : IDataRoot
		{
			public string Name { get { return "AI"; } }

			public string descr = "This is AI data";
		}
	}
}