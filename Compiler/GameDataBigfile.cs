using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildSystem
{
	internal class GameDataBigfile
	{
		public void Save(string filename, List<string> dst_relative_filepaths)
		{
			BigfileBuilder bfb = new BigfileBuilder(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.PubPath, filename);
			bfb.save(BuildSystemCompilerConfig.Endian, true);
		}
	}
}
