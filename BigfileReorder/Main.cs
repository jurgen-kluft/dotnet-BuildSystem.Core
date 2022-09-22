using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using GameCore;
using DataBuildSystem;

namespace BigfileFileReorder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			Log.sInit();

			System.Globalization.CultureInfo oldCurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			if (oldCurrentCulture.IetfLanguageTag != "en-US")
			{
				Console.WriteLine("Please change your regional settings to en-US!");
			}
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			CommandLine arg = new CommandLine(System.Environment.GetCommandLineArgs());
			if (string.Empty != arg["GUI"])
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm(arg));
			}
			else
			{
				ReorderFiles(arg);
			}
        }

		static void ReorderFiles(CommandLine cmdLine)
		{
			if (!Config.init(cmdLine["name"], cmdLine["srcpath"], cmdLine["dstpath"], cmdLine["toolpath"], cmdLine["publishpath"], cmdLine["platform"]))
			{
				Console.WriteLine("Usage: -name [NAME] -srcpath [SRCPATH] -dstpath [DSTPATH] -toolpath [TOOLPATH] -publishpath [PUBLISHPATH] -platform [PLATFORM]");
				Console.WriteLine("Press a key");
				return;
			}

            // Setup xEnvironment
            GameCore.Environment.addVariable("NAME", Config.Name);
			GameCore.Environment.addVariable("PLATFORM", Config.PlatformName);

			// Construct an assembly with the config object
            Filename configFilename = Config.SrcPath + new Filename(String.Format("Config.BigfileReorder.{0}.cs", Config.PlatformName));
            FileInfo configFileInfo = new FileInfo(configFilename);
            Assembly configAssembly = null;
            if (configFileInfo.Exists)
            {
                List<Filename> assemblyReferences = new List<Filename>();
                assemblyReferences.Add(new Filename(Assembly.GetExecutingAssembly().Location));
                List<Filename> filenames = new List<Filename>();
			    filenames.Add(configFilename);
                configAssembly = AssemblyUtil.BuildAssemblyDirectly(Config.SrcPath + new Filename(Config.Name + ".BigfileReorder.Config.dll"), filenames, assemblyReferences);
            }
            else
            {
				Console.WriteLine("Warning: {0} not found, using default configuration.", configFilename);
            }

			IBigfileConfig bigFileConfig = new BigfileDefaultConfig();
			if (configAssembly != null)
			{
				// Instanciate
				bigFileConfig = AssemblyUtil.Create1<IBigfileConfig>(configAssembly);
			}
            if (bigFileConfig!=null)
			    BigfileConfig.Init(bigFileConfig);

			BigfileFileOrder order = new BigfileFileOrder();
            order.Reorder(Config.PublishPath + new Filename(Config.Name + ".gda"), Config.PublishPath + new Filename(Config.Name + ".gdt"), Config.SrcPath + new Filename("FileRecorder.rec"), Config.PlatformName);
		}
    }
}
