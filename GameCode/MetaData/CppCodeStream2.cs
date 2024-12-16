using GameCore;

namespace GameData
{
    using MetaCode;

    // CodeStream for generating C++ header file(s) containing structs that map to 'data'
    public static class CppCodeStream2
    {
        // Save binary data and C++ code for mapping to the data

        // C/C++ code:
        //   - Endian
        //   - Enums
        //   - Member sort for memory alignment
        //   - Database of written references, objects, arrays, strings
        //     - For emitting an object once as well as terminating circular references
        //   - C# class hierarchy is collapsed to one C++ class
        //   - De-duplicated data (strings, arrays, struct instances, class instances)

        // Need to use 'charon' C++ library, since it has predefined structs for:
        // - String data representation (struct string_t)
        // - LString data representation (lstring_t = u64)
        // - FileId data representation (fileid_t = u64)
        // - Array data representation (template<T> array_t { u32 const mByteSize; u32 const mCount; T const* mArray; })

        // Notes:
        // - Embedding a struct (IStruct) will be treated as a value type
        // - Embedding a class will result in a pointer to that class

        // Defined: (big/little endian)
        // double       -> 8 byte
        // float        -> 4 byte
        // ulong/long   -> 8 byte
        // uint/int     -> 4 byte
        // ushort/short -> 2 byte
        // byte         -> 1 byte
        // bool         -> 1 byte (Note: 8 booleans are packed together in one byte)

        public static void Write2(EPlatform platform, object data, StreamWriter codeFileWriter, IBinaryStreamWriter bigfileWriter, ISignatureDataBase signatureDb, out List<ulong> dataUnitsStreamPositions, out List<ulong> dataUnitsStreamSizes)
        {
            // Use string table in MetaCode
            var stringTable = new StringTable();
            var metaCode = new MetaCode2(stringTable, 8192);
            var metaMemberFactory = new MetaMemberFactory(metaCode);
            var typeInformation = new TypeInfo2();

            var reflector = new Reflector2(metaCode, metaMemberFactory, typeInformation);
            reflector.Analyze(data);

            // In every class combine booleans into a set of bits
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
            var memberSortPredicate = new MetaCode2.SortMembersPredicate(metaCode);
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
            var dataStream = new CppDataStream2(platform, stringTable, signatureDb);
            CppDataStreamWriter2.Write(metaCode, stringTable, signatureDb, dataStream);

            // Finalize the DataStream by writing to a (Bigfile) data file
            dataUnitsStreamPositions = [];
            dataUnitsStreamSizes = [];
            dataStream.Finalize(bigfileWriter, dataUnitsStreamPositions, dataUnitsStreamSizes);

            // Generate the c++ code using the CppCodeWriter.
            var codeWriter = new CppCodeWriter2() { MetaCode = metaCode };
            codeWriter.WriteEnums(codeFileWriter);
            codeWriter.WriteClasses(codeFileWriter);
        }
    }
}
