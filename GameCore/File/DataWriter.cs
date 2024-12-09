
namespace GameCore
{
    public interface IDataWriter : IBinaryStreamWriter
    {
        void OpenDataUnit(StreamReference reference);
        StreamReference NewBlock(StreamReference reference, int alignment, int size);
        void OpenBlock(StreamReference reference);
        void WriteBlockReference(StreamReference v);
        void Mark(StreamReference reference);
        void CloseBlock();
        void CloseDataUnit();

        void Final(IBinaryStreamWriter dataWriter);
    }
}
