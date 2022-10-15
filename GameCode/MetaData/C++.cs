using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

using Int8 = System.SByte;
using UInt8 = System.Byte;
using GameCore;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace GameData
{
    using MetaCode;

	// CodeStream for generating C++ header file(s) containing structs that map to 'data'
	public class CppCodeStream
	{
		#region Class and ClassMember data writer

		public class DataStreamWriter : IMemberWriter
		{
			private readonly StringTable mStringTable;
			private readonly FileIdTable mFileIdTable;
			private readonly CppDataStream mDataStream;

			public DataStreamWriter(StringTable stringTable, FileIdTable fileIdTable, CppDataStream dataStream)
			{
				mStringTable = stringTable;
				mFileIdTable = fileIdTable;
				mDataStream = dataStream;
			}

			public bool Open()
			{
				mDataStream.BeginBlock(16);
				return true;
			}
			public bool Close()
			{
				mDataStream.EndBlock();
				return true;
			}

			public bool WriteNullMember(NullMember c)
			{
				mDataStream.Write(0);
				return true;
			}
			public bool WriteBool8Member(BoolMember c)
			{
				mDataStream.Write(c.InternalValue ? 0 : 1);
				return true;
			}
			public bool WriteInt8Member(Int8Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteInt16Member(Int16Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteInt32Member(Int32Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteInt64Member(Int64Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteUInt8Member(UInt8Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteUInt16Member(UInt16Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteUInt32Member(UInt32Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteUInt64Member(UInt64Member c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteFloatMember(FloatMember c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteDoubleMember(DoubleMember c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteStringMember(StringMember s)
            {
                int length = mStringTable.Add(s.InternalValue);
                var reference = mStringTable.ReferenceOf(s.InternalValue);
                mDataStream.Write(length);
                mDataStream.Write(reference);
				return true;
			}
			public bool WriteFileIdMember(FileIdMember c)
			{
				mDataStream.Write(c.InternalValue);
				return true;
			}
			public bool WriteArrayMember(ArrayMember c)
			{
				mDataStream.Write(c.Members.Count);
				if (c.Reference != StreamReference.Empty)
				{
					mDataStream.Write(c.Reference);
				}
				else
				{
					c.Reference = mDataStream.BeginBlock(c.Alignment);
					{
						foreach (IClassMember m in c.Members)
							m.Write(this);
					}
					mDataStream.EndBlock();
					mDataStream.Write(c.Reference);
				}
				return true;
			}
			public bool WriteObjectMember(ClassObject c)
			{
				if (c.Reference != StreamReference.Empty)
				{
					mDataStream.Write(c.Reference);
				}
				else
                {
                    c.Reference = mDataStream.BeginBlock(c.Alignment);
					{
						foreach (IClassMember m in c.Members)
							m.Write(this);
					}
					mDataStream.EndBlock();
					mDataStream.Write(c.Reference);
				}
				return true;
			}
			public bool WriteAtomMember(AtomMember c)
			{
				return c.Member.Write(this);
			}
			public bool WriteCompoundMember(CompoundMember c)
			{
				if (c.IsNullType)
				{
					if (c.Reference != StreamReference.Empty)
					{
						mDataStream.Write(c.Reference);
					}
					else
					{
						c.Reference = mDataStream.BeginBlock(c.Alignment);
						{
							foreach (IClassMember m in c.Members)
								m.Write(this);
						}
						mDataStream.EndBlock();
						mDataStream.Write(c.Reference);
					}
				}
				else
				{
					foreach (IClassMember m in c.Members)
						m.Write(this);
				}
				return true;
			}
		}

		#endregion
		#region C++ Struct Member Writer

		public class MemberWriter : IMemberWriter
		{
			private StreamWriter mWriter;

			public MemberWriter()
			{
				mWriter = null;
			}

			public MemberWriter(StreamWriter writer)
			{
				mWriter = writer;
			}

			public void SetWriter(StreamWriter writer)
			{
				mWriter = writer;
			}

			public bool Open()
			{
				return true;
			}

			public bool Close()
			{
				return true;
			}

			public bool Write(string type, string name, StreamWriter writer)
			{
                writer.WriteLine($"\t{type} const {name};");
				return true;
			}

			public bool WriteNullMember(NullMember c)
			{
				return Write("void*", c.Name, mWriter);
			}
			public bool WriteBool8Member(BoolMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteInt8Member(Int8Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteInt16Member(Int16Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteInt32Member(Int32Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteInt64Member(Int64Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteUInt8Member(UInt8Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteUInt16Member(UInt16Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteUInt32Member(UInt32Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteUInt64Member(UInt64Member c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteFloatMember(FloatMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteDoubleMember(DoubleMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteStringMember(StringMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteFileIdMember(FileIdMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteArrayMember(ArrayMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteObjectMember(ClassObject c)
			{
				return Write(c.Type.TypeName + "*", c.Name, mWriter);
			}
			public bool WriteAtomMember(AtomMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
			public bool WriteCompoundMember(CompoundMember c)
			{
				return Write(c.Type.TypeName, c.Name, mWriter);
			}
		}

		#endregion
		#region C++ Code writer

		public class CppCodeWriter
		{
			private readonly MemberWriter mMemberWriter = new();

            // TODO
            // Write out also any used Enums

			public bool Write(ClassObject c, StreamWriter writer)
			{
				mMemberWriter.SetWriter(writer);

				writer.WriteLine($"struct {c.Name}");
				writer.WriteLine("{");

                // write public getters

                // write private members
				foreach (IClassMember m in c.Members)
				{
					m.Write(mMemberWriter);
				}

                writer.WriteLine("};");

                writer.WriteLine();
				return true;
			}

			public void Write(List<ClassObject> classes, StreamWriter writer)
			{
				Dictionary<string, ClassObject> writtenClasses = new();
				foreach (ClassObject c in classes)
				{
					if (!writtenClasses.ContainsKey(c.Type.TypeName))
					{
						Write(c, writer);
						writtenClasses.Add(c.Type.TypeName, c);
					}
				}
			}
		}

		#endregion

        #region MemberBook

        class MyMemberBook : MemberBook
        {
            public void HandoutReferences()
            {
                // Handout StreamReferences to classes, compounds and arrays taking care of equality of these objects.
                // Note: Strings are a bit of a special case since we also will collect the names of members and classes.

                Dictionary<object, StreamReference> referencesForClassesDict = new();
                foreach (ClassObject c in Classes)
                {
                    if (c.Value != null)
                    {
                        if (referencesForClassesDict.TryGetValue(c.Value, out var reference))
                        {
                            c.Reference = reference;
                        }
                        else
                        {
                            c.Reference = StreamReference.NewReference;
                            referencesForClassesDict.Add(c.Value, c.Reference);
                        }
                    }
                    else
                    {
                        c.Reference = StreamReference.Empty;
                    }
                }

                Dictionary<object, StreamReference> referencesForCompoundsDict = new();
                foreach (CompoundMember c in Compounds)
                {
                    if (c.Value != null)
                    {
                        if (referencesForCompoundsDict.TryGetValue(c.Value, out var reference))
                        {
                            c.Reference = reference;
                        }
                        else
                        {
                            c.Reference = StreamReference.NewReference;
                            referencesForCompoundsDict.Add(c.Value, c.Reference);
                        }
                    }
                    else
                    {
                        c.Reference = StreamReference.Empty;
                    }
                }

                Dictionary<object, StreamReference> referencesForArraysDict = new();
                foreach (ArrayMember a in Arrays)
                {
                    if (a.Value != null)
                    {
                        if (referencesForArraysDict.TryGetValue(a.Value, out var reference))
                        {
                            a.Reference = reference;
                        }
                        else
                        {
                            a.Reference = StreamReference.NewReference;
                            referencesForArraysDict.Add(a.Value, a.Reference);
                        }
                    }
                    else
                    {
                        a.Reference = StreamReference.Empty;
                    }
                }
            }
        }

        #endregion

        // Save binary data and C code for mapping to the data

        // C/C++ code:
        //   - Endian
        //   - Enums
        //   - Member memory alignment
        //   - Database of written references, objects, arrays, strings
        //     - For emitting an object once as well as terminating circular references
        //   - C# class hierarchy is collapsed to one C++ class
        //   - Duplicate data (strings, arrays)

        // Need to define
        // - String data representation (struct string_t)
        //   - string_t { u32 const mLength; u32 const mStr; }
        // - LString data representation (lstring_t = uint64_t)
        // - FileId data representation (fileid_t = uint64_t)
        // - Array data representation (template<T> array_t { u32 const mSize; u32 const mArray; })
        // - Array of Array of Array of String ?
        // - Embedding a struct or class will result in a pointer to that struct/class

        // Defined: (big/little endian)
        // double       -> 8 byte
        // float        -> 4 byte
        // fx32         -> 4 byte
        // fx16         -> 2 byte
        // ulong/long   -> 8 byte
        // uint/int     -> 4 byte
        // ushort/short -> 2 byte
        // byte         -> 1 byte

        // We will use a ResourceDataWriter for writing the resource data as binary data
        // Exporting every class as a struct in C/C++ using a ClassWriter providing enough
        // functionality to write any kind of class, function and member.

        public static void Write(EEndian endian, object data, string dataFilename, string codeFilename, string relocFilename)
        {
            // Analyze Data.Root and generate a list of 'Code.Class' objects from this.
            IMemberGenerator genericMemberGenerator = new GenericMemberGenerator();

            Reflector reflector = new(genericMemberGenerator);

            MyMemberBook book = new();
            reflector.Analyze(data, book);

            // Sort the members on every 'Code.Class' so that alignment of data is solved.
            foreach (ClassObject c in book.Classes)
                c.SortMembers(new SortByMemberAlignment());

            // Insert padding members to be correctly aligning members
            foreach (ClassObject c in book.Classes)
                c.FixMemberAlignment();

            // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
            StringTable stringTable = new();
            FileIdTable fileIdTable = new();

            // Compile every 'Code.Class' to the DataStream.
            CppDataStream dataStream = new(endian);
            GameData.CppCodeStream.DataStreamWriter dataStreamWriter = new(stringTable, fileIdTable, dataStream);
            dataStreamWriter.Open();
            {
                ClassObject bookRoot = book.Classes[0];
                bookRoot.Write(dataStreamWriter);
            }
            dataStreamWriter.Close();

            // Finalize the DataStream and obtain a database of the position of the
            // IReferenceableMember objects in the DataStream.
            FileInfo dataFileInfo = new(dataFilename);
            FileStream dataFileStream = new(dataFileInfo.FullName, FileMode.Create);
            IBinaryWriter dataFileStreamWriter = EndianUtils.CreateBinaryWriter(dataFileStream, endian);
            FileInfo relocFileInfo = new(relocFilename);
            FileStream relocFileStream = new(relocFileInfo.FullName, FileMode.Create);
            IBinaryWriter relocFileStreamWriter = EndianUtils.CreateBinaryWriter(relocFileStream, endian);
            dataStream.Finalize(dataFileStreamWriter);
            dataFileStreamWriter.Close();
            dataFileStream.Close();
            relocFileStreamWriter.Close();
            relocFileStream.Close();

            // Generate the c++ code using the CppCodeWriter.
            FileInfo codeFileInfo = new(codeFilename);
            FileStream codeFileStream = codeFileInfo.Create();
            StreamWriter codeFileStreamWriter = new(codeFileStream);
            GameData.CppCodeStream.CppCodeWriter codeWriter = new ();
            codeWriter.Write(book.Classes, codeFileStreamWriter);
            codeFileStreamWriter.Close();
            codeFileStream.Close();
        }
	}

	/// <summary>
	/// A CppDataStream is used to write DataBlocks, DataBlocks are stored and when
	/// the final data is written identical (Hash) DataBlocks are collapsed to one.
	/// All references (pointers to blocks) are also resolved at the final stage.
	///
	/// Output: a database of the offset of every reference (DataBlock)
	///
	/// </summary>
	public class CppDataStream
	{
		#region DataBlock

		private class DataBlock
		{
			private StreamReference mReference;
			private readonly MemoryStream mDataStream = new();
			private readonly IBinaryWriter mDataWriter;

			private readonly Dictionary<StreamReference, List<StreamOffset>> mPointers = new();

			internal DataBlock(EEndian inEndian)
			{
				mReference = StreamReference.NewReference;
				mDataWriter = EndianUtils.CreateBinaryWriter(mDataStream, inEndian);
			}

			internal StreamReference Reference => mReference;

            public void Align(Int64 align)
			{
				StreamUtils.Align(mDataWriter, align);
			}

			private Int64 Position => mDataStream.Position;

            internal int Size => (int)mDataStream.Length;

            internal void Write(float v)
			{
				Debug.Assert(Alignment.IsAligned(mDataWriter.Position, sizeof(float)));
				mDataWriter.Write(v);
			}
			internal void Write(double v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(double)));
				mDataWriter.Write(v);
			}
			internal void Write(SByte v)
			{
				mDataWriter.Write(v);
			}
			internal void Write(Int16 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int16)));
				mDataWriter.Write(v);
			}
			internal void Write(Int32 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int32)));
				mDataWriter.Write(v);
			}
			internal void Write(Int64 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(Int64)));
				mDataWriter.Write(v);
			}
			internal void Write(Byte v)
			{
				mDataWriter.Write(v);
			}
			internal void Write(UInt16 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt16)));
				mDataWriter.Write(v);
			}
			internal void Write(UInt32 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt32)));
				mDataWriter.Write(v);
			}
			internal void Write(UInt64 v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt64)));
				mDataWriter.Write(v);
			}
			internal void Write(StreamReference v)
			{
				Debug.Assert(StreamUtils.Aligned(mDataWriter, sizeof(UInt32)));

				if (mPointers.ContainsKey(v))
				{
					mPointers[v].Add(new StreamOffset(Position));
				}
				else
				{
					List<StreamOffset> offsets = new() { new StreamOffset(Position) };
                    mPointers.Add(v, offsets);
				}

				mDataWriter.Write(0);
			}

			internal Hash160 ComputeHash()
			{
				Hash160 dataHash = HashUtility.compute(mDataStream);
				return dataHash;
			}

			internal void ReplaceReference(StreamReference oldRef, StreamReference newRef)
			{
				if (mReference == oldRef)
					mReference = newRef;

				if (mPointers.ContainsKey(oldRef))
				{
					List<StreamOffset> oldOffsets = mPointers[oldRef];
					mPointers.Remove(oldRef);

					// Modify data
					StreamOffset currentPos = new(mDataStream.Position);
					foreach (StreamOffset o in oldOffsets)
					{
						mDataWriter.Seek(o);
						mDataWriter.Write(0);
					}
					mDataWriter.Seek(currentPos);

					// Update pointer and offsets
					if (mPointers.ContainsKey(newRef))
					{
						List<StreamOffset> newOffsets = mPointers[newRef];
						foreach (StreamOffset o in oldOffsets)
							newOffsets.Add(o);
					}
					else
					{
						mPointers.Add(newRef, oldOffsets);
					}
				}
			}

			internal void WriteTo(IBinaryWriter outData, IDictionary<StreamReference, StreamOffset> dataOffsetDataBase)
			{
				StreamOffset currentPos = new(mDataStream.Position);
				foreach (KeyValuePair<StreamReference, List<StreamOffset>> k in mPointers)
				{
					if (dataOffsetDataBase.TryGetValue(k.Key, out StreamOffset outDataOffset))
					{
						foreach (StreamOffset o in k.Value)
						{
							mDataWriter.Seek(o); // Seek to the position that has a 'StreamReference'

                            // TODO
                            // Assert when the offset is out of bounds (-2GB < offset < 2GB)

                            mDataWriter.Write(outDataOffset.Offset32); // This needs to be a relative (signed) offset to the class/array/string
						}
					}
				}
				mDataWriter.Seek(currentPos);

				// Write data
				outData.Write(mDataStream.GetBuffer());
			}
		}

		#endregion

		#region Fields

		private readonly List<DataBlock> mData = new();
		private readonly Stack<DataBlock> mStack = new();

		private EEndian Endian { get; set; }
		private DataBlock Current { get; set; } = null;

		#endregion
		#region Constructor

		public CppDataStream(EEndian endian)
		{
			Endian = endian;
		}

		#endregion
		#region Methods

		// Return data block index
		public StreamReference BeginBlock(Int64 alignment)
		{
			if (Current != null)
				mStack.Push(Current);

			Current = new(Endian);
			mData.Add(Current);
			return Current.Reference;
		}

		public void Write(float v)
		{
			Current.Write(v);
		}
		public void Write(double v)
		{
			Current.Write(v);
		}

		public void Write(sbyte v)
		{
			Current.Write(v);
		}
		public void Write(short v)
		{
			Current.Write(v);
		}
		public void Write(int v)
		{
			Current.Write(v);
		}
		public void Write(Int64 v)
		{
			Current.Write(v);
		}
		public void Write(byte v)
		{
			Current.Write(v);
		}
		public void Write(ushort v)
		{
			Current.Write(v);
		}
		public void Write(uint v)
		{
			Current.Write(v);
		}
		public void Write(UInt64 v)
		{
			Current.Write(v);
		}
		public void Write(StreamReference v)
		{
			Current.Write(v);
		}
		public void EndBlock()
		{
			if (mStack.Count > 0)
				Current = mStack.Pop();
			else
				Current = null;
		}

		public void Finalize(IBinaryWriter dataWriter)
		{
			// Dictionary for mapping a Reference object to a Data object
			Dictionary<StreamReference, DataBlock> dataDataBase = new();
			foreach (DataBlock d in mData)
				dataDataBase.Add(d.Reference, d);

			Dictionary<StreamReference, DataBlock> finalDataDataBase = new();
			foreach (DataBlock d in mData)
				finalDataDataBase.Add(d.Reference, d);

			// For all blocks, calculate Hash
			// Collapse identical blocks, and when a collapse has occurred we have
			// to re-iterate again since a collapse changes the Hash of a block.
			bool collapse = true;
			while (collapse)
			{
				Dictionary<StreamReference, List<StreamReference>> duplicateDataBase = new();
				Dictionary<Hash160, StreamReference> dataHashDataBase = new();

				foreach (DataBlock d in mData)
				{
					Hash160 md5 = d.ComputeHash();
					if (dataHashDataBase.ContainsKey(md5))
					{
						// Encountering a block of data which has a duplicate.
						// After the first iteration it might be the case that
						// they have the same 'Reference' since they are collapsed.
						StreamReference newRef = dataHashDataBase[md5];
						if (d.Reference != newRef)
						{
							if (!duplicateDataBase.ContainsKey(newRef))
							{
								if (finalDataDataBase.ContainsKey(d.Reference))
								{
									List<StreamReference> duplicateReferences = new() { d.Reference };
                                    duplicateDataBase[newRef] = duplicateReferences;
								}
							}
							else
							{
								if (finalDataDataBase.ContainsKey(d.Reference))
									duplicateDataBase[newRef].Add(d.Reference);
							}
							finalDataDataBase.Remove(d.Reference);
						}
					}
					else
					{
						// This block of data is unique
						dataHashDataBase.Add(md5, d.Reference);
					}
				}

				foreach (KeyValuePair<StreamReference, List<StreamReference>> p in duplicateDataBase)
				{
					foreach (StreamReference r in p.Value)
					{
						foreach (DataBlock d in mData)
						{
							d.ReplaceReference(r, p.Key);
						}
					}
				}

				// Did we find any duplicates, if so then we also replaced references
				// and by doing so MD5 hashes have changed.
				// Some blocks now might have an identical MD5 due to this.
				collapse = duplicateDataBase.Count > 0;
			}

			// Resolve block references again
			Dictionary<StreamReference, StreamOffset> dataOffsetDataBase = new();

			int offset = 0;
			foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataDataBase)
			{
				dataOffsetDataBase.Add(k.Key, new StreamOffset(offset));
				offset += k.Value.Size;
			}

			// Dump all blocks to outData
			// Dump all reallocation info to outReallocationTable
			// Patch the location of every reference in the memory stream!
			foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataDataBase)
			{
				k.Value.WriteTo(dataWriter, dataOffsetDataBase);
			}
		}

		#endregion
	}

}
