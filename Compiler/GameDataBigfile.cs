using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameData;

namespace DataBuildSystem
{
	internal sealed class GameDataBigfile
	{
		public void Save(string filename, List<DataCompilerOutput> gdcl_output)
		{
			BigfileBuilder bfb = new BigfileBuilder(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.PubPath, filename);
			bfb.Save(BuildSystemCompilerConfig.Endian, true);
		}
	}
}
