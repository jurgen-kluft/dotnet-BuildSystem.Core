using System;
using System.Reflection;
using System.Collections.Generic;

namespace Game.Data
{
    using Core;
    using DataBuildSystem;

    /// <summary>
    /// This compiler will compile C# files into an assembly that will be used as a 'code' dll.
    /// It should only contain definitions, enums and baseclasses used in other RootObject or
    /// ExternalRootObject nodes.
    /// </summary>
    public class CodeAsmCompiler : IDataCompiler, IDataCompilerClient
    {
        private readonly Filename mAsmFilename;
        private readonly List<Filename> mSrcFilenames;
        private readonly List<Filename> mIncludeFilenames;
        private DependencySystem mDependencySystem;
        private Assembly mDataAssembly;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public static List<Filename> sCompiledCodeAssemblyFilenames = new List<Filename>();

        private CodeAsmCompiler(string _dllfilename)
        {
            mAsmFilename = new Filename(Environment.expandVariables(_dllfilename));
            mAsmFilename.ChangeExtension(".code");
            mAsmFilename.PushExtension(".dll");
            mSrcFilenames = new List<Filename>();
            mIncludeFilenames = new List<Filename>();
        }

        public CodeAsmCompiler(string _dllfilename, Filename _filename)
            : this(_dllfilename)
        {
            if (_filename.Extension == ".cs")
                mSrcFilenames.Add(_filename);
            else
                mIncludeFilenames.Add(_filename);
        }

        public CodeAsmCompiler(string _dllfilename, Filename[] _files)
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

        public string group { get { return "CodeAsmCompiler"; } }
        public EDataCompilerStatus status { get { return mStatus; } }
        public EDataCompilerPriority priority { get { return EDataCompilerPriority.CODE_ASSEMBLY; } }

        public void csetup(DependencySystem dependencySystem)
        {
            mDependencySystem = dependencySystem;
        }

        public void ccompile(Game.Data.IDataCompilationServer dataCompilerServer)
        {
            dataCompilerServer.schedule(this);
        }

        public void cteardown()
        {
            mDependencySystem = null;
        }

        public void onExecute()
        {
            mDataAssembly = AssemblyCompiler.Compile(mAsmFilename, mSrcFilenames.ToArray(), mIncludeFilenames.ToArray(), BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, BuildSystemCompilerConfig.ReferencedAssemblies);
            if (mDataAssembly != null)
                sCompiledCodeAssemblyFilenames.Add(new Filename(mDataAssembly.Location));
        }

        public void onFinished()
        {
        }
    }
}

