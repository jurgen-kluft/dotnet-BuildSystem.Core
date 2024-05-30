using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    [Flags]
    public enum DataCompilerResult : ushort
    {
        None       = 0x0000,
        UpToDate   = 0x0001,
        SrcChanged = 0x0002,
        SrcMissing = 0x0004,
        DstChanged = 0x0008,
        DstMissing = 0x0010,
        VerChanged = 0x0100, // Version of the compiler has changed
        Error      = 0x8000,
    }

    public struct DataCompilerOutput
    {
        public string[] Filenames { get; } // Files in 'DstPath'
        public IFileIdProvider FileIdProvider { get; }
        public DataCompilerResult Result { get; }

        public DataCompilerOutput(DataCompilerResult result, string[] filenames, IFileIdProvider fileIdProvider)
        {
            Filenames = filenames;
            FileIdProvider = fileIdProvider;
            Result = result;
        }
    }

    /// <summary>
    /// The data compiler interface.
    ///
    /// A data compiler is a class that takes one or more source files and compiles it into one or more destination files.
    /// The resulting FileId is the identifier and references one or more (compiled) files in the Bigfile.
    ///
    /// </summary>
    public interface IDataCompiler
    {
        ///<summary>
        /// A signature is generated from the 'stable' properties of a DataCompiler.
        /// For example: The CopyCompiler should write SrcFilename and DstFilename into the Writer stream.
        ///</summary>
        void CompilerSignature(IBinaryWriter writer);

        ///<summary>
        /// Write all the necessary properties and data to the stream
        ///</summary>
        void CompilerWrite(IBinaryWriter writer);

        ///<summary>
        /// Read all the properties and data from the stream in the same order and type as they were written
        ///</summary>
        void CompilerRead(IBinaryReader reader);

        ///<summary>
        /// Take the properties from a previous instance as this instance (e.g. 'Copy Constructor')
        ///</summary>
        void CompilerConstruct(IDataCompiler dc);

        ///<summary>
        /// Return the FilesProvider associated with this IDataCompiler (this is for Types.FileId)
        ///</summary>
        IFileIdProvider CompilerFileIdProvider { get; }

        ///<summary>
        /// Execute the compiler and return the result as a DataCompilerOutput struct
        ///</summary>
        DataCompilerOutput CompilerExecute();
    }
}
