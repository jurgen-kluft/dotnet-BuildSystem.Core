using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    public class BigfileToc
    {
        public enum ECompressed
        {
            NO,
            YES
        }

        private interface IReadContext
        {
            #region Properties

            int count { get; }

            #endregion
            #region Methods

            void read(IBinaryReader reader);
            void read(IBinaryReader reader, ITocEntry e);
            bool next();

            #endregion
        }

        private interface IWriteContext
        {
            #region Properties

            int count { get; set; }

            #endregion
            #region Methods

            void write(IBinaryWriter writer);
            void write(IBinaryWriter writer, ITocEntry e);
            bool next();

            #endregion
        }

        private interface ITocEntry
        {
            #region Properties

            StreamOffset[] offsets { get; set; }
            UInt32 size { get; set; }
            bool compressed { get; set; }
            Filename filename { get; set; }
            Hash128 hash { get; set; }
            Hash128 contenthash { get; set; }

            #endregion
            #region FileOffset Methods

            void addFileOffset(StreamOffset offset);

            #endregion
            #region Read/Write

            void read(IBinaryReader reader, IReadContext context);
            void write(IBinaryWriter writer, IWriteContext context);

            #endregion
        }

        /// <summary>
        /// Factory for creating TocEntry, ReadContext and WriteContext
        /// </summary>
        private static class Factory
        {
            #region Create

            public static ITocEntry Create()
            {
                return new TocEntry();
            }
            public static ITocEntry Create(StreamOffset fileOffset, UInt32 fileSize, Filename filename, ECompressed type, Hash128 contentHash)
            {
                return new TocEntry(fileOffset, fileSize, filename, type, contentHash);
            }
            public static ITocEntry Create(UInt32 fileSize, Filename filename, ECompressed type, Hash128 contentHash)
            {
                return new TocEntry(fileSize, filename, type, contentHash);
            }

            public static IReadContext CreateReadTocContext(List<ITocEntry> table)
            {
                return new ReadToc32Context(table);
            }

            public static IReadContext CreateReadFdbContext()
            {
                return new ReadFdbContext();
            }

            public static IReadContext CreateReadHdbContext()
            {
                return new ReadHdbContext();
            }

            public static IWriteContext CreateWriteTocContext()
            {
                return new WriteToc32Context();
            }

            public static IWriteContext CreateWriteFdbContext()
            {
                return new WriteFdbContext();
            }

            public static IWriteContext CreateWriteHdbContext()
            {
                return new WriteHdbContext();
            }

            #endregion
        }

        /// <summary>
        /// TocEntry with a FileOffset(Int32)[] & FileSize 
        /// 
        /// Layout:
        ///     (Int32 FileOffset) OR (Int32 DataOffset to a FileOffset(Int32)[]) depending on bit 31 (0=Single, 1=Multi)
        ///     Int32 FileSize
        /// 
        /// </summary>
        private class TocEntry : ITocEntry
        {
            #region Fields

            private StreamOffset[] mFileOffsets;
            private UInt32 mFileSize;
            private Filename mFilename;
            private Hash128 mHash;
            private Hash128 mContentHash;
            private StreamOffset[] mEmptyOffset = new StreamOffset[] { StreamOffset.Empty };

            #endregion
            #region Constructor

            public TocEntry()
            {
                mFileOffsets = null;
                mFilename = Filename.Empty;
                mHash = Hash128.Empty;
                mContentHash = Hash128.Empty;
                size = 0;
                compressed = false;
            }
            public TocEntry(Filename filename, ECompressed type, Hash128 contentHash)
            {
                mFileOffsets = null;
                mFilename = filename;
                mHash = Hash128.FromString(mFilename.ToString().ToLower());
                mContentHash = contentHash;

                size = 0;
                compressed = (type == ECompressed.YES);
            }
            public TocEntry(StreamOffset fileOffset, UInt32 fileSize, Filename filename, ECompressed type, Hash128 contentHash)
                : this(filename, type, contentHash)
            {
                mFileOffsets = new StreamOffset[] { fileOffset };
                size = fileSize;
            }
            public TocEntry(UInt32 fileSize, Filename filename, ECompressed type, Hash128 contentHash)
                : this(filename, type, contentHash)
            {
                mFileOffsets = null;
                size = fileSize;
            }

            #endregion
            #region Properties

            public StreamOffset[] offsets
            {
                get
                {
                    if (null == mFileOffsets)
                    {
                        return mEmptyOffset;
                    }
                    else
                    {
                        return mFileOffsets;
                    }
                }
                set
                {
                    if (value.Length == 0)
                    {
                        mFileOffsets = new StreamOffset[] { StreamOffset.Empty };
                    }
                    else
                    {
                        mFileOffsets = new StreamOffset[value.Length];
                        value.CopyTo(mFileOffsets, 0);
                    }
                }
            }

            public UInt32 size
            {
                get
                {
                    return mFileSize & 0x7FFFFFFF;
                }
                set
                {
                    mFileSize = mFileSize & 0x80000000;
                    mFileSize |= (value & 0x7FFFFFFF);
                }
            }

            public Filename filename
            {
                get
                {
                    return mFilename;
                }
                set
                {
                    mFilename = value;
                }
            }

            public Hash128 hash
            {
                get
                {
                    return mHash;
                }
                set
                {
                    mHash = value;
                }
            }

            public Hash128 contenthash
            {
                get
                {
                    return mContentHash;
                }
                set
                {
                    mContentHash = value;
                }
            }

            public bool compressed
            {
                get
                {
                    return (mFileSize & 0x80000000) == 0x80000000;
                }
                set
                {
                    if (value)
                        mFileSize = mFileSize | 0x80000000;
                    else
                        mFileSize = mFileSize & 0x7FFFFFFF;
                }
            }

            #endregion
            #region Methods

            public void addFileOffset(StreamOffset o)
            {
                if (null != mFileOffsets && mFileOffsets.Length > 0)
                {
                    StreamOffset[] newOffsets = new StreamOffset[mFileOffsets.Length + 1];
                    mFileOffsets.CopyTo(newOffsets, 0);
                    newOffsets[newOffsets.Length - 1] = o;
                    mFileOffsets = newOffsets;
                }
                else
                {
                    mFileOffsets = new StreamOffset[1];
                    mFileOffsets[0] = o;
                }
            }

            public void read(IBinaryReader reader, IReadContext context)
            {
                context.read(reader, this);
            }

            public void write(IBinaryWriter writer, IWriteContext context)
            {
                context.write(writer, this);
            }

            #endregion

            #region TocEntryHashComparer (IComparer<ITocEntry>)

            public class TocEntryHashComparer : IComparer<ITocEntry>
            {
                public int Compare(ITocEntry x, ITocEntry y)
                {
                    int c = Hash128.Compare(x.hash, y.hash);
                    return c;
                }
            }

            #endregion
        }

        /// <summary>
        /// The (32 bit) Toc (holding TocEntry[] with multiple file offsets) consists of 2 iterations
        /// 
        ///     Iteration 1:
        ///         Reading the TocEntry[] where every TocEntry holds an offset to a FileOffset(Int32)[] and a FileSize(Int32)
        /// 
        ///     Iteration 2:
        ///         Reading the FileOffset(Int32)[] for every TocEntry
        /// 
        /// </summary>
        private class ReadToc32Context : IReadContext
        {
            #region Fields

            private int mCount;
            private int mIteration;
            private int mIndex;
            private List<ITocEntry> mTable;
            private List<UInt32> mMultiOffset;

            #endregion
            #region Constructor

            public ReadToc32Context(List<ITocEntry> table)
            {
                mTable = table;
            }

            #endregion
            #region IReadContext Members

            public int count
            {
                get
                {
                    return mCount;
                }
            }


            public void read(IBinaryReader reader)
            {
                // Read the header
                mCount = reader.ReadInt32();

                mTable.Clear();
                mTable.Capacity = mCount;

                for (int i = 0; i < mCount; i++)
                {
                    ITocEntry fileEntry = Factory.Create();
                    mTable.Add(fileEntry);
                }

            }

            private static bool isValidOffset(UInt32 offset)
            {
                return offset != UInt32.MaxValue;
            }

            private static bool isMultiOffset(UInt32 offset)
            {
                return (offset & 0x80000000) != 0;
            }

            public void read(IBinaryReader reader, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0:
                        {
                            if (mIndex == 0)
                            {
                                mMultiOffset = new List<UInt32>(mCount);
                            }

                            UInt32 offset = reader.ReadUInt32(); // FileOffset OR DataOffset to FileOffset(Int32)[]
                            mMultiOffset.Add(offset);
                            e.size = reader.ReadUInt32(); // File size
                        }
                        break;
                    case 1:
                        {
                            UInt32 offset = mMultiOffset[mIndex];
                            if (isValidOffset(offset))
                            {
                                if (isMultiOffset(offset))
                                {
                                    int instancesNum = reader.ReadInt32();
                                    for (int i = 0; i < instancesNum; i++)
                                        e.addFileOffset(new StreamOffset(reader.ReadUInt32()));
                                }
                                else
                                {
                                    e.addFileOffset(new StreamOffset(offset));
                                }
                            }
                            else
                            {
                                e.addFileOffset(new StreamOffset(offset));
                            }
                        }
                        break;
                }

                ++mIndex;
            }

            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 2;
            }

            #endregion
        }

        /// <summary>
        /// The Fdb (holding the Offset[] and Filenames[]) consists of 3 iterations
        /// 
        ///     Iteration 1:
        ///         Reading the Offset(Int32) to the filename for every TocEntry
        /// 
        ///     Iteration 2:
        ///         Reading the filenames
        /// 
        ///     Iteration 3:
        ///         Getting the filename for every TocEntry
        /// 
        /// </summary>
        private class ReadFdbContext : IReadContext
        {
            #region Fields

            private int mCount;
            private int mIteration;
            private int mIndex;

            #endregion
            #region IReadContext Members

            public int count
            {
                get
                {
                    return mCount;
                }
            }

            public void read(IBinaryReader reader)
            {
                mCount = reader.ReadInt32();
            }

            public void read(IBinaryReader reader, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Read the FilenameOffset(Int32)[]
                        {
                            reader.ReadInt32();
                        }
                        break;
                    case 1: // Read the Filename(string)[]
                        {
                            Filename filename = new Filename(reader.ReadString());
                            e.filename = filename;
                        }
                        break;
                }
                ++mIndex;
            }

            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 2;
            }

            #endregion
        }

        /// <summary>
        /// The Hdb (holding the Hash128[]) consists of 1 iteration
        /// 
        ///     Iteration 1:
        ///         Reading the Hash128[] to the filename for every TocEntry
        /// 
        ///     Iteration 2:
        ///         Reading the filenames
        /// 
        ///     Iteration 3:
        ///         Getting the filename for every TocEntry
        /// 
        /// </summary>
        private class ReadHdbContext : IReadContext
        {
            #region Fields

            private int mCount;
            private int mIteration;
            private int mIndex;

            #endregion
            #region IReadContext Members

            public int count
            {
                get
                {
                    return mCount;
                }
            }

            public void read(IBinaryReader reader)
            {
                mCount = reader.ReadInt32();
            }

            public void read(IBinaryReader reader, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Read the FilenameOffset(Int32)[]
                        {
                            byte[] hash = reader.ReadBytes(16);
                            e.hash = Hash128.ConstructTake(hash);
                        }
                        break;
                }
                ++mIndex;
            }

            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 1;
            }

            #endregion
        }

        /// <summary>
        /// The (32 bit) Toc, holding TocEntry[] (TocEntry with multiple file offsets), needs 3 iterations
        /// 
        ///     Iteration 1:
        ///         Calculating the offset to the FileOffset(Int32)[] for every TocEntry
        /// 
        ///     Iteration 2:
        ///         Writing the TocEntry[]
        /// 
        ///     Iteration 3:
        ///         Writing the FileOffset(Int32)[] for every TocEntry
        /// 
        /// </summary>
        private class WriteToc32Context : IWriteContext
        {
            #region Fields

            private int mCount;
            private int mIteration;
            private int mIndex;
            private int mOffset;
            private List<Int32> mMultiOffset = new List<Int32>();

            #endregion
            #region IWriteContext Members

            public int count
            {
                set
                {
                    mCount = value;
                }
                get
                {
                    return mCount;
                }
            }

            public void write(IBinaryWriter writer)
            {
                writer.Write(mCount);
            }

            private static bool isValidOffset(Int32 offset)
            {
                return offset != -1;
            }

            private static Int32 makeInvalidOffset()
            {
                return -1;
            }

            private static Int32 makeMultiOffset(Int32 offset)
            {
                return (Int32)((UInt32)offset | 0x80000000);
            }

            private static bool isMultiOffset(Int32 offset)
            {
                return (offset & 0x80000000) != 0;
            }

            public void write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Calculate the DataOffset to it's FileOffset(Int32)[] for every TocEntry
                        {
                            if (mIndex == 0)
                            {
                                mMultiOffset = new List<Int32>(mCount);
                                mOffset = (int)writer.Position + mCount * (sizeof(Int32) + sizeof(Int32));
                            }

                            if (e.offsets.Length == 0)
                            {
                                mMultiOffset.Add(makeInvalidOffset());
                            }
                            else if (e.offsets.Length == 1)
                            {
                                mMultiOffset.Add(e.offsets[0].value32);
                            }
                            else
                            {
                                mMultiOffset.Add(makeMultiOffset(mOffset));
                                mOffset += sizeof(Int32) + (e.offsets.Length * sizeof(Int32));
                            }
                        }
                        break;
                    case 1: // Write every TocEntry
                        {
                            Int32 offset = mMultiOffset[mIndex];
                            writer.Write(offset);
                            writer.Write(e.size);
                        }
                        break;
                    case 2: // Write the FileOffset(Int32)[] for every TocEntry that needs it
                        {
                            Int32 offset = mMultiOffset[mIndex];
                            if (isValidOffset(offset) && isMultiOffset(offset))
                            {
                                writer.Write(e.offsets.Length);
                                foreach (StreamOffset o in e.offsets)
                                    writer.Write(o.value32);
                            }
                        }
                        break;
                }
                ++mIndex;
            }


            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 3;
            }

            #endregion
        }

        /// <summary>
        /// The Fdb (holding FilenameOffset(Int32)[] and Filename(string)[]) consists of 3 iterations
        /// 
        ///     Iteration 1:
        ///         Computing the offset to every filename
        /// 
        ///     Iteration 2: FilenameOffset(Int32)[]
        ///         Writing the offset to the filename for every TocEntry
        /// 
        ///     Iteration 3: Filename(string)[]
        ///         Writing the filename (string) for every TocEntry
        /// 
        /// </summary>
        private class WriteFdbContext : IWriteContext
        {
            #region Fields

            private int mCount;
            private int mIteration;
            private int mIndex;
            private int mOffset;
            private List<int> mOffsets = new List<int>();

            #endregion
            #region IWriteContext Members

            public int count
            {
                set
                {
                    mCount = value;
                    int[] array = new int[mCount];
                    mOffsets = new List<int>(array);
                }
                get
                {
                    return mCount;
                }
            }

            public void write(IBinaryWriter writer)
            {
                // The file header
                writer.Write(mCount);
            }

            public void write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Calculating the filename offsets
                        {
                            if (mIndex == 0)
                                mOffset = 0;

                            mOffsets[mIndex] = mOffset;
                            string filename = e.filename;
                            mOffset += filename.Length + 1;
                        }
                        break;
                    case 1: // Writing the FilenameOffset(Int32)[]
                        {
                            writer.Write(mOffsets[mIndex]);
                        }
                        break;
                    case 2: // Write the Filename(string)[]
                        {
                            writer.Write(e.filename);
                        }
                        break;
                }

                ++mIndex;
            }

            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 3;
            }

            #endregion
        }

        /// <summary>
        /// The Hdb (holding Hash128[]) consists of 1 iteration
        /// 
        ///     Iteration 1:
        ///         Write every hash of every TocEntry
        /// 
        /// </summary>
        private class WriteHdbContext : IWriteContext
        {
            #region Fields

            private int mCount = 0;
            private int mIteration = 0;
            private int mIndex = 0;

            #endregion
            #region IWriteContext Members

            public int count
            {
                set
                {
                    mCount = value;
                }
                get
                {
                    return mCount;
                }
            }

            public void write(IBinaryWriter writer)
            {
                // The file header
                writer.Write(mCount);
            }

            public void write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0:
                        {
                            byte[] b = e.hash.Data;
                            writer.Write(b);
                        }
                        break;
                }

                ++mIndex;
            }

            public bool next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 1;
            }

            #endregion
        }

        #region Fields

        private static IBigfileConfig sConfig = new BigfileDefaultConfig();
        private readonly List<ITocEntry> mTable = new List<ITocEntry>();
        private readonly Dictionary<Filename, UInt32> mFilenameToFileEntryIndexDictionary = new Dictionary<Filename, UInt32>(new FilenameInsensitiveComparer());

        #endregion
        #region Properties

        public int Count
        {
            get
            {
                return mTable.Count;
            }
        }

        public BigfileFile infoOf(int index)
        {
            if (index >= 0 && index < Count)
            {
                ITocEntry e = mTable[index];
                return new BigfileFile(new Filename(e.filename), e.size, e.offsets, e.contenthash);
            }
            return BigfileFile.Empty;
        }

        public UInt32 indexOf(Filename filename)
        {
            UInt32 index;
            if (!mFilenameToFileEntryIndexDictionary.TryGetValue(filename, out index))
                index = UInt32.MaxValue;
            return index;
        }

        #endregion
        #region Methods

        public static void setConfig(IBigfileConfig config)
        {
            sConfig = config;
        }

        public static bool exists(Filename filename)
        {
            Filename bigFileTocFilename = filename;
            bigFileTocFilename.Extension = sConfig.BigFileTocExtension;
            FileInfo fileInfo = new FileInfo(bigFileTocFilename);
            return fileInfo.Exists;
        }

        public void add(BigfileFile file, bool isCompressed)
        {
            UInt32 index;
            if (mFilenameToFileEntryIndexDictionary.TryGetValue(file.filename, out index) == false)
            {
                ITocEntry fileEntry = Factory.Create(file.offset, (UInt32)file.size, file.filename, isCompressed ? ECompressed.YES : ECompressed.NO, file.contenthash);
                index = (UInt32)mTable.Count;
                mTable.Add(fileEntry);
                mFilenameToFileEntryIndexDictionary.Add(file.filename, index);
            }
            else
            {
                Debug.Assert(file.size == mTable[(int)index].size);
                mTable[(int)index].addFileOffset(file.offset);
            }
        }
        public void add(BigfileFile file)
        {
            add(file, false);
        }
        public void sort()
        {
            mTable.Sort(new TocEntry.TocEntryHashComparer());
        }
        public void copyFilesOrder(BigfileToc orgToc)
        {
            mTable.Clear();
            mFilenameToFileEntryIndexDictionary.Clear();

            for (int i = 0; i < orgToc.Count; i++)
            {
                ITocEntry oldEntry = orgToc.mTable[i];
                bool isCompressed = (oldEntry.size & 0x80000000) == 0x80000000;
                ITocEntry newEntry = Factory.Create((UInt32)oldEntry.size, oldEntry.filename, isCompressed ? ECompressed.YES : ECompressed.NO, oldEntry.contenthash);
                UInt32 index = (UInt32)mTable.Count;
                mTable.Add(newEntry);
                mFilenameToFileEntryIndexDictionary.Add(new Filename(oldEntry.filename), index);
            }
        }
        public void copyFilesOrder(List<BigfileFile> srcBigfileFiles)
        {
            mTable.Clear();
            mFilenameToFileEntryIndexDictionary.Clear();

            for (int i = 0; i < srcBigfileFiles.Count; i++)
            {
                BigfileFile oldFile = srcBigfileFiles[i];
                bool isCompressed = (oldFile.size & 0x80000000) == 0x80000000;
                ITocEntry newEntry = Factory.Create((UInt32)oldFile.size, oldFile.filename, isCompressed ? ECompressed.YES : ECompressed.NO, oldFile.contenthash);
                UInt32 index = (UInt32)mTable.Count;
                mTable.Add(newEntry);
                mFilenameToFileEntryIndexDictionary.Add(new Filename(oldFile.filename), index);
            }
        }

        private static void sReadTable(List<ITocEntry> table, IReadContext context, FileStream stream, EEndian endian)
        {
            IBinaryReader binaryReader = EndianUtils.CreateBinaryReader(stream, endian);
            {
                context.read(binaryReader);
                do
                {
                    for (int i = 0; i < context.count; i++)
                        table[i].read(binaryReader, context);
                } while (context.next());
            }
            binaryReader.Close();
        }

        private static FileStream sOpenFileStreamForReading(Filename filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("We tried to load " + fileInfo + " but it does not exist.");
                return null;
            }
            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
            return fileStream;
        }

        public bool load(Filename filename, EEndian endian)
        {
            try
            {
                FileStream bigFileTocFileStream = sOpenFileStreamForReading(filename.ChangedExtension(BigfileConfig.BigFileTocExtension));
                FileStream bigFileFdbFileStream = sOpenFileStreamForReading(filename.ChangedExtension(BigfileConfig.BigFileFdbExtension));
                FileStream bigFileHdbFileStream = sOpenFileStreamForReading(filename.ChangedExtension(BigfileConfig.BigFileHdbExtension));
                {
                    try
                    {
                        mTable.Clear();
                        sReadTable(mTable, Factory.CreateReadTocContext(mTable), bigFileTocFileStream, endian);
                        sReadTable(mTable, Factory.CreateReadFdbContext(), bigFileFdbFileStream, endian);
                        sReadTable(mTable, Factory.CreateReadHdbContext(), bigFileHdbFileStream, endian);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
                bigFileTocFileStream.Close();
                bigFileFdbFileStream.Close();
                bigFileHdbFileStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private static void sWriteTable(List<ITocEntry> table, IWriteContext context, FileStream stream, EEndian endian)
        {
            IBinaryWriter binaryWriter = EndianUtils.CreateBinaryWriter(stream, endian);
            {
                context.count = table.Count;
                context.write(binaryWriter);

                do
                {
                    for (int i = 0; i < context.count; i++)
                        table[i].write(binaryWriter, context);
                } while (context.next());

                // We are done, close the writer
                binaryWriter.Close();
            }
        }

        private static FileStream sOpenFileStreamForWriting(Filename filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (!fileInfo.Exists)
            {
                FileStream fileCreationStream = File.Create(fileInfo.FullName);
                fileCreationStream.Close();
            }
            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.Write, 1 * 1024 * 1024, FileOptions.Asynchronous);
            return fileStream;
        }

        public bool save(Filename bigfileFilename, EEndian endian)
        {
            sort();

            try
            {
                FileStream bigFileTocFileStream = sOpenFileStreamForWriting(bigfileFilename.ChangedExtension(BigfileConfig.BigFileTocExtension));
                FileStream bigFileFdbFileStream = sOpenFileStreamForWriting(bigfileFilename.ChangedExtension(BigfileConfig.BigFileFdbExtension));
                FileStream bigFileHdbFileStream = sOpenFileStreamForWriting(bigfileFilename.ChangedExtension(BigfileConfig.BigFileHdbExtension));

                {
                    try
                    {
                        sWriteTable(mTable, Factory.CreateWriteTocContext(), bigFileTocFileStream, endian);
                        sWriteTable(mTable, Factory.CreateWriteFdbContext(), bigFileFdbFileStream, endian);
                        sWriteTable(mTable, Factory.CreateWriteHdbContext(), bigFileHdbFileStream, endian);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }

                bigFileTocFileStream.Close();
                bigFileFdbFileStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        #endregion
    }
}

