
namespace GameCore
{
    public interface IDataWriter : IBinaryStreamWriter
    {
        void NewBlock(StreamReference reference, int alignment, int size);
        void OpenBlock(StreamReference reference);
        void Write(StreamReference v);
        void Mark(StreamReference reference);
        void CloseBlock();

        void Final(IBinaryStreamWriter dataWriter);
    }
}
