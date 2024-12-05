using System;
using System.IO;
using System.Collections.Generic;
using GameCore;

namespace BigfileBuilder
{
    public sealed class BigfileMerger
    {
        public BigfileMerger()
        {
        }

        /// <summary>
        /// Merge more than one BigFile and Bigfile TOC, resulting in one Bigfile and one Bigfile TOC
        /// </summary>
        public void MergeInto(string inDstPath, string inSubPath, string inPubPath, List<Bigfile> srcBigFiles, Bigfile dstBigFile)
        {
            // Merging one or more bigfile data files into one is very straightforward.
            // Create/Open the destination bigfile data file and append each bigfile data to it and while doing it remembering the
            // file offset of each one.

            // Merging the TOCs is also quite easy:
            //  - Sort the source Bigfile list by Bigfile Index
            //  - For each TOC load the section
            //  - Write the final multi-section TOC to the destination Bigfile TOC

        }

    }
}


