using System.Reflection;
using System.Runtime.Loader;

using BigfileBuilder;
using GameCore;
using GameData;
using DataBuildSystem;
using CommandLineParser.Arguments;

namespace GameDataCompiler
{
    class Program
    {
        static int Error()
        {
            return 1;
        }

        static int Success()
        {
            return 0;
        }

        class GameDataCompilerArgs
        {
            public static GameDataCompilerArgs Parse(string[] args)
            {
                var clp = new CommandLineParser.CommandLineParser();
                var vars = new GameDataCompilerArgs();
                clp.ExtractArgumentAttributes(vars);
                clp.ParseCommandLine(args);
                return vars;
            }

            [ValueArgument(typeof(string), 'n', "name", DefaultValue = "Game", Description = "Name of the game")]
            public string name;

            [ValueArgument(typeof(string), 'p', "platform", DefaultValue = "PC", Description = "Platform (PC/PS4/PS5/XBOXONE/XSX)")]
            public string platform;

            [ValueArgument(typeof(string), 't', "target", DefaultValue = "PC", Description = "Target platform (PC/PS4/PS5/XBOXONE/XSX)")]
            public string target;

            [ValueArgument(typeof(string), 'r', "territory", DefaultValue = "Europe", Description = "Territory (Europe/USA/Asia/Japan)")]
            public string territory;

            [ValueArgument(typeof(string), 'b', "basepath", DefaultValue = "", Description = "Base path")]
            public string basepath;

            [ValueArgument(typeof(string), 'g', "gddpath", DefaultValue = "", Description = "Gdd path")]
            public string gddpath;

            [ValueArgument(typeof(string), 's', "srcpath", DefaultValue = "", Description = "Source path")]
            public string srcpath;

            [ValueArgument(typeof(string), 'u', "subpath", DefaultValue = "", Description = "Sub path")]
            public string subpath;

            [ValueArgument(typeof(string), 'd', "dstpath", DefaultValue = "", Description = "Destination path")]
            public string dstpath;

            [ValueArgument(typeof(string), 'l', "pubpath", DefaultValue = "", Description = "Publish path")]
            public string pubpath;

            [ValueArgument(typeof(string), 'o', "toolpath", DefaultValue = "", Description = "Tool path")]
            public string toolpath;
        }

        // --name Game --platform PC --territory Europe --basepath E:\Dev.Net\.NET_BuildSystem\Data.Test --srcpath %BASEPATH%\Src.Data --gddpath %BASEPATH%\Gdd.Data --pubPath %BASEPATH%\Publish.%PLATFORM% --dstpath %BASEPATH%\Bin.%PLATFORM% --toolpath %BASEPATH%\Tools
        static int Main(string[] args)
        {
            var cmdLine = GameDataCompilerArgs.Parse(args);

            // On the command-line we have:
            // - platform     PC                                            (PS4/PS5/XBOXONE/XSX/PC)
            // - target       PC                                            (PS4/PS5/XBOXONE/XSX/PC)
            // - name         Game
            // - territory    Europe                                        (Europe/USA/Asia/Japan)
            // - basepath     i:\Data                                       (Can be set and used as %BASEPATH%)
            // - gddpath      %BASEPATH%\Gdd\Compiled
            // - srcpath      %BASEPATH%\Assets
            // - subpath                                                    (AI, Boot, Levels, Menu\FrontEnd)
            // - dstpath      %BASEPATH%\Bin.%PLATFORM%.%TARGET%
            // - pubpath      %BASEPATH%\Publish.%PLATFORM%.%TARGET%
            // - toolpath     %BASEPATH%\Tools
            if (!BuildSystemConfig.Init(cmdLine.name, cmdLine.platform, cmdLine.target, cmdLine.territory, cmdLine.basepath, cmdLine.srcpath, cmdLine.gddpath, cmdLine.subpath, cmdLine.dstpath, cmdLine.pubpath, cmdLine.toolpath))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -target [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -basepath [BASEPATH]");
                Console.WriteLine("       -gddpath [GDDPATH]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -subpath [SUBPATH]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine();
                return 1;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - GameDataCompiler: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemConfig.Platform.ToString(), BuildSystemConfig.Target.ToString());

            // Record the total build time
            var buildStart = DateTime.Now;

            // Create the destination, gdd and publish output paths
            DirUtils.Create(BuildSystemConfig.DstPath);
            DirUtils.Create(BuildSystemConfig.PubPath);

            // GameDataUnits, have it initialize by loading the GameData.dll
            var gdus = new GameDataUnits();
            var gda = gdus.Initialize("GameData.dll");

            // BuildSystem.DataCompiler configuration
            var configsForCompiler = AssemblyUtil.CreateN<IBuildSystemConfig>(gda);
            if (configsForCompiler != null && configsForCompiler.Length > 0)
            {
                foreach (var config in configsForCompiler)
                {
                    if (config.Platform.HasFlag(BuildSystemConfig.Platform))
                    {
                        BuildSystemConfig.Init(config);
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unable to find a 'BuildSystemCompilerConfig' for {BuildSystemConfig.Platform} -- error");
                return Error();
            }

            // Bigfile configuration
            var configForBigfileBuilder = AssemblyUtil.CreateN<IBigfileConfig>(gda);
            if (configForBigfileBuilder != null && configForBigfileBuilder.Length > 0)
            {
                // Note: There should only be one BigfileConfig
                foreach (var config in configForBigfileBuilder)
                {
                    BigfileConfig.Init(config);
                    break;
                }
            }
            else
            {
                Console.WriteLine($"Unable to find a 'IBigfileConfig' for {BuildSystemConfig.Platform.ToString()} -- error");
                return Error();
            }

            GameDataPath.BigFileExtension = BigfileConfig.BigFileExtension;
            GameDataPath.BigFileTocExtension = BigfileConfig.BigFileTocExtension;
            GameDataPath.BigFileFdbExtension = BigfileConfig.BigFileFdbExtension;
            GameDataPath.BigFileHdbExtension = BigfileConfig.BigFileHdbExtension;

            var start = DateTime.Now;
            Console.WriteLine("------ Initializing data compilation units");
            gdus.Load(BuildSystemConfig.DstPath, BuildSystemConfig.GddPath);
            var end = DateTime.Now;
            Console.WriteLine("Finished initialization -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("------ Data compilation started: {0}", BuildSystemConfig.Name);
            gdus.Cook(BuildSystemConfig.SrcPath, BuildSystemConfig.DstPath);
            end = DateTime.Now;
            Console.WriteLine("Data compilation complete -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            gdus.Save(BuildSystemConfig.DstPath);
            Console.WriteLine("Finished -- Total build time {0}s", (DateTime.Now - buildStart).TotalSeconds);

            return Success();
        }
    }
}

