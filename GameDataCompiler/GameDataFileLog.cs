using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
    public sealed class GameDataFileLog
    {
        private static int Compare(KeyValuePair<Hash160, IDataFile> lhs, KeyValuePair<Hash160, IDataFile> rhs)
        {
            return Hash160.Compare(lhs.Key, rhs.Key);
        }

        public static int Merge(IReadOnlyList<IDataFile> previousCompilers, IReadOnlyList<IDataFile> currentCompilers, out List<IDataFile> mergedCompilers)
        {
            // Cross-reference the 'previous_compilers' (loaded) with the 'current_compilers' (from GameData.___.dll) and combine into 'merged_compilers'.

            // We are doing this using sorted lists and binary search, since we need to be able to consider the order of the compilers.
            var previousCompilerSignatureList = BuildCompilerSignatureList(previousCompilers);
            var currentCompilerSignatureList = BuildCompilerSignatureList(currentCompilers);
            //var comparer = new SignatureComparer();

            // Maximum number of compilers that can be merged is the number of current compilers.
            mergedCompilers = new List<IDataFile>(currentCompilers.Count);

            var currentListIndex = 0;
            var previousListIndex = 0;
            var mergedPreviousCount = 0;
            while (currentListIndex < currentCompilerSignatureList.Count)
            {
                var signature = currentCompilerSignatureList[currentListIndex++];
                // We can just advance the index of the previous compiler signature list until the comparison returns that the current signature is bigger
                // because that means that the current compiler is not in the previous list.
                int c = 1;
                while (previousListIndex < previousCompilerSignatureList.Count)
                {
                    c = Compare(currentCompilerSignatureList[currentListIndex], previousCompilerSignatureList[previousListIndex]);
                    if (c < 1)
                        break;
                    previousListIndex++;
                }

                if (c == 0)
                {
                    mergedPreviousCount++;

                    var pdc = previousCompilerSignatureList[previousListIndex].Value;
                    var cdc = signature.Value;
                    cdc.CopyConstruct(pdc);
                    mergedCompilers.Add(cdc);
                }
                else
                {
                    // The current compiler is not in the previous list so add it to the merged list.
                    mergedCompilers.Add(signature.Value);
                }
            }

            return (mergedPreviousCount == currentCompilers.Count) ? 0 : 1;
        }

        private static List<KeyValuePair<Hash160, IDataFile>> BuildCompilerSignatureList(IReadOnlyList<IDataFile> compilers)
        {
            var signatureList = new List<KeyValuePair<Hash160, IDataFile>>(compilers.Count);
            foreach (var cl in compilers)
            {
                signatureList.Add(new KeyValuePair<Hash160, IDataFile>(cl.Signature, cl));
            }

            signatureList.Sort(Compare);
            return signatureList;
        }

        public static int Cook(IReadOnlyList<IDataFile> compilers, out List<IDataFile> allDataFiles)
        {
            // This is a very simple single threaded compilation approach
            allDataFiles = new(compilers.Count);
            var result = 0;
            foreach (var df in compilers)
            {
                var additionalDataFiles = new List<IDataFile>() { df };

                var i = 0;
                while (i < additionalDataFiles.Count)
                {
                    var c = additionalDataFiles[i];
                    var r = c.Cook(additionalDataFiles);
                    allDataFiles.Add(c);
                    i += 1;

                    if (r.HasFlag(DataCookResult.Error) || !r.HasFlag(DataCookResult.UpToDate))
                        result++;
                }
            }

            return result;
        }

        public static bool Save(EPlatform platform, string filepath, List<IDataFile> dataFiles)
        {
            var writer = ArchitectureUtils.CreateBinaryFileWriter(filepath, platform);
            if (writer == null) return false;

            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();
            if (memoryWriter.Open(memoryStream, ArchitectureUtils.GetEndianForPlatform(platform)))
            {
                foreach (var compiler in dataFiles)
                {
                    memoryWriter.Reset();

                    // byte[20]: IDataFile Type Signature
                    // byte[20]: IDataFile Signature
                    // byte[]: IDataFile State
                    var compilerTypeSignature = HashUtility.Compute(compiler.GetType());
                    compilerTypeSignature.WriteTo(memoryWriter);
                    compiler.Signature.WriteTo(memoryWriter);
                    compiler.SaveState(memoryWriter);
                    writer.Write(memoryStream.Length);
                    writer.Write(memoryStream.GetBuffer().AsSpan());
                }

                memoryWriter.Close();
                writer.Close();
                return true;
            }

            writer.Close();
            return false;
        }

        public static List<IDataFile> Load(string filepath, List<IDataFile> currentDataFileLog)
        {
            var loadedDataFilelog = new List<IDataFile>();

            BinaryFileReader reader = new();
            if (!reader.Open(filepath))
                return loadedDataFilelog;

            var compilerSignatureSet = new HashSet<Hash160>(currentDataFileLog.Count);
            var compilerTypeSet = new Dictionary<Hash160, Type>(currentDataFileLog.Count);
            foreach (var cl in currentDataFileLog)
            {
                var typeSignature = HashUtility.Compute(cl.GetType());
                compilerTypeSet.TryAdd(typeSignature, cl.GetType());
            }

            while (reader.Position < reader.Length)
            {
                var blockSize = reader.ReadUInt32();
                var compilerTypeSignature = Hash160.ReadFrom(reader);
                var compilerSignature = Hash160.ReadFrom(reader);

                // We could have a type signature in the log that doesn't exists anymore because
                // the name of the compiler has been changed. When this is the case we need to
                // inform the user of this class that the log is out-of-date!

                if (compilerTypeSet.TryGetValue(compilerTypeSignature, out var type))
                {
                    var compiler = Activator.CreateInstance(type) as IDataFile;
                    if (compilerSignatureSet.Add(compilerSignature))
                    {
                        loadedDataFilelog.Add(compiler);
                    }

                    compiler.LoadState(reader);
                }
                else
                {
                    if (!reader.SkipBytes(blockSize))
                        break;
                }
            }

            reader.Close();
            return loadedDataFilelog;
        }
    }
}
