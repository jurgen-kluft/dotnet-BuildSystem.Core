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

    public interface IDataCompilerStream
	{
        void Save(ByteSpan s);
        void Load(ByteSpan s);
    }

    /// <summary>
    /// The compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        EDataCompilerStatus status { get; }

        void csetup();
        void csave(GameData.IDataCompilerStream stream);
        void cload(GameData.IDataCompilerStream stream);
    }
}
