using System;

namespace GameData
{
	namespace Fonts
	{
		public class DataRoot : IDataRoot
		{
			public string Name { get { return "Fonts"; } }

			public string descr = "This is fonts data";

			public FileId Font = new (new CopyCompiler("ARCADECLASSIC.TTF"));
		}
	}
}