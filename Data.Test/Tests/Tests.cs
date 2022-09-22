using System;

namespace GameData
{
    static class Variables
    {
        static public Int32 mSpeedDuration = 10;
        static public Int32 mSpeedFactor = (int)(-1 * 55 * Variables.mSpeedDuration * Math.Sin(125) * Math.Sqrt(78));
    }
	
	public class CompoundExample : ICompound
	{
		private string test1 = "Test";
		private float test2 = 1.12f;
		
		public Array Values { get { return new object[] { test1, test2 }; } }
	}

    public class Tests : IDataUnit
    {
        [Comment("This boolean should be True")]
        public bool TestBool = true;

        public FileId TestModel = new FileId(new CopyCompiler(@"Tests\Test.dat"));

        public Int32 TestInt = Variables.mSpeedFactor;
        public Fx16 Testfx16 = new Fx16(2.0f);

       	public FVector2 testFVector2 = new FVector2(0.0, 1.0);
       	public FVector3 testFVector3 = new FVector3(0.0, 1.0, 2.0);
       	public FVector4 testFVector4 = new FVector4(0.0, 1.0, 2.0, 3.0);
		public CompoundExample testCompound = new CompoundExample();

       	public FVector2[] testFVector2Array = { new FVector2(0.0, 1.0), new FVector2(0.0, 1.0) };
       	public FVector3[] testFVector3Array = { new FVector3(0.0, 1.0, 2.0), new FVector3(0.0, 1.0, 2.0) };
       	public FVector4[] testFVector4Array = { new FVector4(0.0, 1.0, 2.0, 3.0), new FVector4(0.0, 1.0, 2.0, 3.0) };
       	
        [Comment("An Fx32 with a Range")]
        [Fx32Range(0.0f, 10.0f)]
        public Fx32 Testfx32 = new Fx32(2.0f);

        public string TestString = "This is a test";

        public bool[] TestBoolArray = { true, false, true };
        public Int32[] TestIntArray = { 111111, 222222, 333333 };
        public Fx16[] Testfx16Array = Fx16.sArray(1.0f, 2.0f, 3.0f);
        public Fx32[] Testfx32Array = Fx32.sArray(3.0f, 2.0f, 1.0f);
        public Int64[] TestInt64Array = { 9999999111111, 9999999222222, 9999999333333 };

        public LString TestLocId = new LString("LOC_MENU_CHAMPIONSHIP");
        public LString[] TestLocIdArray = LString.sArray("LOC_MENU_CHAMPIONSHIP", "LOC_MENU_CHAMPIONSHIP");

        public Test Test = new Test();
        public Test[] TestArray = new Test[] { new DerivedTest(), new DerivedTest(), new DerivedTest() };
    }

    public class Test
    {
        public bool TestBool = false;
        public Int32 TestInt = 123444;
        public Fx16 Testfx16 = new Fx16(2.5f);
        public Fx32 Testfx32 = new Fx32(2.5f);
        public string TestString = "This is a test2";

        public bool[] TestBoolArray = { true, false, true };
        public Int32[] TestIntArray = { 111111, 222222, 333333 };
        public Fx16[] Testfx16Array = Fx16.sArray(1.0f, 2.0f, 3.0f);
        public Fx32[] Testfx32Array = Fx32.sArray(3.0f, 2.0f, 1.0f);
        public float[] TestFloatArray = { 3.3f, 2.2f, 1.1f };

        public bool[] TestBools = { true, false, true, false };
        public Int32[] TestArray = { 88228833 };
    }

    public class DerivedTest : Test
    {
        public bool TestBool2 = false;

        public DerivedTest()
        {
            // Overriding some values
            TestBool = false;
        }
    }

}