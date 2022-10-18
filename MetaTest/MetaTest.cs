using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Loader;

using GameCore;
using GameData;

namespace MetaTest
{
    class Program
    {
        #region Error & Success

        static int Error()
        {
            return 1;
        }

        static int Success()
        {
            return 0;
        }

        #endregion
        #region Main

        public class TestFileIdProvider : IFileIdProvider
        {
            public Int64 FileId { get; set; }
        }

        public enum ETestEnum : uint
        {
            EnumerationA = 0xFFFF0000,
            EnumerationB = 0xFFFF0001,
            EnumerationC = 0xFFFF0002,
            EnumerationD = 0xFFFF0003,
        }

        public class TestRoot
        {
            public bool m_Bool = false;
            public Int32 m_Int32 = 1;
            public SByte m_Int8 = 2;
            public float m_Float = 3.14f;
            public ETestEnum m_Enum = ETestEnum.EnumerationC;
            public Color m_Color = Colors.Aliceblue;

            public TestData m_Data = new();
        }

        public class TestData
        {
            public string Name = "A test string";
            public FileId File = new FileId(new TestFileIdProvider { FileId = 1 });

            public float[] Floats = new float[8];
            public List<Int64> IntegerList = new() { 0,1,2,3,4 };
        }


        static int Main(string[] args)
        {
            CppCodeStream.Write(EEndian.LITTLE, new TestRoot(), "metatest.cdd", "metatest.h", "metatest.cdr");

            return Success();
        }

        #endregion
    }
}

