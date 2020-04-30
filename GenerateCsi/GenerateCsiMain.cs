#region Copyright
/// 
/// BuildSystem.Data.Linker
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

using Core;

namespace DataBuildSystem
{
    // Vocabulary:
    // '.NET BuildSystem's DataCompiler' or in short 'DataCompiler'
    // '.NET BuildSystem's DataLinker' or in short 'DataLinker'

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

        //
        // Generate a .csi file containing paths to .cs files
        // 
        static int Main(string[] args)
        {
            CommandLine cmdLine = new CommandLine(args);

            // On the command-line we have:
            // - Name         Game
            // - Platform     NDS                                           (NDS/WII/PSP/PS2/PS3/XBOX/X360/PC)
            // - Territory    Europe                                        (Europe/USA/Asia/Japan)
            // - SrcPath      I:\Dev\Game\Data\Data
            // - SubPath      I:\Dev\Game\Data\Data
            // - DstPath      %SrcPath%\Bin.%PLATFORM%
            if (!BuildSystemGenerateCsiConfig.init(cmdLine["name"], cmdLine["platform"], cmdLine["territory"], cmdLine["srcpath"], cmdLine["subpath"]))
            {
                Console.WriteLine("Usage: -name [NAME]");
                Console.WriteLine("       -platform [PLATFORM]");
                Console.WriteLine("       -territory [Europe/USA/Asia/Japan]");
                Console.WriteLine("       -srcpath [SRCPATH]");
                Console.WriteLine("       -subpath [SUBPATH]");
                Console.WriteLine();
                Console.WriteLine("Press a key");
                Console.ReadKey();
                return 1;
            }

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("------ DataBuildSystem.NET - Generate .csi: v{0} (Platform: {1}) ------", version, BuildSystemGenerateCsiConfig.PlatformName);

            // Collect all the .cs files
            DirectoryScanner scanner = new DirectoryScanner(BuildSystemGenerateCsiConfig.SrcPath);
            scanner.scanSubDirs = true;
            scanner.collect(BuildSystemGenerateCsiConfig.SubPath, "*.cs", BuildSystemGenerateCsiConfig.SourceCodeFilterDelegate);

            // Write them out into a .csi file
            xTextStream ts = new xTextStream(BuildSystemGenerateCsiConfig.SrcPath + BuildSystemGenerateCsiConfig.SubPath + new Filename(BuildSystemGenerateCsiConfig.Name + ".csi"));

            if (ts.Open(xTextStream.EMode.WRITE))
            {
                foreach (Filename f in scanner.filenames)
                {
                    ts.write.WriteLine(f);
                }

                ts.Close();
            }

            return Success();
        }

    }
}

