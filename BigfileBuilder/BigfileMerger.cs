using System;
using System.IO;
using System.Collections.Generic;
using GameCore;

//
// BuildTools: Merging of multiple Bigfile and BigfileToc
//
namespace DataBuildSystem
{
    // <redesign>
    //
    // A FileId can be divided into {SectionId + FileId} so we
    // do not really have to 're-compute' the FileId's when merging
    // Bigfiles.
    //
    // So we can actually merge TOCs and only need to Touch the
    // Offset of each TocEntry.
    // Maybe this is not even necessary, each TOC could also hold
    // an extra 'member' that holds the base offset in the Bigfile
    // of the file data.
    //
    // </redesign>

    public sealed class BigfileMerger
    {
        #region Fields

        private readonly string mMainBigfileFolder;
        private readonly string mMainBigfileFilename;
        private List<string> mBigfileFilenames = new ();
        private List<string> mBigfileTocFilenames = new ();

        #endregion
        #region Constructor

        public BigfileMerger(Dirname mainBigfileFolder, Filename mainBigfileFilename)
        {
            mMainBigfileFolder = mainBigfileFolder;
            mMainBigfileFilename = mainBigfileFilename;
        }

        #endregion
        #region Public Methods

        public void Add(Filename bigfileFilename)
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
            foreach (string bfn in mBigfileFilenames)
            {
                FileInfo fileInfo = new (bfn);
                Hash160 fileHash = HashUtility.compute(fileInfo);

                // TODO fix merging of bigfiles
                //bfb.Add(bfn, fileHash);

                //TODO: This needs to be implemented!
                StreamOffset offset = new ();
                mainStreamOffsets.Add(offset);
            }

            bfb.Save(BuildSystemCompilerConfig.Endian, mergeBigfileData);

            // TODO:
            // Write out the new format of a merged bigfile TOC
            // This requires an extra indirection table where the top 32 bit part of the FileId
            // is used as an index into an array of TOCs.


            // Create the 'total' BigfileToc
            // The entries are kept in the order as they are added to the Toc.
            // Initialize a list of TFileId base values
            List<Int32> fileIdOffsetList = new ();
            List<BigfileFile> bigfileFiles = new();
            // Initialize an initial TFileId offset
            Int32 fileIdBase = 0;
            // For every BigfileToc:
            for (int i = 0; i < mBigfileTocFilenames.Count; ++i)
            {
                string bft = mBigfileTocFilenames[i];

                //   Load it
                BigfileToc toc = new ();
                toc.Load(bft, BuildSystemCompilerConfig.Endian, out List<BigfileToc.TocSection> sections, out List<BigfileToc.ITocEntry> entries);

                //   Add TFileId offset to the list
                fileIdOffsetList.Add(fileIdBase);

                //   Every item in the BigfileToc add the offset and add it to the 'total' BigfileToc
                for (int j=0; j<entries.Count; ++j)
                {
                    BigfileToc.ITocEntry e = entries[i];
                    BigfileFile bff = new(e.Filename, e.FileSize, e.FileOffset, e.FileId, e.FileContentHash);
                    bff.FileOffset += mainStreamOffsets[i];
                    bigfileFiles.Add(bff);
                }

                // Add count 'number of items in BigfileToc' to main FileId
                fileIdBase += entries.Count;
            }

            BigfileToc mainBigfileToc = new ();

        }

        #endregion
    }
}


