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
                Stack<object> compounds = new Stack<object>();
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
                        FieldInfo[] fields = compoundTypeInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
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

    public class DataAssemblyManager
    {
        #region FileRegistrar

        private class FileRegistrar : GameData.IFileRegistrar
        {
            private Dictionary<Filename, GameData.FileId> mRegistry;

            public FileRegistrar()
            {
                mRegistry = new Dictionary<Filename, GameData.FileId>();
            }

            public Dictionary<Filename, GameData.FileId> items
            {
                get
                {
                    return mRegistry;
                }
            }

            public void Clear()
            {
                mRegistry.Clear();
            }

            public GameData.FileId Add(Filename filename)
            {
                GameData.FileId fileId;
                if (!mRegistry.TryGetValue(filename, out fileId))
                {
                    fileId = GameData.FileId.NewInstance(filename);
                    mRegistry.Add(filename, fileId);
                }
                return fileId;
            }

            public bool SaveAsText(StreamWriter writer)
            {
                try
                {
                    // Length
                    writer.WriteLine(mRegistry.Count);
                    foreach (KeyValuePair<Filename, GameData.FileId> p in mRegistry)
                    {
                        writer.WriteLine(p.Key);
                        writer.WriteLine(p.Value.id.ToString());
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion
        #region Fields

        private Assembly mDataAssembly;
        private GameData.IDataUnit mRoot;

        private List<GameData.IDataCompiler> mCompilers;
        private List<GameData.IFileIdsProvider> mFileIdsProviders;

        private FileRegistrar mFilenameRegistry;

        #endregion
        #region Constructor

        public DataAssemblyManager()
        {
            mCompilers = new List<GameData.IDataCompiler>();
            mFileIdsProviders = new List<GameData.IFileIdsProvider>();
            mFilenameRegistry = new FileRegistrar();
        }

        #endregion
        #region Properties

        public Assembly assembly { get { return mDataAssembly; } }
        public GameData.IDataUnit root { get { return mRoot; } }

        #endregion
        #region Build

        public bool compileAsm(Filename filenameOfAssembly, Filename[] files, Filename[] csincludes, Dirname srcPath, Dirname subPath, Dirname dstPath, Dirname depPath, Filename[] referencedAssemblies)
        {
            mDataAssembly = AssemblyCompiler.Compile(filenameOfAssembly, files, csincludes, srcPath, subPath, dstPath, depPath, referencedAssemblies);
            return mDataAssembly != null;
        }

        #endregion
        #region Instanciate

        private bool instanciate()
        {
            if (mRoot != null)
                return true;

            try
            {
                mRoot = AssemblyUtil.Create1<GameData.IDataUnit>(mDataAssembly);
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

        public bool initializeDataCompilation()
        {
            bool ok = false;
            if (instanciate())
            {
                mCompilers.Clear();

                // Collect:
                // - Compilers
                // - FileId providers
                // - Bigfile providers
                ok = ObjectTreeWalker.Walk(mRoot, delegate (object compound)
                {
                    Type compoundType = compound.GetType();

                    if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                        return true;

                    // TODO what about Array's or List<>'s of DataCompilers?

                    bool handled = false;
                    if (compound is GameData.IDataCompiler)
                    {
                        GameData.IDataCompiler c = compound as GameData.IDataCompiler;
                        mCompilers.Add(c);
                        handled = true;
                    }
                    return handled;
                });
            }
            return ok;
        }

        public void setupDataCompilers( )
        {
            foreach (GameData.IDataCompiler c in mCompilers)
                c.CompilerSetup();
        }


        public bool finalizeDataCompilation( )
        {
            mFileIdsProviders.Clear();

            bool ok = ObjectTreeWalker.Walk(mRoot, delegate (object compound)
            {
                Type compoundType = compound.GetType();

                if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                    return true;

                bool handled = false;
                if (compound is GameData.IFileIdsProvider)
                {
                    GameData.IFileIdsProvider f = compound as GameData.IFileIdsProvider;
                    mFileIdsProviders.Add(f);
                    handled = true;
                }

                return handled;
            });

            // Register and generate FileIds
            mFilenameRegistry.Clear();
            foreach (GameData.IFileIdsProvider c in mFileIdsProviders)
                c.registerAt(mFilenameRegistry);

            return true;
        }

        #endregion

        #endregion
        #region MemberBook

        class MyMemberBook : MemberBook
        {
            public void HandoutReferences()
            {
                // Handout StreamReferences to int64s, uint64s classes, compounds and arrays taking care of equality of these objects.
                // Note: Strings are a bit of a special case since we also will collect the names of members and classes.
                Dictionary<Int64, StreamReference> referencesForInt64Dict = new Dictionary<Int64, StreamReference>();
                foreach (Int64Member i in int64s)
                {
                    StreamReference reference;
                    if (referencesForInt64Dict.TryGetValue(i.int64, out reference))
                    {
                        i.reference = reference;
                    }
                    else
                    {
                        i.reference = StreamReference.Instance;
                        referencesForInt64Dict.Add(i.int64, i.reference);
                    }
                }

                Dictionary<UInt64, StreamReference> referencesForUInt64Dict = new Dictionary<UInt64, StreamReference>();
                foreach (UInt64Member i in uint64s)
                {
                    StreamReference reference;
                    if (referencesForUInt64Dict.TryGetValue(i.uint64, out reference))
                    {
                        i.reference = reference;
                    }
                    else
                    {
                        i.reference = StreamReference.Instance;
                        referencesForUInt64Dict.Add(i.uint64, i.reference);
                    }
                }

                Dictionary<object, StreamReference> referencesForClassesDict = new Dictionary<object, StreamReference>();
                foreach (ObjectMember c in classes)
                {
                    if (c.value != null)
                    {
                        StreamReference reference;
                        if (referencesForClassesDict.TryGetValue(c.value, out reference))
                        {
                            c.reference = reference;
                        }
                        else
                        {
                            c.reference = StreamReference.Instance;
                            referencesForClassesDict.Add(c.value, c.reference);
                        }
                    }
                    else
                    {
                        c.reference = StreamReference.Empty;
                    }
                }

                Dictionary<object, StreamReference> referencesForCompoundsDict = new Dictionary<object, StreamReference>();
                foreach (CompoundMember c in compounds)
                {
                    if (c.value != null)
                    {
                        StreamReference reference;
                        if (referencesForCompoundsDict.TryGetValue(c.value, out reference))
                        {
                            c.reference = reference;
                        }
                        else
                        {
                            c.reference = StreamReference.Instance;
                            referencesForCompoundsDict.Add(c.value, c.reference);
                        }
                    }
                    else
                    {
                        c.reference = StreamReference.Empty;
                    }
                }

                Dictionary<object, StreamReference> referencesForArraysDict = new Dictionary<object, StreamReference>();
                foreach (ArrayMember a in arrays)
                {
                    if (a.value != null)
                    {
                        StreamReference reference;
                        if (referencesForArraysDict.TryGetValue(a.value, out reference))
                        {
                            a.reference = reference;
                        }
                        else
                        {
                            a.reference = StreamReference.Instance;
                            referencesForArraysDict.Add(a.value, a.reference);
                        }
                    }
                    else
                    {
                        a.reference = StreamReference.Empty;
                    }
                }
            }
        }

        #endregion
        #region Generate C++ Code and Data

        // Save the resource data....
        // As binary data and C code for the interface.

        // C code
        // - 2 Builds, Development and Final. During Development we can use a "Property Table" 
        //   defining the name of the member and it's offset so that the data can be out of
        //   sync with the code when designers are rebuilding the data after changing some
        //   values. The Final Analyze does not have the property table and reads data directly 
        //   since the data and code are not out of sync. The code emitted for Development
        //   also does not contain any data members but functions which use the "Property Table"
        //   to obtain the value.
        // - Endian
        // - Replace all data members with accessors (get....)
        // - Warning: Data member alignment!
        // - Duplicate data (strings, arrays)
        // - Database of written references, objects, arrays, strings (For emitting an object once as well as terminating circular references)

        // Need to define
        // - String data representation (class String)
        // - LString data representation (class LString)
        // - FileId data representation (class FileId)
        // - Array data representation (template<> class?)
        // - Array of Array of Array of String ?
        // - Data inheritance structure

        // Defined, big/little endian
        // fx32  -> 4 byte
        // fx16  -> 2 byte
        // int   -> 4 byte
        // short -> 2 byte
        // byte  -> 1 byte

        // We will use a ResourceDataWriter for writing the resource data as binary data
        // Exporting every class as a class in C/C++ using a ClassWriter providing enough
        // functionality to write any kind of class, function and member.
        private void generateCppCodeAndData(object data, string dataFilename, string codeFilename, string relocFilename)
        {
            // Analyze Data.Root and generate a list of 'Code.Class' objects from this.
            Reflector reflector = new Reflector(null);

            MyMemberBook book = new MyMemberBook();
            reflector.Analyze(data, book);
            book.HandoutReferences();

            // Sort the members on every 'Code.Class' so that alignment of data is solved.
            foreach (ObjectMember c in book.classes)
                c.sortMembers(new MemberSizeComparer());

            // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
            StringTable stringTable = new StringTable();
            FileIdTable fileIdTable = new FileIdTable();

            // Compile every 'Code.Class' to the DataStream.
            CppDataStream dataStream = new CppDataStream(BuildSystemCompilerConfig.Endian);
            CppCodeStream.DataStreamWriter dataStreamWriter = new CppCodeStream.DataStreamWriter(stringTable, fileIdTable, dataStream);
            dataStreamWriter.open();
            {
                ObjectMember root = book.classes[0];
                root.write(dataStreamWriter);
            }
            dataStreamWriter.close();

            // Finalize the DataStream and obtain a database of the position of the 
            // 'Code.Class' objects in the DataStream.
            FileInfo dataFileInfo = new FileInfo(BuildSystemCompilerConfig.SrcPath + "\\" + dataFilename);
            FileStream dataFileStream = new FileStream(dataFileInfo.FullName, FileMode.Create);
            IBinaryWriter dataFileStreamWriter = EndianUtils.CreateBinaryWriter(dataFileStream, BuildSystemCompilerConfig.Endian);
            FileInfo relocFileInfo = new FileInfo(BuildSystemCompilerConfig.SrcPath + "\\" + relocFilename);
            FileStream relocFileStream = new FileStream(relocFileInfo.FullName, FileMode.Create);
            IBinaryWriter relocFileStreamWriter = EndianUtils.CreateBinaryWriter(relocFileStream, BuildSystemCompilerConfig.Endian);
            Dictionary<StreamReference, int> referenceOffsetDatabase;
            dataStream.finalize(dataFileStreamWriter, relocFileStreamWriter, out referenceOffsetDatabase);
            dataFileStreamWriter.Close();
            dataFileStream.Close();
            relocFileStreamWriter.Close();
            relocFileStream.Close();

            // Generate the c++ code using the CppCodeWriter.
            FileInfo codeFileInfo = new FileInfo(BuildSystemCompilerConfig.SrcPath + "\\" + codeFilename);
            FileStream codeFileStream = codeFileInfo.Create();
            StreamWriter codeFileStreamWriter = new StreamWriter(codeFileStream);
            CppCodeStream.CppCodeWriter codeWriter = new CppCodeStream.CppCodeWriter();
            codeWriter.write(book.classes, codeFileStreamWriter);
            codeFileStreamWriter.Close();
            codeFileStream.Close();
        }

        #endregion
        #region Generate Std Data

        private void generateStdData(object data, Filename dataFilename, Filename relocFilename)
        {
            // Generate the generic data
            FileInfo dataFileInfo = new FileInfo(dataFilename.ToString());
            FileStream dataStream = new FileStream(dataFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            IBinaryWriter dataStreamWriter = EndianUtils.CreateBinaryWriter(dataStream, BuildSystemCompilerConfig.Endian);

            FileInfo reallocTableFileInfo = new FileInfo(relocFilename.ToString());
            FileStream reallocTableStream = new FileStream(reallocTableFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 2 * 1024 * 1024);
            IBinaryWriter reallocTableStreamWriter = EndianUtils.CreateBinaryWriter(reallocTableStream, BuildSystemCompilerConfig.Endian);

            StdDataStream stdDataStream = new StdDataStream(BuildSystemCompilerConfig.Endian);

            try
            {
                StdDataStream.SizeOfBool = BuildSystemCompilerConfig.SizeOfBool;
                stdDataStream.write(EGenericFormat.STD_FLAT, data, dataStreamWriter, reallocTableStreamWriter);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured: {0}", e.ToString());
            }
            finally
            {
                reallocTableStream.Flush();
                reallocTableStreamWriter.Close();
                reallocTableStream.Close();

                dataStream.Flush();
                dataStreamWriter.Close();
                dataStream.Close();
            }
        }

        #endregion
        #region Save

        public bool save(Dirname path, string fullNameWithoutExtension)
        {
            //generateCppCodeAndData(root, fullNameWithoutExtension + ".rdf", fullNameWithoutExtension + ".rcf", fullNameWithoutExtension + ".rrf");
            Filename dataFilename = new Filename(fullNameWithoutExtension + BuildSystemCompilerConfig.DataFileExtension);
            Filename relocFilename = new Filename(fullNameWithoutExtension + BuildSystemCompilerConfig.DataRelocFileExtension);

            dataFilename = dataFilename.MakeAbsolute(path);
            relocFilename = relocFilename.MakeAbsolute(path);

            GameData.FileCommander.createDirectoryOnDisk(path);

            generateStdData(mRoot, dataFilename, relocFilename);
            return true;
        }

        #endregion
    }
}

