using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using GameCore;
namespace DataBuildSystem
{
    public static class AssemblyCompiler
    {
        private static void Include(Filename _csincludeFilename, List<Filename> collectedCsFiles)
        {
            try
            {
                xTextStream ts = new xTextStream(_csincludeFilename);
                if (ts.Open(xTextStream.EMode.READ))
                {
                    while (!ts.read.EndOfStream)
                    {
                        string filename = ts.read.ReadLine();
                        collectedCsFiles.Add(new Filename(filename));
                    }
                    ts.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Include(Filename[] _csincludeFilenames, List<Filename> collectedCsFiles)
        {
            foreach (Filename f in _csincludeFilenames)
                Include(f, collectedCsFiles);
        }

        public static Assembly Compile(Filename filenameOfAssembly, Filename[] files, Filename[] csincludes, Dirname srcPath, Dirname subPath, Dirname dstPath, Dirname depPath, Filename[] referencedAssemblies)
        {
            List<Filename> collectedCsFiles = new List<Filename>();
            Include(csincludes, collectedCsFiles);

            DepFile depFile = new DepFile(subPath + filenameOfAssembly, dstPath);
            bool isModified = true;
            if (depFile.load(depPath))
            {
                isModified = false;
                foreach (Filename f in files)
                {
                    if (!depFile.hasIn(srcPath + f))
                    {
                        isModified = true;
                        break;
                    }
                }
                if (!isModified)
                {
                    foreach (Filename f in csincludes)
                    {
                        if (!depFile.hasIn(srcPath + f))
                        {
                            isModified = true;
                            break;
                        }
                    }
                }
                if (!isModified)
                {
                    foreach (Filename f in collectedCsFiles)
                    {
                        if (!depFile.hasIn(srcPath + f))
                        {
                            isModified = true;
                            break;
                        }
                    }
                }

                if (!isModified)
                {
                    foreach (Filename f in referencedAssemblies)
                    {
                        if (!depFile.hasIn(f))
                        {
                            isModified = true;
                            break;
                        }
                    }
                }

                if (!isModified)
                {
                    isModified = depFile.isModified();
                }
            }

            Assembly assembly = null;
            if (isModified)
            {
                // Create dependency file
                depFile = new DepFile(subPath + filenameOfAssembly, dstPath);
                depFile.main.Rule = DepInfo.EDepRule.MUST_EXIST;             /// The main file must exist, if it doesn't we need to try and build it again!

                foreach (Filename f in files)
                    depFile.addIn(f, srcPath);
                foreach (Filename f in csincludes)
                    depFile.addIn(f, srcPath);
                foreach (Filename f in collectedCsFiles)
                    depFile.addIn(f, srcPath);

                // The referenced assemblies are using absolute paths
                foreach (Filename f in referencedAssemblies)
                    depFile.addIn(f, Dirname.Empty);

                Console.WriteLine("Compiling assembly {0}.", filenameOfAssembly);
                try
                {
                    List<Filename> sourceFilenames = new List<Filename>();
                    foreach (Filename f in files)
                        sourceFilenames.Add(f);
                    foreach (Filename f in collectedCsFiles)
                        sourceFilenames.Add(f);

                    // If this assembly did not change (dependency file?) then we do not need to build it
                    assembly = AssemblyUtil.CompileFromFiles(filenameOfAssembly, srcPath, subPath, sourceFilenames, dstPath, referencedAssemblies);
                    if (assembly != null)
                        Console.WriteLine("Finished compiling {0}.", filenameOfAssembly);
                    else
                        Console.WriteLine("Error compiling {0}.", filenameOfAssembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception {0} while loading assembly {0}.", e, filenameOfAssembly);
                }

                depFile.save(depPath);
                return assembly;
            }
            else
            {
                try
                {
                    Console.WriteLine("Loading assembly {0}.", filenameOfAssembly);
                    AssemblyName assemblyName = new AssemblyName();
                    assemblyName.CodeBase = dstPath + subPath + filenameOfAssembly;
                    assembly = Assembly.Load(assemblyName);
                    if (assembly != null)
                        Console.WriteLine("Finished loading {0}.", filenameOfAssembly);
                    else
                        Console.WriteLine("Error loading {0}.", filenameOfAssembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception {0} while loading assembly {0}.", e, filenameOfAssembly);
                    return null;
                }
            }
            return assembly;
        }
    }
}
