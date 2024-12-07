using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    internal static class ObjectTreeWalker
    {
        public delegate bool OnObjectDelegate(object compound);

        public static bool Walk(object compound, OnObjectDelegate ood)
        {
            try
            {
                Stack<object> compounds = new();
                compounds.Push(compound);

                while (compounds.Count > 0)
                {
                    compound = compounds.Pop();
                    var compoundTypeInfo = compound.GetType();

                    if (ood(compound))
                        continue;

                    if (compound is GameData.IExternalObjectProvider)
                    {
                        var externalObjectProvider = compound as GameData.IExternalObjectProvider;
                        compounds.Push(externalObjectProvider.extobject);
                        continue;
                    }

                    if (compound is GameData.IDataCompiler)
                        continue;

                    if (compoundTypeInfo.IsArray)
                    {
                        // Analyze element type
                        var elementType = compoundTypeInfo.GetElementType();
                        if (!elementType.IsPrimitive && !compoundTypeInfo.IsEnum)
                        {
                            var objectArray = compound as Array;
                            if (objectArray != null)
                            {
                                for (var i = 0; i < objectArray.Length; i++)
                                {
                                    var e = objectArray.GetValue(i);
                                    if (e != null)
                                        compounds.Push(e);
                                }
                            }
                        }
                    }
                    else
                    {
                        var fields = compoundTypeInfo.GetFields(
                            BindingFlags.Public
                                | BindingFlags.NonPublic
                                | BindingFlags.Instance
                                | BindingFlags.GetField
                        );
                        foreach (var f in fields)
                        {
                            var o = f.GetValue(compound);
                            if (o == null)
                                continue;

                            var objectTypeInfo = o.GetType();
                            if (objectTypeInfo.IsArray)
                            {
                                // Analyze element type
                                var elementType = objectTypeInfo.GetElementType();
                                if (!elementType.IsPrimitive && !compoundTypeInfo.IsEnum)
                                {
                                    var objectArray = o as Array;
                                    if (objectArray != null)
                                    {
                                        for (var i = 0; i < objectArray.Length; i++)
                                        {
                                            var e = objectArray.GetValue(i);
                                            if (e != null)
                                                compounds.Push(e);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!objectTypeInfo.IsPrimitive)
                                    compounds.Push(o);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public sealed class GameDataData
    {
        private IDataUnit mRoot;

        private List<IDataCompiler> mCompilers;
        private List<IFileIdProvider> mFilesProviders;

        public GameDataData(Assembly dataAssembly)
        {
            Assembly = dataAssembly;
            mCompilers = new();
            mFilesProviders = new();
        }

        public Assembly Assembly { get; }

        public IDataUnit Root
        {
            get { return mRoot; }
        }

        private bool Instanciate()
        {
            if (mRoot != null)
                return true;

            try
            {
                mRoot = AssemblyUtil.Create1<IDataUnit>(Assembly);
                return mRoot != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public List<IDataCompiler> CollectDataCompilers()
        {
            var compilers = new List<IDataCompiler>();
            if (Instanciate())
            {
                // Collect:
                // - Compilers
                // - FileId providers
                // - Bigfile providers
                ObjectTreeWalker.Walk(mRoot, delegate (object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataCompiler)
                        {
                            var c = compound as IDataCompiler;
                            compilers.Add(c);
                            return true;
                        }
                        return false;
                    }
                );
            }
            return compilers;
        }

        public void PrepareFilesProviders(List<IDataCompiler> compilers)
        {
            //    PrevSignature = Signature
            if (compilers.Count == 0)
                return;

            var memoryStream = new MemoryStream();
            var memoryWriter = new BinaryMemoryWriter();

            var signatureList = new List<KeyValuePair<Hash160, IDataCompiler>>(compilers.Count);
            foreach (var cl in compilers)
            {
                memoryWriter.Reset();
                var compilerType = cl.GetType();
                var compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                compilerTypeSignature.WriteTo(memoryWriter);
                cl.CompilerSignature(memoryWriter);
                var compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                signatureList.Add(new KeyValuePair<Hash160, IDataCompiler>(compilerSignature, cl));
            }
            int Comparer(KeyValuePair<Hash160, IDataCompiler> lhs, KeyValuePair<Hash160, IDataCompiler> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }
            signatureList.Sort(Comparer);

            var index = (uint)0;
            var prevSignature = signatureList[0].Key;
            foreach (var scl in signatureList)
            {
				var filesProvider = scl.Value.CompilerFileIdProvider;
                filesProvider.FileIndex = index;
                if (prevSignature != scl.Key)
                    index++;
                prevSignature = scl.Key;
            }
        }

        public bool FinalizeDataCompilation()
        {
            mFilesProviders.Clear();

            var ok = ObjectTreeWalker.Walk(mRoot, delegate (object compound)
                {
                    var compoundType = compound.GetType();

                    if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                        return true;

                    var handled = false;
                    if (compound is IFileIdProvider f)
                    {
                        mFilesProviders.Add(f);
                        handled = true;
                    }

                    return handled;
                }
            );


            return true;
        }

        private void GenerateCppCodeAndData(object data, string dataFilename, string codeFilename, string relocFilename)
        {
            try
            {
                CppCodeStream.Write2(BuildSystemCompilerConfig.Platform, data, dataFilename, codeFilename, relocFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        private void GenerateStdData(object data, string dataFilename, string relocFilename)
        {
            // Generate the generic data
            try
            {
                // var stdDataStream = new StdDataStream(BuildSystemCompilerConfig.Platform);
                // StdDataStream.SizeOfBool = BuildSystemCompilerConfig.SizeOfBool;
                // stdDataStream.Write(BuildSystemCompilerConfig.Platform, data, dataFilename, relocFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        public bool Save(string filepath)
        {
            //generateCppCodeAndData(root, fullNameWithoutExtension + ".rdf", fullNameWithoutExtension + ".rcf", fullNameWithoutExtension + ".rrf");
            var dataFilename = Path.ChangeExtension(filepath, BuildSystemCompilerConfig.DataFileExtension);
            var relocFilename = Path.ChangeExtension(filepath, BuildSystemCompilerConfig.DataRelocFileExtension);

            FileCommander.createDirectoryOnDisk(Path.GetDirectoryName(dataFilename));

            GenerateStdData(mRoot, dataFilename, relocFilename);
            GenerateCppCodeAndData(mRoot, dataFilename + "c", Path.ChangeExtension(dataFilename, ".h"), relocFilename + "c");
            return true;
        }
    }
}
