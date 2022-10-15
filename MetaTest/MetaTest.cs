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

        public class TestRoot
        {
            public Int32 mInt32 = 1;
            public SByte mInt8 = 2;
            public float mFloat = 3.14f;

            public TestData mData = new();
        }

        public class TestData
        {
            public string Name = "A test string";
            public FileId File = new FileId(new TestFileIdProvider { FileId = 1 });
        }


        static int Main(string[] args)
        {
            CppCodeStream.Write(EEndian.LITTLE, new TestRoot(), "metatest.cdd", "metatest.h", "metatest.cdr");

            return Success();
        }

        #endregion
    }
}

