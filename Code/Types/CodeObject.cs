using System;
using Core;

namespace Game.Data
{
    public class CodeObject
    {
        private IDataCompiler mCompiler = null;

        public CodeObject(string _dllfilename, string _file)
        {
            mCompiler = new CodeAsmCompiler(_dllfilename, new Filename(_file));
        }
        public CodeObject(string _dllfilename, string[] _files)
        {
            Filename[] _filenames = new Filename[_files.Length];
            for (int i = 0; i < _files.Length; ++i)
                _filenames[i] = new Filename(_files[i]);
            mCompiler = new CodeAsmCompiler(_dllfilename, _filenames);
        }
    }
}
