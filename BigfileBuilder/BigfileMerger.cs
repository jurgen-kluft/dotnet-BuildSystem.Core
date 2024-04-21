using System;
using System.IO;
using System.Collections.Generic;
using GameCore;

//
// BuildTools: Merging of multiple Bigfiles and BigfileTocs
//
namespace DataBuildSystem
{

    public sealed class BigfileMerger
    {
        public BigfileMerger()
        {
        }

        /// <summary>
        /// Merge 1 or more Bigfiles and BigfileTocs to one Bigfile and one BigfileToc
        /// </summary>
        public void MergeInto(string inDstPath, string inSubPath, string inPubPath, List<Bigfile> srcBigfiles, Bigfile dstBigfile)
        {
            // Merging the 1 or more bigfile data files into one is very straightforward.
            // Create/Open the destination bigfile data file and append each bigfile data to it and while doing it remembering the
            // file offset of each one.

            // Merging the TOCs is also quite easy:
            // Create
            // For each TOC load the header, then

        }

    }
}


