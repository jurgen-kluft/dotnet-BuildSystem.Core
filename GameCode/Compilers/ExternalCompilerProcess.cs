using System;

using GameCore;
using DataBuildSystem;

namespace GameData
{
    /// <summary>
    /// An external process
    /// </summary>
    public class ExternalCompilerProcess
    {
        private string mBinarizerName = string.Empty;
        private bool mProcessHasBeenModified = false;
        private Process mProcess;

        public ExternalCompilerProcess(string binarizer, string workDir, int timeOutInMinutes)
        {
            mBinarizerName = binarizer;
            ConstructProcess(new Filename(binarizer), new Dirname(workDir), timeOutInMinutes);
        }

        public string name
        {
            get
            {
                return mBinarizerName;
            }
        }

        public bool isModified
        {
            get
            {
                return mProcessHasBeenModified;
            }
        }

        public Process process
        {
            get
            {
                return mProcess;
            }
        }

        private void ConstructProcess(Filename binarizer, Dirname workDir, int timeOutInMinutes)
        {
            mProcess = new Process(BuildSystemDefaultConfig.ToolPath+binarizer, workDir, timeOutInMinutes);

            FileCommander.createDirectoryOnDisk(BuildSystemDefaultConfig.DstPath);

            //const string additionalExtension = ".tdep";

            mProcessHasBeenModified = true;
        }

        public string Arguments(string filename)
        {
            return string.Format("-srcpath \"{0}\" -dstpath \"{1}\" -srcfile \"{2}\"", BuildSystemDefaultConfig.SrcPath, BuildSystemDefaultConfig.DstPath, filename, BuildSystemDefaultConfig.Platform.ToString());
        }

        public string Arguments(string filename, string appendToCmdLine)
        {
            var a = Arguments(filename);
            a += " " + appendToCmdLine;
            return a;
        }
    }
}
