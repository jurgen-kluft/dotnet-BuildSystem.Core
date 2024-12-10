using System.Reflection;

using GameCore;
using GameData;

namespace DataBuildSystem
{
    internal static class ObjectTreeWalker
    {
        public delegate bool OnObjectDelegate(object compound);

        private static bool ElementRequiresWalking(Type type)
        {
            if (type != null && !type.IsPrimitive && !type.IsEnum)
            {
                return !TypeInfo2.HasGenericInterface(type, typeof(IDataUnit));
            }

            return false;
        }

        private static bool ObjectRequiresWalking(Type type)
        {
            if (type != null && !type.IsPrimitive && !type.IsEnum)
            {
                return !TypeInfo2.HasGenericInterface(type, typeof(IDataUnit));
            }

            return false;
        }

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

                    if (compound is IDataCompiler)
                        continue;

                    if (compoundTypeInfo.IsArray)
                    {
                        // Analyze element type
                        var elementType = compoundTypeInfo.GetElementType();
                        if (ElementRequiresWalking(elementType))
                        {
                            if (compound is Array objectArray)
                            {
                                for (var i = 0; i < objectArray.Length; i++)
                                {
                                    var e = objectArray.GetValue(i);
                                    if (e != null)
                                    {
                                        if (ObjectRequiresWalking(e.GetType()))
                                            compounds.Push(e);
                                    }
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
                            if (o == null || f.IsInitOnly)
                                continue;

                            var objectTypeInfo = o.GetType();
                            if (objectTypeInfo.IsArray)
                            {
                                // Analyze element type
                                var elementType = objectTypeInfo.GetElementType();
                                if (ElementRequiresWalking(elementType))
                                {
                                    if (o is Array objectArray)
                                    {
                                        for (var i = 0; i < objectArray.Length; i++)
                                        {
                                            var e = objectArray.GetValue(i);
                                            if (e != null)
                                            {
                                                if (ObjectRequiresWalking(e.GetType()))
                                                    compounds.Push(e);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ObjectRequiresWalking(o.GetType()))
                                {
                                    compounds.Push(o);
                                }
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
        private IDataUnit mDataUnit;
        private List<IDataCompiler> mCompilers;
        private List<IFileIdInstance> mFilesProviders;

        public GameDataData() : this(null)
        {
        }

        public GameDataData(IDataUnit dataUnit)
        {
            mDataUnit = dataUnit;
            mCompilers = new();
            mFilesProviders = new();
        }

        public IDataUnit DataUnit
        {
            get { return mDataUnit; }
        }

        public bool Instanciate(Assembly assembly)
        {
            if (mDataUnit != null)
                return true;

            try
            {
                mDataUnit = AssemblyUtil.Create1<IRootDataUnit>(assembly);
                return mDataUnit != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public List<IDataCompiler> CollectDataCompilers()
        {
            var compilers = new List<IDataCompiler>();
            {
                // Collect:
                // - Compilers
                // - FileId providers
                // - Bigfile providers
                ObjectTreeWalker.Walk(mDataUnit, delegate (object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IFileId c)
                        {
                            var compiler = c.Compiler;
                            if (compiler != null)
                                compilers.Add(compiler);
                            return true;
                        }
                        return false;
                    }
                );
            }
            return compilers;
        }



        public List<IDataUnit> CollectDataUnits()
        {
            var dataUnits = new List<IDataUnit>();
            {
                // Collect:
                // - IDataUnit
                ObjectTreeWalker.Walk(mDataUnit, delegate (object compound)
                    {
                        var compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataUnit du)
                        {
                            dataUnits.Add(du);
                        }
                        return false;
                    }
                );
            }
            return dataUnits;
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

            var ok = ObjectTreeWalker.Walk(mDataUnit, delegate (object compound)
                {
                    var compoundType = compound.GetType();

                    if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                        return true;

                    var handled = false;
                    if (compound is IFileIdInstance f)
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
                CppCodeStream2.Write2(BuildSystemCompilerConfig.Platform, data, dataFilename, codeFilename, relocFilename);
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

            GenerateStdData(mDataUnit, dataFilename, relocFilename);
            GenerateCppCodeAndData(mDataUnit, dataFilename + "c", Path.ChangeExtension(dataFilename, ".h"), relocFilename + "c");
            return true;
        }
    }
}
