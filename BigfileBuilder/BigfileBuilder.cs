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
            // Compute the final file size of the bigfile
            var simulator = new BigfileWriterSimulator();
            simulator.Open(string.Empty, 0);
            foreach (var bigfile in bigFiles)
            {
                foreach (var bigfileFile in bigfile.Files)
                {
                    var filename = Path.Join(dstPath, bigfileFile.Filename);
                    simulator.Save(filename);
                }
            }
            simulator.Close();
            return simulator.FinalSize;
        }

        // return true if build was successful
        public bool Save(string pubPath, string dstPath, string mainBigfileFilename, List<Bigfile> bigFiles)
        {
            var writer = new BigfileWriter();

            // Run the simulation so that we know the final file size of the Bigfile
            var bigfileFinalSize = Simulate(dstPath, bigFiles);

            // Opening the Bigfile
            if (!writer.Open(Path.Join(pubPath, mainBigfileFilename), bigfileFinalSize))
            {
                Console.WriteLine("Error opening BigfileWriter: {0}", mainBigfileFilename);
                return false;
            }

            // Write all files to the Bigfile Archive
            foreach (var bigfile in bigFiles)
            {
                foreach (var bigfileFile in bigfile.Files)
                {
                    var (ok, fileOffset, fileSize) = writer.Save(Path.Join(dstPath, bigfileFile.Filename));
                    bigfileFile.FileSize = fileSize;
                    bigfileFile.FileOffset = ok ? new StreamOffset(fileOffset) : StreamOffset.sEmpty;
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
