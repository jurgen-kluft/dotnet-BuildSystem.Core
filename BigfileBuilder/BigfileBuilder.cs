using System;
using System.IO;
using System.Diagnostics;
using GameCore;

///
/// BuildTools: Just Another Bigfile Builder, given a list of files like described
///             below it will process these and output a BigFile and BigfileToc.
///
namespace DataBuildSystem
{
    public class BigfileBuilder
    {
        #region Fields

        private readonly string mBigfileFilename;

        private readonly Dictionary<UInt64, BigfileFile> mFileIdToBigfileFile = new();

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

        public UInt64 MaxFileId { get; set; }

        #endregion
        #region Methods

        /// <summary>
        /// A file to add to the data archive
        /// </summary>
        public void Add(UInt64 fileId, string[] filenames)
        {
            if (!mFileIdToBigfileFile.ContainsKey(fileId))
            {
                BigfileFile mainBigfileFile = null;
                for (int i = 0; i < filenames.Length; ++i)
                {
                    string filename = filenames[i];
                    BigfileFile bigfileFile = new(filename);
                    switch (i == 0)
                    {
                        case true: mainBigfileFile = bigfileFile; bigfileFile.FileId = fileId; break;
                        case false: bigfileFile.FileId = MaxFileId++; break;
                    }
                    mFileIdToBigfileFile.Add(bigfileFile.FileId, bigfileFile);

                    if (i > 0)
                    {
                        mainBigfileFile.ChildFileIds.Add(bigfileFile.FileId);
                    }
                }
            }
            else
            {
                Debug.Assert(false, "Should not already have this FileId");
            }
        }

        private void Simulate()
        {
            StreamOffset currentOffset = new(0);

            // Simulation:
            // Compute the file size and offset for each BigfileFile
            for (UInt64 fileId = 0; fileId < MaxFileId; fileId++)
            {
                if (mFileIdToBigfileFile.ContainsKey(fileId))
                {
                    BigfileFile bigfileFile = mFileIdToBigfileFile[fileId];
                    {
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
                        currentOffset.align(BigfileConfig.FileAlignment);
                    }
                }
            }
        }

        /// <summary>
        /// Build the big file and TOC
        /// </summary>
        /// <returns>True if build was successful</returns>
        public bool Save(EEndian endian, bool buildBigfileData)
        {
            List<BigfileFile> bigfileFiles = new();

            Simulate();
            {
                // Opening the Bigfile
                if (!mBigFile.open(Path.Join(mPubPath, mBigfileFilename), Bigfile.EMode.WRITE))
                {
                    Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                    return false;
                }

                for (UInt64 fileId = 0; fileId < MaxFileId; fileId++)
                {
                    if (mFileIdToBigfileFile.ContainsKey(fileId))
                    {
                        BigfileFile bigfileFile = mFileIdToBigfileFile[fileId];
                        bigfileFiles.Add(bigfileFile);
                    }
                }

                // Write all files to the Bigfile
                if (buildBigfileData)
                {
                    if (!mBigFile.save(mDstPath, bigfileFiles))
                    {
                        Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                        return false;
                    }
                }

                mBigFile.close();
            }

            foreach (var bigfileFile in bigfileFiles)
            {
                mBigFileToc.Add(bigfileFile);
            }

            if (!mBigFileToc.Save(Path.Join(mPubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension)), endian))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mBigfileFilename);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Build the big file and TOC
        /// </summary>
        /// <returns>True if build was successful</returns>
        private bool Build2(string dataPath, EEndian endian)
        {
            Simulate();

            List<BigfileFile> bigfileFiles = new();
            for (UInt64 fileId = 0; fileId < MaxFileId; fileId++)
            {
                if (mFileIdToBigfileFile.ContainsKey(fileId))
                {
                    BigfileFile bigfileFile = mFileIdToBigfileFile[fileId];
                    bigfileFiles.Add(bigfileFile);
                }
            }


            // Build list of stream files that should go into the Bigfile
            List<StreamFile> srcFiles = new();
            foreach (BigfileFile bff in bigfileFiles)
                srcFiles.Add(new StreamFile(Path.Join(dataPath, bff.Filename), bff.FileSize, bff.FileOffset));

            // Write all files to the Bigfile
            StreamBuilder streamBuilder = new();
            streamBuilder.Build(srcFiles, Path.Join(mDstPath, mSubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileExtension)));

            // Create the BigfileToc and add BigfileFiles to it with information from
            // the StreamBuilder.
            BigfileToc bigFileToc = new();
            for (int i = 0; i < bigfileFiles.Count; ++i)
            {
                BigfileFile b = bigfileFiles[i];
                StreamFile s = srcFiles[i];
                bigFileToc.Add(new BigfileFile(b.Filename, b.FileSize, s.FileOffset));
            }

            if (!bigFileToc.Save(mDstPath + mSubPath + mBigfileFilename, endian))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mBigfileFilename);
                return false;
            }

            return true;
        }

        public bool Load(string dstPath, EEndian endian, Dictionary<UInt64, BigfileFile> fileIdToBigfileFile)
        {
            Bigfile bigFile = new();
            bigFile.open(mDstPath + mSubPath + mBigfileFilename, Bigfile.EMode.READ);

            BigfileToc bigFileToc = new();
            if (bigFileToc.Load(Path.Join(mDstPath, mSubPath, mBigfileFilename), endian))
            {
                for (int i = 0; i < bigFileToc.Count; i++)
                {
                    BigfileFile bff = bigFileToc.InfoOf(i);
                    if (bff.IsEmpty)
                    {
                        Console.WriteLine("No info for file {0} with index {1}", dstPath + bff.Filename, i);
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
            if (!mBigFile.open(mDstPath + mSubPath + mBigfileFilename, Bigfile.EMode.WRITE))
            {
                Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                return false;
            }
            // Write all files to the Bigfile
            if (!mBigFile.save(dataPath, bigfileFiles))
            {
                Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                return false;
            }
            mBigFile.close();

            bigfileFiles.ForEach(mBigFileToc.Add);

            if (!mBigFileToc.Save(mDstPath + mSubPath + Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension), endian))
            {
                Console.WriteLine("Error saving {0}", Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension));
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
        public static bool Reorder(string srcFilename, string dataPath, List<BigfileFile> srcBigfileFiles, string dstFilename, List<int> remap, EEndian endian)
        {
            BigfileToc bigfileToc = new();
            List<BigfileFile> dstBigfileFiles = new();

            // Create Toc entries in the same order as old one.
            bigfileToc.CopyFilesOrder(srcBigfileFiles);

            //////////////////////////////////////////////////////////////////////////
            /// Simulation
            StreamOffset currentOffset = new(0);
            Int64 dstFileSize = 0;
            for (int i = 0; i < remap.Count; i++)
            {
                int ri = remap[i];

                // Source
                BigfileFile srcFile = srcBigfileFiles[ri];
                BigfileFile dstFile = new(srcFile);
                dstFile.FileOffset = currentOffset;

                bigfileToc.Add(dstFile);
                dstBigfileFiles.Add(dstFile);

                currentOffset += dstFile.FileSize;
                dstFileSize = currentOffset.value;
                currentOffset.align(BigfileConfig.FileAlignment);
            }

            Bigfile srcBigfile = new();
            Bigfile dstBigfile = new();

            if (srcBigfile.open(srcFilename, Bigfile.EMode.READ))
            {
                if (dstBigfile.open(dstFilename, Bigfile.EMode.WRITE))
                {
                    dstBigfile.setLength(dstFileSize);

                    //////////////////////////////////////////////////////////////////////////
                    /// Writing
                    for (int i = 0; i < remap.Count; i++)
                    {
                        // Source
                        BigfileFile srcFile = srcBigfileFiles[remap[i]];
                        // Destination
                        BigfileFile dstFile = dstBigfileFiles[i];

                        // Copy the file from one Bigfile to another at the same or a different offset
                        srcBigfile.copy(srcFile.FileOffset, (Int64)srcFile.FileSize, dstBigfile, dstFile.FileOffset);
                    }

                    srcBigfile.close();
                    dstBigfile.close();

                    if (!bigfileToc.Save(dstFilename, endian))
                    {
                        Console.WriteLine("Error saving BigFileToc: {0}", dstFilename);
                        return false;
                    }
                }
                else
                {
                    srcBigfile.close();
                }
                return true;
            }
            return false;
        }

        public static bool Exists(string PublishPath, Filename bigFileName)
        {
            return BigfileToc.Exists(PublishPath + bigFileName) && Bigfile.exists(PublishPath + bigFileName);
        }
        #endregion
    }
}


