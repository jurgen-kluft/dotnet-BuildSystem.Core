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

            [ValueArgument(typeof(string), 'n', "Name", DefaultValue = "Game", Description = "Name of the game")]
            public string Name { get; set; }

            [ValueArgument(typeof(string), 'p', "Platform", DefaultValue = "PC", Description = "Platform (PC/Playstation4/Playstation5/XboxOne/XboxSeriesX)")]
            public string Platform{ get; set; }

            [ValueArgument(typeof(string), 't', "Target", DefaultValue = "PC", Description = "Target platform (PC/Playstation4/Playstation5/XboxOne/XboxSeriesX)")]
            public string Target{ get; set; }

            [ValueArgument(typeof(string), 'r', "Territory", DefaultValue = "Europe", Description = "Territory (Europe/USA/Asia/Japan)")]
            public string Territory{ get; set; }

            [ValueArgument(typeof(string), 'b', "BasePath", DefaultValue = "", Description = "Base path")]
            public string BasePath{ get; set; }

            [ValueArgument(typeof(string), 'g', "GddPath", DefaultValue = "", Description = "Gdd path")]
            public string GddPath{ get; set; }

            [ValueArgument(typeof(string), 's', "SrcPath", DefaultValue = "", Description = "Source path")]
            public string SrcPath{ get; set; }

            [ValueArgument(typeof(string), 'u', "SubPath", DefaultValue = "", Description = "Sub path")]
            public string SubPath{ get; set; }

            [ValueArgument(typeof(string), 'd', "DstPath", DefaultValue = "", Description = "Destination path")]
            public string DstPath{ get; set; }

            [ValueArgument(typeof(string), 'l', "PubPath", DefaultValue = "", Description = "Publish path")]
            public string PubPath{ get; set; }

            [ValueArgument(typeof(string), 'o', "ToolPath", DefaultValue = "", Description = "Tool path")]
            public string ToolPath{ get; set; }
        }

        // Note: Any command-line option can be used as a %Variable%
        // --name Game --platform Win64 --territory Europe --BasePath E:\Dev.Net\.NET_BuildSystem\Data.Test --SrcPath %BasePath%\Src.Data --GddPath %BasePath%\Gdd.Data --PubPath %BasePath%\Publish.%Platform% --DstPath %BasePath%\Bin.%Platform% --ToolPath %BasePath%\Tools
        static int Main(string[] args)
        {
            var cmdLine = GameDataCompilerArgs.Parse(args);

            // On the command-line we have:
            // - Platform     PC                                            (PC/Playstation4/Playstation5/XboxOne/XboxSeriesX)
            // - Target       PC                                            (PC/Playstation4/Playstation5/XboxOne/XboxSeriesX)
            // - Name         Game
            // - Territory    Europe                                        (Europe/USA/Asia/Japan)
            // - BasePath     i:\Data                                       (Can be set and used as %BasePath%)
            // - GddPath      %BasePath%\Gdd\Compiled
            // - SrcPath      %BasePath%\Assets
            // - SubPath                                                    (AI, Boot, Levels, Menu\FrontEnd)
            // - DstPath      %BasePath%\Bin.%Platform%.%Target%
            // - PubPath      %BasePath%\Publish.%Platform%.%Target%
            // - ToolPath     %BasePath%\Tools
            //
            if (!BuildSystemConfig.Init(cmdLine.Name, cmdLine.Platform, cmdLine.Target, cmdLine.Territory, cmdLine.BasePath, cmdLine.SrcPath, cmdLine.GddPath, cmdLine.SubPath, cmdLine.DstPath, cmdLine.PubPath, cmdLine.ToolPath))
            {
                Console.WriteLine("Usage: -Name [Name]");
                Console.WriteLine("       -Platform [Platform]");
                Console.WriteLine("       -Target [Target]");
                Console.WriteLine("       -Territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -BasePath [BasePath]");
                Console.WriteLine("       -GddPath [GddPath]");
                Console.WriteLine("       -SrcPath [SrcPath]");
                Console.WriteLine("       -SubPath [SubPath]");
                Console.WriteLine("       -DstPath [DstPath]");
                Console.WriteLine("       -PubPath [PubPath]");
                Console.WriteLine("       -ToolPath [ToolPath]");
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

