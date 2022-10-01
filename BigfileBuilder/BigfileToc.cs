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

            int Count { get; }

            #endregion
            #region Methods

            void Read(IBinaryReader reader);
            void Read(IBinaryReader reader, ITocEntry e);
            bool Next();

            #endregion
        }

        private interface IWriteContext
        {
            #region Properties

            int Count { get; set; }

            #endregion
            #region Methods

            void Write(IBinaryWriter writer);
            void Write(IBinaryWriter writer, ITocEntry e);
            bool Next();

            #endregion
        }

        private interface ITocEntry
        {
            #region Properties

            StreamOffset FileOffset { get; set; }
            Int32 FileSize { get; set; }
            bool IsCompressed { get; set; }
            string Filename { get; set; }
            UInt64 FileId { get; set; }
            Hash160 FileContentHash { get; set; }
            List<ITocEntry> ChildEntries { get; set; }

            #endregion
            #region Read/Write

            void Read(IBinaryReader reader, IReadContext context);
            void Write(IBinaryWriter writer, IWriteContext context);

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
            public static ITocEntry Create(StreamOffset fileOffset, Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
            {
                return new TocEntry(fileOffset, fileSize, filename, type, contentHash);
            }
            public static ITocEntry Create(Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
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

            #endregion
            #region Constructor

            public TocEntry()
            {
                FileOffset = StreamOffset.Empty;
                Filename = string.Empty;
                FileId = UInt64.MaxValue;
                FileContentHash = Hash160.Empty;
                FileSize = 0;
                IsCompressed = false;
            }
            public TocEntry(string filename, ECompressed type, Hash160 contentHash)
            {
                FileOffset = StreamOffset.Empty;
                Filename = filename;
                FileId = UInt64.MaxValue;
                FileContentHash = contentHash;

                FileSize = 0;
                IsCompressed = (type == ECompressed.YES);
            }
            public TocEntry(StreamOffset fileOffset, Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
                : this(filename, type, contentHash)
            {
                FileOffset = fileOffset ;
                FileSize = fileSize;
            }
            public TocEntry(Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
                : this(filename, type, contentHash)
            {
                FileOffset = StreamOffset.Empty;
                FileSize = fileSize;
            }

            #endregion
            #region Properties

            public StreamOffset FileOffset { get; set; }
            public Int32 FileSize { get; set; }
            public string Filename { get; set; }
            public UInt64 FileId { get; set; }
            public Hash160 FileContentHash { get; set; }
            public bool IsCompressed { get; set; }
            public List<ITocEntry> ChildEntries { get; set; } = new ();

            public static Int32 EncodeFileSize(Int32 fileSize, bool isCompressed)
            {
                if (isCompressed)
                    return (Int32)(fileSize | 0x80000000);
                return fileSize & 0x7fffffff;
            }

            #endregion
            #region Methods

            public void Read(IBinaryReader reader, IReadContext context)
            {
                context.Read(reader, this);
            }

            public void Write(IBinaryWriter writer, IWriteContext context)
            {
                context.Write(writer, this);
            }

            #endregion

            #region TocEntryFileIdComparer (IComparer<ITocEntry>)

            public class TocEntryFileIdComparer : IComparer<ITocEntry>
            {
                public int Compare(ITocEntry x, ITocEntry y)
                {
                    if (x.FileId == y.FileId)
                        return 0;
                    if (x.FileId < y.FileId)
                        return -1;
                    return 1;
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

            public int Count { get; set; }

            public void Read(IBinaryReader reader)
            {
                // Read the header
                Count = reader.ReadInt32();

                mTable.Clear();
                mTable.Capacity = Count;

                for (int i = 0; i < Count; i++)
                {
                    ITocEntry fileEntry = Factory.Create();
                    mTable.Add(fileEntry);
                }

            }

            private static bool IsValidOffset(UInt32 offset)
            {
                return offset != UInt32.MaxValue;
            }

            private static bool IsMultiOffset(UInt32 offset)
            {
                return (offset & 0x80000000) != 0;
            }

            public void Read(IBinaryReader reader, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0:
                        {
                            if (mIndex == 0)
                            {
                                mMultiOffset = new List<UInt32>(Count);
                            }

                            UInt32 offset = reader.ReadUInt32(); // FileOffset OR DataOffset to FileOffset(Int32)[]
                            mMultiOffset.Add(offset);
                            e.FileSize = reader.ReadInt32(); // File size
                        }
                        break;
                    case 1:
                        {
                            UInt32 offset = mMultiOffset[mIndex];
                            if (IsValidOffset(offset))
                            {
                                if (IsMultiOffset(offset))
                                {
                                    int instancesNum = reader.ReadInt32();
                                    for (int i = 0; i < instancesNum; i++)
                                        e.FileOffset = new StreamOffset(reader.ReadUInt32());
                                }
                                else
                                {
                                    e.FileOffset = new StreamOffset(offset);
                                }
                            }
                            else
                            {
                                e.FileOffset = new StreamOffset(offset);
                            }
                        }
                        break;
                }

                ++mIndex;
            }

            public bool Next()
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

            private int mIteration;
            private int mIndex;

            #endregion
            #region IReadContext Members

            public int Count { get; set; }

            public void Read(IBinaryReader reader)
            {
                Count = reader.ReadInt32();
            }

            public void Read(IBinaryReader reader, ITocEntry e)
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
                            string filename = reader.ReadString();
                            e.Filename = filename;
                        }
                        break;
                }
                ++mIndex;
            }

            public bool Next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 2;
            }

            #endregion
        }

        /// <summary>
        /// The Hdb (holding the Hash[]) consists of 1 iteration
        /// 
        ///     Iteration 1:
        ///         Reading the Hash[] to the filename for every TocEntry
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

            private int mIteration;
            private int mIndex;

            #endregion
            #region IReadContext Members

            public int Count { get; set; }

            public void Read(IBinaryReader reader)
            {
                Count = reader.ReadInt32();
            }

            public void Read(IBinaryReader reader, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: 
                        {
                            byte[] hash = reader.ReadBytes(16);
                            e.FileContentHash = Hash160.ConstructTake(hash);
                        }
                        break;
                }
                ++mIndex;
            }

            public bool Next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 1;
            }

            #endregion
        }

        /// <summary>
        /// The (32 bit) Toc, holding TocEntry[] (TocEntry), needs 3 iterations
        /// 
        ///     Iteration 1:
        ///         Calculating the offset to the FileId(Int32)[] for every TocEntry
        /// 
        ///     Iteration 2:
        ///         Writing the TocEntry[]
        /// 
        ///     Iteration 3:
        ///         Writing the FileId(Int32)[] for every TocEntry
        /// 
        /// </summary>
        private class WriteToc32Context : IWriteContext
        {
            #region Fields

            private int mIteration;
            private int mIndex;
            private int mOffset;
            private List<Int32> mMultiOffset = new List<Int32>();

            #endregion
            #region IWriteContext Members

            public int Count { get; set; }

            public void Write(IBinaryWriter writer)
            {
                writer.Write(Count);
            }

            private static bool IsValidOffset(Int32 offset)
            {
                return offset != -1;
            }

            private static Int32 MakeInvalidOffset()
            {
                return -1;
            }

            private static Int32 MakeMultiOffset(Int32 offset)
            {
                return (Int32)((UInt32)offset | 0x80000000);
            }

            private static bool IsMultiOffset(Int32 offset)
            {
                return (offset & 0x80000000) != 0;
            }

            public void Write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Calculate the DataOffset to it's FileOffset(Int32)[] for every TocEntry
                        {
                            if (mIndex == 0)
                            {
                                mMultiOffset = new List<Int32>(Count);
                                mOffset = (int)writer.Position + Count * (sizeof(Int32) + sizeof(Int32));
                            }

                            if (e.ChildEntries.Count == 0)
                            {
                                mMultiOffset.Add(MakeInvalidOffset());
                            }
                            else if (e.ChildEntries.Count == 1)
                            {
                                mMultiOffset.Add(e.ChildEntries[0].FileOffset.value32);
                            }
                            else
                            {
                                mMultiOffset.Add(MakeMultiOffset(mOffset));
                                mOffset += sizeof(Int32) + (e.ChildEntries.Count * sizeof(Int32));
                            }
                        }
                        break;
                    case 1: // Write every TocEntry
                        {
                            Int32 offset = mMultiOffset[mIndex];
                            writer.Write(offset);
                            writer.Write(e.FileSize);
                        }
                        break;
                    case 2: // Write the FileOffset(Int32)[] for every TocEntry that needs it
                        {
                            Int32 offset = mMultiOffset[mIndex];
                            if (IsValidOffset(offset) && IsMultiOffset(offset))
                            {
                                writer.Write(e.ChildEntries.Count);
                                foreach (ITocEntry te in e.ChildEntries)
                                    writer.Write(te.FileId);
                            }
                        }
                        break;
                }
                ++mIndex;
            }


            public bool Next()
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

            private int mIteration;
            private int mIndex;
            private int mOffset;
            private List<int> mOffsets = new List<int>();

            #endregion
            #region IWriteContext Members

            public int Count { get; set; }

            public void Write(IBinaryWriter writer)
            {
                // The file header
                writer.Write(Count);
            }

            public void Write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Calculating the filename offsets
                        {
                            if (mIndex == 0)
                                mOffset = 0;

                            mOffsets[mIndex] = mOffset;
                            string filename = e.Filename;
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
                            writer.Write(e.Filename);
                        }
                        break;
                }

                ++mIndex;
            }

            public bool Next()
            {
                mIndex = 0;
                mIteration++;
                return mIteration < 3;
            }

            #endregion
        }

        /// <summary>
        /// The Hdb (holding Hash160[]) consists of 1 iteration
        /// 
        ///     Iteration 1:
        ///         Write every hash of every TocEntry
        /// 
        /// </summary>
        private class WriteHdbContext : IWriteContext
        {
            #region IWriteContext Members

            private int Iteration { get; set; }
            private int Index { get; set; }
            public int Count { get; set; }

            public void Write(IBinaryWriter writer)
            {
                // The file header
                writer.Write(Count);
            }

            public void Write(IBinaryWriter writer, ITocEntry e)
            {
                switch (Iteration)
                {
                    case 0:
                        {
                            byte[] b = e.FileContentHash.Data;
                            writer.Write(b);
                        }
                        break;
                }
                ++Index;
            }

            public bool Next()
            {
                Index = 0;
                Iteration++;
                return Iteration < 1;
            }

            #endregion
        }

        #region Fields

        private static IBigfileConfig sConfig = new BigfileDefaultConfig();
        private readonly List<ITocEntry> mTable = new ();
        private readonly Dictionary<string, UInt32> mFilenameToFileEntryIndexDictionary = new (StringComparer.CurrentCultureIgnoreCase);

        #endregion
        #region Properties

        public int Count
        {
            get
            {
                return mTable.Count;
            }
        }

        public BigfileFile InfoOf(int index)
        {
            if (index >= 0 && index < Count)
            {
                ITocEntry e = mTable[index];
                return new BigfileFile(e.Filename, e.FileSize, e.FileOffset, e.FileId, e.FileContentHash);
            }
            return BigfileFile.Empty;
        }

        public UInt32 IndexOf(string filename)
        {
            if (!mFilenameToFileEntryIndexDictionary.TryGetValue(filename, out UInt32 index))
                index = UInt32.MaxValue;
            return index;
        }

        #endregion
        #region Methods

        public static void SetConfig(IBigfileConfig config)
        {
            sConfig = config;
        }

        public static bool Exists(string filename)
        {
            string bigFileTocFilename = filename;
            bigFileTocFilename = Path.ChangeExtension(bigFileTocFilename , sConfig.BigFileTocExtension);
            FileInfo fileInfo = new (bigFileTocFilename);
            return fileInfo.Exists;
        }

        public void Add(BigfileFile file, bool isCompressed)
        {
            UInt32 index;
            if (mFilenameToFileEntryIndexDictionary.TryGetValue(file.Filename, out index) == false)
            {
                ITocEntry fileEntry = Factory.Create(file.FileOffset, file.FileSize, file.Filename, isCompressed ? ECompressed.YES : ECompressed.NO, file.FileContentHash);
                index = (UInt32)mTable.Count;
                mTable.Add(fileEntry);
                mFilenameToFileEntryIndexDictionary.Add(file.Filename, index);
            }
            else
            {
                Debug.Assert(file.FileSize == mTable[(int)index].FileSize);
                mTable[(int)index].FileOffset = file.FileOffset;
            }
        }
        public void Add(BigfileFile file)
        {
            Add(file, false);
        }
        private void Sort()
        {
            mTable.Sort(new TocEntry.TocEntryFileIdComparer());
        }
        public void CopyFilesOrder(BigfileToc orgToc)
        {
            mTable.Clear();
            mFilenameToFileEntryIndexDictionary.Clear();

            for (int i = 0; i < orgToc.Count; i++)
            {
                ITocEntry oldEntry = orgToc.mTable[i];
                bool isCompressed = (oldEntry.IsCompressed);
                ITocEntry newEntry = Factory.Create(oldEntry.FileSize, oldEntry.Filename, isCompressed ? ECompressed.YES : ECompressed.NO, oldEntry.FileContentHash);
                UInt32 index = (UInt32)mTable.Count;
                mTable.Add(newEntry);
                mFilenameToFileEntryIndexDictionary.Add(oldEntry.Filename, index);
            }
        }
        public void CopyFilesOrder(List<BigfileFile> srcBigfileFiles)
        {
            mTable.Clear();
            mFilenameToFileEntryIndexDictionary.Clear();

            for (int i = 0; i < srcBigfileFiles.Count; i++)
            {
                BigfileFile oldFile = srcBigfileFiles[i];
                bool isCompressed = (oldFile.FileSize & 0x80000000) == 0x80000000;
                ITocEntry newEntry = Factory.Create(oldFile.FileSize, oldFile.Filename, isCompressed ? ECompressed.YES : ECompressed.NO, oldFile.FileContentHash);
                UInt32 index = (UInt32)mTable.Count;
                mTable.Add(newEntry);
                mFilenameToFileEntryIndexDictionary.Add(oldFile.Filename, index);
            }
        }

        private static void ReadTable(List<ITocEntry> table, IReadContext context, FileStream stream, EEndian endian)
        {
            IBinaryReader binaryReader = EndianUtils.CreateBinaryReader(stream, endian);
            {
                context.Read(binaryReader);
                do
                {
                    for (int i = 0; i < context.Count; i++)
                        table[i].Read(binaryReader, context);
                } while (context.Next());
            }
            binaryReader.Close();
        }

        private static FileStream OpenFileStreamForReading(string filename)
        {
            FileInfo fileInfo = new (filename);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("We tried to load " + fileInfo + " but it does not exist.");
                return null;
            }
            FileStream fileStream = new (fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
            return fileStream;
        }

        public bool Load(string filename, EEndian endian)
        {
            try
            {
                FileStream bigFileTocFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileTocExtension));
                FileStream bigFileFdbFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileFdbExtension));
                FileStream bigFileHdbFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileHdbExtension));
                {
                    try
                    {
                        mTable.Clear();
                        ReadTable(mTable, Factory.CreateReadTocContext(mTable), bigFileTocFileStream, endian);
                        ReadTable(mTable, Factory.CreateReadFdbContext(), bigFileFdbFileStream, endian);
                        ReadTable(mTable, Factory.CreateReadHdbContext(), bigFileHdbFileStream, endian);
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

        private static void WriteTable(List<ITocEntry> table, IWriteContext context, FileStream stream, EEndian endian)
        {
            IBinaryWriter binaryWriter = EndianUtils.CreateBinaryWriter(stream, endian);
            {
                context.Count = table.Count;
                context.Write(binaryWriter);

                do
                {
                    for (int i = 0; i < context.Count; i++)
                        table[i].Write(binaryWriter, context);
                } while (context.Next());

                // We are done, close the writer
                binaryWriter.Close();
            }
        }

        private static FileStream OpenFileStreamForWriting(string filename)
        {
            FileInfo fileInfo = new (filename);
            if (!fileInfo.Exists)
            {
                FileStream fileCreationStream = File.Create(fileInfo.FullName);
                fileCreationStream.Close();
            }
            FileStream fileStream = new (fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.Write, 1 * 1024 * 1024, FileOptions.Asynchronous);
            return fileStream;
        }

        public bool Save(string bigfileFilename, EEndian endian)
        {
            Sort();

            try
            {
                FileStream bigFileTocFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileTocExtension));
                FileStream bigFileFdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileFdbExtension));
                FileStream bigFileHdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileHdbExtension));

                {
                    try
                    {
                        WriteTable(mTable, Factory.CreateWriteTocContext(), bigFileTocFileStream, endian);
                        WriteTable(mTable, Factory.CreateWriteFdbContext(), bigFileFdbFileStream, endian);
                        WriteTable(mTable, Factory.CreateWriteHdbContext(), bigFileHdbFileStream, endian);
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

