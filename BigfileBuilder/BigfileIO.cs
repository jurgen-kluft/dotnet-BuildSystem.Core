using System.Diagnostics;
using System.Text;

namespace BigfileBuilder
{
    interface IBinaryFileReader
    {
        long Position { get; set; }

        void Close();

        int ReadInt32();
        uint ReadUInt32();
        int Read(byte[] bytes, int index, int count);
    }

    internal class BinaryFileReader : IBinaryFileReader
    {
        private readonly BinaryReader _reader;
        private readonly byte[] _buffer = new byte[4];
        private readonly bool _isLittleEndian;

        public BinaryFileReader(FileStream stream, bool isLittleEndian)
        {
            _reader = new BinaryReader(stream);
            _isLittleEndian = isLittleEndian;
        }

        public long Position
        {
            get => _reader.BaseStream.Position;
            set => _reader.BaseStream.Position = value;
        }

        public void Close()
        {
            _reader.Close();
        }

        public int ReadInt32()
        {
            if (!_isLittleEndian)
            {
                _reader.Read(_buffer, 0, 4);
                return BitConverter.ToInt32(_buffer, 0);
            }
            return _reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            if (!_isLittleEndian)
            {
                _reader.Read(_buffer, 0, 4);
                return BitConverter.ToUInt32(_buffer, 0);
            }
            return _reader.ReadUInt32();
        }

        public int Read(byte[] bytes, int index, int count)
        {
            return _reader.Read(bytes, index, count);
        }
    }

    interface IBinaryFileWriter
    {
        long Position { get; set; }

        void Close();

        void Write(int value);
        void Write(uint value);
        void Write(byte[] bytes, int index, int count);
    }

    internal class BinaryFileWriter : IBinaryFileWriter
    {
        private readonly BinaryWriter _writer;
        private readonly byte[] _buffer = new byte[4];
        private readonly bool _isLittleEndian;

        public BinaryFileWriter(FileStream stream, bool isLittleEndian)
        {
            _writer = new BinaryWriter(stream);
            _isLittleEndian = isLittleEndian;
        }

        public long Position
        {
            get => _writer.BaseStream.Position;
            set => _writer.BaseStream.Position = value;
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Write(int value)
        {
            if (!_isLittleEndian)
            {
                BitConverter.GetBytes(value).CopyTo(_buffer, 0);
                _writer.Write(_buffer, 0, 4);
            }
            else
            {
                _writer.Write(value);
            }
        }

        public void Write(uint value)
        {
            if (!_isLittleEndian)
            {
                BitConverter.GetBytes(value).CopyTo(_buffer, 0);
                _writer.Write(_buffer, 0, 4);
            }
            else
            {
                _writer.Write(value);
            }
        }

        public void Write(byte[] bytes, int index, int count)
        {
            _writer.Write(bytes, index, count);
        }
    }
}
