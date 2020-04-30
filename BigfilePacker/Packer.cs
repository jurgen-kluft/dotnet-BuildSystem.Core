using System;
using System.Collections.Generic;

using Core;
using DataBuildSystem;
namespace BigfilePacker
{
    public class BigfileCompressor
    {
        private static IBigfileConfig sConfig = new BigfileDefaultConfig();
        private readonly float mMaximumRatio = 80.0f / 100.0f;  // 70 %

        private Int64 align(Int64 value)
        {
            return (value + (sConfig.FileAlignment - 1)) & ~(sConfig.FileAlignment - 1);
        }

        public bool Compress(Dirname path, Filename bigfileName, EEndian endian)
        {
            // Open Bigfile
            // Open BigfileToc
            Bigfile currentBigfile = new Bigfile();
            currentBigfile.open(path + bigfileName, Bigfile.EMode.READ);
            BigfileToc currentBigfileToc = new BigfileToc();
            currentBigfileToc.load(path + bigfileName, endian);

            // Create new Bigfile + new BigfileToc
            Bigfile newBigfile = new Bigfile();
            Filename tempBigFilename = bigfileName.PushedExtension(".tmp");
            newBigfile.open(path + tempBigFilename, Bigfile.EMode.WRITE);
            BigfileToc newBigfileToc = new BigfileToc();

            // The compressor
            Codec.LZF compressor = new Codec.LZF();

            // Sort the entries according to their offset and process them in that order
            // For every entry in the list
            //  Read the data from the Bigfile
            //  Compress that data 
            //      if compression ratio is good then 
            //          mark the entry as compressed 
            //          add to new Bigfile
            //      else 
            //          add uncompressed data to new Bigfile
            Dictionary<StreamOffset, List<KeyValuePair<string, Int64>>> offsetFilenameDictionary = new Dictionary<StreamOffset, List<KeyValuePair<string, Int64>>>();
            List<StreamOffset> offsets = new List<StreamOffset>();
            for (int i = 0; i < currentBigfileToc.Count; i++)
            {
                BigfileFile bfile = currentBigfileToc.infoOf(i);

                List<KeyValuePair<string, Int64>> e;
                if (!offsetFilenameDictionary.TryGetValue(bfile.offset, out e))
                {
                    offsets.Add(bfile.offset);

                    e = new List<KeyValuePair<string, Int64>>();
                    offsetFilenameDictionary.Add(bfile.offset, e);
                }
                e.Add(new KeyValuePair<string, Int64>(bfile.filename, bfile.size));
            }

            Int64 currentOffset = 0;

            Int64 totalSize = 0;
            Int64 compressedSize = 0;

            offsets.Sort();
            foreach (StreamOffset fileOffset in offsets)
            {
                List<KeyValuePair<string, Int64>> e;
                if (offsetFilenameDictionary.TryGetValue(fileOffset, out e))
                {
                    byte[] fileData;
                    byte[] compressedData;

                    bool compressed = false;
                    Int64 newFileSize = 0;
                    StreamOffset newFileOffset = StreamOffset.Empty;

                    for (int i = 0; i < e.Count; ++i)
                    {
                        string filename = e[i].Key;
                        Int64 fileSize = e[i].Value;

                        if (i == 0)
                        {
                            newFileOffset = new StreamOffset(currentOffset);

                            // Load file
                            fileData = currentBigfile.read(fileOffset, (int)fileSize);

                            bool can_compress = !filename.EndsWith(".nif", true, System.Globalization.CultureInfo.CurrentCulture);

                            compressedData = null;
                            if (can_compress)
                            {
                                compressedData = compressor.Compress(fileData);
                            }

                            totalSize += fileSize;

                            // Calculate ratio and compare to required ratio and write the appropriate data into the Bigfile.
                            float ratio = 1.0f;
                            if (compressedData != null)
                                ratio = (float)compressedData.Length / fileData.Length;

                            if (ratio < mMaximumRatio)
                            {
                                compressed = true;
                                newFileSize = compressedData.Length;
                                newBigfile.write(newFileOffset, compressedData);
                            }
                            else
                            {
                                compressed = false;
                                newFileSize = fileData.Length;
                                newBigfile.write(newFileOffset, fileData);
                            }
                            compressedSize += newFileSize;
                        }
                        newBigfileToc.add(new BigfileFile(new Filename(filename), newFileSize, newFileOffset), compressed);
                    }

                    currentOffset += newFileSize;
                    currentOffset = align(currentOffset);
                }
            }

            currentBigfile.close();
            newBigfile.close();

            Filename tempBigTocFilename = bigfileName.PushedExtension(".tmp");
            newBigfileToc.save(path + tempBigTocFilename, endian);

            return false;
        }
    }
}
