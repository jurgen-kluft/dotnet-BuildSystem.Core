using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Core
{
    using SystemProcess = System.Diagnostics.Process;
    using SystemProcessStartInfo = System.Diagnostics.ProcessStartInfo;

    public class ProcessInfo
    {
        public const int DefaultTimeout = 120000;
        public const int InfiniteTimeout = 0;

        private SystemProcessStartInfo startInfo = new SystemProcessStartInfo();
        private string standardInputContent = null;
        private int timeout = DefaultTimeout;
        private bool verbose = true;

        public ProcessInfo(string filename)
            : this(filename, null)
        { }

        public ProcessInfo(string filename, string arguments)
            : this(filename, arguments, null)
        { }

        public ProcessInfo(string filename, string arguments, string workingDirectory)
        {
            startInfo.FileName = StringTools.StripQuotes(filename);
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = StringTools.StripQuotes(workingDirectory);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = false;
            RepathExecutableIfItIsInWorkingDirectory();
        }

        private void RepathExecutableIfItIsInWorkingDirectory()
        {
            if (WorkingDirectory != null)
            {
                string executableInWorkingDirectory = Path.Combine(WorkingDirectory, FileName);
                if (File.Exists(executableInWorkingDirectory))
                {
                    startInfo.FileName = executableInWorkingDirectory;
                }
            }
        }

        public StringDictionary EnvironmentVariables
        {
            get { return startInfo.EnvironmentVariables; }
        }

        public string FileName
        {
            get { return startInfo.FileName; }
        }

        public string Arguments
        {
            get { return startInfo.Arguments; }
            set { startInfo.Arguments = value; }
        }

        public string WorkingDirectory
        {
            get { return startInfo.WorkingDirectory; }
            set { startInfo.WorkingDirectory = value; }
        }

        public string StandardInputContent
        {
            get { return standardInputContent; }
            set
            {
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                standardInputContent = value;
            }
        }

        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int TimeOut
        {
            get { return timeout; }
            set { timeout = (value == InfiniteTimeout) ? 0x7fffffff : value; }
        }

        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }

        public Encoding StreamEncoding
        {
            get
            {
                return startInfo.StandardOutputEncoding;
            }
            set
            {
                startInfo.StandardOutputEncoding = value;
                startInfo.StandardErrorEncoding = value;
            }
        }

        public SystemProcess CreateProcess()
        {
            if (!StringTools.IsBlank(WorkingDirectory) && !Directory.Exists(WorkingDirectory))
                throw new DirectoryNotFoundException("Directory does not exist: " + WorkingDirectory);

            SystemProcess process = new SystemProcess();
            process.StartInfo = startInfo;
            return process;
        }

        public override bool Equals(object obj)
        {
            ProcessInfo otherProcessInfo = obj as ProcessInfo;
            if (otherProcessInfo == null)
            {
                return false;
            }

            return (FileName == otherProcessInfo.FileName
                && Arguments == otherProcessInfo.Arguments
                && WorkingDirectory == otherProcessInfo.WorkingDirectory
                && TimeOut == otherProcessInfo.TimeOut
                && StandardInputContent == otherProcessInfo.StandardInputContent);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("FileName: [{0}] -- Arguments: [{1}] -- WorkingDirectory: [{2}] -- StandardInputContent: [{3}] -- Timeout: [{4}]",
                                 FileName, Arguments, WorkingDirectory, StandardInputContent, TimeOut);
        }
    }
}