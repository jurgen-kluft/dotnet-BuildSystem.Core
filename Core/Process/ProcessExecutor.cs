using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Core
{
    using SystemProcess = System.Diagnostics.Process;

	/// <summary>
	/// The ProcessExecutor executes a new <see cref="System.Diagnostics.Process"/> using the properties specified in the input <see cref="ProcessInfo" />.
	/// ProcessExecutor is designed specifically to deal with processes that redirect the results of both
	/// the standard output and the standard error streams.  Reading from these streams is performed in
	/// a separate thread using the <see cref="SystemProcess.Reader"/> class, in order to prevent deadlock while 
	/// blocking on <see cref="SystemProcess.WaitForExit(int)"/>.
	/// If the process does not complete executing within the specified timeout period, the ProcessExecutor will attempt to kill the process.
	/// As process termination is asynchronous, the ProcessExecutor needs to wait for the process to die.  Under certain circumstances, 
	/// the process does not terminate gracefully after being killed, causing the ProcessExecutor to throw an exception.
	/// </summary>
	public class ProcessExecutor
    {
        private const int WAIT_FOR_KILLED_PROCESS_TIMEOUT = 5000;

        public virtual ProcessResult Execute(ProcessInfo processInfo)
        {
            if (processInfo.Verbose)
                Console.WriteLine(string.Format("Executing process {0} {1} in {2}", processInfo.FileName, processInfo.Arguments, processInfo.WorkingDirectory));

            using (SystemProcess process = Start(processInfo))
            {
                using (ProcessReader standardOutput = new ProcessReader(process.StandardOutput), standardError = new ProcessReader(process.StandardError))
                {
                    WriteToStandardInput(process, processInfo);

                    bool hasExited = process.WaitForExit(processInfo.TimeOut);
                    if (hasExited)
                    {
                        standardOutput.WaitForExit();
                        standardError.WaitForExit();
                    }
                    else
                    {
                        Kill(process, processInfo, standardOutput, standardError);
                    }
                    return new ProcessResult(standardOutput.Output, standardError.Output, process.ExitCode, !hasExited);
                }
            }
        }

        private SystemProcess Start(ProcessInfo processInfo)
        {
            SystemProcess process = processInfo.CreateProcess();

            if (processInfo.Verbose)
                Console.WriteLine(string.Format("Attempting to start process [{0}] in working directory [{1}] with arguments [{2}]", process.StartInfo.FileName, process.StartInfo.WorkingDirectory, process.StartInfo.Arguments));

            try
            {
                bool isNewProcess = process.Start();
                if (!isNewProcess)
                {
                    if (processInfo.Verbose)
                        Console.WriteLine("Reusing existing process...");
                }
            }
            catch (Win32Exception e)
            {
                string filename = Path.Combine(process.StartInfo.WorkingDirectory, process.StartInfo.FileName);
                string msg = string.Format("Unable to execute file [{0}].  The file may not exist or may not be executable.", filename);
                throw new IOException(msg, e);
            }
            return process;
        }

        private void Kill(SystemProcess process, ProcessInfo processInfo, ProcessReader standardOutput, ProcessReader standardError)
        {
            if (processInfo.Verbose)
            {
                Console.WriteLine(string.Format("Process timed out: {0} {1}.  Process id: {2}.  This process will now be killed.", processInfo.FileName, processInfo.Arguments, process.Id));

                Console.WriteLine(string.Format("Process stdout: {0}", standardOutput.Output));
                Console.WriteLine(string.Format("Process stderr: {0}", standardError.Output));
            }

            Kill(null, process, processInfo.Verbose);

            if (processInfo.Verbose)
                Console.WriteLine(string.Format("The timed out process has been killed: {0}", process.Id));
        }

        private void Kill(SystemProcess parent, SystemProcess process, bool verbose)
        {
            try
            {
                KillChildren(process, verbose);

                process.Kill();

                if (parent == null)
                {
                    if (verbose)
                        Console.WriteLine(string.Format("Process timed out: {0} {1} - Process id: {2}.  This process will now be killed.", process.StartInfo.FileName, process.StartInfo.Arguments, process.Id));
                }
                else
                {
                    if (verbose)
                        Console.WriteLine(string.Format("Process timed out: {0} {1} - Parent id: {2} - Process id: {3}.  This process will now be killed.", process.StartInfo.FileName, process.StartInfo.Arguments, parent.Id, process.Id));
                }

                if (!process.WaitForExit(WAIT_FOR_KILLED_PROCESS_TIMEOUT))
                    throw new SystemException(string.Format(@"The killed process {0} did not terminate within the allotted timeout period {1}.  The process or one of its child processes may not have died.  This may create problems when trying to re-execute the process.  It may be necessary to reboot the server to recover.", process.Id, WAIT_FOR_KILLED_PROCESS_TIMEOUT, process));
            }
            catch (InvalidOperationException)
            {
                if (verbose)
                    Console.WriteLine(string.Format("Process has already exited before getting killed: {0}", process.Id));
            }
        }

		private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout, out string stdout)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			var process = SystemProcess.Start(startInfo);
			stdout = null;
			if (process.WaitForExit((int)timeout.TotalMilliseconds))
			{
				stdout = process.StandardOutput.ReadToEnd();
			}
			else
			{
				process.Kill();
			}
			return process.ExitCode;
		}
		private static void GetAllChildIdsUnix(int parentId, ISet<int> children, TimeSpan timeout)
		{
			string stdout;
			var exitCode = RunProcessAndWaitForExit("pgrep", $"-P {parentId}", timeout, out stdout);
			if (exitCode == 0 && !string.IsNullOrEmpty(stdout))
			{
				using (var reader = new StringReader(stdout))
				{
					while (true)
					{
						var text = reader.ReadLine();
						if (text == null)
						{
							return;
						}
						int id;
						if (int.TryParse(text, out id))
						{
							children.Add(id);
							// Recursively get the children
							GetAllChildIdsUnix(id, children, timeout);
						}
					}
				}
			}
		}

		private static void KillProcessUnix(int processId, TimeSpan timeout)
		{
			string stdout;
			RunProcessAndWaitForExit("kill", $"-TERM {processId}", timeout, out stdout);
		}

		private void KillChildren(SystemProcess process, bool verbose)
		{
			TimeSpan timeout = new TimeSpan(0, 0, 15);
			string stdout;
			if (_isWindows)
			{
				RunProcessAndWaitForExit("taskkill", $"/T /F /PID {process.Id}", timeout, out stdout);
			}
			else
			{
				var children = new HashSet<int>();
				GetAllChildIdsUnix(process.Id, children, timeout);
				foreach (var childId in children)
				{
					KillProcessUnix(childId, timeout);
				}
				KillProcessUnix(process.Id, timeout);
			}
		}

        private void WriteToStandardInput(SystemProcess process, ProcessInfo processInfo)
        {
            // not tested yet - any ideas?
            // maybe we actually need this line-by-line. In that case we should probably extract this 
            // to a 'ProcessWriter' and do the thread stuff like the Readers do.
            if (process.StartInfo.RedirectStandardInput)
            {
                process.StandardInput.Write(processInfo.StandardInputContent);
                process.StandardInput.Flush();
                process.StandardInput.Close();
            }
        }
    }
}