using GameCore;
using StreamWriter = System.IO.StreamWriter;

namespace GameData
{
    using MetaCode;

    // CodeStream for generating C++ header file(s) containing structs that map to 'data'
    public class CppCodeStream
    {
        // Save binary data and C code for mapping to the data

        // C/C++ code:
        //   - Endian
        //   - Enums
        //   - Member sort for memory alignment
        //   - Database of written references, objects, arrays, strings
        //     - For emitting an object once as well as terminating circular references
        //   - C# class hierarchy is collapsed to one C++ class
        //   - De-duplicated data (strings, arrays)

        // Need to define
        // - String data representation (struct string_t)
        //   - string_t { u32 const mLength; char const* mStr; }
        // - LString data representation (lstring_t = u64)
        // - FileId data representation (fileid_t = u64)
        // - Array data representation (template<T> array_t { u32 const mSize; T const* mArray; })
        // - Embedding a struct (IStruct) will be treated as a value type
        // - Embedding a class will result in a pointer to that class

        // Defined: (big/little endian)
        // double       -> 8 byte
        // float        -> 4 byte
        // fx32         -> 4 byte
        // fx16         -> 2 byte
        // ulong/long   -> 8 byte
        // uint/int     -> 4 byte
        // ushort/short -> 2 byte
        // byte         -> 1 byte
        // bool         -> 1 byte (although many booleans are packed together

        // We will use a ResourceDataWriter for writing the resource data as binary data
        // Exporting every class as a struct in C/C++ using a ClassWriter providing enough
        // functionality to write any kind of class, function and member.

        public static void Write2(EPlatform platform, object data, string dataFilename, string codeFilename, string relocationFilename)
        {
            // Use string table in MetaCode
            var stringTable = new StringTable();
            var metaCode = new MetaCode.MetaCode2(stringTable, 8192);
            var metaMemberFactory = new MetaMemberFactory(metaCode);
            var typeInformation = new GenericTypeInformation();

            var reflector = new Reflector2(metaCode, metaMemberFactory, typeInformation);
            reflector.Analyze(data);

            // In every class combine booleans into bitsets
            for (var ci = 0; ci < metaCode.Count; ++ci)
            {
                var mt = metaCode.MembersType[ci];
                if (!mt.IsClass) continue;
                metaCode.CombineBooleans(ci);
            }

            // Sort the members on every class so that we do not have to consider member alignment
            // Note:
            //   In the list of classes we have many 'duplicates', classes of the same type that are emitted
            //   multiple times. We need to make sure the sorting of members is stable and predictable.
            var memberSortPredicate = new MetaCode.MetaCode2.SortMembersPredicate(metaCode);
            for (var i = 0; i < 2; ++i)
            {
                for (var ci = 0; ci < metaCode.MembersType.Count; ++ci)
                {
                    var mt = metaCode.MembersType[ci];
                    if (!mt.IsClass) continue;
                    metaCode.SortMembers(ci, memberSortPredicate);
                }
            }

            // Write out every underlying member 'data' of the code to a DataStream
            var dataStream = new CppDataStream2(platform, stringTable);
            CppDataStreamWriter2.Write(metaCode, stringTable, dataStream);

            // Finalize the DataStream by writing the data to a file
            var dataFileInfo = new FileInfo(dataFilename);
            var dataFileStream = new FileStream(dataFileInfo.FullName, FileMode.Create);
            var dataFileStreamWriter = ArchitectureUtils.CreateBinaryWriter(dataFileStream, platform);
            var relocationFileInfo = new FileInfo(relocationFilename);
            var relocationFileStream = new FileStream(relocationFileInfo.FullName, FileMode.Create);
            var relocationFileStreamWriter = ArchitectureUtils.CreateBinaryWriter(relocationFileStream, platform);
            dataStream.Finalize(dataFileStreamWriter, relocationFileStreamWriter);
            dataFileStreamWriter.Close();
            dataFileStream.Close();
            relocationFileStreamWriter.Close();
            relocationFileStream.Close();

            // Generate the c++ code using the CppCodeWriter.
            var codeFileInfo = new FileInfo(codeFilename);
            var codeFileStream = codeFileInfo.Create();
            var codeFileStreamWriter = new StreamWriter(codeFileStream);
            var codeWriter = new CppCodeWriter2(metaCode);
            codeWriter.WriteEnums(codeFileStreamWriter);
            codeWriter.WriteClasses(codeFileStreamWriter);
            codeFileStreamWriter.Close();
            codeFileStream.Close();
        }
    }
}
