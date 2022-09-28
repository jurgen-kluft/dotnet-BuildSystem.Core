using System.Collections.Generic;
using System.Buffers;
using DataBuildSystem;
using GameCore;

namespace GameData
{
    /// <summary>
    /// The data compiler interface
    /// </summary>
    public interface IDataCompiler
    {
        /// A signature is generated from the 'stable' properties of a DataCompiler.
        /// For example: for the CopyCompiler should write SrcFilename and DstFilename into the stream.
		void CompilerSignature(IBinaryWriter stream);

        void CompilerSetup();
        void CompilerWrite(IBinaryWriter stream);
        void CompilerRead(IBinaryReader stream);
    }
}
