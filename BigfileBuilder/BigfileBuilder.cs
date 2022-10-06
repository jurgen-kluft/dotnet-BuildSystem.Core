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

        private readonly List<BigfileFile> mBigfileFiles = new();

        private readonly string mDstPath;
        private readonly string mSubPath;
        private readonly string mPubPath;

        private readonly Bigfile mBigFile;
        private readonly BigfileToc mBigFileToc;

        #endregion
        #region Constructor

        public BigfileBuilder(string dstPath, string subPath, string pubPath, string bigfileFilename)
        {
            mDstPath = dstPath;
            mSubPath = subPath;
            mPubPath = pubPath;

            mBigfileFilename = bigfileFilename;

            mBigFile = new();
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
            for (int i = 0; i < filenames.Length; ++i)
            {
                string filename = filenames[i];
                BigfileFile bigfileFile = new(filename);
                switch (i == 0)
                {
                    case true:
                        bigfileFile.FileId = fileId;
                        break;
                    case false:
                        bigfileFile.FileId = -1;
                        mBigfileFiles[0].Children.Add(bigfileFile);
                        break;
                }
                mBigfileFiles.Add(bigfileFile);
            }
        }

        private void Simulate()
        {
            StreamOffset currentOffset = new(0);

            s64 prevFileId = -1;
            for (int fileId = 0; fileId < mBigfileFiles.Count; fileId++)
            {
                BigfileFile bigfileFile = mBigfileFiles[fileId];
                if (bigfileFile.FileId == -1)
                {
                    bigfileFile.FileId = prevFileId;
                }
                else
                {
                    prevFileId = bigfileFile.FileId;
                }
            }

            // Simulation:
            // Compute the file Id, file size and offset for each BigfileFile
            for (int fileId = 0; fileId < mBigfileFiles.Count; fileId++)
            {
                BigfileFile bigfileFile = mBigfileFiles[fileId];

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

        /// <summary>
        /// Build the big file and TOC
        /// </summary>
        /// <returns>True if build was successful</returns>
        public bool Save(EEndian endian, bool buildBigfileData)
        {
            Simulate();

            BigfileFile[] bigfileFiles = new BigfileFile[mBigfileFiles.Count];
            {
                // Opening the Bigfile
                if (!mBigFile.Open(Path.Join(mPubPath, mBigfileFilename), Bigfile.EMode.Write))
                {
                    Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                    return false;
                }

                for (int i = 0; i < mBigfileFiles.Count; i++)
                {
                    BigfileFile bigfileFile = mBigfileFiles[i];
                    bigfileFiles[bigfileFile.FileId] = bigfileFile;
                }

                // Write all files to the Bigfile
                if (buildBigfileData)
                {
                    if (!mBigFile.Save(mDstPath, bigfileFiles))
                    {
                        Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                        return false;
                    }
                }

                mBigFile.Close();
            }

            BigfileToc bft = new BigfileToc();
            if (!bft.Save(Path.Join(mPubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension)), endian, bigfileFiles))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mBigfileFilename);
                return false;
            }

            return true;
        }

        public bool Load(string dstPath, EEndian endian, Dictionary<s64, BigfileFile> fileIdToBigfileFile)
        {
            Bigfile bigFile = new();
            bigFile.Open(mDstPath + mSubPath + mBigfileFilename, Bigfile.EMode.Read);

            BigfileToc bigFileToc = new();
            if (bigFileToc.Load(Path.Join(mDstPath, mSubPath, mBigfileFilename), endian, out List<BigfileToc.TocSection> sections, out List<BigfileToc.ITocEntry> entries))
            {
                foreach(var e in entries)
                {
                    BigfileFile bff = new(e.Filename, e.FileSize, e.FileOffset, e.FileId, e.FileContentHash);
                    if (bff.IsEmpty)
                    {
                        Console.WriteLine("No info for file {0} with id {1}", dstPath + bff.Filename, e.FileId);
                        return false;
                    }

                    if (!fileIdToBigfileFile.ContainsKey(bff.FileId))
                    {
                        fileIdToBigfileFile.Add(bff.FileId, bff);
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool Save(string dataPath, List<BigfileFile> bigfileFiles, EEndian endian)
        {
            // Opening the Bigfile
            if (!mBigFile.Open(mDstPath + mSubPath + mBigfileFilename, Bigfile.EMode.Write))
            {
                Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                return false;
            }

            // Write all files to the Bigfile
            if (!mBigFile.Save(dataPath, bigfileFiles.ToArray()))
            {
                Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                return false;
            }

            mBigFile.Close();

            BigfileToc bft = new();
            string bftFilePath = Path.Join(mDstPath, mSubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension));
            if (!bft.Save(bftFilePath, endian, bigfileFiles.ToArray()))
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
