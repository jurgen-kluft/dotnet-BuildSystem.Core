using GameCore;

namespace DataBuildSystem
{
    public sealed class BigfileBuilder
    {
        public BigfileBuilder(EPlatform platform)
        {
            Platform = platform;
        }

        private EPlatform Platform { get; }

        // Returns the size of the final Bigfile
        private Int64 Simulate(string dstPath, IReadOnlyList<Bigfile> bigFiles)
        {
            // Simulation:
            // Compute the file Id
            foreach (var bigfile in bigFiles)
            {
                var fileId = (Int64)0;
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
                        bigfileFile.FileSize = (Int32)fileInfo.Length;
                    }
                    else
                    {
                        bigfileFile.FileOffset = StreamOffset.Empty;
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
            writer.SetLength(bigfileSize);

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
            if (!bft.Save(Path.Join(pubPath, mainBigfileTocFilename), Platform, bigFiles))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mainBigfileTocFilename);
                return false;
            }

            return true;
        }

        public bool Load(string pubPath, string dstPath, string bigfileFilename, List<Bigfile> bigFiles)
        {
            BigfileReader reader = new();
            reader.Open(Path.Join(pubPath, bigfileFilename));

            BigfileToc bigFileToc = new();
            if (bigFileToc.Load(Path.Join(pubPath, bigfileFilename), Platform, bigFiles))
            {
                foreach(var bf in bigFiles)
                {
                    foreach(var bff in bf.Files)
                    {
                        if (bff.FileOffset != StreamOffset.Empty) continue;

                        Console.WriteLine("No data for file {0} with id {1}", dstPath + bff.Filename, bff.FileId);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool Save2(string pubPath, string dstPath, string bigfileFilename, List<Bigfile> bigFiles)
        {
            var writer = new BigfileWriter();

            // Opening the Bigfile
            if (!writer.Open(Path.Join(pubPath, bigfileFilename)))
            {
                Console.WriteLine("Error opening Bigfile: {0}", bigfileFilename);
                return false;
            }

            // Write all files to the Bigfile
            foreach(var bf in bigFiles)
            {
                foreach(var bff in bf.Files)
                {
                    var offset = writer.Save(Path.Join(dstPath, bff.Filename));
                    if (offset < 0)
                    {
                        Console.WriteLine("Error saving Bigfile: {0}", bigfileFilename);
                        return false;
                    }
                    bff.FileOffset = new StreamOffset(offset);
                }
            }

            writer.Close();

            // Write the TOC
            var bft = new BigfileToc();
            var bftFilePath = Path.Join(pubPath, Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileTocExtension));
            if (bft.Save(bftFilePath, Platform, bigFiles)) return true;

            Console.WriteLine("Error saving {0}", bftFilePath);
            return false;

        }

        /// <summary>
        /// Reorder the current Bigfile by writing the files to a new Bigfile using the reordering map
        /// </summary>
        /// <param name="dataPath">The path of where the data is</param>
        /// <param name="srcFilename">The filename of the source Bigfile</param>
        /// <param name="dstFilename">The filename of the destination Bigfile</param>
        /// <param name="remap">The order in which to write the source BigfileFiles (may contain duplicates)</param>
        /// <param name="littleEndian">The BigfileToc needs to know the endian</param>
        /// <returns>True if all went ok</returns>
        public static bool Reorder(string dataPath, string srcFilename, string dstFilename, List<int> remap, bool littleEndian)
        {
            BigfileToc bigfileToc = new();
            List<BigfileFile> dstBigfileFiles = new();

            // TODO Create Toc entries in the same order as old one.

            return false;
        }

    }
}
