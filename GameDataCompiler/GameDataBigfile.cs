using GameData;
using BigfileBuilder;

namespace DataBuildSystem
{
	internal sealed class GameDataBigfile
	{
        public GameDataBigfile(int index)
        {
            Index = index;
        }

        private int Index { get; }

        private static void Add(Bigfile bigfile, long fileId, IReadOnlyList<string> filenames, ICollection<BigfileFile> children)
        {
            var mainBigfileFile = new BigfileFile(filenames[0]);
            for (var i = 1; i < filenames.Count; ++i)
            {
                var filename = filenames[i];
                var bigfileFile = new BigfileFile(filename)
                {
                    FileId = fileId
                };
                mainBigfileFile.Children.Add(bigfileFile);
                children.Add(bigfileFile);
            }

            bigfile.Files.Add(mainBigfileFile);
        }

        public void AssignFileId(List<DataCompilerOutput> gdClOutput)
        {
            // Explanation:
            // A FileId is actually just a combination of the index of the Bigfile and the index of the BigfileFile within the Bigfile
            // The reason for building a FileId like this is that we can easily combine multiple Bigfile and use Bigfile Index to index
            // into that Section.
            var bigFileIndex = (long)Index << 32;
            foreach (var o in gdClOutput)
            {
                var fileId = (bigFileIndex | o.FileIdProvider.FileId);
                o.FileIdProvider.FileId = fileId;
            }
        }

        public void Save(string filename, List<DataCompilerOutput> gdClOutput)
		{
			var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

            var bigfile = new Bigfile(Index);
            var children = new List<BigfileFile>();

            foreach (var o in gdClOutput)
			{
				Add(bigfile, o.FileIdProvider.FileId, o.Filenames, children);
			}

            var bigFiles = new List<Bigfile>() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
		}
	}
}
