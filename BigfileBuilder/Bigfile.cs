using System.Diagnostics;

namespace BigfileBuilder
{
    public sealed class Bigfile
    {
        public uint Index { get; set; }
        public IReadOnlyList<BigfileFile> Files { get; set; }

        public Bigfile(uint index, IReadOnlyList<BigfileFile> files)
        {
            Index = index;
            Files = files;
        }
    }

    internal interface IBigfileWriter
    {
        bool Open(string filepath, ulong reserveSize);
        void Close();
        bool WriteFile(string filepath, out ulong offset, out ulong size);
    }

    internal sealed class BigfileWriterSimulator : IBigfileWriter
    {
        private ulong Offset { get; set; }
        public ulong FinalSize { get; private set; }

        public bool Open(string filepath, ulong reserveSize)
        {
            Offset = 0;
            return true;
        }

        public void Close()
        {
            FinalSize = Offset;
        }

        public bool WriteFile(string filepath, out ulong offset, out ulong size)
        {
            offset = ulong.MaxValue;
            size = 0;
            if (!File.Exists(filepath))
                return false;

            offset = Offset;
            var fi = new FileInfo(filepath);
            size = (ulong)fi.Length;
            Offset += (size + (BigfileConfig.FileAlignment - 1)) & ~(BigfileConfig.FileAlignment - 1);
            return true;
        }
    }

    internal sealed class BigfileWriter : IBigfileWriter
    {
        private FileStream BigfileFileStream { get; set; }
        private byte[] ReadCache { get; set; } = new byte[BigfileConfig.ReadBufferSize];

        public bool Open(string filepath, ulong reserveSize)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);
                BigfileFileStream = new FileStream(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (int)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);

                // Reserve this file size on disk to speed up the writing of many files
                BigfileFileStream.SetLength((long)reserveSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save all BigfileFiles into the Bigfile, allocate the full size of the Bigfile first
        /// and then use seek to write all the BigfileFiles.
        /// </summary>
        /// <param name="filepath">The name of the Bigfile</param>
        /// <param name="offset">The offset of the file in the Bigfile</param>
        /// <param name="size">The size of the file in the Bigfile</param>
        public bool WriteFile(string filepath, out ulong offset, out ulong size)
        {
            try
            {
                var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize);
                var fileSize = (ulong)fileStream.Length;
                var fileOffset = (ulong)Write(fileStream, (long)fileSize, BigfileFileStream, ReadCache);
                fileStream.Close();
                offset = fileOffset;
                size = fileSize;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                offset = ulong.MaxValue;
                size = 0;
                return false;
            }
        }

        private static long Write(FileStream readStream, long fileSize, FileStream writeStream, byte[] readCache)
        {
            var startPosition = writeStream.Position;

            Debug.Assert(fileSize < int.MaxValue);

            var sizeToWrite = fileSize;
            while (sizeToWrite > 0)
            {
                var sizeToRead = (int)Math.Min(sizeToWrite, readCache.Length);
                var actualRead = readStream.Read(readCache, 0, sizeToRead);
                writeStream.Write(readCache, 0, actualRead);
                sizeToWrite -= actualRead;
            }

            // Align the file, gap should be filled with zeros (for compression and hashing to be deterministic between runs)
            var alignedPosition = ((startPosition + fileSize) + (BigfileConfig.FileAlignment - 1)) & ~(BigfileConfig.FileAlignment - 1);
            var aligningBytesToWrite = (int)(alignedPosition - writeStream.Position);
            if (aligningBytesToWrite>0)
            {
                Array.Fill<byte>(readCache, 0, 0, aligningBytesToWrite);
                writeStream.Write(readCache, 0, aligningBytesToWrite);
            }
            return startPosition;
        }


        public void Close()
        {
            if (BigfileFileStream == null) return;

            BigfileFileStream.Close();
            BigfileFileStream = null;
        }
    }

    public sealed class BigfileReader
    {
        private FileStream _fileStream;
        private BinaryReader _binaryReader;

        public bool Open(string filepath)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);

                _fileStream = new FileStream(bigfileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize, FileOptions.Asynchronous);
                _binaryReader = new BinaryReader(_fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public void Close()
        {
            if (_fileStream == null) return;

            if (_binaryReader != null)
            {
                _binaryReader.Close();
                _binaryReader = null;
            }

            _fileStream.Close();
            _fileStream = null;
        }
    }

}
