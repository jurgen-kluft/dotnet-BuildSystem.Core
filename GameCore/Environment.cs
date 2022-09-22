using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameCore
{
    public partial class Environment
    {
        static Environment()
        {
            HomeDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        }

        private static Dictionary<string, string> sVariables = new Dictionary<string, string>();
		public static void addVariable(string var, string value)
        {
            sVariables.Add(var, value);
        }
        public static string expandVariables(string str)
        {
            foreach(KeyValuePair<string,string> v in sVariables)
			    str = str.Replace(String.Format("%{0}%", v.Key), v.Value);
            return str;
        }

        public static string[] GetCommandLineArgs()
		{
            string[] args = System.Environment.GetCommandLineArgs();
            return args;
		}

        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static string HomeDirectory { get; private set; }
    }
}
