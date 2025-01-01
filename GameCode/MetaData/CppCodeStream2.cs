using GameCore;

namespace GameData
{
    using MetaCode;

    // CodeStream for generating C++ header file containing the code that maps to 'data'
    public static class CppCodeStream2
    {
        // Save binary data and C++ code for mapping to the data

        // C/C++ code:
        //   - Member sorted by size for memory alignment
        //   - Database of written references, objects, arrays, strings
        //     - For emitting an object once as well as terminating circular references
        //   - C# class hierarchy is collapsed to one C++ class
        //   - De-duplicated data (string, array, list, dict, class)

        // Notes:
        // - A struct (IStruct) will be treated as a value type
        // - A class as a member will always be a pointer to that class

        // Defined: (big/little endian)
        // enum         -> 1, 2, 4 or 8 bytes
        // double       -> 8 bytes
        // float        -> 4 bytes
        // ulong/long   -> 8 bytes
        // uint/int     -> 4 bytes
        // ushort/short -> 2 bytes
        // byte         -> 1 byte
        // bool         -> 1 byte (Note: 8 booleans are packed together in one byte)

        public static void Write2(EPlatform platform, IRootDataUnit data, StreamWriter codeFileWriter, IStreamWriter bigfileWriter, IReadOnlySignatureDataBase signatureDb, out List<Hash160> dataUnitsSignatures,  out List<ulong> dataUnitsStreamPositions, out List<ulong> dataUnitsStreamSizes)
        {
            var metaCode = new MetaCode2(8192);
            var metaMemberFactory = new MetaMemberFactory2(metaCode);
            var typeInformation = new TypeInfo2();

            var reflector = new Reflector2(metaCode, metaMemberFactory, typeInformation);
            reflector.Analyze(data);

            // In every class combine booleans into one or more bitsets
            for (var ci = 0; ci < metaCode.Count; ++ci)
            {
                var mt = metaCode.MembersType[ci];
                if (mt.IsClass)
                {
                    metaCode.CombineBooleans(ci);
                }
            }

            // Sort the members on every class so that we do not have to consider member alignment
            // Note:
            //   In the list of classes we have many 'duplicates', classes of the same type that are emitted
            //   multiple times. We need to make sure the sorting of members is stable and predictable.
            var memberSortPredicate = new MetaCode2.SortMembersPredicate(metaCode);
            for (var ci = 0; ci < metaCode.MembersType.Count; ++ci)
            {
                var mt = metaCode.MembersType[ci];
                if (mt.IsClass)
                {
                    metaCode.SortMembers(ci, memberSortPredicate);
                }
            }

            // Write out every underlying member 'data' of the code to a DataStream
            var dataStream = new CppDataStream2(platform, signatureDb);
            CppDataWriter2.Write(metaCode, data.Signature, signatureDb, dataStream);

            // Finalize the DataStream by writing to a Archive data file
            dataStream.Finalize(bigfileWriter, out dataUnitsSignatures, out dataUnitsStreamPositions, out dataUnitsStreamSizes);

            // Generate the c++ code using the CppCodeWriter.
            var codeWriter = new CppCodeWriter2() { MetaCode = metaCode };
            codeWriter.WriteEnums(codeFileWriter);
            codeWriter.WriteStructs(data, codeFileWriter);
        }
    }
}
