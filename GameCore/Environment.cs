using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameCore
{
    public static partial class Environment
    {
        static Environment()
        {
            HomeDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            Variables = new Dictionary<string, string>();
        }

        private static Dictionary<string, string> Variables { get; set; }
		public static void AddVariable(string var, string value)
        {
            Variables.Add(var, value);
        }
        public static string ExpandVariables(string str)
        {
            foreach(var (variable, value) in Variables)
			    str = str.Replace(string.Format("%{0}%", variable), value, StringComparison.OrdinalIgnoreCase);
            return str;
        }

        public static string[] GetCommandLineArgs()
		{
            var args = System.Environment.GetCommandLineArgs();
            return args;
		}

        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static string HomeDirectory { get; private set; }
    }
}
