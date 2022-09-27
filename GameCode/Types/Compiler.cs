using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public enum EDataCompilerStatus
    {
        NONE,
        UPTODATE,
        SUCCESS,
        ERROR,
    }

    /*
    Compiler
    {
        ID
        {
            Length
            Array(Byte)
        }
    }
    */

    /// <summary>
    /// The compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        EDataCompilerStatus CompilerStatus { get; }

        void CompilerSetup();
        void CompilerWrite(IBinaryWriter stream);
        Hash160 CompilerRead(IBinaryReader stream);
    }
}
