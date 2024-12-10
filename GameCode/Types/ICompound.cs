using System;

namespace GameData
{
    /// <summary>
    /// The purpose of ICompound is to provide a way for the user to define a type that
    /// can be cast to a mirroring structure in C/C++.
    ///
    /// An ICompound is treated as an object type, and an object type is referencable
    /// (a pointer in C++).
    ///
    ///         class YourClass : ICompound
	///         {
    ///             public int i;
    ///             public float f;
    ///             public bool b;
    ///             Array Values { get { return new object[] {i,f,b}; } }
	///         }
    ///
    /// </summary>
    public interface ICompound
    {
        Array Values { get; }
    }
}
