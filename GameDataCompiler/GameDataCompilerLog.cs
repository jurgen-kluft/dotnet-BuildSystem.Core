using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
    public sealed class GameDataCompilerLog
    {
        private Dictionary<Hash160, Type> mCompilerTypeSet = new ();
        private HashSet<Hash160> mCompilerSignatureSet = new ();
        private string FilePath { get; set; }

        public GameDataCompilerLog(string filepath)
        {
            FilePath = filepath;
            CompilerLog = new List<IDataFile>();
        }

        private EPlatform Platform { get; set; }

        public List<IDataFile> CompilerLog { get; set; }

        private class SignatureComparer
        {
            public int Compare(KeyValuePair<Hash160, IDataFile> lhs, KeyValuePair<Hash160, IDataFile> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }
        }

        public Result Merge(List<IDataFile> previousCompilers, List<IDataFile> currentCompilers, out List<IDataFile> mergedCompilers)
        {
            // Cross-reference the 'previous_compilers' (loaded) with the 'current_compilers' (from GameData.___.dll) and combine into 'merged_compilers'.

            // We are doing this using sorted lists and binary search, since we need to be able to consider the order of the compilers.
            var previousCompilerSignatureList = BuildCompilerSignatureList(previousCompilers);
            var currentCompilerSignatureList = BuildCompilerSignatureList(currentCompilers);
            var comparer = new SignatureComparer();

            // Maximum number of compilers that can be merged is the number of current compilers.
            mergedCompilers = new List<IDataFile>(currentCompilers.Count);

            var currentListIndex = 0;
            var previousListIndex = 0;
            var mergedPreviousCount = 0;
            var result = Result.Ok;
            while (currentListIndex < currentCompilerSignatureList.Count)
            {
                var signature = currentCompilerSignatureList[currentListIndex++];
                // We can just advance the index of the previous compiler signature list until the comparison returns that the current signature is bigger
                // because that means that the current compiler is not in the previous list.
                int c = 1;
                while (previousListIndex < previousCompilerSignatureList.Count)
                {
                    c = comparer.Compare(currentCompilerSignatureList[currentListIndex], previousCompilerSignatureList[previousListIndex]);
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

            if (mergedPreviousCount == currentCompilers.Count)
            {
                return Result.Ok;
            }

            return Result.OutOfDate;
        }

        private List<KeyValuePair<Hash160, IDataFile>> BuildCompilerSignatureList(List<IDataFile> compilers)
        {
            var signatureList = new List<KeyValuePair<Hash160, IDataFile>>(compilers.Count);
            foreach (var cl in compilers)
            {
                signatureList.Add(new KeyValuePair<Hash160, IDataFile>(cl.Signature, cl));
            }

            int Comparer(KeyValuePair<Hash160, IDataFile> lhs, KeyValuePair<Hash160, IDataFile> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }

            signatureList.Sort(Comparer);

            return signatureList;
        }

        public Result Cook(List<IDataFile> compilers, out List<IDataFile> allDataFiles)
        {
            // Make sure the directory structure of @SrcPath is duplicated at @DstPath
            DirUtils.DuplicateFolderStructure(BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.DstPath);

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

            if (result == 0)
            {
                return Result.Ok;
            }

            return Result.OutOfDate;
        }

        private void RegisterCompilers(List<IDataFile> compilers)
        {
            foreach (var cl in compilers)
            {
                var typeSignature = HashUtility.Compute(cl.GetType());
                mCompilerTypeSet.TryAdd(typeSignature, cl.GetType());
            }
        }

        public Result Save(List<IDataFile> cl)
        {
            var writer = ArchitectureUtils.CreateBinaryWriter(FilePath, LocalizerConfig.Platform);
            if (writer == null) return Result.Error;

            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();
            if (memoryWriter.Open(memoryStream, ArchitectureUtils.GetEndianForPlatform(Platform)))
            {
                foreach (var compiler in cl)
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
                return Result.Ok;
            }

            writer.Close();
            return Result.Error;
        }

        public bool Load()
        {
            CompilerLog.Clear();

            BinaryFileReader reader = new();
            if (!reader.Open(FilePath))
                return false;

            while (reader.Position < reader.Length)
            {
                var blockSize = reader.ReadUInt32();
                var compilerTypeSignature = Hash160.ReadFrom(reader);
                var compilerSignature = Hash160.ReadFrom(reader);

                // We could have a type signature in the log that doesn't exists anymore because
                // the name of the compiler has been changed. When this is the case we need to
                // inform the user of this class that the log is out-of-date!

                if (mCompilerTypeSet.TryGetValue(compilerTypeSignature, out var type))
                {
                    var compiler = Activator.CreateInstance(type) as IDataFile;
                    if (mCompilerSignatureSet.Add(compilerSignature))
                    {
                        CompilerLog.Add(compiler);
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
            return true;
        }
    }

    public struct Result
    {
        private enum ResultEnum : int
        {
            Ok = 0,
            OutOfDate = 1,
            Error = 2,
        }

        private int ResultValue { get; set; }

        private int AsInt => (int)ResultValue;

        public static readonly Result Ok = new() { ResultValue = (int)ResultEnum.Ok };
        public static readonly Result OutOfDate = new() { ResultValue = (int)ResultEnum.OutOfDate };
        public static readonly Result Error = new() { ResultValue = (int)ResultEnum.Error };

        public static Result FromRaw(int b)
        {
            return new() { ResultValue = (int)(b & 0x3) };
        }

        public bool IsOk => ResultValue == 0;
        public bool IsNotOk => ResultValue != 0;
        public bool IsOutOfDate => ((int)ResultValue & (int)(ResultEnum.OutOfDate)) != 0;

        public static bool operator ==(Result b1, Result b2)
        {
            return b1.AsInt == b2.AsInt;
        }

        public static bool operator !=(Result b1, Result b2)
        {
            return b1.AsInt != b2.AsInt;
        }

        public override int GetHashCode()
        {
            return ResultValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (Result)obj;
            return this.AsInt == other.AsInt;

        }
    }
}
