using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameCore;

namespace BigfileBuilder
{
    public interface IBigfileWriter
    {
        bool Open(string filepath, long reserveSize);
        void Close();
        (bool, long, long) Save(string filepath); // Success=true, Offset, Size
    }

    public sealed class BigfileWriterSimulator : IBigfileWriter
    {
        private long Offset { get; set; }
        public long FinalSize { get; private set; }

        public bool Open(string filepath, long reserveSize)
        {
            Offset = 0;
            return true;
        }

        public void Close()
        {
            FinalSize = Offset;
        }

        public (bool, long, long) Save(string filepath)
        {
            if (!File.Exists(filepath))
                return (false, -1, 0);

            var fo = Offset;
            var fi = new FileInfo(filepath);
            Offset += fi.Length;
            Offset = CMath.AlignUp(Offset, BigfileConfig.FileAlignment);
            return (true, fo, fi.Length);
        }
    }

    public sealed class BigfileWriter : IBigfileWriter
    {
        private FileStream FileStream { get; set; }
        private byte[] ReadCache { get; set; } = new byte[BigfileConfig.ReadBufferSize];


        public bool Open(string filepath, long reserveSize)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);
                DirUtils.Create(bigfileInfo.DirectoryName);
                FileStream = new(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (int)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);

                // Reserve this file size on disk to speed up the writing of many files
                FileStream.SetLength(reserveSize);
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
        /// <returns>(success, offset, size)</returns>
        public (bool,long,long) Save(string filepath)
        {
            try
            {
                FileStream inputStream = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize);
                var fileSize = inputStream.Length;
                var fileOffset = Write(inputStream, fileSize);
                inputStream.Close();
                return (true, fileOffset, fileSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return (false, -1, 0);
            }
        }

        private long Write(Stream readStream, long fileSize)
        {
            var position = FileStream.Position;

            Debug.Assert(fileSize < int.MaxValue);

            var sizeToWrite = fileSize;
            while (sizeToWrite > 0)
            {
                var sizeToRead = (int)Math.Min(sizeToWrite, ReadCache.Length);
                var actualRead = readStream.Read(ReadCache, 0, sizeToRead);
                FileStream.Write(ReadCache, 0, actualRead);
                sizeToWrite -= actualRead;
            }

            // Align the file, gap should be filled with zeros (for compression and hashing to be deterministic between runs)
            var alignedPosition = CMath.AlignUp((position + fileSize), BigfileConfig.FileAlignment);
            var aligningBytesToWrite = (int)(alignedPosition - position);
            if (aligningBytesToWrite>0)
            {
                Array.Fill<byte>(ReadCache, 0, 0, aligningBytesToWrite);
                FileStream.Write(ReadCache, 0, aligningBytesToWrite);
            }
            return position;
        }


        public void Close()
        {
            if (FileStream == null) return;

            FileStream.Close();
            FileStream = null;
        }
    }

    public sealed class BigfileReader
    {
        private FileStream mFileStream;
        private BinaryReader mBinaryReader;

        public bool Open(string filepath)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);

                mFileStream = new(bigfileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize, FileOptions.Asynchronous);
                mBinaryReader = new(mFileStream);
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
            if (mFileStream == null) return;

            if (mBinaryReader != null)
            {
                mBinaryReader.Close();
                mBinaryReader = null;
            }

            mFileStream.Close();
            mFileStream = null;
        }
    }

    public sealed class Bigfile
    {
        public List<BigfileFile> Files { get; } = new();
        public uint Index { get; set; }

        public Bigfile(uint index)
        {
            Index = index;
        }

        public void Write(IBigfileWriter writer)
        {
            var additionalSize = (long)0;
            foreach (var bff in Files)
            {
                additionalSize += bff.FileSize;
                additionalSize = CMath.AlignUp(additionalSize, BigfileConfig.FileAlignment);
            }

            foreach (var bff in Files)
            {
                var (ok, fileOffset, fileSize) = writer.Save(bff.Filename);
                bff.FileSize = fileSize;
                bff.FileOffset = ok ? new StreamOffset(fileOffset) : StreamOffset.sEmpty;
            }
        }
    }
}
