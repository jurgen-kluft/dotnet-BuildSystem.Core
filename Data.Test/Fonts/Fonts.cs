using System;

namespace GameData
{
	namespace Fonts
	{
		public class Fonts : IDataRoot
		{
			public string Name { get { return "Fonts"; } }

			public string Description = "This is fonts data";
			public FileId Font = new (new CopyCompiler("Fonts\\ARCADECLASSIC.TTF"));
		}
	}
}