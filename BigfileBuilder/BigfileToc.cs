using System.Diagnostics;
using System.Text;

namespace BigfileBuilder
{
    public sealed class BigfileToc
    {
        private const int HashSize = 8; // ulong

        private interface IReadContext
        {
            void Begin(int outerIter, IBinaryFileReader reader);
            bool Read(int outerIter, int innerIter, IBinaryFileReader reader);
            bool Next(ref int outerIter);
        }

        private interface IWriteContext
        {
            void Begin(int outerIter, IBinaryFileWriter writer);
            bool Write(int outerIter, int innerIter, IBinaryFileWriter writer);
            bool Next(ref int outerIter);
        }

        private interface ITocEntryReader
        {
            int ReadCount(IBinaryFileReader reader);
            uint ReadOffset(IBinaryFileReader reader);
            void ReadFileSize(IBinaryFileReader reader, ITocEntry entry);
            void ReadFileOffset(IBinaryFileReader reader, ITocEntry entry);
        }

        private interface ITocEntryWriter
        {
            void WriteU32(IBinaryFileWriter writer, uint u32);
            void WriteI32(IBinaryFileWriter writer, int i32);
            void WriteFileSize(IBinaryFileWriter writer, ITocEntry entry);
            void WriteFileOffset(IBinaryFileWriter writer, ITocEntry entry);
            int CountInBytes { get; } // Size of this in the data stream
            int OffsetInBytes { get; } // Size of this in the data stream
            int FileOffsetInBytes { get; } // Size of this in the data stream
            int FileSizeInBytes { get; } // Size of this in the data stream
        }

        private sealed class TocEntryReader32 : ITocEntryReader
        {
            // The file offset is aligned to 64 to enable the BigFile size to be maximum 4 GB * 64 = 256 GB
            // Max file size = 2 GB, we use the highest bit to indicate compression
            // Children offset uses the highest bit to indicate if it has children
            public int ReadCount(IBinaryFileReader reader)
            {
                return reader.ReadInt32();
            }

            public uint ReadOffset(IBinaryFileReader reader)
            {
                return reader.ReadUInt32();
            }

            public void ReadFileSize(IBinaryFileReader reader, ITocEntry entry)
            {
                entry.FileSize = reader.ReadUInt32();
            }

            public void ReadFileOffset(IBinaryFileReader reader, ITocEntry entry)
            {
                var fileOffset = reader.ReadUInt32() << 6;
                entry.FileOffset = fileOffset;
            }
        }

        private sealed class TocEntryWriter32 : ITocEntryWriter
        {
            public void WriteI32(IBinaryFileWriter writer, int i32)
            {
                writer.WriteI32(i32);
            }

            public void WriteU32(IBinaryFileWriter writer, uint u32)
            {
                writer.WriteU32(u32);
            }

            public void WriteFileSize(IBinaryFileWriter writer, ITocEntry entry)
            {
                var fileSize = entry.FileSize;
                writer.WriteU32(fileSize);
            }

            public void WriteFileOffset(IBinaryFileWriter writer, ITocEntry entry)
            {
                writer.WriteU32((uint)(entry.FileOffset >> 6));
            }

            public int CountInBytes => 4;
            public int OffsetInBytes => 4;
            public int FileOffsetInBytes => 4;
            public int FileSizeInBytes => 4;
        }

        public interface ITocEntry
        {
            ulong Offset { get; set; }
            string Filename { get; set; }
            uint FilenameOffset { get; set; }
            ulong FileOffset { get; set; }
            uint FileSize { get; set; }
            byte[] FileContentHash { get; set; }
        }

        /// <summary>
        /// Factory for creating TocEntry, ReadContext and WriteContext
        /// </summary>
        private static class Factory
        {
            public static ITocEntry Create(ulong fileOffset, uint fileSize, string filename, byte[] contentHash)
            {
                return new TocEntry(fileOffset, fileSize, filename, contentHash);
            }

            public static IReadContext CreateReadTocContext(List<TocSection> sections, ITocEntryReader tocEntryReader)
            {
                return new ReadToc32Context(sections, tocEntryReader);
            }

            public static IReadContext CreateReadFdbContext(IReadOnlyList<TocSection> sections)
            {
                return new ReadFdbContext(sections);
            }

            public static IReadContext CreateReadHdbContext(IReadOnlyList<TocSection> sections)
            {
                return new ReadHdbContext(sections);
            }

            public static IWriteContext CreateWriteTocContext(IReadOnlyList<TocSection> sections, ITocEntryWriter tocEntryWriter)
            {
                return new WriteTocContext(sections, tocEntryWriter);
            }

            public static IWriteContext CreateWriteFdbContext(IReadOnlyList<TocSection> sections)
            {
                return new WriteFdbContext(sections);
            }

            public static IWriteContext CreateWriteHdbContext(IReadOnlyList<TocSection> sections)
            {
                return new WriteHdbContext(sections);
            }
        }

        private sealed class TocSection
        {
            public uint ArchiveIndex { get; set; } // Archive/Bigfile Index
            public uint ArchiveDataOffset { get; set; } // When merging Bigfiles, this is the offset of the Bigfile data
            public uint TocOffset { get; set; }
            public List<ITocEntry> Toc { get; }
            public int TocCount => Toc.Count;

            public TocSection(int numTocEntries)
            {
                ArchiveIndex = 0;
                Toc = new List<ITocEntry>(numTocEntries);
            }
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
            public TocEntry()
                : this(ulong.MaxValue, 0, string.Empty, new byte[HashSize])
            {
            }

            public TocEntry(ulong fileOffset, uint fileSize, string filename, byte[] contentHash)
            {
                Filename = filename;
                FilenameOffset = 0;
                FileOffset = fileOffset;
                FileSize = fileSize;
                FileContentHash = contentHash;
            }

            public ulong Offset { get; set; } // Offset of this struct in the stream
            public string Filename { get; set; }
            public uint FilenameOffset { get; set; }
            public ulong FileOffset { get; set; }
            public uint FileSize { get; set; }
            public byte[] FileContentHash { get; set; }
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
            private List<TocSection> Sections { get; }
            private ITocEntryReader TocEntryReader { get; }

            public ReadToc32Context(List<TocSection> sections, ITocEntryReader tocEntryReader)
            {
                Sections = sections;
                TocEntryReader = tocEntryReader;
            }

            public void Begin(int outerIter, IBinaryFileReader reader)
            {
                if (outerIter == -1)
                {
                    // Read the header
                    var numberOfSections = TocEntryReader.ReadCount(reader);

                    Sections.Capacity = numberOfSections;
                    for (var j = 0; j < numberOfSections; ++j)
                    {
                        Sections.Add(new TocSection(0));
                    }

                    // iterate for numberOfSections
                }

                if (outerIter < Sections.Count)
                {
                    if ((outerIter&1) == 0)
                    {
                        // iterate for Sections[block].Toc.Count;
                    }
                    else
                    {
                        // iterate for Sections[block].Toc.Count;
                    }
                }

                // iterate for 0
            }

            public bool Read(int outerIter, int innerIter, IBinaryFileReader reader)
            {
                switch (outerIter)
                {
                    case -1:
                        var ts = Sections[innerIter];
                        ts.TocOffset = TocEntryReader.ReadOffset(reader);
                        var numTocEntries = TocEntryReader.ReadCount(reader);
                        ts.Toc.Capacity = numTocEntries;
                        for (var j = 0; j < numTocEntries; ++j)
                        {
                            ts.Toc.Add(new TocEntry());
                        }

                        return innerIter < Sections.Count;

                    case >= 0:
                        var sectionIndex = (outerIter / 2);

                        if ((outerIter & 1) == 0)
                        {
                            // Read Toc Entries
                            var e = Sections[sectionIndex].Toc[innerIter];
                            e.Offset = (ulong)reader.Position;
                            TocEntryReader.ReadFileOffset(reader, e);
                            TocEntryReader.ReadFileSize(reader, e);

                            // This will also mark the flags Compressed & HasChildren which are bits in the file size
                        }
                        else
                        {
                            // Read extra info for some TocEntry
                            var e = Sections[sectionIndex].Toc[innerIter];
                        }

                        return innerIter < Sections[sectionIndex].Toc.Count;
                }

                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter++;
                return outerIter < (Sections.Count * 2);
            }
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
            private IReadOnlyList<TocSection> Sections { get; }
            private byte[] StringBuffer { get; }

            public ReadFdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
                StringBuffer = new byte[65536];
            }

            private int NumSections { get; set; }

            public void Begin(int outerIter, IBinaryFileReader reader)
            {
                if (outerIter == -1)
                {
                    // Read the number of sections
                    NumSections = reader.ReadInt32();
                    Debug.Assert(Sections.Count == NumSections);

                    // Read the sections
                    foreach(var section in Sections)
                    {
                        var thisPosition = reader.Position;
                        var archiveIndex = reader.ReadUInt32();
                        var archiveDataOffset = reader.ReadUInt32();
                        var itemArrayCount = reader.ReadInt32();
                        var itemArrayOffset = (uint)thisPosition + reader.ReadUInt32(); // Section offset, relative to absolute

                        // section.ArchiveIndex = archiveIndex;
                        // section.ArchiveDataOffset = archiveDataOffset;
                        // section.Toc.Capacity = itemArrayCount;
                        // section.TocOffset = itemArrayOffset;
                    }
                }
                else
                {
                    reader.Position = Sections[outerIter].TocOffset;

                    foreach(var te in Sections[outerIter].Toc)
                    {
                        var offset = reader.ReadUInt32(); // Offset in stream to start of filename
                        te.FilenameOffset = offset;
                    }
                }
            }

            public bool Read(int outerIter, int innerIter, IBinaryFileReader reader)
            {
                switch (outerIter)
                {
                    case >= 0:
                        var section = Sections[outerIter];
                        var te = section.Toc[innerIter];

                        // Set stream position to the offset of where the filename is
                        // This really shouldn't do anything since the stream position should be the same as the TocEntry Filename Offset
                        reader.Position = te.FilenameOffset;

                        // Read Toc Entry filename
                        var byteLength = reader.ReadInt32(); // Byte length
                        var _ = reader.ReadInt32(); // String length
                        var bytesRead = reader.Read(StringBuffer, 0, byteLength); // String bytes
                        Debug.Assert(byteLength == bytesRead);

                        te.Filename = Encoding.UTF8.GetString(StringBuffer, 0, bytesRead); // String

                        return innerIter < section.TocCount;
                }

                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < Sections.Count;
            }
        }

        // <summary>
        // The Hdb (holding the Hash[]) consists of 1 iteration
        //
        //     Iteration 1:
        //         Reading the Hash[] to the filename for every TocEntry
        //
        // </summary>
        private sealed class ReadHdbContext : IReadContext
        {
            private IReadOnlyList<TocSection> Sections { get; }
            public ReadHdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            private int NumSections { get; set; }

            public void Begin(int outerIter, IBinaryFileReader reader)
            {
                if (outerIter == -1)
                {
                    // Read the header
                    NumSections = reader.ReadInt32();

                    foreach (var section in Sections)
                    {
                        var thisPosition = reader.Position;
                        var archiveIndex = reader.ReadUInt32();
                        var archiveDataOffset = reader.ReadUInt32();
                        var itemArrayCount = reader.ReadInt32();
                        var itemArrayOffset = (uint)thisPosition + reader.ReadUInt32(); // Section offset, relative to absolute

                        section.ArchiveIndex = archiveIndex;
                        section.ArchiveDataOffset = archiveDataOffset;
                        section.Toc.Capacity = itemArrayCount;
                        section.TocOffset = itemArrayOffset;
                    }
                }
            }

            public bool Read(int outerIter, int innerIter, IBinaryFileReader reader)
            {
                switch (outerIter)
                {
                    case -1:
                        return innerIter < NumSections;

                    case >= 0:
                        // Read Toc Entry content hash
                        var hash = new byte[HashSize];
                        reader.Read(hash, 0, hash.Length);

                        var te = Sections[outerIter].Toc[innerIter];
                        te.FileContentHash = hash;

                        return innerIter < Sections[outerIter].Toc.Count;
                }

                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < Sections.Count;
            }
        }

        // <summary>
        // The TOC or Table of Contents
        // This is also a multi-section TOC
        // </summary>
        private sealed class WriteTocContext : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; }

            private ITocEntryWriter TocEntryWriter { get; }

            public WriteTocContext(IReadOnlyList<TocSection> sections, ITocEntryWriter tocEntryWriter)
            {
                Sections = sections;
                TocEntryWriter = tocEntryWriter;

                // Simulation:
                // - Compute the offset of each section

                // Num Sections           => sizeof(int)
                // Num Sections * Section => Num Sections * (4 * sizeof(int) + sizeof(void*))
                var offset = (uint)tocEntryWriter.CountInBytes;
                offset += (uint)Sections.Count * (uint)(4 * tocEntryWriter.CountInBytes + tocEntryWriter.OffsetInBytes);
                foreach (var section in Sections)
                {
                    section.TocOffset = offset;
                    // The size of TocEntry[]
                    offset += (uint)section.Toc.Count * (uint)(tocEntryWriter.FileOffsetInBytes + tocEntryWriter.FileSizeInBytes);
                }
            }

            public void Begin(int outerIter, IBinaryFileWriter writer)
            {
                if (outerIter == -1)
                {
                    // Header
                    writer.WriteI32(Sections.Count);
                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryFileWriter writer)
            {
                switch (outerIter)
                {
                    case -1:
                        {
                            // struct section_t
                            // {
                            //     u32         m_ArchiveIndex;
                            //     u32         m_ArchiveOffset;
                            //     u32         m_ItemArrayCount;
                            //     u32         m_ItemArrayOffset;
                            // };

                            var thisPosition = writer.Position;
                            var section = Sections[innerIter];
                            writer.WriteU32(section.ArchiveIndex);
                            writer.WriteU32(section.ArchiveDataOffset);
                            writer.WriteI32(section.Toc.Count);
                            writer.WriteU32(section.TocOffset - (uint)thisPosition);
                            return (innerIter+1) < Sections.Count;
                        }
                    case >= 0:
                        {
                            var section = Sections[outerIter / 2];
                            if (innerIter < section.TocCount)
                            {
                                var e = section.Toc[innerIter];
                                if ((outerIter & 1) == 0)
                                {
                                    TocEntryWriter.WriteFileOffset(writer, e);
                                    TocEntryWriter.WriteFileSize(writer, e);
                                    return (innerIter + 1) < section.Toc.Count;
                                }
                            }

                            return (innerIter+1) < section.Toc.Count;
                        }
                }

                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < (2 * Sections.Count);
            }
        }

        // <summary>
        // The Fdb or Filename Database
        // Each section of the Fdb holds {FilenameOffset(UInt32)[], Filename(string)[]}
        // </summary>
        private sealed class WriteFdbContext : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; }
            private byte[] StringByteBuffer { get; set; }

            public WriteFdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            public void Begin(int outerIter, IBinaryFileWriter writer)
            {
                // For each section, per TocEntry compute the offset to the filename
                if (outerIter == -1)
                {
                    var offset = (uint)(sizeof(int));
                    offset += (uint)Sections.Count * (4 * sizeof(uint));

                    var maxStrByteLen = (uint)0;
                    foreach (var section in Sections)
                    {
                        section.TocOffset = offset;
                        offset += (uint)section.TocCount * sizeof(uint);

                        foreach (var e in section.Toc)
                        {
                            e.FilenameOffset = offset;

                            // We need to figure out the length of the string in bytes, treat the string as UTF-8 and do include a null terminator.
                            // ByteLen(FileName) + StrLen(FileName) + byte[] + alignment
                            var strByteLen = (uint)(Encoding.UTF8.GetByteCount(e.Filename));
                            maxStrByteLen = Math.Max(maxStrByteLen, strByteLen);
                            offset += sizeof(uint); // byte length
                            offset += sizeof(uint); // rune length
                            offset += strByteLen + 1; // string
                            offset = (offset + (4 - 1)) & ~(uint)(4 - 1);
                        }
                    }

                    // section count and the array of 'sections'
                    writer.WriteI32(Sections.Count);
                    foreach (var section in Sections)
                    {
                        var thisPosition = writer.Position;
                        writer.WriteU32(section.ArchiveIndex);
                        writer.WriteU32(section.ArchiveDataOffset);
                        writer.WriteI32(section.Toc.Count);
                        writer.WriteU32(section.TocOffset - (uint)thisPosition);
                    }

                    StringByteBuffer = new byte[(maxStrByteLen + 64 + (256 - 1)) & ~(256 - 1)];
                }
                else
                {
                    // Per section, write the array of 'offset to filename'
                    var section = Sections[outerIter];
                    foreach (var e in section.Toc)
                    {
                        // The offset that we actually write is relative to the offset of the section
                        var relOffset = (uint)e.FilenameOffset - (uint)section.TocOffset;
                        writer.WriteU32(relOffset);
                    }
                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryFileWriter writer)
            {
                if (outerIter < 0 || outerIter >= Sections.Count)
                    return false;

                var section = Sections[outerIter];
                if (innerIter < section.TocCount)
                {
                    var filename = section.Toc[innerIter].Filename;

                    var strNumEncodedBytes = 1 + Encoding.UTF8.GetBytes(filename, 0, filename.Length, StringByteBuffer, 0);
                    StringByteBuffer[strNumEncodedBytes - 1] = 0; // Null terminate the string
                    var numGapBytes = (4 - (strNumEncodedBytes & 3)) & 3; // Align to the next multiple of 4
                    Array.Fill<byte>(StringByteBuffer, 0, strNumEncodedBytes, numGapBytes);

                    Debug.Assert(writer.Position == section.Toc[innerIter].FilenameOffset);

                    writer.WriteU32((uint)strNumEncodedBytes); // Write the byte length of the filename (includes null terminator)
                    writer.WriteI32(filename.Length); // Write the character length of the filename
                    writer.WriteU32(StringByteBuffer, 0, strNumEncodedBytes + numGapBytes); // Write the filename with padding to align to a multiple of 4 bytes
                }

                return (innerIter+1) < section.TocCount;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < Sections.Count;
            }
        }

        // <summary>
        // The Hdb or Hash Database (holding ulong[])
        // </summary>
        private sealed class WriteHdbContext : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; }

            public WriteHdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            public void Begin(int outerIter, IBinaryFileWriter writer)
            {
                if (outerIter == -1)
                {
                    // Compute the offset of each section
                    var offset = (uint)(sizeof(int) + Sections.Count * (4 * sizeof(uint)));
                    foreach (var section in Sections)
                    {
                        section.TocOffset = offset;
                        offset += (uint)(section.TocCount * HashSize);
                    }

                    // Write the section array, containing the offset and count of each section
                    writer.WriteI32(Sections.Count);
                    foreach (var section in Sections)
                    {
                        var thisPosition = writer.Position;

                        writer.WriteU32(section.ArchiveIndex);
                        writer.WriteU32(section.ArchiveDataOffset);
                        writer.WriteI32(section.Toc.Count);
                        // Make the offset relative to the position of this section
                        writer.WriteU32(section.TocOffset - (uint)thisPosition);
                    }

                    // Per section, write Array<hash>
                    foreach (var section in Sections)
                    {
                        foreach (var te in section.Toc)
                        {
                            writer.WriteU32(te.FileContentHash, 0, HashSize);
                        }
                    }
                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryFileWriter writer)
            {
                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < 0;
            }
        }

        private static void ReadTable(IReadContext context, FileStream fileStream, bool isLittleEndian)
        {
            var binaryReader = new BinaryFileReader(fileStream, isLittleEndian);
            {
                var block = -1;
                do
                {
                    context.Begin(block, binaryReader);

                    var i = 0;
                    while (context.Read(block, i, binaryReader))
                    {
                        ++i;
                    }
                } while (context.Next(ref block));
            }
            binaryReader.Close();
        }

        private static FileStream OpenFileStreamForReading(string filename)
        {
            FileInfo fileInfo = new(filename);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("We tried to open '" + fileInfo + "' but it does not exist.");
                return null;
            }

            FileStream fileStream = new(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
            return fileStream;
        }

        private static void WriteTable(IWriteContext context, FileStream fileStream, bool isLittleEndian)
        {
            var binaryWriter = new BinaryFileWriter(fileStream, isLittleEndian);
            {
                var block = -1;
                do
                {
                    context.Begin(block, binaryWriter);

                    var i = 0;

                    // TODO, REFACTOR, the iteration should move into the context
                    while (context.Write(block, i, binaryWriter))
                    {
                        ++i;
                    }
                } while (context.Next(ref block));
            }
            binaryWriter.Close();
        }

        private static FileStream OpenFileStreamForWriting(string filename)
        {
            var fileInfo = new FileInfo(filename);
            if (!fileInfo.Exists)
            {
                var fileCreationStream = File.Create(fileInfo.FullName);
                fileCreationStream.Close();
            }

            var fileStream = new FileStream(fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.Write, 16 * 1024);
            return fileStream;
        }

        public static bool Load(string filename, List<Bigfile> bigFiles)
        {
            const bool isLittleEndian = true;
            var sections = new List<TocSection>();

            try
            {
                var bigFileTocFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileTocExtension));
                var bigFileFdbFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileFdbExtension));
                var bigFileHdbFileStream = OpenFileStreamForReading(Path.ChangeExtension(filename, BigfileConfig.BigFileHdbExtension));

                // Read the TocEntry file size and file offset as 32-bit integers
                var tocEntryReader = new TocEntryReader32();

                {
                    try
                    {
                        ReadTable(Factory.CreateReadTocContext(sections, tocEntryReader), bigFileTocFileStream, isLittleEndian);
                        ReadTable(Factory.CreateReadFdbContext(sections), bigFileFdbFileStream, isLittleEndian);
                        ReadTable(Factory.CreateReadHdbContext(sections), bigFileHdbFileStream, isLittleEndian);
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

        public static bool Save(string bigfileFilename, List<Bigfile> bigFiles)
        {
            const bool isLittleEndian = true;

            // Create all TocEntry items in the same order as the Bigfile files which is important
            // because the FileId is equal to the location(index) in the List/Array.
            var totalNumEntries = 0;
            foreach (var bf in bigFiles)
            {
                totalNumEntries += bf.Files.Count;
            }

            var sections = new List<TocSection>(bigFiles.Count);
            foreach (var bf in bigFiles)
            {
                var section = new TocSection(bf.Files.Count);
                foreach (var file in bf.Files)
                {
                    var fileEntry = Factory.Create(file.Offset, (uint)file.Size, file.Filename, file.ContentHash);
                    section.Toc.Add(fileEntry);
                }

                sections.Add(section);
            }

            try
            {
                var bigFileTocFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileTocExtension));
                var bigFileFdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileFdbExtension));
                var bigFileHdbFileStream = OpenFileStreamForWriting(Path.ChangeExtension(bigfileFilename, BigfileConfig.BigFileHdbExtension));

                // Write the TocEntry file size and file offset using 32-bit integers
                var tocEntryWriter = new TocEntryWriter32();

                {
                    try
                    {
                        WriteTable(Factory.CreateWriteTocContext(sections, tocEntryWriter), bigFileTocFileStream, isLittleEndian);
                        WriteTable(Factory.CreateWriteFdbContext(sections), bigFileFdbFileStream, isLittleEndian);
                        WriteTable(Factory.CreateWriteHdbContext(sections), bigFileHdbFileStream, isLittleEndian);
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
    }
}
