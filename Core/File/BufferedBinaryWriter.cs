using System;
using System.IO;

namespace Core
{
    public class BufferedBinaryWriter : IBinaryWriter
    {
        #region Fields

        private readonly Stream mStream;
        private int mBufferIdx;
        private IAsyncResult mASyncResult;
        private readonly IBinaryWriter[] mEndianWriter;
        private readonly MemoryStream[] mMemoryStream;
        private Int64 mNumWrittenBytes = 0;
        private readonly Int64 mMaxBufferedBytes = 64 * 1024;

        #endregion
        #region Constructor

        public BufferedBinaryWriter(EEndian endian, Stream stream, int bufferSize)
        {
            mStream = stream;

            mMemoryStream = new MemoryStream[2];
            mMemoryStream[0] = new MemoryStream(bufferSize);
            mMemoryStream[1] = new MemoryStream(bufferSize);

            mEndianWriter = new IBinaryWriter[2];
            mEndianWriter[0] = EndianUtils.CreateBinaryWriter(mMemoryStream[0], endian);
            mEndianWriter[1] = EndianUtils.CreateBinaryWriter(mMemoryStream[1], endian);
           
            mMaxBufferedBytes = bufferSize;
        }

        #endregion
        #region Properties

        public Int64 Position
        {
            get
            {
                return mNumWrittenBytes;
            }
        }

        public Int64 Length
        {
            get
            {
                return mNumWrittenBytes;
            }
        }

        #endregion
        #region ASync write callback

        public void ASyncWriteCallback(IAsyncResult ar)
        {

        }

        #endregion
        #region Private Methods

        private void Flush()
        {
            if (mASyncResult != null)
            {
                mStream.EndWrite(mASyncResult);
                mASyncResult = null;
            }

            if (mMemoryStream[mBufferIdx].Length > 0)
            {
                mASyncResult = mStream.BeginWrite(mMemoryStream[mBufferIdx].GetBuffer(), 0,
                                                              (int) mMemoryStream[mBufferIdx].Length,
                                                              ASyncWriteCallback,
                                                              mBufferIdx);
                // Toggle between 0 and 1
                mBufferIdx = 1 - mBufferIdx;

                // Reset memory stream to zero length
                mMemoryStream[mBufferIdx].SetLength(0);
            }
        }

        #endregion
        #region IBinaryWriter Methods

        public Int64 Write(byte[] data)
        {
            if ((mMemoryStream[mBufferIdx].Position + data.Length) > mMaxBufferedBytes)
                Flush();

            Int64 b = mEndianWriter[mBufferIdx].Write(data, 0, data.Length);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(byte[] data, int index, int count)
        {
            if ((mMemoryStream[mBufferIdx].Position + count) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(data, index, count);
            mNumWrittenBytes += b;
            return b;
        }


        public Int64 Write(sbyte v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 1) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(byte v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 1) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(Int16 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 2) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(UInt16 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 2) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(Int32 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(UInt32 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(Int64 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(UInt64 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(float v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(double v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }

        public Int64 Write(string v)
        {
            if ((mMemoryStream[mBufferIdx].Position + v.Length + 1) > mMaxBufferedBytes)
                Flush();
            Int64 b = mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += b;
            return b;
        }


        public bool Seek(StreamOffset offset)
        {
            throw new NotSupportedException("Seeking using a buffered stream is not supported");
        }

        public void Close()
        {
            Flush();
            Flush();

            mEndianWriter[0].Close();
            mEndianWriter[1].Close();

            mMemoryStream[0].Close();
            mMemoryStream[1].Close();
        }

        #endregion
    }
}
