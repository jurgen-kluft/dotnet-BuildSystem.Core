using System;

namespace GameData
{

    public interface IDataCompilerClient
    {
        void CompilerExecute();
        void CompilerFinished();
    }

}
