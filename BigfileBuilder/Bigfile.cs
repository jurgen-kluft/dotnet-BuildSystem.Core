using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GameCore;

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
        bool Write(string filepath, out ulong offset, out ulong size);
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

        public bool Write(string filepath, out ulong offset, out ulong size)
        {
            offset = ulong.MaxValue;
            size = 0;
            if (!File.Exists(filepath))
                return false;

            offset = Offset;
            var fi = new FileInfo(filepath);
            size = (ulong)fi.Length;
            Offset += CMath.AlignUp(offset, BigfileConfig.FileAlignment);
            return true;
        }
    }

    internal sealed class BigfileWriter : IBigfileWriter
    {
        private FileStream FileStream { get; set; }
        private byte[] ReadCache { get; set; } = new byte[BigfileConfig.ReadBufferSize];

        public bool Open(string filepath, ulong reserveSize)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);
                DirUtils.Create(bigfileInfo.DirectoryName);
                FileStream = new(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (int)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);

                // Reserve this file size on disk to speed up the writing of many files
                FileStream.SetLength((long)reserveSize);
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
        public bool Write(string filepath, out ulong offset, out ulong size)
        {
            try
            {
                FileStream inputStream = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize);
                var fileSize = (ulong)inputStream.Length;
                var fileOffset = (ulong)Write(inputStream, (long)fileSize);
                inputStream.Close();
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

}
