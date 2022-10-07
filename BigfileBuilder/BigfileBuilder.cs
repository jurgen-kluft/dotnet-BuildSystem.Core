using System;
using System.IO;
using System.Diagnostics;
using GameCore;

// BuildTools: Just Another Bigfile Builder, given a list of files like described
//             below it will process these and output a BigFile and BigfileToc.
//
namespace DataBuildSystem
{
    using u64 = UInt64;
    using s64 = Int64;

    public sealed class BigfileBuilder
    {
        #region Fields

        private readonly string mBigfileFilename;

        private readonly string mDstPath;
        private readonly string mSubPath;
        private readonly string mPubPath;

        private readonly List<Bigfile> mBigFiles;
        private readonly BigfileToc mBigFileToc;

        #endregion
        #region Constructor

        public BigfileBuilder(string dstPath, string subPath, string pubPath, string bigfileFilename)
        {
            mDstPath = dstPath;
            mSubPath = subPath;
            mPubPath = pubPath;

            mBigfileFilename = bigfileFilename;

            mBigFiles = new();
            mBigFileToc = new();
        }

        #endregion
        #region Properties

        #endregion
        #region Methods

        /// <summary>
        /// A file to add to the data archive
        /// </summary>
        public void Add(s64 fileId, string[] filenames)
        {
            BigfileFile mainBigfileFile = new(filenames[0]);
            for (int i = 1; i < filenames.Length; ++i)
            {
                string filename = filenames[i];
                BigfileFile bigfileFile = new(filename);
                bigfileFile.FileId = -1;
                mainBigfileFile.Children.Add(bigfileFile);
            }

            // TODO Determine the Bigfile linked to this FileId
            mBigFiles[0].Files.Add(mainBigfileFile);
        }

        // Returns the size of the final Bigfile
        private Int64 Simulate()
        {
            // Simulation:
            // Compute the file Id
            for (int i = 0; i < mBigFiles.Count; i++)
            {
                Bigfile bigfile = mBigFiles[i];
                
                s64 fileId = 0;
                for (int j = 0; j < bigfile.Files.Count; j++)
                {
                    BigfileFile bigfileFile = bigfile.Files[j];
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
            StreamOffset currentOffset = new(0);
            for (int i = 0; i < mBigFiles.Count; i++)
            {
                Bigfile bigfile = mBigFiles[i];

                for (int j = 0; j < bigfile.Files.Count; j++)
                {
                    BigfileFile bigfileFile = bigfile.Files[j];

                    FileInfo fileInfo = new(Path.Join(mDstPath, bigfileFile.Filename));
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
            return currentOffset.value;
        }

        /// <summary>
        /// Build the big file and TOC
        /// </summary>
        /// <returns>True if build was successful</returns>
        public bool Save(EEndian endian, bool buildBigfileData)
        {
            BigfileWriter writer = new ();

            // Opening the Bigfile
            if (!writer.Open(Path.Join(mPubPath, mBigfileFilename)))
            {
                Console.WriteLine("Error opening BigfileWriter: {0}", mBigfileFilename);
                return false;
            }

            Int64 bigfileSize = Simulate();
            writer.SetLength(bigfileSize);

            // Write all files to the Bigfile
            if (buildBigfileData)
            {
                for (int i = 0; i < mBigFiles.Count; i++)
                {
                    Bigfile bigfile = mBigFiles[i];

                    for (int j = 0; j < bigfile.Files.Count; j++)
                    {
                        BigfileFile bigfileFile = bigfile.Files[i];
                        Int64 offset = writer.Save(Path.Join(mDstPath, bigfileFile.Filename));
                        bigfileFile.FileOffset = new StreamOffset(offset);
                    }
                }
            }

            writer.Close();

            BigfileToc bft = new BigfileToc();
            if (!bft.Save(Path.Join(mPubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension)), endian, mBigFiles))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mBigfileFilename);
                return false;
            }

            return true;
        }

        public bool Load(string dstPath, EEndian endian, List<Bigfile> bigfiles)
        {
            BigfileReader reader = new();
            reader.Open(Path.Join(mDstPath, mSubPath, mBigfileFilename));
            
            BigfileToc bigFileToc = new();
            if (bigFileToc.Load(Path.Join(mDstPath, mSubPath, mBigfileFilename), endian, bigfiles))
            {
                foreach(var bf in bigfiles)
                {
                    foreach(var bff in bf.Files)
                    {
                        if (bff.IsEmpty)
                        {
                            Console.WriteLine("No info for file {0} with id {1}", dstPath + bff.Filename, bff.FileId);
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool Save(string dataPath, List<Bigfile> bigfiles, EEndian endian)
        {
            BigfileWriter writer = new();

            // Opening the Bigfile
            if (!writer.Open(Path.Join(mDstPath, mSubPath, mBigfileFilename)))
            {
                Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                return false;
            }

            // Write all files to the Bigfile
            foreach(var bf in bigfiles)
            {
                foreach(var bff in bf.Files)
                {
                    Int64 offset = writer.Save(Path.Join(dataPath, bff.Filename));
                    bff.FileOffset = new StreamOffset(offset);
                    if (offset < 0)
                    {
                        Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                        return false;
                    }
                }
            }
            // Close the Bigfile
            writer.Close();

            // Write the TOC
            BigfileToc bft = new();
            string bftFilePath = Path.Join(mDstPath, mSubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension));
            if (!bft.Save(bftFilePath, endian, bigfiles))
            {
                Console.WriteLine("Error saving {0}", bftFilePath);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reorder the current Bigfile by writing the files to a new Bigfile using the reordering map
        /// </summary>
        /// <param name="srcFilename">The filename of the source Bigfile</param>
        /// <param name="dataPath">The path of where the data is</param>
        /// <param name="srcBigfileFiles">The BigfileFiles of the source Bigfile</param>
        /// <param name="dstFilename">The filename of the destination Bigfile</param>
        /// <param name="remap">The order in which to write the source BigfileFiles (may contain duplicates)</param>
        /// <param name="endian">The BigfileToc needs to know the endian</param>
        /// <returns>True if all went ok</returns>
        public static bool Reorder(string dataPath, string srcFilename, string dstFilename, List<int> remap, EEndian endian)
        {
            BigfileToc bigfileToc = new();
            List<BigfileFile> dstBigfileFiles = new();

            // TODO
            // Create Toc entries in the same order as old one.

            return false;
        }

        #endregion
    }
}
