using System;
using System.IO;
using System.Collections.Generic;

using GameCore;
using DataBuildSystem;
namespace GameData
{
    public class LocalizationCompiler : IDataCompiler, IDataCompilerClient, IFileIdsProvider
    {
        private readonly Filename mSrcFilename;
        private List<Filename> mDstFilenames;
        private FileId[] mFileIds = null;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        public LocalizationCompiler(string localizationFile)
        {
            mSrcFilename = new Filename(localizationFile).ChangedExtension(".loc").PushedExtension(".ids").PushedExtension(".lst");
        }

        public string group { get { return "LocalizationCompiler"; } }
        public EDataCompilerStatus CompilerStatus { get { return mStatus; } }

        public void CompilerSetup()
        {
        }

        public void CompilerSave(GameData.IDataCompilerLog stream)
        {
        }

        public void CompilerLoad(GameData.IDataCompilerLog stream)
        {
        }

        public void cteardown()
        {
        }

        public void CompilerExecute()
        {
            //if (mDependencySystem.isModified(mSrcFilename))
            {
                // Load 'languages list' file
                try
                {
                    xTextStream ts = new xTextStream(mSrcFilename);
                    ts.Open(xTextStream.EMode.READ);
                    while (!ts.read.EndOfStream)
                    {
                        string filename = ts.read.ReadLine();
                        if (String.IsNullOrEmpty(filename))
                            mDstFilenames.Add(new Filename(filename));
                    }
                    ts.Close();
                }
                catch (Exception)
                {
                    mStatus = EDataCompilerStatus.ERROR;
                }
                finally
                {
                    mStatus = EDataCompilerStatus.SUCCESS;
                }
            }
            //else
            //{
            //    mStatus = EDataCompilerStatus.UPTODATE;
            //}
        }

        public void CompilerFinished()
        {
            //mDependencySystem = null;
        }

        public void registerAt(IFileRegistrar registrar)
        {
            int i = 0;
            mFileIds = new FileId[mDstFilenames.Count];
            foreach(Filename f in mDstFilenames)
            {
                mFileIds[i] = registrar.Add(f);
                ++i;
            }
        }

        public FileId[] fileIds { get { return mFileIds; } }
    }
}
