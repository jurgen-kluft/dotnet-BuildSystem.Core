using System;

namespace Game.Data.Cars
{
	public class Cars : IDataUnit
	{
		public string name { get { return "Cars"; } }
		
		public FileIdList[] cars = new FileIdList[] {
			new FileIdList(new ("bmw", "Cars\\BMW\\bmw.csi")),
			new FileIdList(new ExternalRootNodeCompiler("mercedes", "Cars\\Mercedes\\mercedes.csi")),
			new FileIdList(new ExternalRootNodeCompiler("lexus", "Cars\\Lexus\\lexus.csi"))
		};
	}
}