using System.Diagnostics;
using System.Text;
using GameCore;
using GameData;

namespace BigfileBuilder
{
    public sealed class BigfileToc
    {
        [Flags]
        public enum ETocFlags : byte
        {
            None = 0,
            Compressed = 1,
        }

        private interface IReadContext
        {
            void Begin(int outerIter, IBinaryStreamReader reader);
            bool Read(int outerIter, int innerIter, IBinaryStreamReader reader);
            bool Next(ref int outerIter);
        }

        private interface IWriteContext
        {
            void Begin(int outerIter, IBinaryStreamWriter writer);
            bool Write(int outerIter, int innerIter, IBinaryStreamWriter writer);
            bool Next(ref int outerIter);
        }

        private interface ITocEntryReader
        {
            int ReadCount(IBinaryStreamReader reader);
            uint ReadOffset(IBinaryStreamReader reader);
            void ReadFileSize(IBinaryStreamReader reader, ITocEntry entry);
            void ReadFileOffset(IBinaryStreamReader reader, ITocEntry entry);
        }

        private interface ITocEntryWriter
        {
            void WriteCount(IBinaryStreamWriter writer, int count);
            void WriteOffset(IBinaryStreamWriter writer, long offset);
            void WriteFileSize(IBinaryStreamWriter writer, ITocEntry entry);
            void WriteFileOffset(IBinaryStreamWriter writer, ITocEntry entry);
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
            public int ReadCount(IBinaryStreamReader reader)
            {
                return reader.ReadInt32();
            }

            public uint ReadOffset(IBinaryStreamReader reader)
            {
                return reader.ReadUInt32();
            }

            public void ReadFileSize(IBinaryStreamReader reader, ITocEntry entry)
            {
                entry.FileSize = reader.ReadUInt32();
            }

            public void ReadFileOffset(IBinaryStreamReader reader, ITocEntry entry)
            {
                var fileOffset = reader.ReadUInt32() << 6;
                entry.FileOffset = fileOffset;
            }
        }

        private sealed class TocEntryWriter32 : ITocEntryWriter
        {
            public void WriteCount(IBinaryStreamWriter writer, int count)
            {
                writer.Write(count);
            }

            public void WriteOffset(IBinaryStreamWriter writer, long offset)
            {
                writer.Write((int)offset);
            }

            public void WriteChildFileId(IBinaryStreamWriter writer, ulong childFileId)
            {
                writer.Write(childFileId);
            }

            public void WriteFileSize(IBinaryStreamWriter writer, ITocEntry entry)
            {
                var fileSize = entry.FileSize;
                writer.Write(fileSize);
            }

            public void WriteFileOffset(IBinaryStreamWriter writer, ITocEntry entry)
            {
                writer.Write((uint)(entry.FileOffset >> 6));
            }

            public int CountInBytes => 4;
            public int OffsetInBytes => 4;
            public int FileOffsetInBytes => 4;
            public int FileSizeInBytes => 4;
        }

        public interface ITocEntry
        {
            ulong Offset { get; set; }
            ETocFlags Flags { get; set; }
            string Filename { get; set; }
            uint FilenameOffset { get; set; }
            ulong FileOffset { get; set; }
            uint FileSize { get; set; }
            Hash160 FileContentHash { get; set; }
        }

        /// <summary>
        /// Factory for creating TocEntry, ReadContext and WriteContext
        /// </summary>
        private static class Factory
        {
            public static ITocEntry Create(ulong fileOffset, uint fileSize, string filename, ETocFlags type, Hash160 contentHash)
            {
                return new TocEntry(fileOffset, fileSize, filename, type, contentHash);
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
            public uint TocOffset { get; set; }
            public List<ITocEntry> Toc { get; }
            public int TocCount => Toc.Count;

            public TocSection(int numTocEntries)
            {
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
                : this(ulong.MaxValue, 0, string.Empty, ETocFlags.None, Hash160.Empty)
            {
            }

            public TocEntry(ulong fileOffset, uint fileSize, string filename, ETocFlags tocFlags, Hash160 contentHash)
            {
                Flags = tocFlags;
                Filename = filename;
                FilenameOffset = 0;
                FileOffset = fileOffset;
                FileSize = fileSize;
                FileContentHash = contentHash;
            }

            public ulong Offset { get; set; } // Offset of this struct in the stream
            public ETocFlags Flags { get; set; }
            public string Filename { get; set; }
            public uint FilenameOffset { get; set; }
            public ulong FileOffset { get; set; }
            public uint FileSize { get; set; }
            public Hash160 FileContentHash { get; set; }
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

            public void Begin(int outerIter, IBinaryStreamReader reader)
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
                    if (outerIter.IsEven())
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

            public bool Read(int outerIter, int innerIter, IBinaryStreamReader reader)
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

                        if (outerIter.IsEven())
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

            public void Begin(int outerIter, IBinaryStreamReader reader)
            {
                if (outerIter == -1)
                {

                    // Read the header
                    NumSections = reader.ReadInt32();
                    Debug.Assert(Sections.Count == NumSections);

                    // Read the section offsets
                    foreach(var section in Sections)
                    {
                        section.TocOffset = reader.ReadUInt32();
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

            public bool Read(int outerIter, int innerIter, IBinaryStreamReader reader)
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

            public void Begin(int outerIter, IBinaryStreamReader reader)
            {
                if (outerIter == -1)
                {
                    // Read the header
                    NumSections = reader.ReadInt32();

                    foreach (var section in Sections)
                    {
                        section.TocOffset = reader.ReadUInt32(); // Section offset
                    }
                }
            }

            public bool Read(int outerIter, int innerIter, IBinaryStreamReader reader)
            {
                switch (outerIter)
                {
                    case -1:
                        return innerIter < NumSections;

                    case >= 0:
                        // Read Toc Entry content hash
                        var hash = new byte[Hash160.Size];
                        reader.Read(hash, 0, hash.Length);

                        var te = Sections[outerIter].Toc[innerIter];
                        te.FileContentHash = Hash160.ConstructTake(hash);

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

                // Num Sections
                // sizeof(int) + (sizeof(int) + sizeof(int))[Num Sections]
                var offset = (uint)tocEntryWriter.CountInBytes + (uint)tocEntryWriter.CountInBytes + (uint)Sections.Count * (uint)(tocEntryWriter.OffsetInBytes + tocEntryWriter.CountInBytes);
                foreach (var section in Sections)
                {
                    section.TocOffset = offset;

                    // The size of TocEntry[]
                    offset += (uint)section.Toc.Count * (uint)(tocEntryWriter.FileOffsetInBytes + tocEntryWriter.FileSizeInBytes);
                }
            }

            public void Begin(int outerIter, IBinaryStreamWriter writer)
            {
                if (outerIter == -1)
                {
                    // Header
                    writer.Write(Sections.Count);
                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryStreamWriter writer)
            {
                switch (outerIter)
                {
                    case -1:
                        {
                            var section = Sections[innerIter];
                            TocEntryWriter.WriteOffset(writer, section.TocOffset);
                            TocEntryWriter.WriteCount(writer, section.Toc.Count);
                            return innerIter < Sections.Count;
                        }
                    case >= 0:
                        {
                            var section = Sections[outerIter / 2];
                            var e = section.Toc[innerIter];
                            if (outerIter.IsEven())
                            {
                                TocEntryWriter.WriteFileOffset(writer, e);
                                TocEntryWriter.WriteFileSize(writer, e);
                                return innerIter < section.Toc.Count;
                            }

                            return innerIter < section.Toc.Count;
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

            public void Begin(int outerIter, IBinaryStreamWriter writer)
            {
                if (outerIter == -1)
                {
                    // For each section, per TocEntry compute the offset to the filename
                    var offset = (uint)(sizeof(int) + Sections.Count * sizeof(int));
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
                            offset += sizeof(uint);
                            offset += sizeof(uint);
                            offset += strByteLen + 1;
                            offset = CMath.AlignUp32(offset, 4);
                        }
                    }

                    // Header
                    writer.Write(Sections.Count);
                    foreach (var section in Sections)
                    {
                        writer.Write(section.TocOffset);
                    }

                    StringByteBuffer = new byte[CMath.AlignUp32(maxStrByteLen + 64, 256)];
                }
                else
                {
                    var section = Sections[outerIter];
                    foreach (var e in section.Toc)
                    {
                        // The offset that we actually write is relative to the offset of the section
                        var offset = (uint)e.FilenameOffset - (uint)section.TocOffset;
                        writer.Write(offset);
                    }

                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryStreamWriter writer)
            {
                if (outerIter < 0 || outerIter >= Sections.Count)
                    return false;

                var section = Sections[outerIter];
                var filename = section.Toc[innerIter].Filename;

                var strNumEncodedBytes = 1 + Encoding.UTF8.GetBytes(filename, 0, filename.Length, StringByteBuffer, 0);
                StringByteBuffer[strNumEncodedBytes - 1] = 0; // Null terminate the string
                var numGapBytes = (4 - (strNumEncodedBytes & 3)) & 3; // Align to the next multiple of 4
                Array.Fill<byte>(StringByteBuffer, 0, strNumEncodedBytes, numGapBytes);

                Debug.Assert(writer.Position == section.Toc[innerIter].FilenameOffset);

                writer.Write((uint)strNumEncodedBytes); // Write the byte length of the filename (includes null terminator)
                writer.Write(filename.Length); // Write the character length of the filename
                writer.Write(StringByteBuffer, 0, strNumEncodedBytes + numGapBytes);  // Write the filename with padding to align to a multiple of 4 bytes

                return innerIter < section.TocCount;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < Sections.Count;
            }
        }

        // <summary>
        // The Hdb or Hash Database (holding Hash160[])
        // </summary>
        private sealed class WriteHdbContext : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; }

            public WriteHdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            public void Begin(int outerIter, IBinaryStreamWriter writer)
            {
                if (outerIter == -1)
                {
                    // Header
                    writer.Write(Sections.Count);

                    // Compute the offset of each section
                    // Count:Sections.Count + (Section.Count * (Offset:sizeof(uint) + Count:sizeof(uint)))
                    var offset = (uint)(sizeof(int) + Sections.Count * (sizeof(uint) + sizeof(uint)));
                    foreach (var section in Sections)
                    {
                        writer.Write(offset); // Write the offset of this section
                        writer.Write(section.TocCount); // Write the count of this section
                        offset += (uint)(section.TocCount * Hash160.Size); // The size of Hash160
                    }
                    // Per section, write the hash of each TocEntry
                    foreach (var section in Sections)
                    {
                        foreach (var te in section.Toc)
                        {
                            te.FileContentHash.WriteTo(writer);
                        }
                    }
                }
            }

            public bool Write(int outerIter, int innerIter, IBinaryStreamWriter writer)
            {
                return false;
            }

            public bool Next(ref int outerIter)
            {
                outerIter += 1;
                return outerIter < 0;
            }
        }

        private static void ReadTable(IReadContext context, Stream stream, EPlatform platform)
        {
            var binaryReader = ArchitectureUtils.CreateBinaryReader(stream, platform);
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

        private static void WriteTable(IWriteContext context, Stream stream, EPlatform platform)
        {
            var binaryWriter = ArchitectureUtils.CreateBinaryWriter(stream, platform);
            {
                var block = -1;
                do
                {
                    context.Begin(block, binaryWriter);

                    var i = 0;
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

            var fileStream = new FileStream(fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.Write, 1 * 1024 * 1024, FileOptions.Asynchronous);
            return fileStream;
        }

        public bool Load(string filename, EPlatform platform, List<Bigfile> bigFiles)
        {
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
                        ReadTable(Factory.CreateReadTocContext(sections, tocEntryReader), bigFileTocFileStream, platform);
                        ReadTable(Factory.CreateReadFdbContext(sections), bigFileFdbFileStream, platform);
                        ReadTable(Factory.CreateReadHdbContext(sections), bigFileHdbFileStream, platform);
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

        public static bool Save(EPlatform platform, string bigfileFilename, List<Bigfile> bigFiles)
        {
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
                    var fileEntry = Factory.Create(file.Offset, (uint)file.Size, file.Filename, ETocFlags.None, file.ContentHash);
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
                        WriteTable(Factory.CreateWriteTocContext(sections, tocEntryWriter), bigFileTocFileStream, platform);
                        WriteTable(Factory.CreateWriteFdbContext(sections), bigFileFdbFileStream, platform);
                        WriteTable(Factory.CreateWriteHdbContext(sections), bigFileHdbFileStream, platform);
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
