using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Loader;

using GameCore;
using GameData;
using GameData.MetaCode;

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

        public enum ETestEnum : uint
        {
            EnumerationA = 0xFFFF0000,
            EnumerationB = 0xFFFF0001,
            EnumerationC = 0xFFFF0002,
            EnumerationD = 0xFFFF0003,
        }

        public class TestDataUnit : IDataUnit
        {
            public string UnitId => "TestDataUnit";

            public float m_Float = 3.14f;
            public int m_Int = 1;
            public bool m_Bool1 = true;
            public bool m_Bool2 = false;
            public bool m_Bool3 = true;
        }

        public class TestRoot : IRootDataUnit
        {
            public string UnitId => "TestRoot";

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
            public TestData m_Data = new(); // Pointer
        }

        public class TestArrayElement
        {
            public int Int = 1;
            public float Float = 2;
        }

        public class TestData
        {
            public string Name = "A test string";
            public DataFile File = new DataFile(Hash160.FromDateTime(DateTime.Now), "TestFile");

            public float[] Floats = new float[8];
            public List<long> IntegerList = new() { 0,1,2,3,4 };

			// The classes/structs are serialized in-place (not as a pointer)
			[ArrayElementsInPlace]
            public TestArrayElement[] ObjectArray = new TestArrayElement[2] { new(), new() };

            public long?[] IntPtrArray = new long?[1];

            public TestDataUnit TestDataUnit = new TestDataUnit();
        }

        static int Main(string[] args)
        {
            CppCodeStream2.Write2(EPlatform.Win64, new TestRoot(), "metatest.cdd", "metatest.h");
            return Success();
        }

        #endregion
    }
}

