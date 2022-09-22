using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public class StreamFile
    {
        #region Fields

        public readonly static StreamFile Empty = new StreamFile(Filename.Empty, 0, StreamOffset.Empty);

        private Filename mFilename;
        private readonly Int64 mFileSize;
        private StreamOffset[] mFileOffsets;

        #endregion
        #region Constructor

        public StreamFile(StreamFile file)
        {
            mFilename = file.filename;
            mFileSize = file.size;
            mFileOffsets = file.offsets;
        }

        public StreamFile(Filename filename, Int64 size, StreamOffset offset)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[] { offset };
        }

        public StreamFile(Filename filename, Int64 size, StreamOffset[] offsets)
        {
            mFilename = filename;
            mFileSize = size;
            mFileOffsets = new StreamOffset[offsets.Length];
            offsets.CopyTo(mFileOffsets, 0);
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return mFileSize == 0 && mFileOffsets[0].isEmpty && mFilename.IsEmpty;
            }
        }

        public Filename filename
        {
            set
            {
                mFilename = value;
            }
            get
            {
                return mFilename;
            }
        }

        public Int32 size32
        {
            get
            {
                return (Int32)mFileSize;
            }
        }

        public Int64 size
        {
            get
            {
                return mFileSize;
            }
        }

        public StreamOffset offset
        {
            set
            {
                mFileOffsets = new StreamOffset[1];
                mFileOffsets[0] = value;
            }
            get
            {
                return mFileOffsets[0];
            }
        }

        public StreamOffset[] offsets
        {
            set
            {
                mFileOffsets = new StreamOffset[value.Length];
                value.CopyTo(mFileOffsets, 0);
            }
            get
            {
                return mFileOffsets;
            }
        }

        #endregion
        #region Operators

        public static bool operator ==(StreamFile a, StreamFile b)
        {
            if (a.mFileSize == b.mFileSize && a.mFilename == b.mFilename && a.offsets.Length == b.offsets.Length)
            {
                for (int i = 0; i < a.offsets.Length; ++i)
                    if (a.offsets[i] != b.offsets[i])
                        return false;

                return true;
            }
            return false;
        }
        public static bool operator !=(StreamFile a, StreamFile b)
        {
            if (a.mFileSize != b.mFileSize || a.mFilename != b.mFilename || a.offsets.Length != b.offsets.Length)
                return false;

            for (int i = 0; i < a.offsets.Length; ++i)
                if (a.offsets[i] != b.offsets[i])
                    return true;

            return false;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public override bool Equals(object o)
        {
            if (o is StreamFile)
            {
                StreamFile other = (StreamFile)o;
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return mFileOffsets[0].value32;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class StreamBuilder
    {
        #region StreamToCache

        public class StreamToCache
        {
            public static readonly StreamToCache Null = new StreamToCache();

            private byte[] mCache;
            private int mCachePos;

            private FileStream mStream;
            private IAsyncResult mAsyncResult;

            internal StreamToCache()
            {
            }

            public void Init(byte[] buffer, int cachePos)
            {
                mCache = buffer;
                mCachePos = cachePos;
            }

            public void WriteToCacheReadFromStream(StreamFile file)
            {
                // Read from Stream = Write to Cache
                mStream = new FileStream(file.filename, FileMode.Open, FileAccess.Read, FileShare.Read, 8, true);
                mAsyncResult = mStream.BeginRead(mCache, mCachePos, file.size32, null, null);
            }

            internal bool IsBusy
            {
                get
                {
                    if (mAsyncResult != null)
                    {
                        if (mAsyncResult.IsCompleted)
                        {
                            mStream.EndRead(mAsyncResult);
                            mStream.Dispose();
                            mStream.Close();
                            mAsyncResult = null;
                            mStream = null;
                            return false;
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        #endregion
        #region CacheToStream

        public class CacheToStream
        {
            public static readonly CacheToStream Null = new CacheToStream(null);

            private byte[] mCache;
            private int mSize;

            private readonly FileStream mStream;
            private IAsyncResult mAsyncResult;

            public CacheToStream(FileStream stream)
            {
                mCache = null;
                mSize = 0;
                mStream = stream;
                mAsyncResult = null;
            }

            public byte[] cache
            {
                get
                {
                    return mCache;
                }
                set
                {
                    mCache = value;
                }
            }

            public Int32 size
            {
                get
                {
                    return mSize;
                }
                set
                {
                    mSize = value;
                }
            }

            public Int32 offset
            {
                get
                {
                    return (Int32)mStream.Position;
                }
            }

            public void ReadFromCacheWriteToStream()
            {
                Debug.Assert(mSize > 0);
                mAsyncResult = mStream.BeginWrite(mCache, 0, mSize, null, null);
            }

            internal bool IsBusy
            {
                get
                {
                    if (mAsyncResult != null)
                    {
                        if (mAsyncResult.IsCompleted)
                        {
                            mStream.EndWrite(mAsyncResult);
                            mAsyncResult = null;
                            return false;
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        #endregion
        #region StreamCache

        public enum ECacheState
        {
            CACHE_IS_EMPTY,
            WRITING_TO_CACHE,
            CACHE_IS_FULL,
            READING_FROM_CACHE,
        }

        public class StreamCache
        {
            private readonly Int32 mSize;

            private Int32 mCacheOffset;
            private Int32 mNumBytesInCache;
            private ECacheState mState;

            private const int mMaxStreamToCache = 8;
            private readonly StreamToCache[] mStreamToCache;
            private readonly StreamToCache[] mStreamToCacheLocked;

            private CacheToStream mCacheToStream;
            private CacheToStream mCacheToStreamLocked;

            private readonly byte[] mCache;

            public StreamCache(Int32 inCacheSize, FileStream dstStream)
            {
                mState = ECacheState.CACHE_IS_EMPTY;

                mSize = inCacheSize;
                mCache = new byte[inCacheSize];

                mStreamToCache = new StreamToCache[mMaxStreamToCache];
                for (int i = 0; i < mMaxStreamToCache; ++i)
                    mStreamToCache[i] = new StreamToCache();
                mStreamToCacheLocked = new StreamToCache[mMaxStreamToCache];

                // Write to Stream = Read from Cache
                mCacheToStream = new CacheToStream(dstStream);
            }

            public ECacheState State
            {
                get
                {
                    return mState;
                }
                set
                {
                    mState = value;
                }
            }

            /// <summary>
            /// Can the Cache do a write lock, is there enough space?
            /// </summary>
            public bool CanCacheWriteLock(StreamFile file, Int32 alignment)
            {
                if (mNumBytesInCache == 0)
                {
                    mCacheOffset = file.offset.value32;
                }

                if ((CacheAlign(file.offset.value32 - mCacheOffset, alignment) + (Int32)file.size) <= mSize)
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// CacheWriteLock, can we lock the cache for this file to read and writing to the cache?
            /// </summary>
            public bool CacheWriteLock(out StreamToCache buffer, StreamFile file, Int32 alignment)
            {
                for (int i = 0; i < mMaxStreamToCache; ++i)
                {
                    if (mStreamToCache[i]!=null)
                    {
                        // The cache is now locked, move it to another array
                        mStreamToCacheLocked[i] = mStreamToCache[i];
                        buffer = mStreamToCache[i];
                        mStreamToCache[i] = null;
                        mState = ECacheState.WRITING_TO_CACHE;

                        // The offset of where the file is written in the output stream should match the precalculated one
                        Debug.Assert(file.offset.value32 == (mCacheOffset + mNumBytesInCache));
                        buffer.Init(mCache, (Int32)(file.offset.value - mCacheOffset));

                        mNumBytesInCache = CacheAlign(mNumBytesInCache + (Int32)file.size, alignment);

                        return true;
                    }
                }

                buffer = StreamToCache.Null;
                return false;
            }

            /// <summary>
            /// CacheReadLock, can we lock the cache to read it and write to a stream?
            /// </summary>
            public bool CacheReadLock(out CacheToStream buffer)
            {
                if (mCacheToStream != null)
                {
                    buffer = mCacheToStream;
                    buffer.cache = mCache;
                    buffer.size = mNumBytesInCache;
                    mCacheToStreamLocked = mCacheToStream;
                    mCacheToStream = null;

                    mState = ECacheState.READING_FROM_CACHE;

                    return true;
                }

                buffer = CacheToStream.Null;
                return false;
            }

            /// <summary>
            /// Are there stream writing to the cache?
            /// </summary>
            /// <returns>True if no one is writing to the cache</returns>
            public bool CacheWriteUnlock()
            {
                int numLocked = 0;
                for (int i = 0; i < mMaxStreamToCache; ++i)
                {
                    if (mStreamToCacheLocked[i] != null)
                    {
                        ++numLocked;
                        if (!mStreamToCacheLocked[i].IsBusy)
                        {
                            Console.CursorLeft = 0;
                            Console.Write("{0}                ", mNumBytesInCache);

                            --numLocked;
                            mStreamToCache[i] = mStreamToCacheLocked[i];
                            mStreamToCacheLocked[i] = null;
                            mStreamToCache[i].Init(null, 0);
                        }
                    }
                }
                return numLocked == 0;
            }

            /// <summary>
            /// Are there any streams reading from the cache?
            /// </summary>
            /// <returns>True if there no one is reading from the cache</returns>
            public bool CacheReadUnlock()
            {
                if (mCacheToStreamLocked != null)
                {
                    if (!mCacheToStreamLocked.IsBusy)
                    {
                        mNumBytesInCache = 0;
                        mState = ECacheState.CACHE_IS_EMPTY;
                        mCacheToStream = mCacheToStreamLocked;
                        mCacheToStreamLocked = null;

                        mCacheToStream.cache = null;
                        mCacheToStream.size = 0;
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        #endregion
        #region Fields

        private Int32 mAlignment = 32;
        private FileStream mDstFilestream;
        private StreamCache[] mCache;

        #endregion
        #region Properties

        public Int32 Alignment
        {
            get
            {
                return mAlignment;
            }
            set
            {
                mAlignment = value;
            }
        }

        #endregion
        #region Read/Write Process

        internal static Int32 CacheAlign(Int32 value, Int32 alignment)
        {
            return (value + (alignment - 1)) & (~(alignment - 1));
        }

        internal static Int64 CacheAlign(Int64 value, Int64 alignment)
        {
            return (value + (alignment - 1)) & (~(alignment - 1));
        }

        public Int64 ReadWriteProcess(List<StreamFile> files)
        {
            int fileIndex = 0;
            Int32 writeToCacheIndex = 0;
            Queue<Int32> readFromCacheQueue = new Queue<Int32>();

            Int64 resultingSize = 0;
            while (true)
            {
                try
                {
                    for (int i = 0; i < 2; i++)
                    {
                        mCache[i].CacheWriteUnlock();
                        mCache[i].CacheReadUnlock();
                    }

                    if (fileIndex < files.Count)
                    {
                        if (mCache[writeToCacheIndex].State == ECacheState.WRITING_TO_CACHE || mCache[writeToCacheIndex].State == ECacheState.CACHE_IS_EMPTY)
                        {
                            if (mCache[writeToCacheIndex].State == ECacheState.CACHE_IS_EMPTY)
                                Console.WriteLine("Reading to cache {0}", writeToCacheIndex);

                            // See if we can read a file
                            StreamFile file = files[fileIndex];
                            if (mCache[writeToCacheIndex].CanCacheWriteLock(file, mAlignment))
                            {
                                StreamToCache buffer;
                                if (mCache[writeToCacheIndex].CacheWriteLock(out buffer, file, mAlignment))
                                {
                                    buffer.WriteToCacheReadFromStream(file);
                                    ++fileIndex;

                                    // Last file?
                                    if (fileIndex == files.Count)
                                    {
                                        // Mark cache as full since we have no more files to process
                                        mCache[writeToCacheIndex].State = ECacheState.CACHE_IS_FULL;
                                    }
                                }
                            }
                            else
                            {
                                mCache[writeToCacheIndex].State = ECacheState.CACHE_IS_FULL;
                            }
                        }
                    }
                    if (mCache[writeToCacheIndex].State == ECacheState.CACHE_IS_FULL)
                    {
                        // Can i switch to the other cache? (e.g: is it done writing the cache to the stream?)
                        if (mCache[1 - writeToCacheIndex].CacheReadUnlock())
                        {
                            readFromCacheQueue.Enqueue(writeToCacheIndex);
                            writeToCacheIndex = 1 - writeToCacheIndex;
                        }
                    }

                    if (readFromCacheQueue.Count > 0)
                    {
                        Int32 readFromCacheIndex = readFromCacheQueue.Peek();
                        if (mCache[readFromCacheIndex].State == ECacheState.CACHE_IS_FULL)
                        {
                            // Does the current cache still have some streams writing to it?
                            if (mCache[readFromCacheIndex].CacheWriteUnlock())
                            {
                                // Dequeue it since we are now kicking it
                                readFromCacheIndex = readFromCacheQueue.Dequeue();

                                // Kick the current cache to write to the destination stream
                                CacheToStream buffer;
                                bool readlock = mCache[readFromCacheIndex].CacheReadLock(out buffer);
                                Debug.Assert(readlock);

                                Console.WriteLine();
                                Console.WriteLine("Writing cache {0} with size {1} to stream at pos {2}", readFromCacheIndex, buffer.size, buffer.offset);
                                resultingSize += buffer.size;

                                buffer.ReadFromCacheWriteToStream();
                            }
                        }
                    }

                    if (fileIndex == files.Count)
                    {
                        if (mCache[0].State == ECacheState.CACHE_IS_EMPTY && mCache[1].State == ECacheState.CACHE_IS_EMPTY)
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[StreamBuffer:EXCEPTION] ReadWriteProcess, {0}", e);
                }
            }

            Console.WriteLine("Writing {0} bytes to stream was successful ({1}Mb).", resultingSize, ((resultingSize + (1024*1024 - 1))/(1024*1024)));

            return resultingSize;
        }

        #endregion
        #region Build

        public void Build(List<StreamFile> srcFiles, Filename dstFile)
        {
            mCache = new StreamCache[2];

            Int64 offset = 0;
            foreach (StreamFile s in srcFiles)
            {
                s.offset = new StreamOffset(offset);
                offset = CacheAlign(offset + s.size, mAlignment);
            }
            Int64 fileSize = offset;

            mDstFilestream = new FileStream(dstFile.ToString(), FileMode.Create, FileAccess.Write, FileShare.Write, 8, FileOptions.WriteThrough | FileOptions.Asynchronous);
            mDstFilestream.SetLength(fileSize);
            mDstFilestream.Position = 0;

            mCache[0] = new StreamCache(32 * 1024 * 1024, mDstFilestream);
            mCache[1] = new StreamCache(32 * 1024 * 1024, mDstFilestream);

            ReadWriteProcess(srcFiles);

            mDstFilestream.Dispose();
            mDstFilestream.Close();

            mCache[0] = null;
            mCache[1] = null;
            mCache = null;
        }

        #endregion
    }
}