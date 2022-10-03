using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    public sealed class BigfileToc
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
            List<ITocEntry> Children { get; set; }

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

            public static ITocEntry Create(UInt64 fileId, StreamOffset fileOffset, Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
            {
                return new TocEntry(fileId, fileOffset, fileSize, filename, type, contentHash);
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
        private sealed class TocEntry : ITocEntry
        {
            #region Fields

            #endregion
            #region Constructor

            public TocEntry()
                : this(UInt64.MaxValue, StreamOffset.Empty, -1, String.Empty, ECompressed.NO, Hash160.Empty)
            {
            }

            public TocEntry(string filename, ECompressed type, Hash160 contentHash)
                : this(UInt64.MaxValue, StreamOffset.Empty, -1, filename, type, contentHash)
            {
            }

            public TocEntry(Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
                : this(UInt64.MaxValue, StreamOffset.Empty, fileSize, filename, type, contentHash)
            {
            }

            public TocEntry(UInt64 fileId, StreamOffset fileOffset, Int32 fileSize, string filename, ECompressed type, Hash160 contentHash)
            {
                FileOffset = fileOffset;
                Filename = filename;
                FileId = fileId;
                FileContentHash = contentHash;
                FileSize = fileSize;
                IsCompressed = (type == ECompressed.YES);
            }

            #endregion
            #region Properties

            public UInt64 FileId { get; set; }
            public StreamOffset FileOffset { get; set; }
            public Int32 FileSize { get; set; }
            public string Filename { get; set; }
            public Hash160 FileContentHash { get; set; }
            public bool IsCompressed { get; set; }
            public List<ITocEntry> Children { get; set; } = new();

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

        // <summary>
        // The (32 bit) Toc (holding TocEntry[] with multiple file offsets) consists of 2 iterations
        //
        //     Iteration 1:
        //         Reading the TocEntry[] where every TocEntry holds an offset to a FileOffset(Int32)[] and a FileSize(Int32)
        //
        //     Iteration 2:
        //         Reading the FileOffset(Int32)[] for every TocEntry
        //
        // </summary>
        private sealed class ReadToc32Context : IReadContext
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
                        e.FileSize = reader.ReadInt32(); // File size
                        mMultiOffset.Add(offset);
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

        // <summary>
        // The Fdb (holding the Offset[] and Filenames[]) consists of 3 iterations
        //
        //     Iteration 1:
        //         Reading the Offset(Int32) to the filename for every TocEntry
        //
        //     Iteration 2:
        //         Reading the filenames
        //
        //     Iteration 3:
        //         Getting the filename for every TocEntry
        //
        // </summary>
        private sealed class ReadFdbContext : IReadContext
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
                        reader.Position = Alignment.Align(reader.Position, 4);
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

        // <summary>
        // The Hdb (holding the Hash[]) consists of 1 iteration
        //
        //     Iteration 1:
        //         Reading the Hash[] to the filename for every TocEntry
        //
        //     Iteration 2:
        //         Reading the filenames
        //
        //     Iteration 3:
        //         Getting the filename for every TocEntry
        //
        // </summary>
        private sealed class ReadHdbContext : IReadContext
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
                        byte[] hash = reader.ReadBytes(Hash160.Size);
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

        // <summary>
        // The (32 bit) Toc, holding TocEntry[] (TocEntry), needs 3 iterations
        //
        //     Iteration 1:
        //         Calculating the offset to the FileId(Int32)[] for every TocEntry
        //
        //     Iteration 2:
        //         Writing the TocEntry[]
        //
        //     Iteration 3:
        //         Writing the FileId(Int32)[] for every TocEntry
        //
        // </summary>
        private sealed class WriteToc32Context : IWriteContext
        {
            #region Fields

            private int mIteration;
            private int mIndex;
            private int mOffset;

            #endregion
            #region IWriteContext Members

            public int Count { get; set; }

            public void Write(IBinaryWriter writer)
            {
                writer.Write(Count);
            }

            private static bool HasChildren(ITocEntry e)
            {
                return e.Children.Count > 0;
            }

            private static bool IsValidOffset(Int32 offset)
            {
                return offset != -1;
            }

            private static Int32 MakeInvalidOffset()
            {
                return -1;
            }

            private static Int32 MarkHasChildrenInFileSize(Int32 value)
            {
                return (Int32)((UInt32)value | (UInt32)0x80000000);
            }

            private static Int32 MarkHasDuplicatesInFileSize(Int32 value)
            {
                return (Int32)((UInt32)value | (UInt32)0x40000000);
            }

            public void Write(IBinaryWriter writer, ITocEntry e)
            {
                switch (mIteration)
                {
                    case 0: // Calculate the DataOffset to it's FileOffset(Int32)[] for every TocEntry
                    {
                        // Ok, so we are going to use the following flags in the FileSize
                        // - bit 31: Multi Offsets
                        // - bit 30: Multi FileIds

                        if (mIndex == 0)
                        {
                            mOffset = (int)writer.Position + Count * (sizeof(Int32) + sizeof(Int32));
                        }
                    }
                        break;
                    case 1: // Write every TocEntry
                    {
                        Int32 offset = (Int32)(e.FileOffset.value >> 5);
                        Int32 size = e.FileSize;
                        if (HasChildren(e))
                        {
                            // Mark file size so that it is known that offset is actually an offset
                            // within TOC to an array:
                            // Int32   Number of Children
                            // Int32[] FileId
                            size = MarkHasChildrenInFileSize(size);
                        }

                        writer.Write(offset); // 32-bit
                        writer.Write(size); // 32-bit

                        // Increment the offset with the size of the Array(Length + Count + sizeof(FileId))
                        mOffset += sizeof(Int32) + (e.Children.Count * sizeof(Int32));
                    }
                        break;
                    case 2: // Write the FileId(Int32)[] for every TocEntry that needs it
                    {
                        if (HasChildren(e))
                        {
                            writer.Write(e.Children.Count);
                            foreach (ITocEntry te in e.Children)
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

        // <summary>
        // The Fdb (holding FilenameOffset(Int32)[] and Filename(string)[]) consists of 3 iterations
        //
        //     Iteration 1:
        //         Computing the offset to every filename
        //
        //     Iteration 2: FilenameOffset(Int32)[]
        //         Writing the offset to the filename for every TocEntry
        //
        //     Iteration 3: Filename(string)[]
        //         Writing the filename (string) for every TocEntry
        //
        // </summary>
        private sealed class WriteFdbContext : IWriteContext
        {
            #region Fields

            private int mIteration;
            private int mIndex;
            private Int32 mOffset;
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
                        mOffset = Alignment.Align32(mOffset, 4);
                        mOffset += sizeof(Int32) + filename.Length + 1;
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
        private sealed class WriteHdbContext : IWriteContext
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

        private ITocEntry[] mTable;

        #endregion
        #region Constructor(s)

        public BigfileToc()
        {
        }

        #endregion
        #region Properties

        public int Count
        {
            get { return mTable.Length; }
        }

        #endregion
        #region Methods

        public void Reset()
        {
            mTable = Array.Empty<ITocEntry>();
        }

        public BigfileFile InfoOf(int fileId)
        {
            ITocEntry e = mTable[fileId];
            return new BigfileFile(e.Filename, e.FileSize, e.FileOffset, e.FileId, e.FileContentHash);
        }

        public static bool Exists(string filename)
        {
            string bigFileTocFilename = filename;
            bigFileTocFilename = Path.ChangeExtension(bigFileTocFilename, BigfileConfig.BigFileTocExtension);
            FileInfo fileInfo = new(bigFileTocFilename);
            return fileInfo.Exists;
        }

        public static void CopyFilesOrder(BigfileToc currentToc, BigfileToc sourceToc)
        {
            // TODO
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
            FileInfo fileInfo = new(filename);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("We tried to load " + fileInfo + " but it does not exist.");
                return null;
            }

            FileStream fileStream = new(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
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
                        List<ITocEntry> table = new List<ITocEntry>();
                        ReadTable(table, Factory.CreateReadTocContext(table), bigFileTocFileStream, endian);
                        ReadTable(table, Factory.CreateReadFdbContext(), bigFileFdbFileStream, endian);
                        ReadTable(table, Factory.CreateReadHdbContext(), bigFileHdbFileStream, endian);
                        mTable = table.ToArray();
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
            FileInfo fileInfo = new(filename);
            if (!fileInfo.Exists)
            {
                FileStream fileCreationStream = File.Create(fileInfo.FullName);
                fileCreationStream.Close();
            }

            FileStream fileStream = new(fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.Write, 1 * 1024 * 1024, FileOptions.Asynchronous);
            return fileStream;
        }

        public bool Save(string bigfileFilename, EEndian endian, BigfileFile[] bigfileFiles)
        {
            // Create all TocEntry items in the same order as the Bigfile files which is important
            // because the FileId is equal to the location(index) in the List/Array.
            List<ITocEntry> table = new List<ITocEntry>(bigfileFiles.Length);
            foreach (var file in bigfileFiles)
            {
                ITocEntry fileEntry = Factory.Create(file.FileId, file.FileOffset, file.FileSize, file.Filename, ECompressed.NO, file.FileContentHash);
                table.Add(fileEntry);
            }

            // Manage children of each TocEntry
            foreach (var file in bigfileFiles)
            {
                ITocEntry entry = table[(int)file.FileId];
                foreach (var childFile in file.Children)
                {
                    ITocEntry childEntry = table[(int)childFile.FileId];
                    entry.Children.Add(childEntry);
                }
            }

            mTable = table.ToArray();

            try
            {
                FileStream bigFileTocFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileTocExtension));
                FileStream bigFileFdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileFdbExtension));
                FileStream bigFileHdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileHdbExtension));

                {
                    try
                    {
                        WriteTable(table, Factory.CreateWriteTocContext(), bigFileTocFileStream, endian);
                        WriteTable(table, Factory.CreateWriteFdbContext(), bigFileFdbFileStream, endian);
                        WriteTable(table, Factory.CreateWriteHdbContext(), bigFileHdbFileStream, endian);
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
