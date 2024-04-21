using System.Diagnostics;
using GameCore;

namespace DataBuildSystem
{
    public sealed class BigfileWriter
    {
        private FileStream FileStream { get; set; }
        private byte[] ReadCache { get; set; }

        public Int64 Position { get; private set; }

        public BigfileWriter()
        {
            ReadCache = new byte[BigfileConfig.ReadBufferSize];
        }

        public void SetLength(Int64 length)
        {
            FileStream.SetLength(length);
        }

        public bool Open(string filepath)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                var bigfileInfo = new FileInfo(bigfileFilepath);
                DirUtils.Create(bigfileInfo.DirectoryName);
                FileStream = new(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (Int32)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save all BigfileFiles into the Bigfile, this uses a different approach. It allocates
        /// the full size of the Bigfile first and uses seek to write all the BigfileFiles.
        /// </summary>
        /// <param name="filepath">The name of the bigfile</param>
        /// <returns>True if successful</returns>
        public Int64 Save(string filepath)
        {
            try
            {
                FileStream inputStream = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Int64 fileOffset = Write(inputStream, inputStream.Length);
                inputStream.Close();
                return fileOffset;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private Int64 Write(Stream readStream, Int64 fileSize)
        {
            // Align the file on the calculated additionalLength
            FileStream.Position = CMath.Align(FileStream.Position, BigfileConfig.FileAlignment);
            var position = FileStream.Position;

            Debug.Assert(fileSize < Int32.MaxValue);

            if (fileSize <= BigfileConfig.ReadBufferSize)
            {
                readStream.Read(ReadCache, 0, (Int32)fileSize);
                FileStream.Write(ReadCache, 0, (Int32)fileSize);
            }
            else
            {
                int br;
                while ((br = readStream.Read(ReadCache, 0, ReadCache.Length)) > 0)
                    FileStream.Write(ReadCache, 0, br);
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
        private FileStream _fileStream;
        private BinaryReader _binaryReader;

        public bool Open(string filepath)
        {
            try
            {
                Close();

                var bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                FileInfo bigfileInfo = new(bigfileFilepath);

                _fileStream = new(bigfileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, (Int32)BigfileConfig.ReadBufferSize, FileOptions.Asynchronous);
                _binaryReader = new(_fileStream);
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

    public sealed class Bigfile
    {
        #region Fields

        public List<BigfileFile> Files {get;set;} = new();
        public int Index { get; set; }

        #endregion

        #region Methods

        public void WriteTo(BigfileWriter writer)
        {
            Int64 additionalLength = 0;
            foreach(var bff in Files)
            {
                additionalLength += bff.FileSize;
                additionalLength = CMath.Align(additionalLength, BigfileConfig.FileAlignment);
            }
            writer.SetLength(writer.Position + additionalLength);

            foreach(var bff in Files)
            {
                Int64 fileOffset = writer.Save(bff.Filename);
                bff.FileOffset = new StreamOffset(fileOffset);
            }
        }

        #endregion
    }
}
