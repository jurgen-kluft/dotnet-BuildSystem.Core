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

        private static void Add(Bigfile bigfile, uint fileIndex, IReadOnlyList<string> filenames)
        {
            var mainBigfileFile = new BigfileFile(filenames[0])
            {
                FileId = ((ulong)bigfile.Index << 32) | (ulong)fileIndex
            };

            for (var i = 1; i < filenames.Count; ++i)
            {
                var filename = filenames[i];
                var bigfileFile = new BigfileFile(filename)
                {
                    FileId = ((ulong)bigfile.Index << 32) | (ulong)fileIndex
                };
                mainBigfileFile.Children.Add(bigfileFile);
            }

            bigfile.Files.Add(mainBigfileFile);
        }

        public void AssignFileId(List<FileId> gdClOutput)
        {
            // Explanation:
            // A FileId is actually just a combination of the index of the Bigfile and the index of the BigfileFile within the Bigfile
            // The reason for building a FileId like this is that we can easily combine multiple Bigfile and use Bigfile Index to index
            // into that Section.
            var fileIndex = (uint)0;
            foreach (var o in gdClOutput)
            {
                o.BigFileIndex = Index;
                o.FileIndex = fileIndex++;
            }
        }

        public void Save(string filename, List<FileId> gdClOutput)
		{
			var bfb = new BigfileBuilder.BigfileBuilder(BigfileConfig.Platform);

            var bigfile = new Bigfile(Index);
            foreach (var o in gdClOutput)
			{
				Add(bigfile, o.FileIndex, o.FileNames);
			}

            var bigFiles = new List<Bigfile>() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
		}
	}
}
