using System;
using System.IO;

namespace GameCore
{
    public enum EStreamAlignment : int
    {
        NONE = 0,
        ALIGN_8 = 1,
        ALIGN_16 = 2,
        ALIGN_32 = 4,
        ALIGN_64 = 8,
        ALIGN_128 = 16,
    }

    public static class Alignment
    {
        public static Int64 calculate(Int64 position, EStreamAlignment alignment)
        {
            // Example: ALIGN_32 = 4
            // Position = 0 --> p = 0
            // Position = 1 --> p = 3
            // Position = 2 --> p = 2
            // Position = 3 --> p = 1
            // Position = 4 --> p = 0

            Int64 p = ((Int64)alignment) - (position & ((Int64)alignment - 1)) & (Int64)((Int64)alignment - 1);
            return p;
        }
    }

    public class xTextStream
    {
        private Filename mFilename;

        private FileStream mFileStream;
        private StreamWriter mWriter;
        private StreamReader mReader;

        public xTextStream(Filename filename)
        {
            mFilename = filename;
        }

        public enum EMode
        {
            READ,
            WRITE,
        }

        public Filename filename { get { return mFilename;  } }
        public StreamWriter write { get { return mWriter; } }
        public StreamReader read { get { return mReader; } }

        public bool Exists()
        {
            return File.Exists(mFilename);
        }

        public bool Open(EMode mode)
        {
            bool success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(mFilename, (mode == EMode.READ) ? FileMode.Open : FileMode.Create, (mode == EMode.READ) ? FileAccess.Read : FileAccess.Write);
                if (mode == EMode.WRITE) mWriter = new StreamWriter(stream);
                else if (mode == EMode.READ) mReader = new StreamReader(stream);
            }
            finally
            {
                mFileStream = stream;
                success = true;
            }
            return success;
        }

        public void Close()
        {
            if (mWriter != null)
            {
                mWriter.Flush();
                mWriter.Close();
                mFileStream.Close();
            }
            else if (mReader != null)
            {
                mReader.Close();
                mFileStream.Close();
            }
            mWriter = null;
            mReader = null;
            mFileStream = null;
        }
    }

    public class xBinaryStream
    {
        private Filename mFilename;
        private EEndian mEndian;

        private FileStream mFileStream;
        private IBinaryWriter mWriter;
        private IBinaryReader mReader;

        public xBinaryStream(Filename filename, EEndian endian)
        {
            mFilename = filename;
            mEndian = endian;
        }

        public enum EMode
        {
            READ,
            WRITE,
        }

        public Filename filename { get { return mFilename; } }
        public EEndian endian { get { return mEndian; } }
        public IBinaryWriter write { get { return mWriter; } }
        public IBinaryReader read { get { return mReader; } }

        public bool Open(EMode mode)
        {
            bool success = false;
            FileStream stream = null;
            try
            {
                stream = new FileStream(mFilename, (mode == EMode.READ) ? FileMode.Open : FileMode.Create, (mode == EMode.READ) ? FileAccess.Read : FileAccess.Write);
                if (mode == EMode.WRITE) mWriter = EndianUtils.CreateBinaryWriter(stream, mEndian);
                else if (mode == EMode.READ) mReader = EndianUtils.CreateBinaryReader(stream, mEndian);
            }
            catch(Exception)
            {
                stream = null;
            }
            finally
            {
                mFileStream = stream;
                success = true;
            }
            return success;
        }

        public void Close()
        {
            if (mWriter != null)
            {
                mWriter.Close();
                mFileStream.Close();
            }
            else if (mReader != null)
            {
                mReader.Close();
                mFileStream.Close();
            }
            mWriter = null;
            mReader = null;
            mFileStream = null;
        }
    }

    public static class StreamUtils
    {
        private static Int64 position(IBinaryWriter writer)
        {
            Int64 p = writer.Position;
            return p;
        }

        public static bool aligned(IBinaryWriter writer, EStreamAlignment alignment)
        {
            Int64 p = Alignment.calculate(writer.Position, alignment);
            return (p == 0);
        }

        public static void align(IBinaryWriter writer, EStreamAlignment alignment)
        {
            Int64 p = Alignment.calculate(writer.Position, alignment);
            while (--p >= 0)
                writer.Write((byte)0xCD);
        }
    }
}
