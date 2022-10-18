using System;
using System.IO;

namespace GameCore
{
    public class BufferedBinaryWriter : IBinaryStream
    {
        #region Fields

        private readonly Stream mStream;
        private int mBufferIdx;
        private IAsyncResult mASyncResult;
        private readonly IBinaryStream[] mEndianWriter;
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

            mEndianWriter = new IBinaryStream[2];
            mEndianWriter[0] = EndianUtils.CreateBinaryStream(mMemoryStream[0], endian);
            mEndianWriter[1] = EndianUtils.CreateBinaryStream(mMemoryStream[1], endian);

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
            set
            {
                mNumWrittenBytes = value;
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

        public void Write(byte[] data)
        {
            if ((mMemoryStream[mBufferIdx].Position + data.Length) > mMaxBufferedBytes)
                Flush();

            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(data, 0, data.Length);
            mNumWrittenBytes += (Position - pos);
        }

        public void Write(byte[] data, int index, int count)
        {
            if ((mMemoryStream[mBufferIdx].Position + count) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(data, index, count);
            mNumWrittenBytes += (Position-pos);
        }


        public void Write(sbyte v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 1) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(byte v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 1) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(Int16 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 2) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(UInt16 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 2) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(Int32 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(UInt32 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(Int64 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(UInt64 v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(float v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 4) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(double v)
        {
            if ((mMemoryStream[mBufferIdx].Position + 8) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
        }

        public void Write(string v)
        {
            if ((mMemoryStream[mBufferIdx].Position + v.Length + 1) > mMaxBufferedBytes)
                Flush();
            Int64 pos = Position;
            mEndianWriter[mBufferIdx].Write(v);
            mNumWrittenBytes += (Position-pos);
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
