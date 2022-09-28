using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using GameCore;

namespace DataBuildSystem
{
    public sealed class Bigfile : IDisposable
    {
        #region Fields

        private byte[] mReadCache;
        private FileStream mFileStream;
        private BinaryReader mBinaryReader;

        public enum EMode
        {
            READ,
            WRITE
        }

        #endregion
        #region Methods

		public static bool exists(string filename)
		{
			string bigFileFilename = Path.ChangeExtension(filename, BigfileConfig.BigFileExtension);
			FileInfo fileInfo = new FileInfo(bigFileFilename);
			return fileInfo.Exists;
		}

        public bool open(string filename, EMode mode)
        {
			try
			{
				close();

                string bigFilename = filename;
                bigFilename = Path.ChangeExtension(bigFilename, BigfileConfig.BigFileExtension);
				FileInfo bigfileInfo = new FileInfo(bigFilename);

                mReadCache = new byte[BigfileConfig.ReadBufferSize];

                if (mode == EMode.WRITE)
                {
                    DirUtils.Create(bigFilename);

                    if (!bigfileInfo.Exists)
                    {
                        FileStream bigFileTempStream = bigfileInfo.Create();
                        bigFileTempStream.Close();
                    }

                    mFileStream = new FileStream(bigfileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.None, (Int32)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);
                }
                else
                {
                    mFileStream = new FileStream(bigfileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, (Int32)BigfileConfig.ReadBufferSize, FileOptions.Asynchronous);
                    mBinaryReader = new BinaryReader(mFileStream);
                }
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

            return true;
        }

        public void close()
        {
            if (mFileStream != null)
            {
                if (mBinaryReader != null)
                {
                    mBinaryReader.Close();
                    mBinaryReader = null;
                }
                mFileStream.Close();
                mFileStream = null;
            }
        }

        /// <summary>
        /// Set the size of the Bigfile that we are writing to
        /// </summary>
        /// <param name="length"></param>
        public void setLength(Int64 length)
        {
            if (mFileStream.CanWrite)
            {
                mFileStream.SetLength(length);
            }
        }

        public void align(EStreamAlignment alignment)
        {
            byte[] data = { 0xCD };
            Int64 p = Alignment.calculate(mFileStream.Position, alignment);
            while (--p >= 0)
                mFileStream.Write(data, 0, 1);
        }

        /// <summary>
        /// Save the data into the Bigfile
        /// </summary>
        /// <param name="offset">Offset into the Bigfile</param>
        /// <param name="data">Data to write into the Bigfile</param>
        /// <returns></returns>
        public void write(StreamOffset offset, byte[] data)
        {
            if (data == null)
                return;

            // Expand stream length if necessary
            Int64 lengthNeeded = (offset.value + data.Length);
            if (mFileStream.Length < lengthNeeded)
                mFileStream.SetLength(lengthNeeded);

            mFileStream.Seek(offset.value, SeekOrigin.Begin);
            mFileStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Save the data into the Bigfile
        /// </summary>
        /// <param name="offset">Offset into the Bigfile</param>
        /// <param name="data">Data to write into the Bigfile</param>
        /// <param name="count">Number of bytes to write into the Bigfile</param>
        public void write(StreamOffset offset, byte[] data, int count)
        {
            if (data == null)
                return;

            // Expand stream length if necessary
            Int64 lengthNeeded = (offset.value + count);
            if (mFileStream.Length < lengthNeeded)
                mFileStream.SetLength(lengthNeeded);

            mFileStream.Seek(offset.value, SeekOrigin.Begin);
            mFileStream.Write(data, 0, count);
        }
        
        /// <summary>
        /// Save all BigfileFiles into the Bigfile, this uses a different approach. It allocates 
        /// the full size of the Bigfile first and uses seek to write all the BigfileFiles.
        /// </summary>
        /// <param name="path">The absolute path of where 'files' can be found</param>
        /// <param name="files">All the files to include in the Bigfile</param>
        /// <returns>True if successful</returns>
        public bool save(string path, List<BigfileFile> files)
        {
            try
            {
                // Compute the full size of the Bigfile by tracking the largest file offset + file size
                Int64 bigfileFilesize = 0;

                // Simulation of writing
                StreamOffset currentOffset = new StreamOffset(0);
                for (int i = 0; i < files.Count; ++i)
                {
                    BigfileFile e = files[i];

                    FileInfo fileInfo = new FileInfo(Path.Join(path, e.filename));
                    if (!fileInfo.Exists)
                    {
                        e.offsets = new StreamOffset[] { StreamOffset.Empty };
                        e.size = 0;
                    }

                    foreach (StreamOffset o in e.offsets)
                    {
                        if (bigfileFilesize < o.value + e.size)
                            bigfileFilesize = o.value + e.size;
                    }

                    currentOffset += e.size32;
                    currentOffset.align(BigfileConfig.FileAlignment);

                    // Write back the updated BigfileFile into the array
                    files[i] = e;
                }

                mFileStream.SetLength((Int64)bigfileFilesize);

                foreach (BigfileFile e in files)
                {
                    if (e.offsets.Length == 0)
                        continue;

                    FileStream inputStream = new FileStream(path + e.filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                    foreach (StreamOffset o in e.offsets)
                        write(inputStream, o, e.size);

                    inputStream.Dispose();
                    inputStream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private void write(Stream readStream, StreamOffset fileOffset, Int64 fileSize)
        {
            // Align the file on the calculated offset
            mFileStream.Seek(fileOffset.value, SeekOrigin.Begin);

            if (fileSize < BigfileConfig.ReadBufferSize)
            {
                readStream.Read(mReadCache, 0, (Int32)fileSize);
                mFileStream.Write(mReadCache, 0, (Int32)fileSize);
            }
            else
            {
                int br;
                while ((br = readStream.Read(mReadCache, 0, mReadCache.Length)) != 0)
                    mFileStream.Write(mReadCache, 0, br);
            }
        }

        public void copy(StreamOffset offset, Int64 size, Bigfile dstBigfile, StreamOffset dstOffset)
        {
            // Align the file on the calculated offset
            mFileStream.Seek(offset.value, SeekOrigin.Begin);
            dstBigfile.mFileStream.Seek(dstOffset.value, SeekOrigin.Begin);

            if (size < mReadCache.Length)
            {
                mFileStream.Read(mReadCache, 0, (Int32)size);
                dstBigfile.mFileStream.Write(mReadCache, 0, (Int32)size);
            }
            else
            {
                int cacheReadSize = mReadCache.Length;
                while (size > 0)
                {
                    int numBytesRead = mFileStream.Read(mReadCache, 0, cacheReadSize);
					if (size > 0 && 0 == numBytesRead)
					{
						throw new Exception("Invalid stream offset");
					}
                    size -= numBytesRead;
                    if (size > 0)
                    {
                        if (cacheReadSize > size)
                            cacheReadSize = (Int32)size;
                    }
                    dstBigfile.mFileStream.Write(mReadCache, 0, numBytesRead);
                }
            }
        }

        public byte[] read(StreamOffset fileOffset, Int32 fileSize)
        {
            byte[] data;
            if (mBinaryReader != null)
            {
                data = new byte[fileSize];
                mFileStream.Seek(fileOffset.value, SeekOrigin.Begin);
                if (mBinaryReader.Read(data, 0, fileSize) != fileSize)
                    data = new byte[0];
            }
            else
            {
                data = new byte[0];
            }
            return data;
        }

        #endregion
        #region IDisposable Members

        public void Dispose()
        {
            close();
        }

        #endregion
    }
}
