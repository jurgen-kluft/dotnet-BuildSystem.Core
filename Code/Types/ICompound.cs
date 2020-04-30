using System;

namespace Game.Data
{
    /// <summary>
    /// The purpose of ICompound is to provide a way for the of user to define a type that 
    /// can be casted to a mirroring structure in C/C++.
    /// 
    /// An ICompound is treated as a value-type and not as an object type, and object type
    /// is referencable (a pointer in C++).
    /// 
    /// So if you have a class that you want to cast to a struct/class in C/C++ then you 
    /// need to define it like this:
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
