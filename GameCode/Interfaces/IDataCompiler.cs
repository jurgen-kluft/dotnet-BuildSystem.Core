using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    public struct DataCompilerOutput
    {
        public Int64 OutFileId { get; set; }
        public string[] OutFilenames { get; set; }

        public DataCompilerOutput(Int64 fileId, string[] filenames)
        {
            OutFileId = fileId;
            OutFilenames = filenames;
        }
    }

    /// <summary>
    /// The data compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        ///<summary>
        /// A signature is generated from the 'stable' properties of a DataCompiler.
        /// For example: for the CopyCompiler should write SrcFilename and DstFilename into the Writer stream.
        ///</summary>
        void CompilerSignature(IBinaryWriter writer);

        ///<summary>
        /// Write all the necessary properties and data to the stream
        ///</summary>
        void CompilerWrite(IBinaryWriter writer);

        ///<summary>
        /// Read all the properties and data from the stream in the same order and type as they where written
        ///</summary>
        void CompilerRead(IBinaryReader reader);

        ///<summary>
        /// Return the FilesProvider associated with this IDataCompiler (this is for Types.FileId)
        ///</summary>
        IFileIdProvider CompilerFileIdProvider { get; }

        ///<summary>
        /// Execute the compiler and add the destination filenames to @dst_relative_filepaths using ctx.StringDb
        /// - return 0 if src and dst are up-to-date
        /// - return 1 if execution was successful and dst files where updated
        /// - return 2 if execution was successful and dst files where updated and compiler version changes where detected
        /// - return -1 if execution failed
        ///</summary>
        int CompilerExecute(List<DataCompilerOutput> output);
    }
}
