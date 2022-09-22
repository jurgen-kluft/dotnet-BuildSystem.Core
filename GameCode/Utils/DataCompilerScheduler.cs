using System;

namespace GameData
{

    public interface IDataCompilerClient
    {
        void onExecute();
        void onFinished();
    }

}
