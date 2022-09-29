using System;
using System.IO;
using System.Collections.Generic;
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

        private List<Hash160> mFileHash = new List<Hash160>();
        private List<BigfileFile> mBigfileFiles = new List<BigfileFile>();
        private Dictionary<string, int> mFile = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        private string mDstPath;
        private string mSubPath;
        private string mPubPath;

        private Bigfile mBigFile;
        private BigfileToc mBigFileToc;

        #endregion
        #region Constructor

        public BigfileBuilder(string dstPath, string subPath, string pubPath, string bigfileFilename)
        {
            mDstPath = dstPath;
            mSubPath = subPath;
            mPubPath = pubPath;

            mBigfileFilename = bigfileFilename;

            mBigFile = new Bigfile();
            mBigFileToc = new BigfileToc();
        }

        #endregion
        #region Methods

        /// <summary>
        /// A file to add to the data archive, return the fileId
        /// </summary>
        public Int32 add(string filename)
        {
            int i;
            if (!mFile.TryGetValue(filename, out i))
            {
                i = mFileHash.Count;
                mFile.Add(filename, i);
                mFileHash.Add(Hash160.Empty);
            }
            return i;
        }
        public Int32 add(string filename, Hash160 md5)
        {
            int i;
            if (!mFile.TryGetValue(filename, out i))
            {
                i = mFileHash.Count;
                mFile.Add(filename, i);
                mFileHash.Add(md5);
            }
            return i;
        }

        private void simulate()
        {
            StreamOffset currentOffset = new StreamOffset(0);

            // Simulation
            // MD5 -> <Offset, Size> dictionary
            mBigfileFiles.Clear();
            Dictionary<Hash160, BigfileFile> md5Dictionary = new Dictionary<Hash160, BigfileFile>();
            foreach (KeyValuePair<string, int> p in mFile)
            {
                string f = p.Key;
                Hash160 fileHash = mFileHash[p.Value];

                BigfileFile existingBigfileFile;
                if (md5Dictionary.TryGetValue(fileHash, out existingBigfileFile))
                {
                    // File already exists in the Bigfile, so add it to the BigfileToc using the same file offset and file size.
                    BigfileFile duplicateBigfileFile = new BigfileFile(existingBigfileFile);
                    duplicateBigfileFile.filename = f;
                    mBigFileToc.add(duplicateBigfileFile);
                    mBigfileFiles.Add(existingBigfileFile);
                }
                else
                {
                    StreamOffset offsetToFile;
                    UInt32 sizeOfFile;
                    FileInfo fileInfo = new FileInfo(mDstPath + f);
                    if (fileInfo.Exists)
                    {
                        offsetToFile = new StreamOffset(currentOffset);
                        sizeOfFile = (UInt32)fileInfo.Length;
                    }
                    else
                    {
                        offsetToFile = StreamOffset.Empty;
                        sizeOfFile = 0;
                    }

                    BigfileFile newBigfileFile = new BigfileFile(f, sizeOfFile, offsetToFile, fileHash);

                    if (!BigfileConfig.AllowDuplicateFiles)
                    {
                        // Keep track of the MD5 in the dictionary so that duplicate files can be tracked!
                        md5Dictionary.Add(fileHash, newBigfileFile);
                    }

                    currentOffset += sizeOfFile;
                    currentOffset.align(BigfileConfig.FileAlignment);

                    mBigFileToc.add(newBigfileFile);
                    mBigfileFiles.Add(newBigfileFile);
                }
            }
        }

        /// <summary>
        /// Build the big file and TOC
        /// </summary>
        /// <returns>True if build was successful</returns>
        public bool save(EEndian endian, bool buildBigfileData)
        {
            simulate();
            {
                // Opening the Bigfile
                if (!mBigFile.open(Path.Join(mPubPath, mBigfileFilename), Bigfile.EMode.WRITE))
                {
                    Console.WriteLine("Error opening Bigfile: {0}", mBigfileFilename);
                    return false;
                }

                // Write all files to the Bigfile
                if (buildBigfileData)
                {
                    if (!mBigFile.save(mDstPath, mBigfileFiles))
                    {
                        Console.WriteLine("Error saving Bigfile: {0}", mBigfileFilename);
                        return false;
                    }
                }

                mBigFile.close();
            }

            if (!mBigFileToc.save(Path.Join(mPubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension)), endian))
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
        public bool build2(string dataPath, EEndian endian)
        {
            List<BigfileFile> bigfileFiles = new List<BigfileFile>();

            // MD5 -> <Offset, Size> dictionary
            Dictionary<Hash160, BigfileFile> md5Dictionary = new Dictionary<Hash160, BigfileFile>();
            foreach (KeyValuePair<string, int> p in mFile)
            {
                string f = p.Key;
                Hash160 fileHash = mFileHash[p.Value];

                BigfileFile existingBigfileFile;
                if (!md5Dictionary.TryGetValue(fileHash, out existingBigfileFile))
                {
                    FileInfo fileInfo = new FileInfo(Path.Join(dataPath , f));
                    if (fileInfo.Exists)
                    {
                        UInt32 sizeOfFile = (UInt32)fileInfo.Length;

                        BigfileFile newBigfileFile = new BigfileFile(f, sizeOfFile);
                        bigfileFiles.Add(newBigfileFile);

                        if (!BigfileConfig.AllowDuplicateFiles)
                        {
                            // Keep track of the MD5 in the dictionary so that duplicate files can be tracked!
                            md5Dictionary.Add(fileHash, newBigfileFile);
                        }
                    }
                }
            }

            // Build list of stream files that should go into the Bigfile
            List<StreamFile> srcFiles = new List<StreamFile>();
            foreach(BigfileFile bff in bigfileFiles)
                srcFiles.Add(new StreamFile(Path.Join(dataPath, bff.filename), bff.size, bff.offsets));

            // Write all files to the Bigfile
            StreamBuilder streamBuilder = new StreamBuilder();
            streamBuilder.Build(srcFiles, Path.Join(mDstPath , mSubPath, Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileExtension)));

            // Create the BigfileToc and add BigfileFiles to it with information from
            // the StreamBuilder.
            BigfileToc bigFileToc = new BigfileToc();
            for (int i = 0; i < bigfileFiles.Count; ++i)
            {
                BigfileFile b = bigfileFiles[i];
                StreamFile  s = srcFiles[i];
                bigFileToc.add(new BigfileFile(b.filename, b.size, s.offset));
            }

            if (!bigFileToc.save(mDstPath + mSubPath + mBigfileFilename, endian))
            {
                Console.WriteLine("Error saving BigFileToc: {0}", mBigfileFilename);
                return false;
            }

            return true;
        }

        public bool load(string dstPath, EEndian endian, Dictionary<string, Hash160> filenameToHashDictionary)
        {
            mFile.Clear();
            mFileHash.Clear();

            Bigfile bigFile = new Bigfile();
            bigFile.open(mDstPath + mSubPath + mBigfileFilename, Bigfile.EMode.READ);

            BigfileToc bigFileToc = new BigfileToc();
            if (bigFileToc.load(Path.Join(mDstPath , mSubPath , mBigfileFilename), endian))
            {
                for (int i = 0; i < bigFileToc.Count; i++)
                {
                    BigfileFile bff = bigFileToc.infoOf(i);
                    if (bff.IsEmpty)
                    {
                        Console.WriteLine("No info for file {0} with index {1}", dstPath + bff.filename, i);
                        return false;
                    }
                    Hash160 hash;
                    if (!filenameToHashDictionary.TryGetValue(bff.filename, out hash))
                    {
                        filenameToHashDictionary.Add(bff.filename, bff.contenthash);
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool save(string dataPath, List<BigfileFile> bigfileFiles, EEndian endian)
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

            bigfileFiles.ForEach(mBigFileToc.add);

            if (!mBigFileToc.save(mDstPath + mSubPath + Path.ChangeExtension(mBigfileFilename, BigfileConfig.BigFileTocExtension), endian))
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
        public static bool sReorder(Filename srcFilename, string dataPath, List<BigfileFile> srcBigfileFiles, Filename dstFilename, List<int> remap, EEndian endian)
        {
            BigfileToc bigfileToc = new BigfileToc();
            List<BigfileFile> dstBigfileFiles = new List<BigfileFile>();

			// Create Toc entries in the same order as old one.
			bigfileToc.copyFilesOrder(srcBigfileFiles);

            //////////////////////////////////////////////////////////////////////////
            /// Simulation
            StreamOffset currentOffset = new StreamOffset(0);
            Int64 dstFileSize = 0;
            for (int i = 0; i < remap.Count; i++)
            {
                int ri = remap[i];

                // Source
                BigfileFile srcFile = srcBigfileFiles[ri];
                BigfileFile dstFile = new BigfileFile(srcFile);
                dstFile.offset = currentOffset;

                bigfileToc.add(dstFile);
                dstBigfileFiles.Add(dstFile);

                currentOffset += dstFile.size;
                dstFileSize = currentOffset.value;
                currentOffset.align(BigfileConfig.FileAlignment);
            }

            Bigfile srcBigfile = new Bigfile();
            Bigfile dstBigfile = new Bigfile();

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
                        srcBigfile.copy(srcFile.offset, (Int64)srcFile.size, dstBigfile, dstFile.offset);
                    }

                    srcBigfile.close();
                    dstBigfile.close();

                    if (!bigfileToc.save(dstFilename, endian))
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

		public static bool exists(string PublishPath, Filename bigFileName)
		{
			return BigfileToc.exists(PublishPath + bigFileName) && Bigfile.exists(PublishPath + bigFileName);
		}
        #endregion
    }
}


