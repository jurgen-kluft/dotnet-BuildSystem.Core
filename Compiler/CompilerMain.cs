using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Loader;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    class Program
    {
        #region Error & Success

        static int Error()
        {
            return 1;
        }

        static int Success()
        {
            return 0;
        }

        #endregion
        #region Main

        // -name track3 -platform PC -target PC -territory Europe -srcpath i:\Data\Assets -gddpath i:\Data\Gdd -subpath Tracks\Track3 -dstpath i:\Data\Bin.PC -pubpath i:\Data\Publish.PC -toolpath i:\Data\Tools
        // -name MJ -platform PC -territory Europe -srcpath i:\Data\Assets -pubPath i:\Data\Publish.%PLATFORM% -dstpath i:\Data\Bin.%PLATFORM% -toolpath i:\Data\Tools

        // -name MJ -platform PC -territory Europe -srcpath E:\GameData\Assets -gddpath E:\GameData\Dlls -dstpath E:\GameData\Bin.%PLATFORM% -pubPath E:\GameData\Publish.%PLATFORM% -toolpath E:\GameData\Tools
        // -name "MJ" -platform "MAC" -territory "Europe" -basepath "/Users/obnosis1" -srcpath "/Users/obnosis1/Data/Assets" -gddpath "/Users/obnosis1/Data/GameData" -pubpath "/Users/obnosis1/Data/Publish.%PLATFORM%" -dstpath "/Users/obnosis1/Data/Bin.%PLATFORM%" -toolpath "/Users/obnosis1/Data/Tools"
        static int Main(string[] args)
        {
            CommandLine cmdLine = new CommandLine(args);

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

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Compiler: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemCompilerConfig.PlatformName, BuildSystemCompilerConfig.TargetName);

            // Record the total build time
            DateTime buildStart = DateTime.Now;

            // Create the destination, gdd and publish output paths
            DirUtils.Create(BuildSystemCompilerConfig.DstPath);
            DirUtils.Create(BuildSystemCompilerConfig.PubPath);

            string gameDataRootDllName = "GameData.Root.DLL";

            Console.WriteLine("dll path: " + BuildSystemCompilerConfig.GddPath + "/" + gameDataRootDllName);

            // Using 'AssemblyLoadContext' so that we can also Unload the GameData DLL
            AssemblyLoadContext gameDataAssemblyContext = new AssemblyLoadContext("GameData", true);
            byte[] dllBytes = File.ReadAllBytes(BuildSystemCompilerConfig.GddPath + "/" + gameDataRootDllName);
            Assembly gameDataRootAssembly = gameDataAssemblyContext.LoadFromStream(new MemoryStream(dllBytes));

            // BuildSystem.DataCompiler configuration
            IBuildSystemCompilerConfig[] configsForCompiler = AssemblyUtil.CreateN<IBuildSystemCompilerConfig>(gameDataRootAssembly);
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
            IBigfileConfig[] configsForBigfileBuilder = AssemblyUtil.CreateN<IBigfileConfig>(gameDataRootAssembly);
            if (configsForBigfileBuilder.Length > 0)
            {
                foreach (var config in configsForBigfileBuilder)
                {
                    if (config.Platform == BuildSystemCompilerConfig.Platform)
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

            gameDataAssemblyContext.Unload();

            // NOTES
            //       We should be able to generate the .sln and .csproj/Directory.Build.props files, so that we do not have to manage them
            //       and track them in source control.
            //       This does mean that each 'folder' in the Data folder is going to be a project or how else do we know where the generate
            //       a project?

            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            GameDataUnits gdus = new();

            start = DateTime.Now;
            Console.WriteLine("------ Initializing data compilation");
            gdus.Load(BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.GddPath);
            end = DateTime.Now;
            Console.WriteLine("Finished initialization -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            start = DateTime.Now;
            Console.WriteLine("------ Data compilation started: {0}", BuildSystemCompilerConfig.Name);
            gdus.Update(BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.DstPath);
            end = DateTime.Now;
            Console.WriteLine("Data compilation complete -- ok (Duration: {0}s)", (end - start).TotalSeconds);

            gdus.Save(BuildSystemCompilerConfig.DstPath);
            Console.WriteLine("Finished -- Total build time {0}s", (DateTime.Now - buildStart).TotalSeconds);

            return Success();
        }

        #endregion
    }
}

