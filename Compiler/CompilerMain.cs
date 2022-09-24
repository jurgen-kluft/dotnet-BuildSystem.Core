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
            // - gddpath      I:\Dev\Game\Data\Data\Compiled
            // - srcpath      I:\Dev\Game\Data\Assets
            // - subpath      AI                                            (AI, Boot, Levels, Menu\FrontEnd)
            // - dstpath      %SRCPATH%\Bin.%PLATFORM%.%TARGET%
            // - deppath      %SRCPATH%\Dep.%PLATFORM%.%TARGET%
            // - pubpath      %SRCPATH%\Publish.%PLATFORM%.%TARGET%
            // - toolpath     I:\Dev\Game\Data\Tools
            if (!BuildSystemCompilerConfig.Init(cmdLine["name"], cmdLine.HasParameter("bigfile"), cmdLine["platform"], cmdLine["target"], cmdLine["territory"], cmdLine["srcpath"], cmdLine["subpath"], cmdLine["dstpath"], cmdLine["deppath"], cmdLine["toolpath"], cmdLine["pubpath"]))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -target [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -subpath [SUBPATH]");
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

            // The dynamic instantiation helper class needs the data assembly to find classes by name and construct them.
            GameData.Instanciate.assembly = dataAssemblyManager.assembly;

            // TODO  Write up full design with all possible cases of data modification

            // A 'Data Unit' consists of (.GDU):
            //     - Unique Hash
            //     - Index
            //     - 'Game Data DLL' (.DLL)
            //     - 'Game Data Compiler Log' (.GDL)
            //     - 'Game Data Bigfile/TOC/Filename/Hashes' (.BFN, .BFH, .BFT, .BFD)

            // datapath   = path with all the gamedata DLL's
            // srcpath    = path containing all the 'intermediate' assets (TGA, PGN, TRI, processed FBX files)
            // dstpath    = path containing all the 'cooked' assets and databases
            // pubpath    = path where all the 'Game Data' files and Bigfiles will be written (they are also written in the dstpath)

            // Collect all Game Data DLL's that need to be processed

            // Need a database that can map from Hash -> Index
            // There is a dependency on this database by the generation of FileId's.
            // If this file is deleted then ALL Game Data and Bigfiles have to be regenerated.
            // The pollution of this database with stale items is ok, it does not impact memory usage.
            // It mainly results in empty bigfile sections, each of them being an offset of 4 bytes.

            // DataUnit can be saved and loaded from a BinaryFile
            // So we can create a DataUnits.slog file.

            //
            // Foreach 'Game Data DLL' construct/use-existing DataUnit
            //    Associated with a 'Game Data DLL'
            //    Generate the hash for the DataUnit (from the name of the DLL)
            //    
            //
            // Sort DataUnits by Hash
            // Foreach DataUnit
            //    Register the hash and get the index    
            //
            // Foreach DataUnit
            //    Check the date-time and size signature of:
            //       - 'Game Data DLL'
            //       - 'Game Data Compiler Log'
            //       - 'Game Data File' and 'Game Data Relocation File'
            //       - 'BigFile Toc/Filename/Hash Files'
            //       - Check if source files have changed
           
            //    If all up-to-date then done
            //    Else
            //       Case A:
            //           - 'BigFile Toc/Filename/Hash Files' is out-of-date/missing
            //           - Load 'Game Data Compiler Log'
            //           - Using 'Game Data Compiler Log' check if all 'source' files are up-to-date
            //             - If not up-to-date execute 'Game Data Compiler Log'
            //           - Build a database of Hash-FileId, sort Hashes and assign FileId
            //           - Load the 'Game Data DLL', inject with GameCore and GameCode
            //              - Find IDataRoot object
            //              - Instanciate the root object
            //              - Hand-out all the FileId's
            
            //       Case B:
            //           - 'Game Data DLL' is out-of-date
            //              - Load the 'Game Data DLL', inject with GameCore and GameCode
            //              - Find IDataRoot object
            //              - Instanciate the root object
            //              - Collect all IDataCompiler objects
            //              - Load 'Game Data Compiler Log'
            //                - See if there are any missing/added/changed IDataCompiler objects
            //                - So a IDataCompiler needs to build a unique Hash of itself!
            //                - Save 'Game Data Compiler Log'

            //       Case C:
            //           - 'Game Data Compiler Log' is out-of-date or missing
            //           - This is bad, we have lost our source to target dependency information
            //           - So we have to rebuild the log and all the data
            //           - And after that the 'Game Data File' and 'Game Data Relocation File' and
            //             'Game Data File' and 'Game Data Relocation File'.

            //       Case D:
            //           - 'Game Data File' and 'Game Data Relocation File' are out-of-date or missing
            //           - Using 'Game Data Compiler Log' check if all 'source' files are up-to-date
            //           - If any source file is out-of-date 
            //             - Execute 'Game Data Compiler Log'
            //           - Build a database of Hash-FileId, sort Hashes and assign FileId
            //           - Load the 'Game Data DLL', inject with GameCore and GameCode
            //              - Find IDataRoot object
            //              - Instanciate the root object
            //              - Hand-out all the FileId's
            //              - Save 'Game Data File' and 'Game Data Relocation File'

            // Note: We could mitigate this by adding full dependency information as a file header of each target file.

            // Get the GameData.Root assembly, in there we should have all the configurations
            Assembly gameDataRootAssembly = Assembly.LoadFile(BuildSystemCompilerConfig.DataFileExtension

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


                        // For each 'Data Unit' we need to detect if any of the source assets that we are dependent on
                        // have changed. We can do this with an up-to-date 'Game Data Compiler Log'.

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

