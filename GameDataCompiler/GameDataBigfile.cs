using GameData;
using BigfileBuilder;

namespace DataBuildSystem
{
	internal sealed class GameDataBigfile
	{
        private static void Add(Bigfile bigfile, long fileId, IReadOnlyList<string> filenames, ICollection<BigfileFile> children)
        {
            var mainBigfileFile = new BigfileFile(filenames[0]);
            for (var i = 1; i < filenames.Count; ++i)
            {
                var filename = filenames[i];
                var bigfileFile = new BigfileFile(filename)
                {
                    FileId = -1
                };
                mainBigfileFile.Children.Add(bigfileFile);
                children.Add(bigfileFile);
            }

            // TODO Determine the Bigfile linked to this FileId
            bigfile.Files.Add(mainBigfileFile);
        }

        public void Save(string filename, List<DataCompilerOutput> gdClOutput)
		{
			var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

            var bigfile = new Bigfile();
            var children = new List<BigfileFile>();
            var fileId = (long)0;
            foreach (var o in gdClOutput)
			{
				Add(bigfile, fileId++, o.Filenames, children);
			}
            foreach(var c in children)
			{
                c.FileId = fileId++;
                bigfile.Files.Add(c);
			}

            var bigFiles = new List<Bigfile>() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
		}
	}
}
