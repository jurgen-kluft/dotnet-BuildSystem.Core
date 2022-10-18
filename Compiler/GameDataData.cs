using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using GameCore;
using GameData;
using GameData.MetaCode;

namespace DataBuildSystem
{
    static class ObjectTreeWalker
    {
        #region Walk

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
                    Type compoundTypeInfo = compound.GetType();

                    if (ood(compound))
                        continue;

                    if (compound is GameData.IExternalObjectProvider)
                    {
                        GameData.IExternalObjectProvider externalObjectProvider = compound as GameData.IExternalObjectProvider;
                        compounds.Push(externalObjectProvider.extobject);
                        continue;
                    }

                    if (compound is GameData.IDataCompiler)
                        continue;

                    if (compoundTypeInfo.IsArray)
                    {
                        // Analyze element type
                        Type elementType = compoundTypeInfo.GetElementType();
                        if (!elementType.IsPrimitive && !compoundTypeInfo.IsEnum)
                        {
                            Array objectArray = compound as Array;
                            if (objectArray != null)
                            {
                                for (int i = 0; i < objectArray.Length; i++)
                                {
                                    object e = objectArray.GetValue(i);
                                    if (e != null)
                                        compounds.Push(e);
                                }
                            }
                        }
                    }
                    else
                    {
                        FieldInfo[] fields = compoundTypeInfo.GetFields(
                            BindingFlags.Public
                                | BindingFlags.NonPublic
                                | BindingFlags.Instance
                                | BindingFlags.GetField
                        );
                        foreach (FieldInfo f in fields)
                        {
                            object o = f.GetValue(compound);
                            if (o == null)
                                continue;

                            Type objectTypeInfo = o.GetType();
                            if (objectTypeInfo.IsArray)
                            {
                                // Analyze element type
                                Type elementType = objectTypeInfo.GetElementType();
                                if (!elementType.IsPrimitive && !compoundTypeInfo.IsEnum)
                                {
                                    Array objectArray = o as Array;
                                    if (objectArray != null)
                                    {
                                        for (int i = 0; i < objectArray.Length; i++)
                                        {
                                            object e = objectArray.GetValue(i);
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

        #endregion
    }

    public sealed class GameDataData
    {
        #region Fields

        private Assembly mDataAssembly;
        private GameData.IDataRoot mRoot;

        private List<IDataCompiler> mCompilers;
        private List<IFileIdProvider> mFilesProviders;

        #endregion
        #region Constructor

        public GameDataData(Assembly dataAssembly)
        {
            mDataAssembly = dataAssembly;
            mCompilers = new();
            mFilesProviders = new();
        }

        #endregion
        #region Properties

        public Assembly assembly
        {
            get { return mDataAssembly; }
        }
        public GameData.IDataRoot root
        {
            get { return mRoot; }
        }

        #endregion
        #region Instanciate

        private bool Instanciate()
        {
            if (mRoot != null)
                return true;

            try
            {
                mRoot = AssemblyUtil.Create1<GameData.IDataRoot>(mDataAssembly);
                return mRoot != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        #endregion
        #region Compile

        #region Data Compilation steps

        public List<IDataCompiler> CollectDataCompilers()
        {
            List<IDataCompiler> compilers = new();
            if (Instanciate())
            {
                // Collect:
                // - Compilers
                // - FileId providers
                // - Bigfile providers
                ObjectTreeWalker.Walk(mRoot, delegate (object compound)
                    {
                        Type compoundType = compound.GetType();
                        if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                            return true;

                        // TODO what about Array's or List<>'s of DataCompilers?

                        if (compound is IDataCompiler)
                        {
                            IDataCompiler c = compound as IDataCompiler;
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

            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();

            List<KeyValuePair<Hash160, IDataCompiler>> signatureList = new(compilers.Count);
            foreach (IDataCompiler cl in compilers)
            {
                memoryWriter.Reset();
                Type compilerType = cl.GetType();
                Hash160 compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                compilerTypeSignature.WriteTo(memoryWriter);
                cl.CompilerSignature(memoryWriter);
                Hash160 compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                signatureList.Add(new KeyValuePair<Hash160, IDataCompiler>(compilerSignature, cl));
            }
            int Comparer(KeyValuePair<Hash160, IDataCompiler> lhs, KeyValuePair<Hash160, IDataCompiler> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }
            signatureList.Sort(Comparer);

            Int64 index = 0;
            Hash160 prevSignature = signatureList[0].Key;
            foreach (var scl in signatureList)
            {
				IFileIdProvider filesProvider = scl.Value.CompilerFileIdProvider;
                filesProvider.FileId = index;
                if (prevSignature != scl.Key)
                    index++;
                prevSignature = scl.Key;
            }
        }

        public bool FinalizeDataCompilation()
        {
            mFilesProviders.Clear();

            bool ok = ObjectTreeWalker.Walk(mRoot, delegate (object compound)
                {
                    Type compoundType = compound.GetType();

                    if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                        return true;

                    bool handled = false;
                    if (compound is IFileIdProvider)
                    {
                        IFileIdProvider f = compound as IFileIdProvider;
                        mFilesProviders.Add(f);
                        handled = true;
                    }

                    return handled;
                }
            );


            return true;
        }

        #endregion

        #endregion
        #region Generate C++ Code and Data

        private void GenerateCppCodeAndData(object data, string dataFilename, string codeFilename, string relocFilename)
        {
            try
            {
                CppCodeStream.Write(BuildSystemCompilerConfig.Endian, data, dataFilename, codeFilename, relocFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        #endregion
        #region Generate Std Data

        private void GenerateStdData(object data, string dataFilename, string relocFilename)
        {
            // Generate the generic data
            try
            {
                StdDataStream stdDataStream = new StdDataStream(BuildSystemCompilerConfig.Endian);
                StdDataStream.SizeOfBool = BuildSystemCompilerConfig.SizeOfBool;
                stdDataStream.Write(BuildSystemCompilerConfig.Endian, data, dataFilename, relocFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        #endregion
        #region Save

        public bool Save(string filepath)
        {
            //generateCppCodeAndData(root, fullNameWithoutExtension + ".rdf", fullNameWithoutExtension + ".rcf", fullNameWithoutExtension + ".rrf");
            string dataFilename = Path.ChangeExtension(filepath, BuildSystemCompilerConfig.DataFileExtension);
            string relocFilename = Path.ChangeExtension(filepath, BuildSystemCompilerConfig.DataRelocFileExtension);

            GameData.FileCommander.createDirectoryOnDisk(Path.GetDirectoryName(dataFilename));

            GenerateStdData(mRoot, dataFilename, relocFilename);
            GenerateCppCodeAndData(mRoot, dataFilename + "c", Path.ChangeExtension(dataFilename, ".h"), relocFilename + "c");
            return true;
        }

        #endregion
    }
}
