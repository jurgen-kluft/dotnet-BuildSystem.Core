using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
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
			private readonly CppDataStream mDataStream;

			public DataStreamWriter(StringTable stringTable, CppDataStream dataStream)
			{
				mStringTable = stringTable;
				mDataStream = dataStream;
			}

			public bool Open()
			{
				mDataStream.BeginBlock(8);
				return true;
			}
			public bool Close()
			{
				mDataStream.EndBlock();
				return true;
			}

			public void WriteNullMember(NullMember c)
			{
				mDataStream.Write(0);
			}
			public void WriteBool8Member(BoolMember c)
			{
				mDataStream.Write(c.InternalValue ? 0 : 1);
			}
			public void WriteInt8Member(Int8Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteInt16Member(Int16Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteInt32Member(Int32Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteInt64Member(Int64Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteUInt8Member(UInt8Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteUInt16Member(UInt16Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteUInt32Member(UInt32Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteUInt64Member(UInt64Member c)
			{
				mDataStream.Write(c.InternalValue);
			}
            public void WriteEnumMember(EnumMember c)
            {
                ulong e = c.InternalValue;
                switch (c.Alignment)
                {
                    case 1:  mDataStream.Write((byte)e); break;
                    case 2:  mDataStream.Write((ushort)e); break;
                    case 4:  mDataStream.Write((uint)e); break;
                    case 8:  mDataStream.Write(e); break;
                }
            }
			public void WriteFloatMember(FloatMember c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteDoubleMember(DoubleMember c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteStringMember(StringMember s)
            {
                int length = mStringTable.Add(s.InternalValue);
                var reference = mStringTable.ReferenceOf(s.InternalValue);
                mDataStream.Write(length);
                mDataStream.Write(reference);
			}
			public void WriteFileIdMember(FileIdMember c)
			{
				mDataStream.Write(c.InternalValue);
			}
			public void WriteArrayMember(ArrayMember c)
			{
				if (c.Reference == StreamReference.Empty)
				{
					c.Reference = mDataStream.BeginBlock(c.Alignment);
					{
						foreach (IClassMember m in c.Members)
							m.Write(this);
					}
					mDataStream.EndBlock();
				}
                mDataStream.Write(c.Members.Count);
                mDataStream.Write(c.Reference);
			}
			public void WriteObjectMember(ClassObject c)
			{
				if (c.Reference == StreamReference.Empty)
				{
                    c.Reference = mDataStream.BeginBlock(c.Alignment);
					{
						foreach (IClassMember m in c.Members)
							m.Write(this);
					}
					mDataStream.EndBlock();
				}
                mDataStream.Write(c.Reference);
			}
			public void  WriteStructMember(StructMember c)
			{
				 c.Internal.StructWrite(mDataStream);
			}
		}

		#endregion
		#region C++ Class Member Getter writer

        public class MemberGetterWriter : IMemberWriter
        {
            private StreamWriter mWriter;

            public MemberGetterWriter()
            {
                mWriter = null;
            }

            public MemberGetterWriter(StreamWriter writer)
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

            public void WriteNullMember(NullMember c)
            {
                string line = "\tvoid*\tget" + c.MemberName + "() const { return 0; }";
                mWriter.WriteLine(line);
            }
            public void WriteBool8Member(BoolMember c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteInt8Member(Int8Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteInt16Member(Int16Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteInt32Member(Int32Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteInt64Member(Int64Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteUInt8Member(UInt8Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteUInt16Member(UInt16Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteUInt32Member(UInt32Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteUInt64Member(UInt64Member c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteEnumMember(EnumMember c)
            {
                string line = "\t" + c.EnumType.Name + "\tget" + c.MemberName + "() const {" + c.EnumType.Name + " e; m_" + c.MemberName + ".get(e); return e; }";
                mWriter.WriteLine(line);
            }
            public void WriteFloatMember(FloatMember c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteDoubleMember(DoubleMember c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteStringMember(StringMember c)
            {
                string line = "\tstring_t" + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + ".str(); }";
                mWriter.WriteLine(line);
            }
            public void WriteFileIdMember(FileIdMember c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
            }
            public void WriteArrayMember(ArrayMember c)
            {
                string line = $"\tarray_t<{c.Element.TypeName}>\tget" + c.MemberName + "() const { return m_" + c.MemberName + ".array(); }";
                mWriter.WriteLine(line);
            }
            public void WriteObjectMember(ClassObject c)
            {
                string line = $"\tconst {c.TypeName}*\tget{c.MemberName}() const {{ return m_{c.MemberName}.ptr(); }}";
                mWriter.WriteLine(line);
            }
            public void WriteStructMember(StructMember c)
            {
                string line = "\t" + c.TypeName + "\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                mWriter.WriteLine(line);
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
                writer.WriteLine($"\t{type} const m_{name};");
				return true;
			}

			public void WriteNullMember(NullMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteBool8Member(BoolMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteInt8Member(Int8Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteInt16Member(Int16Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteInt32Member(Int32Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteInt64Member(Int64Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteUInt8Member(UInt8Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteUInt16Member(UInt16Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteUInt32Member(UInt32Member c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
            public void WriteUInt64Member(UInt64Member c)
            {
                Write(c.TypeName, c.MemberName, mWriter);
            }
            public void WriteEnumMember(EnumMember c)
            {
				Write(c.TypeName, c.MemberName, mWriter);
            }
			public void WriteFloatMember(FloatMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteDoubleMember(DoubleMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteStringMember(StringMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteFileIdMember(FileIdMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteArrayMember(ArrayMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
			public void WriteObjectMember(ClassObject c)
			{
				Write($"rawobj_t<{c.TypeName}>", c.MemberName, mWriter);
			}
            public void WriteStructMember(StructMember c)
			{
				Write(c.TypeName, c.MemberName, mWriter);
			}
		}

		#endregion
		#region C++ Code writer

		public class CppCodeWriter
		{
            private readonly MemberGetterWriter mGetterWriter = new();
			private readonly MemberWriter mMemberWriter = new();

            public void WriteEnum(EnumMember e, StreamWriter writer)
            {
                writer.WriteLine($"enum {e.EnumType.Name}");
                writer.WriteLine("{");
                foreach (var en in Enum.GetValues(e.EnumType))
                {
					object val = System.Convert.ChangeType(en, System.Enum.GetUnderlyingType(e.EnumType));
					writer.WriteLine($"\t{Enum.GetName(e.EnumType, en)} = {val},");
                }
                writer.WriteLine("};");
                writer.WriteLine();
            }

            public void Write(List<EnumMember> enums, StreamWriter writer)
            {
                HashSet<string> writtenEnums = new();
                foreach (EnumMember c in enums)
                {
                    if (!writtenEnums.Contains(c.EnumType.Name))
                    {
                        WriteEnum(c, writer);
                        writtenEnums.Add(c.EnumType.Name);
                    }
                }
				writer.WriteLine();
			}

			public bool Write(ClassObject c, StreamWriter writer)
			{
				writer.WriteLine($"class {c.TypeName}");
                writer.WriteLine("{");
                writer.WriteLine("public:");

                // write public getters
                mGetterWriter.SetWriter(writer);
                foreach (IClassMember m in c.Members)
                {
                    m.Write(mGetterWriter);
                }
                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
                mMemberWriter.SetWriter(writer);
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
				classes.Reverse();

				HashSet<string> writtenClasses = new();
				foreach (ClassObject c in classes)
				{
					if (!writtenClasses.Contains(c.TypeName))
					{
						Write(c, writer);
						writtenClasses.Add(c.TypeName);
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
        // bool         -> 4 byte (although many booleans are packed into 32 bits)

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

			// TODO Booleans into bitset_t
			// All bool members of a class should be combined to fall under one or more bitset_t member(s)
			// So basically C# bool members should just take one bit of space.
			// Hmmm, what about an array of bools, should that be converted into a bitarray_t

            // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
            StringTable stringTable = new();

            // Compile every 'Code.Class' to the DataStream.
            CppDataStream dataStream = new(endian);
            DataStreamWriter dataStreamWriter = new(stringTable, dataStream);
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
            IBinaryStream dataFileStreamWriter = EndianUtils.CreateBinaryStream(dataFileStream, endian);
            FileInfo relocFileInfo = new(relocFilename);
            FileStream relocFileStream = new(relocFileInfo.FullName, FileMode.Create);
            IBinaryStream relocFileStreamWriter = EndianUtils.CreateBinaryStream(relocFileStream, endian);
            dataStream.Finalize(dataFileStreamWriter, stringTable);
            dataFileStreamWriter.Close();
            dataFileStream.Close();
            relocFileStreamWriter.Close();
            relocFileStream.Close();

            // Generate the c++ code using the CppCodeWriter.
            FileInfo codeFileInfo = new(codeFilename);
            FileStream codeFileStream = codeFileInfo.Create();
            StreamWriter codeFileStreamWriter = new(codeFileStream);
            CppCodeWriter codeWriter = new ();
			codeWriter.Write(book.Enums, codeFileStreamWriter);
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
    ///

	public class CppDataStream : IBinaryWriter
	{
		#region DataBlock

		private class DataBlock
		{
			private readonly MemoryStream mDataStream = new();
			private readonly IBinaryStream mDataWriter;

			private readonly Dictionary<StreamReference, List<StreamOffset>> mPointers = new();

			internal DataBlock(EEndian inEndian, int alignment)
            {
                Alignment = alignment;
                // TODO Optimization
                // Using MemoryStream with many DataBlock instances is not optimal, this could be
                // seriously optimized
				Reference = StreamReference.NewReference;
				mDataWriter = EndianUtils.CreateBinaryStream(mDataStream, inEndian);
			}

            internal StreamReference Reference { get; set; }
            public int Alignment { get; }

			private Int64 Position => mDataStream.Position;

            internal int Size => (int)mDataStream.Length;

            internal void End()
            {
                int gap = CMath.Align32(Size, Alignment) - Size;

                // Write actual data to reach the size alignment requirement

                int gap4 = gap / 4;
                for (int i = 0; i < gap4; ++i)
                {
                    mDataWriter.Write((uint)0xCDCDCDCD);
                }

                gap -= (gap4 * 4);
                while (gap > 0)
                {
                    mDataWriter.Write((byte)0xCD);
                    gap -= 1;
                }
            }

            internal void Write(float v)
			{
				Debug.Assert(CMath.IsAligned(mDataWriter.Position, sizeof(float)));
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
			internal void Write(byte[] data, int index, int count)
			{
				mDataWriter.Write(data, index, count);
			}
			internal void Write(string str)
			{
				mDataWriter.Write(str);
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

				mDataWriter.Write(v.id);
			}

			internal Hash160 ComputeHash()
			{
				Hash160 dataHash = HashUtility.compute(mDataStream);
				return dataHash;
			}

			internal void ReplaceReference(StreamReference oldRef, StreamReference newRef)
			{
				if (Reference == oldRef)
					Reference = newRef;

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

			internal void WriteTo(IBinaryStream outData, IDictionary<StreamReference, StreamOffset> dataOffsetDataBase)
			{
                StreamUtils.Align(outData, Alignment);

				StreamOffset currentPos = new(mDataStream.Position);
				foreach (KeyValuePair<StreamReference, List<StreamOffset>> k in mPointers)
				{
					if (dataOffsetDataBase.TryGetValue(k.Key, out StreamOffset outDataOffset))
					{
						foreach (StreamOffset o in k.Value)
						{
							// Seek to the position that has the 'StreamReference'
							mDataWriter.Seek(o); 

                            // Write the relative (signed) offset
                            int offset = (int)(outDataOffset.Offset - o.Offset);
                            // TODO: Assert when the offset is out of bounds (-2GB < offset < 2GB)
                            mDataWriter.Write(offset); 
						}
					}
				}
				mDataWriter.Seek(currentPos);

				// Write data
				outData.Write(mDataStream.GetBuffer(), 0, (int)mDataStream.Length);
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

		public StreamReference BeginBlock(int alignment)
		{
			if (Current != null)
				mStack.Push(Current);

			Current = new(Endian, alignment);
			mData.Add(Current);
			return Current.Reference;
		}

        public void EndBlock()
        {
            Current.End();
            if (mStack.Count > 0)
                Current = mStack.Pop();
            else
                Current = null;
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
		public void Write(byte[] data)
		{
			Current.Write(data, 0, data.Length);
		}
		public void Write(byte[] data, int index, int count)
		{
			Current.Write(data, index, count);
		}
		public void Write(string str)
		{
			Current.Write(str);
		}
		public void Write(StreamReference v)
		{
			Current.Write(v);
		}

		public void Finalize(IBinaryStream dataWriter, StringTable stringTable)
		{
			// Dictionary for mapping a Reference object to a Data object
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
					Hash160 hash = d.ComputeHash();
					if (dataHashDataBase.ContainsKey(hash))
					{
						// Encountering a block of data which has a duplicate.
						// After the first iteration it might be the case that
						// they have the same 'Reference' since they are collapsed.
						StreamReference newRef = dataHashDataBase[hash];
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
						dataHashDataBase.Add(hash, d.Reference);
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
				// and by doing so hashes have changed.
				// Some blocks now might have an identical hash due to this.
				collapse = duplicateDataBase.Count > 0;
			}

			// Resolve block references again
			Dictionary<StreamReference, StreamOffset> dataOffsetDataBase = new();

            // Write strings first to the output and for each string remember the stream offset
            stringTable.Write(dataWriter, dataOffsetDataBase);

            Int64 offset = dataWriter.Position;
            foreach (KeyValuePair<StreamReference, DataBlock> k in finalDataDataBase)
            {
                offset = CMath.Align(offset, k.Value.Alignment);
				dataOffsetDataBase.Add(k.Key, new (offset));
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
