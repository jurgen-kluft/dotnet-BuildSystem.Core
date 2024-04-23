using GameCore;
using GameData;

namespace DataBuildSystem
{
    public sealed class BigfileToc
    {
        [Flags]
        public enum ETocFlags : byte
        {
            None = 0,
            Compressed = 1,
            HasChildren = 2,
        }

        private interface IReadContext
        {
            void Begin(int block, IBinaryStreamReader reader);
            bool Read(int block, int item, IBinaryStreamReader reader);
            bool Next(ref int block);
        }

        private interface IWriteContext
        {
            void Begin(int block, IBinaryStreamWriter writer);
            bool Write(int block, int item, IBinaryStreamWriter writer);
            bool Next(ref int block);
        }

        public interface ITocEntryReader
        {
            void ReadFileSize(IBinaryStreamReader reader, ITocEntry entry);
            void ReadFileOffset(IBinaryStreamReader reader, ITocEntry entry);
        }

        public interface ITocEntryWriter
        {
            void WritePointer(IBinaryStreamWriter writer, long offset);
            void WriteFileSize(IBinaryStreamWriter writer, ITocEntry entry);
            void WriteFileOffset(IBinaryStreamWriter writer, ITocEntry entry);
            int FileOffsetInBytes { get; }
            int FileSizeInBytes { get; }
        }

        public sealed class TocEntryReader32 : ITocEntryReader
        {
            // The file offset is aligned to 64 to enable the BigFile size to be maximum 4 GB * 64 = 256 GB
            // Max file size = 1 GB, highest 2 bits are used to indicate compression and if the FileId has children

            public void ReadFileSize(IBinaryStreamReader reader, ITocEntry entry)
            {
                var fileSize = reader.ReadInt32();
                entry.FileSize = (fileSize & 0x3FFFFFFF);
                entry.Flags |= (fileSize & 0x80000000) != 0 ? ETocFlags.Compressed : 0;
                entry.Flags |= (fileSize & 0x40000000) != 0 ? ETocFlags.HasChildren : 0;
            }

            public void ReadFileOffset(IBinaryStreamReader reader, ITocEntry entry)
            {
                var fileOffset = reader.ReadInt32() << 6;
                entry.FileOffset = new StreamOffset(fileOffset);
            }
        }

        public sealed class TocEntryWriter32 : ITocEntryWriter
        {
            public void WritePointer(IBinaryStreamWriter writer, long offset)
            {
                // 32-bit pointer
                writer.Write((int)offset);
            }

            public void WriteFileSize(IBinaryStreamWriter writer, ITocEntry entry)
            {
                var fileSize = entry.FileSize;
                if (entry.Flags.HasFlag(ETocFlags.Compressed))
                {
                    fileSize |= 0x80000000;
                }
                if (entry.Children.Count>0)
                {
                    fileSize =  (long)((ulong)fileSize | 0x40000000);
                }
                writer.Write((int)fileSize);
            }

            public void WriteFileOffset(IBinaryStreamWriter writer, ITocEntry entry)
            {
                writer.Write((int)(entry.FileOffset.Offset >> 6));
            }
            public int FileOffsetInBytes => 4;
            public int FileSizeInBytes => 4;

        }

        public interface ITocEntry
        {
            string Filename { get; set; }
            long FileId { get; set; }
            StreamOffset FileOffset { get; set; }
            long FileSize { get; set; }
            ETocFlags Flags { get; set; }
            Hash160 FileContentHash { get; set; }
            List<ITocEntry> Children { get; set; }
        }

        /// <summary>
        /// Factory for creating TocEntry, ReadContext and WriteContext
        /// </summary>
        private static class Factory
        {
            public static ITocEntry Create(long fileId, StreamOffset fileOffset, long fileSize, string filename, ETocFlags type, Hash160 contentHash)
            {
                return new TocEntry(fileId, fileOffset, fileSize, filename, type, contentHash);
            }

            public static ITocEntry Create(int fileSize, string filename, ETocFlags type, Hash160 contentHash)
            {
                return new TocEntry(fileSize, filename, type, contentHash);
            }

            public static IReadContext CreateReadTocContext(List<TocSection> sections, List<ITocEntry> entries, ITocEntryReader tocEntryReader)
            {
                return new ReadToc32Context(sections, entries, tocEntryReader);
            }

            public static IReadContext CreateReadFdbContext(IReadOnlyList<TocSection> sections)
            {
                return new ReadFdbContext(sections);
            }

            public static IReadContext CreateReadHdbContext(IReadOnlyList<TocSection> sections)
            {
                return new ReadHdbContext(sections);
            }

            public static IWriteContext CreateWriteTocContext(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries, ITocEntryWriter tocEntryWriter)
            {
                return new WriteToc32Context(sections, entries, tocEntryWriter);
            }

            public static IWriteContext CreateWriteFdbContext(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries)
            {
                return new WriteFdbContext(sections, entries);
            }

            public static IWriteContext CreateWriteHdbContext(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries)
            {
                return new WriteHdbContext(sections, entries);
            }
        }


        private sealed class TocSection
        {
            public int TocOffset { get; set; }

            public int TocCount => Toc.Count;
            public long DataOffset { get; set; }
            public List<ITocEntry> Toc { get; set; } = new();
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
                : this(long.MaxValue, StreamOffset.Empty, -1, string.Empty, ETocFlags.None, Hash160.Empty)
            {
            }

            public TocEntry(string filename, ETocFlags tocFlags, Hash160 contentHash)
                : this(long.MaxValue, StreamOffset.Empty, -1, filename, tocFlags, contentHash)
            {
            }

            public TocEntry(int fileSize, string filename, ETocFlags tocFlags, Hash160 contentHash)
                : this(long.MaxValue, StreamOffset.Empty, fileSize, filename, tocFlags, contentHash)
            {
            }

            public TocEntry(long fileId)
                : this(fileId, StreamOffset.Empty, -1, string.Empty, ETocFlags.None, Hash160.Empty)
            {
            }

            public TocEntry(long fileId, StreamOffset fileOffset, int fileSize)
                : this(fileId, fileOffset, fileSize, string.Empty, ETocFlags.None, Hash160.Empty)
            {
            }

            public TocEntry(long fileId, StreamOffset fileOffset, long fileSize, string filename, ETocFlags tocFlags, Hash160 contentHash)
            {
                FileOffset = fileOffset;
                Filename = filename;
                FileId = fileId;
                FileContentHash = contentHash;
                FileSize = fileSize;
                Flags = tocFlags;
            }

            public TocSection Section { get; set; }
            public long FileId { get; set; }
            public StreamOffset FileOffset { get; set; }
            public long FileSize { get; set; }
            public string Filename { get; set; }
            public Hash160 FileContentHash { get; set; }
            public ETocFlags Flags { get; set; }
            public List<ITocEntry> Children { get; set; } = new();
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
            private List<TocSection> Sections { get; set; }
            private List<ITocEntry> Entries { get; set; }
            private ITocEntryReader TocEntryReader { get; set; }

            public ReadToc32Context(List<TocSection> sections, List<ITocEntry> entries, ITocEntryReader tocEntryReader)
            {
                Sections = sections;
                Entries = entries;
                TocEntryReader = tocEntryReader;
            }

            public void Begin(int block, IBinaryStreamReader reader)
            {
                if (block == -1)
                {
                    // Read the header
                    var totalNumberOfEntries = reader.ReadInt32();
                    var numberOfSections = reader.ReadInt32();

                    Sections.Capacity = numberOfSections;
                    Entries.Capacity = totalNumberOfEntries;

                    // iterate for numberOfSections
                }

                if (block < Sections.Count)
                {
                    if (block.IsEven())
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

            public bool Read(int block, int item, IBinaryStreamReader reader)
            {
                switch (block)
                {
                    case -1:
                        var ts = Sections[item];
                        var count = reader.ReadInt32();
                        ts.DataOffset = reader.ReadInt32() << 5;
                        ts.TocOffset = reader.ReadInt32();
                        ts.Toc = new(count);
                        for (var j = 0; j < count; ++j)
                        {
                            var e = new TocEntry();
                            ts.Toc.Add(e);
                            Entries.Add(e);
                        }

                        return item < Sections.Count;

                    case >= 0:
                        var sectionIndex = (block / 2);

                        if (block.IsEven())
                        {
                            // Read Toc Entries
                            var e = Sections[sectionIndex].Toc[item];
                            TocEntryReader.ReadFileOffset(reader, e);
                            TocEntryReader.ReadFileSize(reader, e);

                            // This will also mark the flags Compressed & HasChildren which are bits in the file size
                        }
                        else
                        {
                            // Read extra info for some TocEntry
                            var e = Sections[sectionIndex].Toc[item];
                            if (e.Flags.HasFlag(ETocFlags.HasChildren))
                            {
                                var numChildren = reader.ReadInt32();
                                for (var i = 0; i < numChildren; ++i)
                                {
                                    var childFileId = reader.ReadInt32();
                                    var childEntry = Sections[sectionIndex].Toc[childFileId];
                                    e.Children.Add(childEntry);
                                }
                            }

                        }

                        return item < Sections[sectionIndex].Toc.Count;
                }

                return false;
            }

            public bool Next(ref int block)
            {
                block++;
                return block < (Sections.Count * 2);
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
            private IReadOnlyList<TocSection> Sections { get; set; }

            public ReadFdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            private int NumTotalEntries { get; set; }
            private int NumSections { get; set; }

            public void Begin(int block, IBinaryStreamReader reader)
            {
                if (block == -1)
                {
                    // Read the header
                    NumTotalEntries = reader.ReadInt32();
                    NumSections = reader.ReadInt32();

                    // numSections
                }

                if (block.IsEven())
                {
                    // Sections[block].Toc.Count
                }

                // Sections[block].TocExtraCount
            }

            public bool Read(int block, int item, IBinaryStreamReader reader)
            {
                switch (block)
                {
                    case -1:
                        reader.ReadInt32(); // read total number of entries
                        reader.ReadInt32(); // read number of sections

                        return item < NumSections;

                    case >= 0:
                        // Read Toc Entry filenames
                        if (block.IsEven())
                        {
                            reader.ReadInt32(); // Offset (skip)
                        }
                        else
                        {
                            reader.Position = CMath.Align(reader.Position, 4);

                            var sectionIndex = (block / 2);
                            var te = Sections[sectionIndex].Toc[item];
                            te.Filename = reader.ReadString();
                        }

                        return item < Sections[block].Toc.Count;

                }

                return false;
            }

            public bool Next(ref int block)
            {
                block += 1;
                return block < (Sections.Count * 2);
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
            private IReadOnlyList<TocSection> Sections { get; set; }

            public ReadHdbContext(IReadOnlyList<TocSection> sections)
            {
                Sections = sections;
            }

            private int NumTotalEntries { get; set; }
            private int NumSections { get; set; }

            public void Begin(int block, IBinaryStreamReader reader)
            {
                if (block == -1)
                {
                    // Read the header
                    NumTotalEntries = reader.ReadInt32();
                    NumSections = reader.ReadInt32();

                    // Return 'numSections' as the iterator count for 'block' = -1
                    // numSections
                }

                if (block < Sections.Count)
                {
                    // Return the number of TocEntry's in the section as the iterator count for 'block' >= 0
                    // Sections[block].Toc.Count
                }

                // 0
            }

            public bool Read(int sectionIndex, int item, IBinaryStreamReader reader)
            {
                switch (sectionIndex)
                {
                    case -1:
                        var ts = Sections[item];
                        var count = reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt64();

                        return item < NumSections;

                    case >= 0:
                        // Read Toc Entry content hash
                        var hash = new byte[Hash160.Size];
                        reader.Read(hash, 0, hash.Length);

                        var te = Sections[sectionIndex].Toc[item];
                        te.FileContentHash = Hash160.ConstructTake(hash);

                        return item < Sections[sectionIndex].Toc.Count;
                }

                return false;
            }

            public bool Next(ref int sectionIndex)
            {
                sectionIndex += 1;
                return sectionIndex < Sections.Count;
            }
        }

        // <summary>
        // Write the multi-section TOC
        // </summary>
        private sealed class WriteToc32Context : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; }
            private IReadOnlyList<ITocEntry> Entries { get; }

            private ITocEntryWriter TocEntryWriter { get; }

            public WriteToc32Context(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries, ITocEntryWriter tocEntryWriter)
            {
                Sections = sections;
                Entries = entries;
                TocEntryWriter = tocEntryWriter;

                // Compute the offset of each section
                var offset = sizeof(int) + sizeof(int) + Sections.Count * sizeof(int);
                foreach (var section in Sections)
                {
                    section.TocOffset = offset;

                    // The size of TocEntry[]
                    offset += section.Toc.Count * (tocEntryWriter.FileOffsetInBytes + tocEntryWriter.FileSizeInBytes);

                    // However we also have an 'extra' block where we are writing Children of TocEntry's that have them
                    foreach (var te in section.Toc)
                    {
                        if (HasChildren(te))
                        {
                            offset += sizeof(int) + te.Children.Count * sizeof(int);
                        }
                    }
                }
            }

            private static bool HasChildren(ITocEntry e)
            {
                return e.Children.Count > 0;
            }

            public void Begin(int block, IBinaryStreamWriter writer)
            {
                if (block == -1)
                {
                    // Header
                    writer.Write(Entries.Count);
                    writer.Write(Sections.Count);
                    // Sections.Count;
                }

                var section = Sections[block / 2];
                // section.TocCount;
            }

            public bool Write(int block, int item, IBinaryStreamWriter writer)
            {
                switch (block)
                {
                    case -1:
                        {
                            // Write the offset to each section, use 64-bit so that
                            // the C++ side can replace it with a pointer after loading.
                            var section = Sections[item];
                            TocEntryWriter.WritePointer(writer, section.TocOffset);
                            return item < Sections.Count;
                        }
                    case >= 0:
                        {
                            var section = Sections[block / 2];
                            var e = section.Toc[item];
                            if (block.IsEven())
                            {
                                TocEntryWriter.WriteFileOffset(writer, e);
                                TocEntryWriter.WriteFileSize(writer, e);
                                return item < section.Toc.Count;
                            }

                            if (HasChildren(e))
                            {
                                writer.Write(e.Children.Count);
                                foreach (var ce in e.Children)
                                {
                                    writer.Write(ce.FileId.Lower32());
                                }
                            }
                            return item < section.Toc.Count;
                        }
                }

                return false;
            }

            public bool Next(ref int block)
            {
                block += 1;
                return block < (2 * Sections.Count);
            }
        }

        // <summary>
        // Each section of the Fdb holds {FilenameOffset(Int32)[],  Filename(string)[]}
        // </summary>
        private sealed class WriteFdbContext : IWriteContext
        {
            private int Index { get; set; }

            private IReadOnlyList<TocSection> Sections { get; set; }
            private IReadOnlyList<ITocEntry> Entries { get; set; }
            private List<int> SectionOffsets { get; set; }
            private List<int> FilenameOffsets { get; set; }

            public WriteFdbContext(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries)
            {
                Sections = sections;
                Entries = entries;

                SectionOffsets = new(Sections.Count);
                FilenameOffsets = new(entries.Count);

                // Compute the offset of each section
                var offset = sizeof(int) + sizeof(int) + Sections.Count * sizeof(int);
                foreach (var section in Sections)
                {
                    SectionOffsets.Add(offset);

                    offset += section.TocCount * sizeof(int); // The size of Offset[]
                    foreach (var e in section.Toc)
                    {
                        FilenameOffsets.Add(offset);
                        offset += sizeof(int) + e.Filename.Length + 1;
                        offset = CMath.Align32(offset, 4);
                    }
                }
            }

            public void Begin(int block, IBinaryStreamWriter writer)
            {
                Index = 0;

                if (block == -1)
                {
                    // Header
                    writer.Write(Entries.Count);
                    writer.Write(Sections.Count);
                    // Sections.Count;
                }

                var section = Sections[block / 2];
                // section.TocCount;
            }

            public bool Write(int block, int item, IBinaryStreamWriter writer)
            {
                if (block == -1)
                {
                    writer.Write(SectionOffsets[item]); // Write the offset to each section
                    return item < Sections.Count;
                }
                else
                {
                    var section = Sections[block / 2];
                    if (block.IsEven())
                    {
                        writer.Write(FilenameOffsets[Index++]);
                    }
                    else
                    {
                        writer.Write(section.Toc[item].Filename);
                    }
                    return item < section.TocCount;
                }
            }

            public bool Next(ref int block)
            {
                block += 1;
                return block < (2 * Sections.Count);
            }
        }

        // <summary>
        // The Hdb (holding Hash160[])
        // </summary>
        private sealed class WriteHdbContext : IWriteContext
        {
            private IReadOnlyList<TocSection> Sections { get; set; }
            private IReadOnlyList<ITocEntry> Entries { get; set; }
            private List<int> SectionOffsets { get; set; }

            public WriteHdbContext(IReadOnlyList<TocSection> sections, IReadOnlyList<ITocEntry> entries)
            {
                Sections = sections;
                Entries = entries;

                SectionOffsets = new(Sections.Count);

                // Compute the offset of each section

                // Entries.Count + Sections.Count + (SectionOffsets.Count * sizeof(int))
                var offset = sizeof(int) + sizeof(int) + Sections.Count * sizeof(int);

                foreach (var section in Sections)
                {
                    SectionOffsets.Add(offset);
                    offset += section.TocCount * Hash160.Size; // The size of Hash160
                }
            }

            public void Begin(int block, IBinaryStreamWriter writer)
            {
                if (block == -1)
                {
                    // Header
                    writer.Write(Entries.Count);
                    writer.Write(Sections.Count);
                    // iterate for Sections.Count;
                }

                var section = Sections[block];
                // iterate for section.TocCount;
            }

            public bool Write(int block, int item, IBinaryStreamWriter writer)
            {
                if (block == -1)
                {
                    writer.Write(SectionOffsets[item]); // Write the offset to each section
                    return item < SectionOffsets.Count;
                }
                else
                {
                    var section = Sections[block];
                    section.Toc[item].FileContentHash.WriteTo(writer);
                    return item < section.Toc.Count;
                }
            }

            public bool Next(ref int block)
            {
                block += 1;
                return block < Sections.Count;
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
            var entries = new List<ITocEntry>();

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
                        ReadTable(Factory.CreateReadTocContext(sections, entries, tocEntryReader), bigFileTocFileStream, platform);
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

        public bool Save(string bigfileFilename, EPlatform platform, List<Bigfile> bigFiles)
        {
            // Create all TocEntry items in the same order as the Bigfile files which is important
            // because the FileId is equal to the location(index) in the List/Array.
            var totalEntries = 0;
            foreach (var bf in bigFiles)
            {
                totalEntries += bf.Files.Count;
            }

            var sections = new List<TocSection>();
            var entries = new List<ITocEntry>(totalEntries);
            foreach (var bf in bigFiles)
            {
                var section = new TocSection();
                foreach (var file in bf.Files)
                {
                    var fileEntry = Factory.Create(file.FileId, file.FileOffset, file.FileSize, file.Filename, ETocFlags.None, file.FileContentHash);
                    entries.Add(fileEntry);
                    section.Toc.Add(fileEntry);
                }
                sections.Add(section);
            }

            // Manage children of each TocEntry
            for (var i=0; i<bigFiles.Count; ++i)
            {
                var bf = bigFiles[i];
                var section = sections[i];

                foreach (var bff in bf.Files)
                {
                    var entry = entries[(int)bff.FileId];
                    foreach (var child in bff.Children)
                    {
                        var childEntry = entries[(int)child.FileId];
                        entry.Children.Add(childEntry);
                    }
                }
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
                        WriteTable(Factory.CreateWriteTocContext(sections, entries, tocEntryWriter), bigFileTocFileStream, platform);
                        WriteTable(Factory.CreateWriteFdbContext(sections, entries), bigFileFdbFileStream, platform);
                        WriteTable(Factory.CreateWriteHdbContext(sections, entries), bigFileHdbFileStream, platform);
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
