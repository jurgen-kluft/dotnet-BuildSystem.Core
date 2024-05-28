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
                var ts = new TextStream(_csincludeFilename);
                if (ts.Open(TextStream.EMode.Read))
                {
                    while (!ts.Reader.EndOfStream)
                    {
                        var filename = ts.Reader.ReadLine();
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
            foreach (var f in _csincludeFilenames)
                Include(f, collectedCsFiles);
        }

        public static Assembly Compile(Filename filenameOfAssembly, Filename[] files, Filename[] csincludes, Dirname srcPath, Dirname subPath, Dirname dstPath, Dirname depPath, Filename[] referencedAssemblies)
        {
            var collectedCsFiles = new List<Filename>();
            Include(csincludes, collectedCsFiles);

            var depFile = new DepFile(subPath + filenameOfAssembly, dstPath);
            var isModified = true;
            if (depFile.load(depPath))
            {
                isModified = false;
                foreach (var f in files)
                {
                    if (!depFile.hasIn(srcPath + f))
                    {
                        isModified = true;
                        break;
                    }
                }
                if (!isModified)
                {
                    foreach (var f in csincludes)
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
                    foreach (var f in collectedCsFiles)
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
                    foreach (var f in referencedAssemblies)
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

                foreach (var f in files)
                    depFile.addIn(f, srcPath);
                foreach (var f in csincludes)
                    depFile.addIn(f, srcPath);
                foreach (var f in collectedCsFiles)
                    depFile.addIn(f, srcPath);

                // The referenced assemblies are using absolute paths
                foreach (var f in referencedAssemblies)
                    depFile.addIn(f, Dirname.Empty);

                Console.WriteLine("Compiling assembly {0}.", filenameOfAssembly);
                try
                {
                    var sourceFilenames = new List<Filename>();
                    foreach (var f in files)
                        sourceFilenames.Add(f);
                    foreach (var f in collectedCsFiles)
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
                    var assemblyName = new AssemblyName();
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
