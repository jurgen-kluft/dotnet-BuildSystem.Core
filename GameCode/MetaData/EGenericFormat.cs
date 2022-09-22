namespace GameData
{
    public enum EGenericFormat
    {
        CPP_KEEP,

        STD_FLAT,
        /// <example name=FLAT>
        ///     Explanation of KEEP
        ///     
        ///     class A
        ///     {
        ///         int a = 1;
        ///         int b = 2;
        ///     }
        ///     
        ///     class B : A
        ///     {
        ///         int c = 3;
        ///     }
        ///     
        ///     class C : B
        ///     {
        ///         int d = 4;
        ///         C()
        ///         {
        ///             a = 81;
        ///             b = 82;
        ///         }
        ///     }
        ///     
        ///     Will be emitted as one object, e.g.
        ///     
        ///     class OBJECT
        ///     {
        ///         int a = 1;
        ///         int b = 2;
        ///         int c = 3;
        ///         int d = 4;
        ///     }
        /// </example>

        STD_KEEP,
        /// <example name=KEEP>
        ///     class A
        ///     {
        ///         int a = 1;
        ///         int b = 2;
        ///     }
        ///     
        ///     class B : A
        ///     {
        ///         int c = 3;
        ///     }
        ///     
        ///     class C : B
        ///     {
        ///         int d = 4;
        ///         C()
        ///         {
        ///             a = 81;
        ///             b = 82;
        ///             c = 83;
        ///         }
        ///     }
        ///     
        ///     Will be emitted as 3 objects, e.g.
        ///     
        ///     class OBJECT_A
        ///     {
        ///         object* base = null;
        ///         int a = 81;
        ///         int b = 82;
        ///     }
        /// 
        ///     class OBJECT_B
        ///     {
        ///         object* base = OBJECT_A;
        ///         int c = 83;
        ///     }
        /// 
        ///     class OBJECT_C
        ///     {
        ///         object* base = OBJECT_B;
        ///         int d = 4;
        ///     }
        /// </example>


        STD_KEEP_MITIGATE,
        /// <example name=KEEP_MITIGATE>
        /// 
        /// As you can see in this example, objects OBJECT_A and OBJECT_B are emitted with their default values. 
        /// This is good for the collapsing identical data in general.
        /// 
        ///     class A
        ///     {
        ///         int a = 1;
        ///         int b = 2;
        ///     }
        ///     
        ///     class B : A
        ///     {
        ///         int c = 3;
        ///     }
        ///     
        ///     class C : B
        ///     {
        ///         int d = 4;
        ///         C()
        ///         {
        ///             a = 81;
        ///             b = 82;
        ///         }
        ///     }
        ///     
        ///     Will be emitted as 3 objects, e.g. Observe that OBJECT_A and OBJECT_B will be 'standard' objects and 
        ///     will be emitted the same for any other derived class.
        ///     Member 'c' is not included in OBJECT_C, but when requested will divert to OBJECT_B through the base
        ///     pointer in OBJECT_C.
        ///     
        ///     class OBJECT_A
        ///     {
        ///         object* base = null;
        ///         int a = 1;
        ///         int b = 2;
        ///     }
        /// 
        ///     class OBJECT_B
        ///     {
        ///         object* base = OBJECT_A;
        ///         int c = 3;
        ///     }
        /// 
        ///     class OBJECT_C
        ///     {
        ///         object* base = OBJECT_B;
        ///         int a = 81;
        ///         int b = 82;
        ///         int d = 4;
        ///     }
        /// </example>
    }
}
