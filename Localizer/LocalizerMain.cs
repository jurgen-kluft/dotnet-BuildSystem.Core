using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using GameCore;

namespace DataBuildSystem
{
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

        /// <summary>
        /// Convert a set of excel localization files into a text format based file containing the ID table
        /// DependencySystem is used to detect changes in INPUT and OUTPUT files to minimize building time.
        /// </summary>
        static int Main(string[] args)
        {
            CommandLine cmdLine = new (args);

            // On the command-line we have:
            // - Name         MyGame                                        Name of the project
            // - Platform     NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - Territory    Europe                                        (Europe/USA/Asia/Japan)
            // - Config       Config.%PLATFORM%.cs                          (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - SrcPath      I:\Dev\Game\Data\Data
            // - Excel0       Localization\Localization1.xls                Mandatory
            // - Excel*       Localization\Localization*.xls                Optional
            // - Worksheet0   name
            // - Worksheet*   name
            // - DstPath      I:\Dev\Game\Data\Data\Bin.NDS
            // - DepPath      I:\Dev\Game\Data\Data\Dep.NDS
            // - PublishPath  I:\Dev\Game\Data\Data\Publish.NDS
            // - ToolPath     I:\Dev\Game\Data\Tools
            //
            // example:
            //     -name MJ -platform PC -territory Europe -config "Config.%PLATFORM%.cs" -srcpath "D:\Dev\.NET_BuildSystem\Data.Test" -excel0 "Loc\Localization.xls" -worksheets0 "Localization" -dstpath "%SRCPATH%\Bin.%PLATFORM%" -deppath "%SRCPATH%\Dep.%PLATFORM%" -pubpath "%SRCPATH%\Publish.%PLATFORM%" -toolpath "%SRCPATH%\Tools"
            //
            if (!LocalizerConfig.Init(cmdLine["name"], cmdLine["platform"], cmdLine["territory"], cmdLine["config"], cmdLine["srcpath"], cmdLine["excel0"], cmdLine["dstpath"], cmdLine["deppath"], cmdLine["pubpath"], cmdLine["toolpath"]))
            {
                Console.WriteLine("Usage: -platform [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -config [FILENAME]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -excel* [FILENAME]");
                Console.WriteLine("       -worksheets* [\"NAME,NAME,..\"]");
                Console.WriteLine("       -dstpath [DSTPATH]");
                Console.WriteLine("       -deppath [DEPPATH]");
                Console.WriteLine("       -pubpath [PUBLISHPATH]");
                Console.WriteLine("       -toolpath [TOOLPATH]");
                Console.WriteLine();
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Localizer: v{0} (Platform: {1}) ------", version, LocalizerConfig.PlatformName);

            // Referenced assemblies, we always include ourselves
            List<Filename> referencedAssemblies = new List<Filename>();
            cmdLine.CollectIndexedParams(0, true, "asm", delegate(string param) { referencedAssemblies.Add(new Filename(param)); });
            referencedAssemblies.Insert(0, new Filename(Assembly.GetExecutingAssembly().Location));

            // Build configuration object
            Assembly configDataAssembly = null;
            if (!LocalizerConfig.ConfigFilename.IsEmpty)
            {
                List<Filename> configSrcFiles = new List<Filename>();
                configSrcFiles.Add(new Filename(GameCore.Environment.expandVariables(LocalizerConfig.ConfigFilename)));
                Filename configAsmFilename = new Filename("Localizer" + "." + LocalizerConfig.ConfigFilename + ".dll");
                Filename configAssemblyFilename = configAsmFilename;
                configDataAssembly = AssemblyCompiler.Compile(configAssemblyFilename, configSrcFiles.ToArray(), new Filename[0], LocalizerConfig.SrcPath, LocalizerConfig.SubPath, LocalizerConfig.DstPath, LocalizerConfig.DepPath, referencedAssemblies.ToArray());
            }

            // Configuration for data build system compiler
            IBuildSystemCompilerConfig configForBuildSystemCompiler = AssemblyUtil.Create1<IBuildSystemCompilerConfig>(configDataAssembly);
            if (configForBuildSystemCompiler != null)
                BuildSystemCompilerConfig.Init(configForBuildSystemCompiler);

            // Configuration for dependency system
            //IDependencySystemConfig configForDependencySystem = AssemblyUtil.Create1<IDependencySystemConfig>(configDataAssembly);
            //if (configForDependencySystem!=null)
            //    DependencySystemConfig.Init(configForDependencySystem);

            // Configuration for localizer
            IBuildSystemLocalizerConfig buildSystemLocalizerConfig = AssemblyUtil.Create1<IBuildSystemLocalizerConfig>(configDataAssembly) ?? new BuildSystemLocalizerDefaultConfig();
            if (buildSystemLocalizerConfig!=null)
                LocalizerConfig.SetConfig(buildSystemLocalizerConfig);

            // Manually supplied additional excel files
            List<string> sourceFiles = new List<string>();
            cmdLine.CollectIndexedParams(0, true, "excel", delegate(string param) { sourceFiles.Add(param); });

            // Manually supplied worksheet names for every excel files
            List<string[]> worksheetNamesList = new List<string[]>();
            cmdLine.CollectIndexedParams(0, true, "worksheets", delegate(string param) { string[] names = param.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); worksheetNamesList.Add(names); });

            // Every excel file has these outputs:
            // 1) "filename.xls.ids"
            // 2) for every language in the "filename.xls" file a "filename.%LANGUAGE%.loc" file (binary) is exported
            // 3) a "filename.xls.sdep" file with the "filename.xls" file as INPUT and "filename.ids", "filename.%LANGUAGE%.loc" files as OUTPUT
            Localization.LocDatabase ldb = new Localization.LocDatabase();
            ldb.init(LocalizerConfig.Excel0);

            Localization.Builder[] builders = new Localization.Builder[sourceFiles.Count];

            bool result = true;
            bool anyModified = false;
            for (int i = 0; i < sourceFiles.Count; ++i)
            {
                string[] worksheetNames;
                if (i < worksheetNamesList.Count)
                    worksheetNames = worksheetNamesList[i];
                else
                    worksheetNames = new string[] { "main" };
                builders[i] = new Localization.Builder(sourceFiles[i], worksheetNames);

                result = builders[i].init(ldb);
                if (result)
                {
                    if (builders[i].isModified)
                    {
                        anyModified = true;
                        // if .sdep says it is NOT up-to-date
                        // 1) load "filename.xls" file
                        // 2) parse worksheets
                        // 3) build ID and Language string tables
                        // 4) write .ids file through LocDatabase
                        // 5) write all .loc files through LocDatabase
                        // 6) write .sdep file
                        result = builders[i].build(ldb);
                    }
                }

                if (!result)
                    break;
            }

            if (result)
            {
                // Is any of the sources modified, if not we can terminate successfully here
                if (!ldb.isModified && !anyModified)
                    return Success();

                // Some source has been rebuild, now we need to load all of them and save the combined stuff.
                for (int i = 0; i < sourceFiles.Count; ++i)
                {
                    // Add all the data from the builders that where already up-to-date
                    // 1) load "filename.ids" file
                    // 2) load all "filename.%LANGUAGE%.loc" files
                    if (!builders[i].isModified)
                    {
                        result = builders[i].load(ldb);
                        if (!result)
                            return Error();
                    }
                }
                return (ldb.save()) ? Success() : Error();
            }
            else
            {
                return Error();
            }
        }


    }
}
