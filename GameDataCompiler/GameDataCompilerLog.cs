using System;
using GameData;
using GameCore;

namespace DataBuildSystem
{
    public sealed class GameDataCompilerLog
    {
        private Dictionary<Hash160, Type> mCompilerTypeSet = new Dictionary<Hash160, Type>();
        private HashSet<Hash160> mCompilerSignatureSet = new HashSet<Hash160>();
        private string FilePath { get; set; }

        public GameDataCompilerLog(string filepath)
        {
            FilePath = filepath;
        }

        private EPlatform Platform { get; set; }

        private class SignatureComparer
        {
            public int Compare(KeyValuePair<Hash160, IDataCompiler> lhs, KeyValuePair<Hash160, IDataCompiler> rhs)
            {
                return Hash160.Compare(lhs.Key, rhs.Key);
            }
        }

        public Result Merge(List<IDataCompiler> previousCompilers, List<IDataCompiler> currentCompilers, out List<IDataCompiler> mergedCompilers)
        {
            // Cross-reference the 'previous_compilers' (loaded) with the 'current_compilers' (from GameData.___.dll) and combine into 'merged_compilers'.

            // We are doing this using sorted lists and binary search, since we need to be able to consider the order of the compilers.
            var previousCompilerSignatureList = BuildCompilerSignatureList(previousCompilers);
            var currentCompilerSignatureList = BuildCompilerSignatureList(currentCompilers);
            var comparer = new SignatureComparer();

            // Maximum number of compilers that can be merged is the number of current compilers.
            mergedCompilers = new List<IDataCompiler>(currentCompilers.Count);

            var currentListIndex = 0;
            var previousListIndex = 0;
            var mergedPreviousCount = 0;
            var result = Result.Ok;
            while (currentListIndex < currentCompilerSignatureList.Count)
            {
                var signature = currentCompilerSignatureList[currentListIndex++];
                // We can just advance the index of the previous compiler signature list until the comparison returns that the current signature is bigger
                // because that means that the current compiler is not in the previous list.
                int c;
                do
                {
                    c = comparer.Compare(currentCompilerSignatureList[currentListIndex], previousCompilerSignatureList[previousListIndex]);
                } while (c > 0 && ++previousListIndex < previousCompilerSignatureList.Count);

                if (c == 0)
                {
                    mergedPreviousCount++;

                    var pdc = previousCompilerSignatureList[previousListIndex].Value;
                    var cdc = signature.Value;
                    cdc.CompilerConstruct(pdc);
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

            return Result.OutOfData;
        }

        private List<KeyValuePair<Hash160, IDataCompiler>> BuildCompilerSignatureList(List<IDataCompiler> compilers)
        {
            List<KeyValuePair<Hash160, IDataCompiler>> signatureList = new(compilers.Count);

            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();
            if (memoryWriter.Open(memoryStream, ArchitectureUtils.GetEndianForPlatform(Platform)))
            {
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
            }

            return signatureList;
        }

        private Dictionary<Hash160, IDataCompiler> BuildCompilerSignatureDict(List<IDataCompiler> compilers)
        {
            Dictionary<Hash160, IDataCompiler> signatureDict = new(compilers.Count);

            MemoryStream memoryStream = new();
            BinaryMemoryWriter memoryWriter = new();
            if (memoryWriter.Open(memoryStream, ArchitectureUtils.GetEndianForPlatform(Platform)))
            {
                foreach (var cl in compilers)
                {
                    memoryWriter.Reset();
                    var compilerType = cl.GetType();
                    var compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                    compilerTypeSignature.WriteTo(memoryWriter);
                    cl.CompilerSignature(memoryWriter);
                    var compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                    signatureDict.Add(compilerSignature, cl);
                }
            }

            return signatureDict;
        }

        public void AssignFileId(int unitIndex, List<IDataCompiler> compilers)
        {
            long fileId = unitIndex;
            fileId = fileId << 32;

            var sortedCompilerList = BuildCompilerSignatureList(compilers);
            foreach (var cl in sortedCompilerList)
            {
                cl.Value.CompilerFileIdProvider.FileId = fileId;
                fileId += 1;
            }
        }

        public Result Execute(List<IDataCompiler> compilers, out List<DataCompilerOutput> gdClOutput)
        {
            // Make sure the directory structure of @SrcPath is duplicated at @DstPath
            DirUtils.DuplicateFolderStructure(BuildSystemCompilerConfig.SrcPath, BuildSystemCompilerConfig.DstPath);

            gdClOutput = new(compilers.Count);
            var result = 0;
            foreach (var c in compilers)
            {
                var r = c.CompilerExecute();
                if (r.Result.HasFlag(DataCompilerOutput.EResult.Error))
                    result++;
                else if (!r.Result.HasFlag(DataCompilerOutput.EResult.Ok))
                    result++;
                gdClOutput.Add(r);
            }

            // TODO Need to be able to determine
            // - source files out of date
            // - destination files missing or out of date
            // - compiler version mismatch
            // - compiler bundle out of date

            if (result == 0)
            {
                return Result.Ok;
            }

            return Result.OutOfData;
        }

        private void RegisterCompilers(List<IDataCompiler> compilers)
        {
            foreach (var cl in compilers)
            {
                var type = cl.GetType();
                var typeSignature = HashUtility.Compute_ASCII(type.FullName);
                mCompilerTypeSet.TryAdd(typeSignature, type);
            }
        }

        public Result Save(List<IDataCompiler> cl)
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
                    var compilerType = compiler.GetType();
                    var compilerTypeSignature = HashUtility.Compute_ASCII(compilerType.FullName);
                    compilerTypeSignature.WriteTo(memoryWriter);
                    compiler.CompilerSignature(memoryWriter);
                    var compilerSignature = HashUtility.Compute(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);

                    // byte[4]: Length of Block
                    // byte[20]: GameDataCompiler Type Signature
                    // byte[20]: GameDataCompiler Signature
                    // byte[]: GameDataCompiler Property Data

                    memoryWriter.Reset();
                    compilerTypeSignature.WriteTo(memoryWriter);
                    compilerSignature.WriteTo(memoryWriter);
                    compiler.CompilerWrite(memoryWriter);
                    writer.Write(memoryStream.Length);
                    writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                }

                memoryWriter.Close();
                writer.Close();
                return Result.Ok;
            }

            writer.Close();
            return Result.Error;
        }

        public bool Load(List<IDataCompiler> compilers)
        {
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
                    var compiler = Activator.CreateInstance(type) as IDataCompiler;
                    if (mCompilerSignatureSet.Add(compilerSignature))
                    {
                        compilers.Add(compiler);
                    }

                    compiler.CompilerRead(reader);
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
        public static readonly Result OutOfData = new() { ResultValue = (int)ResultEnum.OutOfDate };
        public static readonly Result Error = new() { ResultValue = (int)ResultEnum.Error };

        public static Result FromRaw(int b)
        {
            return new() { ResultValue = (int)(b & 0x3) };
        }

        public bool IsOk => ResultValue == 0;
        public bool IsNotOk => ResultValue != 0;
        public bool IsOutOfData => ((int)ResultValue & (int)(ResultEnum.OutOfDate)) != 0;
        public bool IsError => ((int)ResultValue & (int)(ResultEnum.Error)) != 0;

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
