using System.Collections.Generic;

using Core;
using DataBuildSystem;

namespace Game.Data
{
    public class CopyCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private readonly Filename mFilename;
        private FileId fileId = new FileId();
        private DepFile mDepFile;
        private DependencySystem mDependencySystem;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public CopyCompiler(string filename)
        {
            filename = Environment.expandVariables(filename);
            mFilename = new Filename(filename);
        }

        public string group { get { return "CopyCompiler"; } }
        public EDataCompilerStatus status { get { return mStatus; } }
        public EDataCompilerPriority priority { get { return EDataCompilerPriority.ATOMIC_ASSET; } }

        public void csetup(DependencySystem dependencySystem)
        {
            mDependencySystem = dependencySystem;

            mDepFile = mDependencySystem.get(mFilename);
            if (mDepFile==null)
            {
                mDepFile = new DepFile(mFilename, BuildSystemCompilerConfig.SrcPath);
                if (!mDepFile.load(BuildSystemCompilerConfig.DepPath))
                {
                    // Construct it manually
                    mDepFile.addOut(mFilename, BuildSystemCompilerConfig.DstPath);
                }
                mDependencySystem.add(mDepFile);
            }
        }

        public void ccompile(IDataCompilationServer dataCompilerServer)
        {
            dataCompilerServer.schedule(this);
        }

        public void cteardown()
        {
            mDependencySystem = null;
        }

        public void onExecute()
        {
            if (mDependencySystem.isModified(mFilename))
            {
                if (FileCommander.copyFromTo(BuildSystemCompilerConfig.SrcPath, mFilename, BuildSystemCompilerConfig.DstPath, mFilename))
                    mStatus = EDataCompilerStatus.SUCCESS;
                else
                    mStatus = EDataCompilerStatus.ERROR;
            }
            else
            {
                mStatus = EDataCompilerStatus.UPTODATE;
            }
        }

        public void onFinished()
        {
            mDepFile.save(mDependencySystem.depPath);
        }

        public void registerAt(IFileRegistrar registrar)
        {
            fileId = registrar.Add(mFilename);
        }

        public FileId[] fileIds { get { return new FileId[] { fileId }; } }
    }
}
