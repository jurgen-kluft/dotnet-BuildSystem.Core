
namespace GameCore
{
    public interface IDataWriter : IBinaryStreamWriter
    {
        void OpenDataUnit(int dataUnitReference);
        int NewBlock(StreamReference reference, int alignment, int size);
        void OpenBlock(StreamReference reference);
        void WriteDataBlockReference(StreamReference v);
        void Mark(StreamReference reference);
        void CloseBlock();
        void CloseDataUnit();

        void Final(IBinaryStreamWriter dataWriter);
    }
}
