using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace GameCore
{
    public static class AssemblyUtil
    {
        #region Build assembly from .csproj

        /// <summary>
        /// Collects all .cs files in the Config.SrcPath
        /// </summary>
        /// <returns></returns>
        public static bool BuildAssemblyFromCsProj(string csProjFilename)
        {
            try
            {
				var success = false;

				// Load .csproj file as text, extract:
				// - all references
				// - all .cs files
				//
				// Call -> BuildAssemblyDirectly(Filename assemblyFilename, List<Filename> sourceFilenames, List<Filename> referencedAssemblies, bool inMemory)

				return success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static Assembly Load(string filename)
        {
            var assembly = Assembly.LoadFile(filename);
            return assembly;
        }

        #endregion
        #region Build assembly directly from .cs files

        /// <summary>
        /// Build an assembly directly from .cs files
        /// </summary>
        /// <returns></returns>
        public static Assembly BuildAssemblyDirectly(Filename sourceFilename)
        {
            var referencedAssemblies = new List<Filename>();
            referencedAssemblies.Add(new Filename(Assembly.GetExecutingAssembly().Location));
            var sourceFilenames = new List<Filename>();
            sourceFilenames.Add(sourceFilename);
            return BuildAssemblyDirectly(Filename.Empty, sourceFilenames, referencedAssemblies, true);
        }

        public static Assembly BuildAssemblyDirectly(List<Filename> sourceFilenames)
        {
            var referencedAssemblies = new List<Filename>();
            referencedAssemblies.Add(new Filename(Assembly.GetExecutingAssembly().Location));
            return BuildAssemblyDirectly(Filename.Empty, sourceFilenames, referencedAssemblies, true);
        }

        public static Assembly BuildAssemblyDirectly(Filename assemblyFilename, List<Filename> sourceFilenames)
        {
            var referencedAssemblies = new List<Filename>();
            referencedAssemblies.Add(new Filename(Assembly.GetExecutingAssembly().Location));
            return BuildAssemblyDirectly(assemblyFilename, sourceFilenames, referencedAssemblies, false);
        }

        public static Assembly BuildAssemblyDirectly(Filename assemblyFilename, List<Filename> sourceFilenames, List<Filename> referencedAssemblies)
        {
            return BuildAssemblyDirectly(assemblyFilename, sourceFilenames, referencedAssemblies, false);
        }

        public static Assembly BuildAssemblyDirectly(Filename assemblyFilename, List<Filename> sourceFilenames, List<Filename> referencedAssemblies, bool inMemory)
        {
			try
			{
				var parsedCode = new List<SyntaxTree>(sourceFilenames.Count);
				foreach (var csfile in sourceFilenames)
				{
                    string source_filename = csfile;
                    var source_code = File.ReadAllText(source_filename);
					var parsed_code = CSharpSyntaxTree.ParseText(source_code);
					parsedCode.Add(parsed_code);
				}

				var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true, optimizationLevel: OptimizationLevel.Release, generalDiagnosticOption: ReportDiagnostic.Error);

                var references = new List<MetadataReference>();
                references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                referencedAssemblies.ForEach(l => references.Add(MetadataReference.CreateFromFile(path: l)));

                var compilation = CSharpCompilation.Create(
						"_" + Guid.NewGuid().ToString("D"),
						references: references,
						syntaxTrees: parsedCode,
						options: compilerOptions
				);
				using (var ms = new MemoryStream())
				{
					var compilationResult = compilation.Emit(ms);

					foreach(var diagnostic in compilationResult.Diagnostics)
					{
						Console.WriteLine(diagnostic.ToString());
					}

					if (compilationResult.Success)
					{
						ms.Seek(0, SeekOrigin.Begin);
						return Assembly.Load(ms.ToArray());
					}

					// Assembly could not be created! -> compilationResult
					return null;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
        }

        [STAThread]
        public static Assembly CompileFromFiles(Filename filenameOfAssembly, Dirname srcPath, Dirname subPath, List<Filename> files, Dirname dstPath, Filename[] referencedAssemblies)
        {
            try
            {
                Assembly assembly = null;
                {
                    var filesAbsolutePath = new List<Filename>();
                    files.ForEach(f => filesAbsolutePath.Add(srcPath + f));

                    var referencedAssembliesAbsolutePath = new List<Filename>();
                    foreach(var f in referencedAssemblies)
                    {
                        if (f.IsAbsolute)
                            referencedAssembliesAbsolutePath.Add(f);
                        else
                            referencedAssembliesAbsolutePath.Add(dstPath + f);
                    }
                    referencedAssembliesAbsolutePath.Add(new Filename(Assembly.GetExecutingAssembly().Location));

                    assembly = AssemblyUtil.BuildAssemblyDirectly(dstPath + subPath + filenameOfAssembly, filesAbsolutePath, referencedAssembliesAbsolutePath);
                }
                return assembly;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        #endregion

        #region HasGenericInterface<T>

        public static bool HasGenericInterface<T>(Type interfaceType) where T : class
        {
            var baseTypes = typeof(T).GetType().GetInterfaces();
            foreach (var t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        public static bool HasGenericInterface(Type objectType, Type interfaceType)
        {
            var baseTypes = objectType.GetInterfaces();
            foreach (var t in baseTypes)
                if (t == interfaceType)
                    return true;
            return false;
        }

        #endregion
        #region Object/Objects instanciation

        /// <summary>
        /// Construct all objects derived from interface T
        /// </summary>
        /// <returns></returns>
        public static T[] CreateN<T>(Assembly assembly) where T : class
        {
            if (assembly == null)
                return null;
            try
            {
                var objects = new List<T>();

                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (HasGenericInterface(t, typeof(T)))
                    {
                        var o = assembly.CreateInstance(t.FullName) as T;
                        objects.Add(o);
                    }
                }
                return objects.ToArray();
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static T Create1<T>(Assembly assembly) where T : class
        {
            if (assembly == null)
                return null;
            try
            {
                var objects = new List<T>();

                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (HasGenericInterface(t, typeof(T)))
                    {
                        var o = assembly.CreateInstance(t.FullName) as T;
                        return o;
                    }
                }
                return null;
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        #endregion
    }
}
