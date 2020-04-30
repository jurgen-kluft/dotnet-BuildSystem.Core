using System.Collections.Generic;

namespace Game.Data
{
    using DataBuildSystem;
    using Core;

    public enum EDataCompilerStatus
    {
        NONE,
        UPTODATE,
        SUCCESS,
        ERROR,
    }

    /// <summary>
    /// The compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        EDataCompilerStatus status { get; }

        void csetup(DependencySystem dependencySystem);
        void ccompile(Game.Data.IDataCompilationServer dataCompilerServer);
        void cteardown();
    }
}
