using GameCore;

namespace BigfileBuilder
{
    public sealed class BigfileBuilder
    {
        public BigfileBuilder(EPlatform platform)
        {
            Platform = platform;
        }

        private EPlatform Platform { get; }

        // Returns the size of the final Bigfile
        private long Simulate(string dstPath, IReadOnlyList<Bigfile> bigFiles)
        {
            // Simulation:
            // Compute the file Id
            foreach (var bigfile in bigFiles)
            {
                var fileId = (long)0;
                foreach (var bigfileFile in bigfile.Files)
                {
                    if (bigfileFile.FileId == -1)
                    {
                        bigfileFile.FileId = fileId;
                    }
                    else
                    {
                        fileId = bigfileFile.FileId;
                    }
                }
            }

            // Simulation:
            // Compute the file size and offset for each BigfileFile
            var currentOffset = new StreamOffset(0);
            foreach (var bigfile in bigFiles)
            {
                foreach (var bigfileFile in bigfile.Files)
                {
                    var fileInfo = new FileInfo(Path.Join(dstPath, bigfileFile.Filename));
                    if (fileInfo.Exists)
                    {
                        bigfileFile.FileOffset = new(currentOffset);
                        bigfileFile.FileSize = fileInfo.Length;
                    }
                    else
                    {
                        bigfileFile.FileOffset = StreamOffset.sEmpty;
                        bigfileFile.FileSize = 0;
                    }

                    currentOffset += bigfileFile.FileSize;
                    currentOffset.Align(BigfileConfig.FileAlignment);
                }
            }
            return currentOffset.Offset;
        }

        // return true if build was successful
        public bool Save(string pubPath, string dstPath, string mainBigfileFilename, List<Bigfile> bigFiles)
        {
            var writer = new BigfileWriter();

            // Opening the Bigfile
            if (!writer.Open(Path.Join(pubPath, mainBigfileFilename)))
            {
                Console.WriteLine("Error opening BigfileWriter: {0}", mainBigfileFilename);
                return false;
            }

            // Run the simulation so that we know the final file size of the Bigfile
            var bigfileSize = Simulate(dstPath, bigFiles);
            writer.AddSize(bigfileSize);

            // Write all files to the Bigfile Archive
            foreach (var bigfile in bigFiles)
            {
                foreach (var bigfileFile in bigfile.Files)
                {
                    var offset = writer.Save(Path.Join(dstPath, bigfileFile.Filename));
                    bigfileFile.FileOffset = new StreamOffset(offset);
                }
            }

            writer.Close();

            var bft = new BigfileToc();
            var mainBigfileTocFilename = Path.ChangeExtension(mainBigfileFilename, BigfileConfig.BigFileTocExtension);
            if (bft.Save(Path.Join(pubPath, mainBigfileTocFilename), Platform, bigFiles))
                return true;

            Console.WriteLine("Error saving BigFileToc: {0}", mainBigfileTocFilename);
            return false;
        }

        public bool Load(string pubPath, string dstPath, string bigfileFilename, List<Bigfile> bigFiles)
        {
            BigfileReader reader = new();
            reader.Open(Path.Join(pubPath, bigfileFilename));

            BigfileToc bigFileToc = new();
            if (!bigFileToc.Load(Path.Join(pubPath, bigfileFilename), Platform, bigFiles))
                return false;

            foreach(var bf in bigFiles)
            {
                foreach(var bff in bf.Files)
                {
                    if (bff.FileOffset != StreamOffset.sEmpty) continue;

                    Console.WriteLine("No data for file {0} with id {1}", dstPath + bff.Filename, bff.FileId);
                    return false;
                }
            }

            return true;
        }
    }
}
