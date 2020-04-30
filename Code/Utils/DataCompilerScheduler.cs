using System;

namespace Game.Data
{
    public enum EDataCompilerPriority
    {
        CODE_ASSEMBLY = 0,
        COMPILED_ASSEMBLY,
        EXTERNAL_ASSEMBLY,
        ATOMIC_ASSET,
        COMPOUND_ASSET,
        PACKAGING_ASSET,
        EXTERNAL_COMPILER,
    }

    public interface IDataCompilerClient
    {
        string group { get; }
        EDataCompilerPriority priority { get; }

        void onExecute();
        void onFinished();
    }

    public interface IDataCompilationServer
    {
        void schedule(IDataCompilerClient client);
        void execute();
    }
}
