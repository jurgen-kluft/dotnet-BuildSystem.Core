using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Core;
using DataBuildSystem;
namespace BigfilePacker
{
    class Program
    {
        #region Fields

        public static TraceSwitch Trace = new TraceSwitch("BuildTools.BigfileCompressor", "Switch for tracing of the BigfileCompressor");

        #endregion
        #region Main

        // -name PMR2 -publishpath I:\Dev\PMRII_Export\Data\Publish.PS2 -srcpath I:\Dev\PMRII_Export\Data -dstpath I:\Dev\PMRII_Export\Data\Bin.PS2 -platform PS2 -toolpath I:\Dev\PMRII_Dev\Tools\Release
        // -name TEST -publishpath H:/BuildSystem_TEST/Data/Publish.Mac -srcpath H:/BuildSystem_TEST/Data -dstpath H:/BuildSystem_TEST/Data/Bin.Mac -platform Mac -toolpath H:/BuildSystem_TEST/Tools/Release
        static int Main(string[] args)
        {
            CommandLine cmdLine = new CommandLine(args);

            if (!Config.init(cmdLine["name"], cmdLine["srcpath"], cmdLine["dstpath"], cmdLine["toolpath"], cmdLine["publishpath"], cmdLine["platform"]))
            {
                Console.WriteLine("Usage: -name [NAME] -srcpath [SRCPATH] -dstpath [DSTPATH] -toolpath [TOOLPATH] -publishpath [PUBLISHPATH] -platform [PLATFORM]");
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ BigfilePacker: v{0} (Platform: {1}) ------", version, Config.PlatformName);

            // Setup xEnvironment
            Core.Environment.addVariable("NAME", Config.Name);
            Core.Environment.addVariable("PLATFORM", Config.PlatformName);

            // Construct an assembly with the config object
            List<Filename> assemblyReferences = new List<Filename>();
            assemblyReferences.Add(new Filename(Assembly.GetExecutingAssembly().Location));
            List<Filename> filenames = new List<Filename>();
            filenames.Add(Config.SrcPath + new Filename(String.Format("Config.BigfileBuilder.{0}.cs", Config.PlatformName)));
            Assembly configAssembly = AssemblyUtil.BuildAssemblyDirectly(Config.DstPath + new Filename(Config.Name + "BigfilePacker.Config.dll"), filenames, assemblyReferences);

            IBigfileConfig bigFileConfig = new BigfileDefaultConfig();
            if (configAssembly != null)
            {
                // Instanciate
                bigFileConfig = AssemblyUtil.Create1<IBigfileConfig>(configAssembly);
                if (bigFileConfig != null)
                    BigfileConfig.Init(bigFileConfig);
            }

            // Record the total build time
            DateTime buildStart = DateTime.Now;

            BigfileCompressor compressor = new BigfileCompressor();

            Filename bigFilename = new Filename(bigFileConfig.BigfileName);
            EEndian bigFileEndian = bigFileConfig.LittleEndian ? EEndian.LITTLE : EEndian.BIG;

            if (compressor.Compress(Config.PublishPath, bigFilename, bigFileEndian))
            {
                Console.WriteLine("Compression complete -- error");
                return 1;
            }
            else
            {
                Console.WriteLine("Compression complete -- 0 errors, 0 warnings");
            }

            return 0;
        }

        #endregion
    }
}
