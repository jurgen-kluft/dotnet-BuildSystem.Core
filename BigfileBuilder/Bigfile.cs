using System.Diagnostics;
using GameCore;

namespace BigfileBuilder
{
    public sealed class BigfileWriter
    {
        private FileStream FileStream { get; set; }
        private byte[] ReadCache { get; set; } = new byte[BigfileConfig.ReadBufferSize];

        public void AddSize(long length)
        {
            FileStream.SetLength(FileStream.Position + length);
        }

        public bool Open(string filepath)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);
                DirUtils.Create(bigfileInfo.DirectoryName);
                FileStream = new(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (int)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);
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
        /// <param name="filepath">The name of the bigfile</param>
        /// <returns>True if successful</returns>
        public long Save(string filepath)
        {
            try
            {
                FileStream inputStream = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, (int)BigfileConfig.ReadBufferSize);
                var fileOffset = Write(inputStream, inputStream.Length);
                inputStream.Close();
                return fileOffset;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private long Write(Stream readStream, long fileSize)
        {
            // Align the file on the calculated additionalLength
            FileStream.Position = CMath.Align(FileStream.Position, BigfileConfig.FileAlignment);
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

        public void Write(BigfileWriter writer)
        {
            var additionalSize = (long)0;
            foreach (var bff in Files)
            {
                additionalSize += bff.FileSize;
                additionalSize = CMath.Align(additionalSize, BigfileConfig.FileAlignment);
            }

            writer.AddSize(additionalSize);

            foreach (var bff in Files)
            {
                var fileOffset = writer.Save(bff.Filename);
                bff.FileOffset = new StreamOffset(fileOffset);
            }
        }
    }
}
