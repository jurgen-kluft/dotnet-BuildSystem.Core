namespace BigfileBuilder
{
    public static class BigfileBuilder
    {
        // Returns the size of the final Bigfile
        private static ulong Simulate(string dstPath, IReadOnlyList<Bigfile> bigFiles)
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
                    simulator.WriteFile(filename, out var _, out var _);
                }
            }
            simulator.Close();
            return simulator.FinalSize;
        }

        // return true if build was successful
        public static bool Save(string pubPath, string dstPath, string mainBigfileFilename, List<Bigfile> bigFiles)
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
                    writer.WriteFile(Path.Join(dstPath, bigfileFile.Filename), out var fileOffset, out var fileSize);
                    bigfileFile.Size = fileSize;
                    bigfileFile.Offset = fileOffset;
                }
            }

            writer.Close();

            var mainBigfileTocFilename = Path.ChangeExtension(mainBigfileFilename, BigfileConfig.BigFileTocExtension);
            if (BigfileToc.Save(Path.Join(pubPath, mainBigfileTocFilename), bigFiles))
                return true;

            Console.WriteLine("Error saving BigFileToc: {0}", mainBigfileTocFilename);
            return false;
        }

        public static bool Load(string pubPath, string dstPath, string bigfileFilename, List<Bigfile> bigFiles)
        {
            BigfileReader reader = new();
            reader.Open(Path.Join(pubPath, bigfileFilename));

            if (!BigfileToc.Load(Path.Join(pubPath, bigfileFilename), bigFiles))
                return false;

            foreach(var bf in bigFiles)
            {
                foreach(var bff in bf.Files)
                {
                    if (bff.IsValid) continue;

                    Console.WriteLine("No data for file {0}", dstPath + bff.Filename);
                    return false;
                }
            }

            return true;
        }
    }
}
