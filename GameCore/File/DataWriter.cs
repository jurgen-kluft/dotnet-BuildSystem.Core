
namespace GameCore
{
    public interface IDataWriter : IStreamWriter
    {
        void OpenDataUnit(int dataUnitReference);
        void NewBlock(StreamReference reference, int alignment);
        void OpenBlock(StreamReference reference);
        void WriteDataBlockReference(StreamReference v);
        void Mark(StreamReference reference);
        void CloseBlock();
        void CloseDataUnit();

        void Final(IStreamWriter dataWriter);
    }
}
