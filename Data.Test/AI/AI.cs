using System;

namespace GameData
{
	namespace AI
	{
		public class AI : IDataRoot
		{
			public string Name { get { return "AI"; } }
			public FileId ReactionCurve = new (new CopyCompiler("AI\\ReactionCurve.curve"));
			public string Description = "This is AI data";
		}
	}
}