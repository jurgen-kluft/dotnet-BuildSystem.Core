using System.Collections.Generic;
using System.Buffers;

namespace GameData
{
    using DataBuildSystem;
    using GameCore;

    public enum EDataCompilerStatus
    {
        NONE,
        UPTODATE,
        SUCCESS,
        ERROR,
    }

    public struct ByteSpan
	{
        public byte[] Data;
        public int Length;
        public int Offset;
	}

    public interface IDataCompilerLog
	{
        void CompilerSave(ByteSpan s);
        void CompilerLoad(ByteSpan s);
    }

    /// <summary>
    /// The compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        EDataCompilerStatus CompilerStatus { get; }

        void CompilerSetup();
        void CompilerSave(GameData.IDataCompilerLog stream);
        void CompilerLoad(GameData.IDataCompilerLog stream);
    }
}
