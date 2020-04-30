using System;
using System.Reflection;

using Core;
using DataBuildSystem;

namespace Game.Data
{
    public class ExternalRootNodeCompiler : IDataCompiler, IDataCompilerClient, IDataCompilerNode, IFileIdsProvider
    {
        private readonly string mName;
        private readonly Dirname mDataAssemblyFolder;
        private readonly Filename mCsIncludeFilename;
        private Filename[] mDstFilenames = new Filename[0];
        private Filename mBigfileNodeFilename = Filename.Empty;
        private FileId[] mFileIds = new FileId[0];

        private Process mBuildSystemCompilerProcess;
        private string mStdError;
        private string mStdOut;
        private bool mTimedOut;
        private int mExitCode;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        private DependencySystem mDependencySystem;

        public ExternalRootNodeCompiler(string name, string _csincludeFilename)
        {
            mName = name;
			mCsIncludeFilename = new Filename(Core.Environment.expandVariables(_csincludeFilename));
            mDataAssemblyFolder = mCsIncludeFilename.Path;

            Filename dstFilename = mDataAssemblyFolder + new Filename(mName + ".tmp");
            mDstFilenames = new Filename[2];

            // These 2 data files should end up in the final Bigfile
            mDstFilenames[0] = dstFilename.ChangedExtension(BuildSystemCompilerConfig.DataFileExtension);
            mDstFilenames[1] = dstFilename.ChangedExtension(BuildSystemCompilerConfig.DataRelocFileExtension);

            mBigfileNodeFilename = dstFilename.ChangedExtension(BigfileConfig.BigFileNodeExtension);
        }

        public string group { get { return "ExternalRootNodeCompiler"; } }
        public string name { get { return mName; } }
        public EDataCompilerStatus status { get { return mStatus; } }
        public EDataCompilerPriority priority { get { return EDataCompilerPriority.EXTERNAL_COMPILER; } }

        public Filename bigfileFilename { get { return mBigfileNodeFilename; } }

        public void csetup(DependencySystem dependencySystem)
        {
            mDependencySystem = dependencySystem;

            string dataBuildSystemCompilerFilename = Assembly.GetExecutingAssembly().Location;
            mBuildSystemCompilerProcess = new Process(new Filename(dataBuildSystemCompilerFilename), BuildSystemCompilerConfig.SrcPath, 0);
            mBuildSystemCompilerProcess.Verbose = false;
        }

        // This 'compile' function could be extended by passing a 'process scheduler' which can handle the
        // execution of process. An advanced scheduler can execute a process on a remote PC or on a 2nd core.
        public void ccompile(IDataCompilationServer dataCompilerServer)
        {
            /// 1) Execute the BuildSystem.Compiler in this folder
            /// 2) The source file is empty
            /// 3) The destination files are the .rdf and .raf files
            /// 4) Save dependency file containing destination files and empty 
            ///    source file and register it so that these files end up in the Bigfile.
            dataCompilerServer.schedule(this);
        }

        public void cteardown()
        {
            mDependencySystem = null;
        }

        public void onExecute()
        {
            CommandLineBuilder clb = new CommandLineBuilder();
            clb.AddArgument("-name", mName);
            clb.AddArgument("-config", BuildSystemCompilerConfig.ConfigFilename);
            clb.AddArgument("-platform", BuildSystemCompilerConfig.PlatformName);
            clb.AddArgument("-target", BuildSystemCompilerConfig.TargetName);
            clb.AddArgument("-territory", BuildSystemCompilerConfig.TerritoryName);
            clb.AddArgument("-srcpath", BuildSystemCompilerConfig.SrcPath);
            clb.AddArgument("-subpath", mDataAssemblyFolder);
            clb.AddArgument("-file0", mCsIncludeFilename);
            clb.AddArgument("-dstpath", BuildSystemCompilerConfig.DstPath);
            clb.AddArgument("-deppath", BuildSystemCompilerConfig.DepPath);
            clb.AddArgument("-toolpath", BuildSystemCompilerConfig.ToolPath);
            clb.AddArgument("-pubpath", BuildSystemCompilerConfig.DstPath);

            int a = 0;
            foreach (Filename refAsm in BuildSystemCompilerConfig.ReferencedAssemblies)
                clb.AddArgument(String.Format("-asm{0}", a++), refAsm);
            foreach (Filename codeAsm in CodeAsmCompiler.sCompiledCodeAssemblyFilenames)
                clb.AddArgument(String.Format("-asm{0}", a++), codeAsm);

            ProcessResult pr = mBuildSystemCompilerProcess.Execute(clb.ToString());
            mStdError = pr.StandardError;
            mStdOut = pr.StandardOutput;
            mTimedOut = pr.TimedOut;
            mExitCode = pr.ExitCode;

            //TODO: Dump StdOut of process to this StdOut?
            if (mExitCode != 0)
            {
                Console.WriteLine(mStdOut);
                Console.WriteLine(mStdError);
            }
        }

        public void onFinished()
        {

        }

        public void registerAt(IFileRegistrar registrar)
        {
            mFileIds = new FileId[2];

            int i = 0;
            Array.ForEach(mDstFilenames, f => { mFileIds[i++] = registrar.Add(f); });
        }

        public FileId[] fileIds { get { return mFileIds; } }
    }
}
