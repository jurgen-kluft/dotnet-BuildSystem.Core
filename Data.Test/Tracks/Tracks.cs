using System;

namespace Game.Data
{
	public class Tracks : IDataUnit
	{
		private CodeObject code = new CodeObject("Concepts", "Concepts\\Concepts.csi");
		
		public string name { get { return "Tracks"; } }
	
		public FileIdList[] tracks = new FileIdList[] {
			new FileIdList(new ExternalRootNodeCompiler("Track1", "Tracks\\Track1\\Track1.csi")),
			new FileIdList(new ExternalRootNodeCompiler("Track2", "Tracks\\Track2\\Track2.csi")),
			new FileIdList(new ExternalRootNodeCompiler("Track3", "Tracks\\Track3\\Track3.csi"))
		};
	}
}