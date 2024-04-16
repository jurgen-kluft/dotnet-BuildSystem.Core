using System.Diagnostics;
using GameCore;
using StreamWriter = System.IO.StreamWriter;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

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
        //   - Member memory alignment
        //   - Database of written references, objects, arrays, strings
        //     - For emitting an object once as well as terminating circular references
        //   - C# class hierarchy is collapsed to one C++ class
        //   - Duplicate data (strings, arrays)

        // Need to define
        // - String data representation (struct string_t)
        //   - string_t { u32 const mLength; u32 const mStr; }
        // - LString data representation (lstring_t = uint64_t)
        // - FileId data representation (fileid_t = uint64_t)
        // - Array data representation (template<T> array_t { u32 const mSize; u32 const mArray; })
        // - Array of Array of Array of String ?
        // - Embedding a struct or class will result in a pointer to that struct/class

        // Defined: (big/little endian)
        // double       -> 8 byte
        // float        -> 4 byte
        // fx32         -> 4 byte
        // fx16         -> 2 byte
        // ulong/long   -> 8 byte
        // uint/int     -> 4 byte
        // ushort/short -> 2 byte
        // byte         -> 1 byte
        // bool         -> 4 byte (although many booleans are packed into 32 bits)

        // We will use a ResourceDataWriter for writing the resource data as binary data
        // Exporting every class as a struct in C/C++ using a ClassWriter providing enough
        // functionality to write any kind of class, function and member.


        public static void Write2(EPlatform platform, object data, string dataFilename, string codeFilename, string relocationFilename)
        {
            // Use string table in MetaCode
            var stringTable = new StringTable();
            var metaCode = new MetaCode.MetaCode(stringTable, 8192);
            var metaMemberFactory = new MetaMemberFactory(metaCode);
            var typeInformation = new GenericTypeInformation();

            var reflector = new Reflector2(metaCode, metaMemberFactory, typeInformation);
            reflector.Analyze(data);

            // In every class combine booleans into bitsets
            for (var ci = 0; ci < metaCode.MembersType.Count; ++ci)
            {
                var mt = metaCode.MembersType[ci];
                if (!mt.IsClass) continue;
                metaCode.CombineBooleans(ci);
            }

            // Sort the members on every class so that alignment of data is solved.
            // NOTE
            // In the list of classes we have many 'duplicates', classes of the same type that are emitted
            // multiple times. We need to make sure the sorting of members is stable and deterministic.
            var memberSizeComparer = new MetaCode.MetaCode.SortMembersBySize(metaCode);
            for (var i = 0; i < 2; ++i)
            {
                for (var ci = 0; ci < metaCode.MembersType.Count; ++ci)
                {
                    var mt = metaCode.MembersType[ci];
                    if (!mt.IsClass) continue;
                    metaCode.SortMembers(ci, memberSizeComparer);
                }
            }

            // Write out every underlying member 'data' of the code to a DataStream
            var dataStream = new CppDataStream2(platform, stringTable);
            DataStreamWriter2.Write(metaCode, stringTable, dataStream);

            // Finalize the DataStream by writing the data to a file
            var dataFileInfo = new FileInfo(dataFilename);
            var dataFileStream = new FileStream(dataFileInfo.FullName, FileMode.Create);
            var dataFileStreamWriter = EndianUtils.CreateBinaryWriter(dataFileStream, platform);
            dataStream.Finalize(dataFileStreamWriter);
            dataFileStreamWriter.Close();
            dataFileStream.Close();

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
