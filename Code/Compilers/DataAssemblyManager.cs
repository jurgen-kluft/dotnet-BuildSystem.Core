using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Core;
using Core.MetaCode;

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

                    if (compound is Game.Data.IExternalObjectProvider)
                    {
                        Game.Data.IExternalObjectProvider externalObjectProvider = compound as Game.Data.IExternalObjectProvider;
                        compounds.Push(externalObjectProvider.extobject);
                        continue;
                    }

                    if (compound is Game.Data.IDataCompiler)
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

        private class FileRegistrar : Game.Data.IFileRegistrar
        {
            private Dictionary<Filename, Game.Data.FileId> mRegistry;

            public FileRegistrar()
            {
                mRegistry = new Dictionary<Filename, Game.Data.FileId>();
            }

            public Dictionary<Filename, Game.Data.FileId> items
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

            public Game.Data.FileId Add(Filename filename)
            {
                Game.Data.FileId fileId;
                if (!mRegistry.TryGetValue(filename, out fileId))
                {
                    fileId = Game.Data.FileId.NewInstance(filename);
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
                    foreach (KeyValuePair<Filename, Game.Data.FileId> p in mRegistry)
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
        private Game.Data.IDataUnit mRoot;

        private List<Game.Data.IDataCompiler> mCompilers;
        private List<Game.Data.IFileIdsProvider> mFileIdsProviders;
        private List<Game.Data.IDataCompilerNode> mDataCompilerNodes;

        private Game.Data.IDataCompilationServer mCompilationServer = new GroupedAndPrioritizedSequentialDataCompilationServer();
        private FileRegistrar mFilenameRegistry;

        #endregion
        #region Constructor

        public DataAssemblyManager()
        {
            mCompilers = new List<Game.Data.IDataCompiler>();
            mFileIdsProviders = new List<Game.Data.IFileIdsProvider>();
            mDataCompilerNodes = new List<Game.Data.IDataCompilerNode>();
            mFilenameRegistry = new FileRegistrar();
        }

        #endregion
        #region Properties

        public Assembly assembly { get { return mDataAssembly; } }
        public Game.Data.IDataUnit root { get { return mRoot; } }
        public Game.Data.IDataCompilationServer compilationServer { get { return mCompilationServer; } set { mCompilationServer = value; } }

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
                mRoot = AssemblyUtil.Create1<Game.Data.IDataUnit>(mDataAssembly);
                return mRoot != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        #endregion
        #region Compile

        #region Immediate Data Compilation Server

        public class ImmediateSequentialDataCompilationServer : Game.Data.IDataCompilationServer
        {
            public void schedule(Game.Data.IDataCompilerClient client)
            {
                client.onExecute();
                client.onFinished();
            }

            public void execute()
            {
            }
        }

        #endregion
        #region Grouped Data Compilation Server

        public class GroupedAndPrioritizedSequentialDataCompilationServer : Game.Data.IDataCompilationServer
        {
            private Dictionary<string, List<Game.Data.IDataCompilerClient>> mGroupedCompilers;

            public GroupedAndPrioritizedSequentialDataCompilationServer()
            {
                mGroupedCompilers = new Dictionary<string, List<Game.Data.IDataCompilerClient>>();
            }

            public void schedule(Game.Data.IDataCompilerClient client)
            {
                List<Game.Data.IDataCompilerClient> compilers;
                if (mGroupedCompilers.TryGetValue(client.group, out compilers))
                {
                    compilers.Add(client);
                }
                else
                {
                    compilers = new List<Game.Data.IDataCompilerClient>();
                    compilers.Add(client);
                    mGroupedCompilers.Add(client.group, compilers);
                }
            }

            public void execute()
            {
                List<int> priorityValues = new List<int>();
                Dictionary<int, List<Game.Data.IDataCompilerClient>> prioritizedCompilers = new Dictionary<int, List<Game.Data.IDataCompilerClient>>();
                foreach (KeyValuePair<string, List<Game.Data.IDataCompilerClient>> p in mGroupedCompilers)
                {
                    int priority = (int)p.Value[0].priority;
                    List<Game.Data.IDataCompilerClient> compilers;
                    if (!prioritizedCompilers.TryGetValue(priority, out compilers))
                    {
                        priorityValues.Add(priority);
                        prioritizedCompilers.Add(priority, p.Value);
                    }
                    else
                    {
                        foreach (Game.Data.IDataCompilerClient client in p.Value)
                            compilers.Add(client);
                    }
                }

                priorityValues.Sort();

                foreach (int priority in priorityValues)
                {
                    List<Game.Data.IDataCompilerClient> compilers;
                    if (prioritizedCompilers.TryGetValue(priority, out compilers))
                    {
                        foreach (Game.Data.IDataCompilerClient client in compilers)
                        {
                            client.onExecute();
                            client.onFinished();
                        }
                    }
                }
            }
        }

        #endregion
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

                    bool handled = false;
                    if (compound is Game.Data.IDataCompiler)
                    {
                        Game.Data.IDataCompiler c = compound as Game.Data.IDataCompiler;
                        mCompilers.Add(c);
                        handled = true;
                    }
                    return handled;
                });
            }
            return ok;
        }

        public void setupDataCompilers(DependencySystem dependencyChecker)
        {
            foreach (Game.Data.IDataCompiler c in mCompilers)
                c.csetup(dependencyChecker);
        }

        public void executeDataCompilers(DependencySystem dependencyChecker, bool execute)
        {
            foreach (Game.Data.IDataCompiler c in mCompilers)
                c.ccompile(compilationServer);

            if (execute)
                compilationServer.execute();
        }

        public void teardownDataCompilers(DependencySystem dependencyChecker)
        {
            foreach (Game.Data.IDataCompiler c in mCompilers)
                c.cteardown();
        }

        public bool finalizeDataCompilation(DependencySystem dependencyChecker)
        {

            mFileIdsProviders.Clear();
            mDataCompilerNodes.Clear();

            bool ok = ObjectTreeWalker.Walk(mRoot, delegate (object compound)
            {
                Type compoundType = compound.GetType();

                if (compoundType.IsPrimitive || compoundType.IsEnum || compoundType == typeof(string))
                    return true;

                bool handled = false;
                if (compound is Game.Data.IFileIdsProvider)
                {
                    Game.Data.IFileIdsProvider f = compound as Game.Data.IFileIdsProvider;
                    mFileIdsProviders.Add(f);
                    handled = true;
                }
                if (compound is Game.Data.IDataCompilerNode)
                {
                    Game.Data.IDataCompilerNode f = compound as Game.Data.IDataCompilerNode;
                    mDataCompilerNodes.Add(f);
                    handled = true;
                }

                return handled;
            });

            // Register and generate FileIds
            mFilenameRegistry.Clear();
            foreach (Game.Data.IFileIdsProvider c in mFileIdsProviders)
                c.registerAt(mFilenameRegistry);

            // The Node Info file
            {
                // + A list of all the 'sub node info' filenames
                // + All the filenames in the FilenameRegistry
                xTextStream ts = new xTextStream(BuildSystemCompilerConfig.DstPath + BuildSystemCompilerConfig.SubPath + new Filename(BuildSystemCompilerConfig.Name + BigfileConfig.BigFileNodeExtension));
                ts.Open(xTextStream.EMode.WRITE);
                {
                    foreach (Game.Data.IDataCompilerNode node in mDataCompilerNodes)
                        ts.write.WriteLine("Node={0}", node.bigfileFilename);
                    foreach (Filename f in mFilenameRegistry.items.Keys)
                        ts.write.WriteLine("File={0}", f);
                }
                ts.Close();
            }

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

            Game.Data.FileCommander.createDirectoryOnDisk(path);

            generateStdData(mRoot, dataFilename, relocFilename);
            return true;
        }

        #endregion
    }
}

