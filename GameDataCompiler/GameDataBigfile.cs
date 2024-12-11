using GameData;
using BigfileBuilder;

namespace DataBuildSystem
{
	internal sealed class GameDataBigfile
	{
        public GameDataBigfile(uint index)
        {
            Index = index;
        }

        private uint Index { get; }

        public void Save(string filename, List<IDataFile> dataFiles)
		{
			var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

            var bigfile = new Bigfile(Index);
            foreach (var o in dataFiles)
            {
                var mainBigfileFile = new BigfileFile(o.CookedFilename);
                bigfile.Files.Add(mainBigfileFile);
			}

            var bigFiles = new List<Bigfile>() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
		}
	}
}
