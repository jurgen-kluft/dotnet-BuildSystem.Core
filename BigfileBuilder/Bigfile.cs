using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using GameCore;

namespace DataBuildSystem
{
    public sealed class Bigfile
    {
        #region Fields

        private byte[] mReadCache;
        private FileStream mFileStream;
        private BinaryReader mBinaryReader;

        public enum EMode
        {
            Read,
            Write
        }

        #endregion

        #region Methods

        public bool Open(string filepath, EMode mode)
        {
            try
            {
                Close();

                string bigfileFilepath = Path.ChangeExtension(filepath, BigfileConfig.BigFileExtension);
                FileInfo bigfileInfo = new(bigfileFilepath);

                mReadCache = new byte[BigfileConfig.ReadBufferSize];

                if (mode == EMode.Write)
                {
                    DirUtils.Create(bigfileInfo.DirectoryName);
                    mFileStream = new(bigfileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, (Int32)BigfileConfig.WriteBufferSize, FileOptions.Asynchronous);
                }
                else
                {
                    mFileStream = new(bigfileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, (Int32)BigfileConfig.ReadBufferSize, FileOptions.Asynchronous);
                    mBinaryReader = new(mFileStream);
                }
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
        /// Save all BigfileFiles into the Bigfile, this uses a different approach. It allocates
        /// the full size of the Bigfile first and uses seek to write all the BigfileFiles.
        /// </summary>
        /// <param name="path">The absolute path of where 'files' can be found</param>
        /// <param name="files">All the files to include in the Bigfile</param>
        /// <returns>True if successful</returns>
        public bool Save(string path, BigfileFile[] files)
        {
            try
            {
                // Compute the full size of the Bigfile by tracking the largest file offset + file size
                Int64 bigfileFilesize = 0;
                for (int i = 0; i < files.Length; ++i)
                {
                    BigfileFile e = files[i];

                    if (bigfileFilesize < e.FileOffset.value + e.FileSize)
                        bigfileFilesize = e.FileOffset.value + e.FileSize;
                }

                mFileStream.SetLength((Int64)bigfileFilesize);

                foreach (BigfileFile e in files)
                {
                    FileStream inputStream = new(Path.Join(path, e.Filename), FileMode.Open, FileAccess.Read, FileShare.Read);
                    Write(inputStream, e.FileOffset, e.FileSize);

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

        private void Write(Stream readStream, StreamOffset fileOffset, Int64 fileSize)
        {
            // Align the file on the calculated offset
            mFileStream.Seek(fileOffset.value, SeekOrigin.Begin);

            Debug.Assert(fileSize < Int32.MaxValue);

            if (fileSize <= BigfileConfig.ReadBufferSize)
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

        #endregion
    }
}
