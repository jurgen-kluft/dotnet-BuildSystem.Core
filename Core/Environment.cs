using System;
using System.Collections.Generic;

namespace Core
{

    public static class Environment
    {
        private static Dictionary<string, string> sVariables = new Dictionary<string, string>();

		public static string CurrentDirectory { get; set; }
		public static string NewLine { get; set; } = "\n";
		public static string[] GetCommandLineArgs()
		{
			return new string[0];
		}

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
    }
}
