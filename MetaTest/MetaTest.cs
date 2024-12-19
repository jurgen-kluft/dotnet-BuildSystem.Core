using GameCore;
using GameData;
using BigfileBuilder;

namespace MetaTest
{
    using TestHandle = Int64;

    class Program
    {
        #region Error & Success

        private static int Error()
        {
            return 1;
        }

        private static int Success()
        {
            return 0;
        }

        #endregion

        #region Main

        public class FixedSignature : ISignature
        {
            public Hash160 Signature => HashUtility.Random();
        }

        public enum ETestEnum : uint
        {
            EnumerationA = 0xFFFF0000,
            EnumerationB = 0xFFFF0001,
            EnumerationC = 0xFFFF0002,
            EnumerationD = 0xFFFF0003,
        }

        public class TestDataUnit : IDataUnit
        {
            public string Name => "TestDataUnit";
            public string Signature => "c90d7236-b441-497e-a3a8-c84c17981777";

            public float m_Float = 3.14f;
            public int m_Int = 1;
            public bool m_Bool1 = true;
            public bool m_Bool2 = false;
            public bool m_Bool3 = true;
        }

        public class TestRoot : IRootDataUnit, IDataUnit
        {
            public string Name => "TestRoot";
            public string Signature => "6e65acae-6ba3-48f9-9ea3-c4194ce3103a";

            public float m_Float = 3.14f;
            public int m_Int = 1;
            public bool m_Bool1 = true;
            public bool m_Bool2 = false;
            public bool m_Bool3 = true;
            public bool m_Bool4 = false;
            public sbyte m_Int8 = 2;
            public ETestEnum m_Enum = ETestEnum.EnumerationC;
            public Color m_Color = Colors.Aliceblue;

            public TestHandle? m_Handle = 55; // Pointer
            public TestData m_Data = new();   // Pointer
        }

        public class TestArrayElement
        {
            public int Int = 1;
            public float Float = 2;
        }

        public class TestData
        {
            public string Name = "A test string";
            public DataFile File = new DataFile(new FixedSignature(), "TestFile");

            public float[] Floats = new float[8];
            public List<long> IntegerList = new() { 0, 1, 2, 3, 4 };

            // The classes/structs are serialized in-place (not as a pointer)
            [ArrayElementsInPlace] public TestArrayElement[] ObjectArray = new TestArrayElement[2] { new(), new() };

            public long?[] IntPtrArray = new long?[1];

            public TestDataUnit TestDataUnit = new TestDataUnit();
        }

        public class TestSignatureDb : ISignatureDataBase
        {
            public (uint primary, uint secondary) GetEntry(Hash160 signature)
            {
                return (0, 0);
            }

            public bool Register(Hash160 signature, uint primary, uint secondary)
            {
                return true;
            }

            public void RemovePrimary(uint index)
            {

            }

            public bool Load(string filepath)
            {
                return true;
            }

            public bool Save(string filepath)
            {
                return true;
            }
        }

        static int Main(string[] args)
        {
            const EPlatform platform = EPlatform.Win64;
            var rootDataUnit = new TestRoot();

            var codeFileInfo = new FileInfo(GameDataPath.GameDataCppCode.GetFilePath("TestData"));
            var codeFileStream = codeFileInfo.Create();
            var codeFileWriter = new StreamWriter(codeFileStream);

            var bigfileGameCodeDataFilepath = GameDataPath.GameDataCppData.GetFilePath("TestData");
            var bigfileDataFileInfo = new FileInfo(bigfileGameCodeDataFilepath);
            var bigfileDataStream = new FileStream(bigfileDataFileInfo.FullName, FileMode.Create);
            var bigfileDataStreamWriter = ArchitectureUtils.CreateBinaryFileWriter(bigfileDataStream, platform);

            var signatureDb = new TestSignatureDb();
            var rootDataUnitSignature = HashUtility.Compute_ASCII(rootDataUnit.Signature);
            signatureDb.Register(rootDataUnitSignature, 0, 0);

            CppCodeStream2.Write2(platform, rootDataUnit, codeFileWriter, bigfileDataStreamWriter, signatureDb, out var dataUnitsSignatures, out var dataUnitsStreamPositions, out var dataUnitsStreamSizes);
            bigfileDataStreamWriter.Close();
            bigfileDataStream.Close();
            codeFileWriter.Close();
            codeFileStream.Close();

            var bigfileGameCodeFiles = new BigfileFile[dataUnitsSignatures.Count];
            for (var i = 0; i < dataUnitsStreamPositions.Count; ++i)
            {
                var (_, fileIndex) = signatureDb.GetEntry(dataUnitsSignatures[i]);
                bigfileGameCodeFiles[fileIndex] = new BigfileFile() { Filename = "DataUnit", Offset = dataUnitsStreamPositions[i], Size = dataUnitsStreamSizes[i] };
            }

            var bigfileGameCode = new Bigfile(0, bigfileGameCodeFiles);
            var bigfileGameCodeTocFilepath = GameDataPath.GameDataUnitBigFileToc.GetFilePath("TestData");
            BigfileToc.Save(bigfileGameCodeTocFilepath, new List<Bigfile>() { bigfileGameCode });

            return Success();
        }

        #endregion
    }
}