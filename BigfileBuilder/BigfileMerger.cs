using System;
using System.IO;
using System.Collections.Generic;
using GameCore;

///
/// BuildTools: Merging of multiple Bigfile and BigfileToc
///
namespace DataBuildSystem
{
    /// <redesign>
    ///   
    /// A FileId can be divided into {SectionId + FileId} so we
    /// do not really have to 're-compute' the FileId's when merging
    /// Bigfiles.
    /// 
    /// So we can actually merge TOC's and only need to patch the 
    /// Offset of each TocEntry.
    /// 
    /// </redesign>

    public class BigfileMerger
    {
        #region Fields

        private readonly string mMainBigfileFolder;
        private readonly string mMainBigfileFilename;
        private List<Filename> mBigfileFilenames = new List<Filename>();
        private List<Filename> mBigfileTocFilenames = new List<Filename>();

        #endregion
        #region Constructor
        
        public BigfileMerger(Dirname mainBigfileFolder, Filename mainBigfileFilename)
        {
            mMainBigfileFolder = mainBigfileFolder;
            mMainBigfileFilename = mainBigfileFilename;
        }

        #endregion
        #region Public Methods

        public void add(Filename bigfileFilename)
        {
            mBigfileFilenames.Add(bigfileFilename);
            mBigfileTocFilenames.Add(bigfileFilename.ChangedExtension(BigfileConfig.BigFileTocExtension));
        }

        /// <summary>
        /// Merge the Bigfiles and BigfileTocs and output one Bigfile and one BigfileToc
        /// </summary>
        public void MergeInto(string inDstPath, string inSubPath, string inPubPath, string inBigfilePath, string inBigfileFilename, bool mergeBigfileData)
        {
            // Initialize a list of main Bigfile StreamOffsets of every sub Bigfile that is streamed into it, we need this for the BigfileToc items
            List<StreamOffset> mainStreamOffsets = new ();

            BigfileBuilder bfb = new (inDstPath, inSubPath, inPubPath, inBigfileFilename);
            foreach (Filename bfn in mBigfileFilenames)
            {
                FileInfo fileInfo = new (bfn);
                Hash160 fileHash = HashUtility.compute(fileInfo);
                bfb.add(bfn, fileHash);

                //TODO: This needs to be implemented!
                StreamOffset offset = new ();
                mainStreamOffsets.Add(offset);
            }

            bfb.save(BuildSystemCompilerConfig.Endian, mergeBigfileData);

            // Create the 'total' BigfileToc
            // The entries are kept in the order as they are added to the Toc.
            BigfileToc mainBigfileToc = new ();
            // Initialize a list of TFileId base values
            List<Int32> fileIdOffsetList = new ();
            // Initialize an initial TFileId offset
            Int32 fileIdBase = 0;
            // For every BigfileToc:
            for (int i = 0; i < mBigfileTocFilenames.Count; ++i)
            {
                Filename bft = mBigfileTocFilenames[i];

                //   Load it
                BigfileToc toc = new ();
                toc.load(bft, BuildSystemCompilerConfig.Endian);

                //   Add TFileId offset to the list
                fileIdOffsetList.Add(fileIdBase);

                //   Every item in the BigfileToc add the offset and add it to the 'total' BigfileToc
                for (int j=0; j<toc.Count; ++j)
                {
                    BigfileFile bff = toc.infoOf(j);
                    bff.offset += mainStreamOffsets[i];
                    mainBigfileToc.add(bff, false);
                }
                //   Add count 'number of items in BigfileToc' to main TFileId
                fileIdBase += toc.Count;
            }
        }

        #endregion
    }
}


