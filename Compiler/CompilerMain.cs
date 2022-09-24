using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

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
        // -name MJ -platform PC -territory Europe -srcpath i:\Data\Assets -pubPath i:\Data\Publish.%PLATFORM% -dstpath i:\Data\Bin.%PLATFORM% -toolpath i:\Data\Tools

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
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -gddpath [GDDPATH]");
                Console.WriteLine("       -subpath [SUBPATH]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine();
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Compiler: v{0} (Platform: {1}, Target: {2}) ------", version, BuildSystemCompilerConfig.PlatformName, BuildSystemCompilerConfig.TargetName);

            // Record the total build time
            DateTime buildStart = DateTime.Now;

            // Create the destination, gdd and publish output paths
            DirUtils.Create(BuildSystemCompilerConfig.DstPath);
            DirUtils.Create(BuildSystemCompilerConfig.GddPath);
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

            // TODO  Write up full design with all possible cases of data modification

            // A 'Game Data Unit' consists of (.GDU):
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
            Assembly gameDataRootAssembly = Assembly.LoadFile(Path.Join(BuildSystemCompilerConfig.GddPath, "GameData.Root.DLL"));
            Assembly configDataAssembly = gameDataRootAssembly;

            // The dynamic instantiation helper class needs the data assembly to find classes by name and construct them.
            GameData.Instanciate.assembly = gameDataRootAssembly;

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

                        // TODO need to implement the compiler streaming execution

                        // TODO Hash->FileId
                        //      Since in DEV mode we only need this per 'Game Data Unit' it should actually be sufficient to
                        //      just resolve this just before building the Bigfile.
                        //      Collect all the Hashes and assign them a FileId (Dictionary<Hash160, UInt64>)

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

