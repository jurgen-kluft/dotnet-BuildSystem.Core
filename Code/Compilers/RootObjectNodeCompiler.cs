using System;
using System.Reflection;
using System.Collections.Generic;

namespace Game.Data
{
    using Core;
    using DataBuildSystem;

    /// <summary>
    /// This compiler will compile C# files into an assembly and instantiate a root object.
    /// All IDataCompilers objects are transparently routed to the main dataCompilerServer.
    /// </summary>
    public class RootObjectNodeCompiler : IDataCompiler, IExternalObjectProvider
    {
        private readonly Filename mAsmFilename;
        private readonly List<Filename> mSrcFilenames;
        private readonly List<Filename> mIncludeFilenames;
        private DependencySystem mDependencySystem;
        private DataAssemblyManager mDataAssemblyManager;
        private bool mValid = true;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        private RootObjectNodeCompiler(string _dllfilename)
        {
            mAsmFilename = new Filename(Environment.expandVariables(_dllfilename));
            mSrcFilenames = new List<Filename>();
            mIncludeFilenames = new List<Filename>();
        }

        public RootObjectNodeCompiler(string _dllfilename, Filename _filename)
            : this(_dllfilename)
        {
            if (_filename.Extension == ".cs")
                mSrcFilenames.Add(_filename);
            else
                mIncludeFilenames.Add(_filename);
        }

        public RootObjectNodeCompiler(string _dllfilename, Filename[] _files)
            : this(_dllfilename)
        {
            for (int i = 0; i < _files.Length; ++i)
            {
                string filename = Environment.expandVariables(_files[i]);
                if (filename.EndsWith(".cs"))
                    mSrcFilenames.Add(new Filename(filename));
                else
                    mIncludeFilenames.Add(new Filename(filename));
            }
        }

        public EDataCompilerStatus status { get { return mStatus; } }
        public EDataCompilerPriority priority { get { return EDataCompilerPriority.COMPILED_ASSEMBLY; } }

        public void csetup(DependencySystem dependencySystem)
        {
            mDependencySystem = dependencySystem;

            /// 1) Compile the C# files into an assembly
            /// 2) Instantiate the root object and initialize mObject
            /// 3) Initialize data compilation
            /// 4) Collect and Execute all IDataCompilers
            /// 5) Finalize data compilation

            List<Filename> referencedAssemblies = new List<Filename>();
            referencedAssemblies.Add(new Filename(Assembly.GetExecutingAssembly().Location));

            foreach (Filename refAsm in BuildSystemCompilerConfig.ReferencedAssemblies)
                referencedAssemblies.Add(refAsm);

            mDataAssemblyManager = new DataAssemblyManager();
            mValid = mDataAssemblyManager.compileAsm(mAsmFilename, mSrcFilenames.ToArray(), mIncludeFilenames.ToArray(), BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, referencedAssemblies.ToArray());

            mDataAssemblyManager.initializeDataCompilation();
            mDataAssemblyManager.setupDataCompilers(mDependencySystem);
        }

        public void ccompile(Game.Data.IDataCompilationServer dataCompilerServer)
        {
            // Schedule all IDataCompiler objects at the main dataCompilerServer
            mDataAssemblyManager.compilationServer = dataCompilerServer;
            mDataAssemblyManager.executeDataCompilers(mDependencySystem, false);
        }

        public void cteardown()
        {
            mDataAssemblyManager.teardownDataCompilers(mDependencySystem);
            mDataAssemblyManager.finalizeDataCompilation(mDependencySystem);
            mDependencySystem = null;

            // We have executed all compilers, so where they all up-to-date or
            // did one or more of them actually do something.
        }

        public object extobject
        {
            get
            {
                return mDataAssemblyManager.root;
            }
        }
    }
}

