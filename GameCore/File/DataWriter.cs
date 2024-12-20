
namespace GameCore
{
    public interface IDataBlockWriter : IStreamWriter
    {
        void NewBlock(StreamReference reference, int alignment);
        void OpenBlock(StreamReference reference);
        void WriteDataBlockReference(StreamReference reference);
        void CloseBlock();
    }
}
