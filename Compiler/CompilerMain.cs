#region Copyright
/// 
/// BuildSystem.DataCompiler
/// Copyright (C) 2009 J.J.Kluft
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
/// 
/// You should have received a copy of the GNU General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
    using Int8 = SByte;
    using UInt8 = Byte;

    /// <summary>
    /// 
    /// A separate application that loads the MJ.bfn and:
    /// 
    ///   1) Either generates one single BigfileToc
    ///   2) Both generates a BigfileToc and a Bigfile plus resolves all FileId's to an index
    ///   
    /// </summary>

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

        // -name track3 -platform PC -target PC -territory Europe -srcpath i:\HgDev\.NET_BuildSystem\Data.Test -folder Tracks\Track3 -config Config.%PLATFORM%.cs -dstpath i:\HgDev\.NET_BuildSystem\Data.Test\Bin.PC -deppath i:\HgDev\.NET_BuildSystem\Data.Test\Dep.PC -toolpath i:\HgDev\.NET_BuildSystem\Data.Test\Tools -pubpath i:\HgDev\.NET_BuildSystem\Data.Test\Publish.PC
        // -name MJ -platform PC  -territory Europe -srcpath i:\HgDev\.NET_BuildSystem\Data.Test -file root.cs -config Config.%PLATFORM%.cs -pubPath %SRCPATH%\Publish.%PLATFORM% -dstpath %SRCPATH%\Bin.%PLATFORM% -deppath %SRCPATH%\Dep.%PLATFORM% -toolpath i:\HgDev\.NET_BuildSystem\Data.Test\Tools
        // -name MJ -platform PC  -territory Europe -srcpath D:\Dev\.NET_BuildSystem\Data.Test -file root.cs -config Config.%PLATFORM%.cs -pubPath %SRCPATH%\Publish.%PLATFORM% -dstpath %SRCPATH%\Bin.%PLATFORM% -deppath %SRCPATH%\Dep.%PLATFORM% -toolpath %SRCPATH%\Data.Test\Tools

        static int Main(string[] args)
        {
            CommandLine cmdLine = new CommandLine(args);

            // On the command-line we have:
            // - platform     NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - target       NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - name         Game
            // - territory    Europe                                        (Europe/USA/Asia/Japan)
            // - config       Config.%PLATFORM%.cs                          Configuration objects for BuildSystem, DependencySystem and BigfileBuilder
            // - srcpath      I:\Dev\Game\Data\Data
            // - file0        root.cs
            // - file1        bases\concepts.cs
            // - subpath      AI                                            (AI, Boot, Levels, Menu\FrontEnd)
            // - dstpath      %SRCPATH%\Bin.%PLATFORM%.%TARGET%
            // - deppath      %SRCPATH%\Dep.%PLATFORM%.%TARGET%
            // - pubpath      %SRCPATH%\Publish.%PLATFORM%.%TARGET%
            // - toolpath     I:\Dev\Game\Data\Tools
            if (!BuildSystemCompilerConfig.Init(cmdLine["name"], cmdLine["config"], cmdLine.HasParameter("bigfile"), cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["srcpath"], cmdLine["subpath"], cmdLine["dstpath"], cmdLine["deppath"], cmdLine["toolpath"], cmdLine["pubpath"]))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -config [FILENAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -target [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -subpath [SUBPATH]");
                Console.WriteLine("       -file* [FILENAME]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -deppath [DSTDEPPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine();
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Compiler: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemCompilerConfig.PlatformName, BuildSystemCompilerConfig.TargetName);

            // Record the total build time
            DateTime buildStart = DateTime.Now;

            // Create the destination, dependency and publish output paths
            DirUtils.Create(BuildSystemCompilerConfig.DstPath);
            DirUtils.Create(BuildSystemCompilerConfig.DepPath);
            DirUtils.Create(BuildSystemCompilerConfig.PubPath);

            // Referenced assemblies, we always include ourselves
            // Inject ourselves with the referenced assemblies
            {
                List<Filename> referencedAssemblies = new List<Filename>();
                cmdLine.CollectIndexedParams(0, true, "asm", delegate (string param) { referencedAssemblies.Add(new Filename(param)); });
                BuildSystemCompilerConfig.AddReferencedAssemblies(referencedAssemblies);
                foreach (Filename assemblyFilename in referencedAssemblies)
                {
                    AssemblyName assemblyName = new AssemblyName();
                    assemblyName.CodeBase = assemblyFilename;
                    Assembly asm = Assembly.Load(assemblyName);
                }
            }

            // Manually supplied source files and folders
            DataAssemblyManager dataAssemblyManager = new DataAssemblyManager();
            List<Filename> sourceFiles = new List<Filename>();
            List<Filename> csIncludes = new List<Filename>();
            cmdLine.CollectIndexedParams(0, true, "file", delegate (string param) { if (param.EndsWith(".csi")) csIncludes.Add(new Filename(param)); else sourceFiles.Add(new Filename(param)); });

            Filename dataAssemblyFilename = new Filename(BuildSystemCompilerConfig.Name + ".dll");
            bool dataAssemblyCompiledSuccesfully = dataAssemblyManager.compileAsm(dataAssemblyFilename, sourceFiles.ToArray(), csIncludes.ToArray(), BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, BuildSystemCompilerConfig.ReferencedAssemblies);
            if (!dataAssemblyCompiledSuccesfully)
                return Error();

            Assembly configDataAssembly = null;
            if (!BuildSystemCompilerConfig.ConfigFilename.IsEmpty)
            {
                List<Filename> configSrcFiles = new List<Filename>();
                configSrcFiles.Add(new Filename(GameCore.Environment.expandVariables(BuildSystemCompilerConfig.ConfigFilename)));
                Filename configAsmFilename = new Filename(BuildSystemCompilerConfig.Name + ".Compiler.Config.dll");
                Filename configAssemblyFilename = configAsmFilename;
                configDataAssembly = AssemblyCompiler.Compile(configAssemblyFilename, configSrcFiles.ToArray(), new Filename[0], BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.SubPath, BuildSystemCompilerConfig.DstPath, BuildSystemCompilerConfig.DepPath, BuildSystemCompilerConfig.ReferencedAssemblies);
            }

            // The dynamic instantiation helper class needs the data assembly to find classes by name and construct them.
            GameData.Instanciate.assembly = dataAssemblyManager.assembly;

            // BuildSystem.DataCompiler configuration
            IBuildSystemCompilerConfig[] configsForCompiler = AssemblyUtil.CreateN<IBuildSystemCompilerConfig>(configDataAssembly);
            if (configsForCompiler.Length > 0)
            {
                foreach (var config in configsForCompiler)
				{
                    if (config.Platform == cmdLine["platform"])
                        BuildSystemCompilerConfig.Init(config);
                }
            }

            // Bigfile configuration
            IBigfileConfig[] configsForBigfileBuilder = AssemblyUtil.CreateN<IBigfileConfig>(configDataAssembly);
            if (configsForBigfileBuilder.Length > 0)
			{
                foreach (var config in configsForBigfileBuilder)
                {
                    if (config.Platform == cmdLine["platform"])
                        BigfileConfig.Init(config);
                }
            }

            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            {
                {
                    Console.WriteLine("------ Initializing data compilation");
                    start = DateTime.Now;
                    bool dataCompilationInitializationResult = dataAssemblyManager.initializeDataCompilation();
                    end = DateTime.Now;
                    if (dataCompilationInitializationResult)
                    {
                        Console.WriteLine("Finished initialization -- ok (Duration: {0}s)", (end - start).TotalSeconds);

                        start = DateTime.Now;
                        Console.WriteLine("------ Data compilation started: {0}", BuildSystemCompilerConfig.Name);

                        // TODO; need to implement the compiler streaming execution

                        // TODO "Signature File Verification" of all 'Data Unit's

                        // TODO Hash->FileId
                        //      Since in DEV mode we only need this per 'Data Unit' it should actually be sufficient to
                        //      just resolve this just before building the Bigfile.
                        //      Collect all the Hashes and assign them a FileId (Dictionary<Hash160, int>)

                        // A 'Data Unit' consists of:
                        //     - 'Game Data DLL'
                        //     - 'Game Data Compiler Log'
                        //     - 'Game Data Bigfile' and its TOC etc..

                        // If all 'Data Unit's are verified then we have nothing to do.
                        // 
                        // If a 'Data Unit' was changed:
                        //     - if the 'Game Data Compiler Log' doesn't exist collect all Compilers from the game data DLL and 
                        //       generate a new 'Game Data Compiler Log'.
                        //     - Job; stream-in 'Game Data Compiler Log' and pushing into 'Compiler Queue'
                        //     - Job System that feeds of the 'Compiler Queue and executes and when finished pushes them to
                        //       a Job that will save and delete them.
                        //     - the 'Game Data Compiler Log' for saving is a Job that receives finished Compilers
                        // 

                        // Development: Do not merge all bigfiles into one

                        // Development: Game has a large list of all bigfiles, their hash, their filehandle (if opened).


                        end = DateTime.Now;
                        Console.WriteLine("Data compilation complete -- ok (Duration: {0}s)", (end - start).TotalSeconds);

                        start = DateTime.Now;
                        string resourceFilename = BuildSystemCompilerConfig.DataFilename(BuildSystemCompilerConfig.Name);
                        Console.WriteLine("------ Generating game data started: Project: {0}", resourceFilename);

                        // The resource data
                        if (!dataAssemblyManager.save(BuildSystemCompilerConfig.PubPath + BuildSystemCompilerConfig.SubPath, resourceFilename))
                        {
                            end = DateTime.Now;
                            Console.WriteLine("Generating game data finished -- error (Duration: {0}s)", (end - start).TotalSeconds);
                            return Error();
                        }

                        end = DateTime.Now;
                        Console.WriteLine("Generating game data finished -- ok (Duration: {0}s)", (end - start).TotalSeconds);
                    }
                    else
                    {
                        Console.WriteLine("Finished initializing data compilation -- error (Duration: {0}s)", (end - start).TotalSeconds);
                        return Error();
                    }
                }
            }

            DateTime buildEnd = DateTime.Now;
            Console.WriteLine("Finished -- Total build time {0}s", (buildEnd - buildStart).TotalSeconds);

            return Success();
        }

        #endregion
    }
}

