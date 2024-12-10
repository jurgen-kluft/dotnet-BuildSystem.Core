using System.Reflection;
using System.Runtime.Loader;

using GameCore;
using BigfileBuilder;
using GameData;

namespace DataBuildSystem
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

        // -name track3 -platform PC -target PC -territory Europe -srcpath i:\Data\Assets -gddpath i:\Data\Gdd -subpath Tracks\Track3 -dstpath i:\Data\Bin.PC -pubpath i:\Data\Publish.PC -toolpath i:\Data\Tools
        // -name MJ -platform PC -territory Europe -srcpath i:\Data\Assets -pubPath i:\Data\Publish.%PLATFORM% -dstpath i:\Data\Bin.%PLATFORM% -toolpath i:\Data\Tools

        // -name MJ -platform PC -territory Europe -srcpath E:\GameData\Assets -gddpath E:\GameData\Dlls -dstpath E:\GameData\Bin.%PLATFORM% -pubPath E:\GameData\Publish.%PLATFORM% -toolpath E:\GameData\Tools
        // -name "MJ" -platform "MAC" -territory "Europe" -basepath "/Users/obnosis1" -srcpath "/Users/obnosis1/Data/Assets" -gddpath "/Users/obnosis1/Data/GameData" -pubpath "/Users/obnosis1/Data/Publish.%PLATFORM%" -dstpath "/Users/obnosis1/Data/Bin.%PLATFORM%" -toolpath "/Users/obnosis1/Data/Tools"
        static int Main(string[] args)
        {
            var cmdLine = new CommandLine(args);

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
            if (!BuildSystemCompilerConfig.Init(cmdLine["name"], cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["basepath"], cmdLine["srcpath"], cmdLine["gddpath"], cmdLine["subpath"], cmdLine["dstpath"], cmdLine["pubpath"], cmdLine["toolpath"]))
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
            Console.WriteLine("------ DataBuildSystem.NET - GameDataCompiler: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemCompilerConfig.PlatformName, BuildSystemCompilerConfig.TargetName);

            // Record the total build time
            var buildStart = DateTime.Now;

            // Create the destination, gdd and publish output paths
            DirUtils.Create(BuildSystemCompilerConfig.DstPath);
            DirUtils.Create(BuildSystemCompilerConfig.PubPath);

            var gameDataRootDllName = "GameData.DLL";
            Console.WriteLine("dll path: " + BuildSystemCompilerConfig.GddPath + "/" + gameDataRootDllName);

            // Using 'AssemblyLoadContext' so that we can also Unload the GameData DLL
            var gdus = new GameDataUnits();

            var gameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            var dllBytes = File.ReadAllBytes(BuildSystemCompilerConfig.GddPath + "/" + gameDataRootDllName);
            var gameDataAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));

            // BuildSystem.DataCompiler configuration
            var configsForCompiler = AssemblyUtil.CreateN<IBuildSystemCompilerConfig>(gameDataAssembly);
            if (configsForCompiler.Length > 0)
            {
                foreach (var config in configsForCompiler)
                {
                    if (config.Platform == BuildSystemCompilerConfig.PlatformName)
                    {
                        BuildSystemCompilerConfig.Init(config);
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unable to find a 'BuildSystemCompilerConfig' for {BuildSystemCompilerConfig.PlatformName} -- error");
                return Error();
            }

            // Bigfile configuration
            var configsForBigfileBuilder = AssemblyUtil.CreateN<IBigfileConfig>(gameDataAssembly);
            if (configsForBigfileBuilder.Length > 0)
            {
                foreach (var config in configsForBigfileBuilder)
                {
                    if (config.Platform.HasFlag(BuildSystemCompilerConfig.Platform))
                    {
                        BigfileConfig.Init(config);
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Unable to find a 'IBigfileConfig' for {BuildSystemCompilerConfig.PlatformName} -- error");
                return Error();
            }

            GameDataPath.BigFileExtension = BigfileConfig.BigFileExtension;
            GameDataPath.BigFileTocExtension = BigfileConfig.BigFileTocExtension;
            GameDataPath.BigFileFdbExtension = BigfileConfig.BigFileFdbExtension;
            GameDataPath.BigFileHdbExtension = BigfileConfig.BigFileHdbExtension;

            var start = DateTime.Now;
            Console.WriteLine("------ Initializing data compilation units");
            gdus.Load(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.GddPath);
            var end = DateTime.Now;
            Console.WriteLine("Finished initialization -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("------ Data compilation started: {0}", BuildSystemCompilerConfig.Name);
            gdus.Update(BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.DstPath);
            end = DateTime.Now;
            Console.WriteLine("Data compilation complete -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            gdus.Save(BuildSystemCompilerConfig.DstPath);
            Console.WriteLine("Finished -- Total build time {0}s", (DateTime.Now - buildStart).TotalSeconds);

            gameDataAssemblyContext.Unload();

            return Success();
        }
    }
}

