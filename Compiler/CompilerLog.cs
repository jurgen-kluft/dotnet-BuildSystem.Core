using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
    public class GameDataCompilerLog
    {
        private BinaryFileWriter mWriter;
        private BinaryFileReader mReader;

        private Dictionary<UInt32, Type> mCompilers = new Dictionary<uint, Type>();

        public bool Open()
        {
            return false;
        }
        public bool Create()
        {
            return false;
        }

        public void RegisterCompiler(Type type)
        {
            // 32-bit hash of type.Name
        }

        public void Save(IDataCompiler dc)
        {

        }

        private IDataCompiler Load()
        {
            UInt32 compilerID = mReader.ReadUInt32();

            Type type;
            if (mCompilers.TryGetValue(compilerID, out type))
            {
                IDataCompiler compiler = Activator.CreateInstance(type) as IDataCompiler;
                return compiler;
            }
            return null;
        }

        public bool StreamIn(List<IDataCompiler> compilers, int count)
        {
            return false;
        }
    }
}