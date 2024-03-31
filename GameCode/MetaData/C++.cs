using System.Diagnostics;
using GameCore;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace GameData
{
    using MetaCode;

	// CodeStream for generating C++ header file(s) containing structs that map to 'data'
	public class CppCodeStream
	{
		#region Class and ClassMember data writer

		private sealed class DataStreamWriter : IMemberWriter
		{
			private readonly StringTable _mStringTable;
			private readonly CppDataStream _mDataStream;

			public DataStreamWriter(StringTable stringTable, CppDataStream dataStream)
			{
				_mStringTable = stringTable;
				_mDataStream = dataStream;
			}

			public bool Open()
			{
				_mDataStream.BeginBlock(8);
				return true;
			}
			public bool Close()
			{
				_mDataStream.EndBlock();
				return true;
			}

            private void AlignTo(int alignment)
            {
                _mDataStream.AlignTo(alignment);
            }

			public void WriteNullMember(NullMember c)
			{
				_mDataStream.Write(0);
			}

            private void Write(IClassMember c, Action<StreamReference> aSetReference, Func<bool> aWrite)
            {
                if (c.IsPointerTo)
                {
                    // If as a member we are a pointer to the type then we need to make sure we have
                    // stream out our value so that we have a reference (pointer) to it.
                    var streamReference = c.Reference;
                    if (streamReference == StreamReference.Empty)
                    {
                        streamReference = _mDataStream.BeginBlock(c.Alignment);
                        {
                            aSetReference(streamReference);
                            aWrite();
                        }
                        _mDataStream.EndBlock();
                    }

                    // This is our pointer
                    _mDataStream.Write(streamReference);
                }
                else
                {
                    aWrite();
                }
            }

            public void WriteBoolMember(BoolMember c)
            {
                Write(c, streamReference => c.Reference = streamReference, delegate {
                    _mDataStream.Write(c.InternalValue ? 0 : 1);
                    return true;
                });
            }

            public void WriteBitSetMember(BitSetMember c)
            {
	            Write(c, streamReference => c.Reference = streamReference, delegate {
		            _mDataStream.Write(c.InternalValue);
		            return true;
	            });
            }

            public void WriteInt8Member(Int8Member c)
            {
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
            }
			public void WriteInt16Member(Int16Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteInt32Member(Int32Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteInt64Member(Int64Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteUInt8Member(UInt8Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteUInt16Member(UInt16Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteUInt32Member(UInt32Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteUInt64Member(UInt64Member c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
            public void WriteEnumMember(EnumMember c)
            {
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    var e = c.InternalValue;
                    switch (c.Alignment)
                    {
                        case 1:
                            _mDataStream.Write((byte)e);
                            break;
                        case 2:
                            AlignTo(2);
                            _mDataStream.Write((ushort)e);
                            break;
                        case 4:
                            AlignTo(4);
                            _mDataStream.Write((uint)e);
                            break;
                        case 8:
                            AlignTo(8);
                            _mDataStream.Write(e);
                            break;
                    }

                    return true;
                });
            }
			public void WriteFloatMember(FloatMember c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteDoubleMember(DoubleMember c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
			public void WriteStringMember(StringMember c)
            {
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    var length = _mStringTable.Add(c.InternalValue);
                    var reference = _mStringTable.ReferenceOf(c.InternalValue);
                    AlignTo(c.Alignment);
                    _mDataStream.Write(reference);
                    _mDataStream.Write(length);
                    return true;
                });
			}
			public void WriteFileIdMember(FileIdMember c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.InternalValue);
                    return true;
                });
			}
            public void  WriteStructMember(StructMember c)
            {
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    c.Internal.StructWrite(_mDataStream);
                    return true;
                });
            }
			public void WriteArrayMember(ArrayMember c)
			{
				if (c.ArrayDataReference == StreamReference.Empty)
				{
					c.ArrayDataReference = _mDataStream.BeginBlock(c.Alignment);
					{
						foreach (var m in c.Members)
							m.Write(this);
					}
					_mDataStream.EndBlock();
				}

                Write(c, streamReference => c.Reference = streamReference, delegate
                {
                    AlignTo(c.Alignment);
                    _mDataStream.Write(c.ArrayDataReference);
                    _mDataStream.Write(c.Members.Count);
                    return true;
                });
			}
			public void WriteObjectMember(ClassObject c)
			{
                Write(c, streamReference => c.Reference = streamReference, delegate
                {
	                AlignTo(c.Alignment); // Align the stream to the alignment requested by this class
                    foreach (var m in c.Members)
                        m.Write(this);
                    return true;
                });
            }
		}

		#endregion
		#region C++ Class Member Getter writer

        private sealed class MemberGetterWriter : IMemberWriter
        {
            private readonly StreamWriter _mWriter;

            public MemberGetterWriter(StreamWriter writer)
            {
                _mWriter = writer;
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
                var line = $"\tvoid*\tget{c.MemberName}() const {{ return 0; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteBoolMember(BoolMember c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteBitSetMember(BitSetMember c)
            {
	            int n = (c.Members.Count + 31) / 32;
	            for (var i=0; i<n; ++i)
	            {
		            uint bit = 1;
		            string fieldName = c.Members[i * 32].MemberName;
		            foreach (var m in c.Members)
		            {
			            var line =
				            $"\t{m.TypeName}\tget{m.MemberName}() const {{ return (m_{fieldName} & {bit}) != 0; }}";
			            _mWriter.WriteLine(line);
			            bit = bit << 1;
		            }
	            }
            }

            public void WriteInt8Member(Int8Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteInt16Member(Int16Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteInt32Member(Int32Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteInt64Member(Int64Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteUInt8Member(UInt8Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteUInt16Member(UInt16Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteUInt32Member(UInt32Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteUInt64Member(UInt64Member c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteEnumMember(EnumMember c)
            {
                var line = $"\t{c.EnumType.Name}\tget{c.MemberName}() const {{ {c.EnumType.Name} e; m_{c.MemberName}.get(e); return e; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteFloatMember(FloatMember c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteDoubleMember(DoubleMember c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteStringMember(StringMember c)
            {
                if (c.IsPointerTo)
                {
                    var line = $"\tstring_t\tget{c.MemberName}() const {{ return m_{c.MemberName}->str(); }}";
                    _mWriter.WriteLine(line);
                }
                else
                {
                    var line = $"\tstring_t\tget{c.MemberName}() const {{ return m_{c.MemberName}.str(); }}";
                    _mWriter.WriteLine(line);
                }
            }

            public void WriteFileIdMember(FileIdMember c)
            {
                var d = c.IsPointerTo ? "*" : "";
                var line = $"\t{c.TypeName}\tget{c.MemberName}() const {{ return {d}m_{c.MemberName}; }}";
                _mWriter.WriteLine(line);
            }

            public void WriteStructMember(StructMember c)
            {
                if (c.IsPointerTo)
                {
                    var line = $"\t{c.TypeName} const&\tget{c.MemberName}() const {{ return *m_{c.MemberName}; }}";
                    _mWriter.WriteLine(line);
                }
                else
                {
                    var line = $"\t{c.TypeName} const&\tget{c.MemberName}() const {{ return m_{c.MemberName}; }}";
                    _mWriter.WriteLine(line);
                }
            }

            public void WriteArrayMember(ArrayMember c)
            {
                var line = $"\tarray_t<{c.Element.TypeName}>\tget" + c.MemberName + "() const { return m_" + c.MemberName + "; }";
                _mWriter.WriteLine(line);
            }

            public void WriteObjectMember(ClassObject c)
            {
                if (c.IsPointerTo)
                {
                    var line = $"\tconst {c.TypeName}*\tget{c.MemberName}() const {{ return m_{c.MemberName}; }}";
                    _mWriter.WriteLine(line);
                }
                else
                {
                    var line = $"\tconst {c.TypeName}*\tget{c.MemberName}() const {{ return &m_{c.MemberName}; }}";
                    _mWriter.WriteLine(line);
                }
            }
        }

        #endregion
		#region C++ Struct Member Writer

		private sealed class MemberWriter : IMemberWriter
		{
			private readonly StreamWriter _mWriter;

			public MemberWriter(StreamWriter writer)
			{
				_mWriter = writer;
			}

			public bool Open()
			{
				return true;
			}

			public bool Close()
			{
				return true;
			}

			private static void Write(string type, string name, bool pointer, TextWriter writer)
			{
                //writer.WriteLine($"\t{type} const m_{name};");
                writer.Write('\t');
                writer.Write(type);
                if (pointer)
	                writer.Write('*');
                writer.Write(" const m_");
                writer.Write(name);
                writer.WriteLine(";");
			}

			public void WriteNullMember(NullMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteBoolMember(BoolMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteBitSetMember(BitSetMember c)
			{
				var n = (c.Members.Count + 31) / 32;
				for (var i=0; i<n; ++i)
				{
					var fieldName = c.Members[i * 32].MemberName;
					Write(c.TypeName, fieldName, false, _mWriter);
				}
			}
			public void WriteInt8Member(Int8Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteInt16Member(Int16Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteInt32Member(Int32Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteInt64Member(Int64Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteUInt8Member(UInt8Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteUInt16Member(UInt16Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteUInt32Member(UInt32Member c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
            public void WriteUInt64Member(UInt64Member c)
            {
                Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
            }
            public void WriteEnumMember(EnumMember c)
            {
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
            }
			public void WriteFloatMember(FloatMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteDoubleMember(DoubleMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteStringMember(StringMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteFileIdMember(FileIdMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
            public void WriteStructMember(StructMember c)
            {
                Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
            }
			public void WriteArrayMember(ArrayMember c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
			}
			public void WriteObjectMember(ClassObject c)
			{
				Write(c.TypeName, c.MemberName, c.IsPointerTo, _mWriter);
            }
		}

		#endregion
		#region C++ Code writer

		private class CppCodeWriter
		{
			private static void WriteEnum(EnumMember e, StreamWriter writer)
            {
                writer.WriteLine($"enum {e.EnumType.Name}");
                writer.WriteLine("{");
                foreach (var en in Enum.GetValues(e.EnumType))
                {
					var val = System.Convert.ChangeType(en, System.Enum.GetUnderlyingType(e.EnumType));
					writer.WriteLine($"\t{Enum.GetName(e.EnumType, en)} = {val},");
                }
                writer.WriteLine("};");
                writer.WriteLine();
            }

            public static void Write(List<EnumMember> enums, StreamWriter writer)
            {
                HashSet<string> writtenEnums = new();
                foreach (var c in enums)
                {
	                if (writtenEnums.Contains(c.EnumType.Name)) continue;
	                WriteEnum(c, writer);
	                writtenEnums.Add(c.EnumType.Name);
                }
				writer.WriteLine();
			}

            private static void Write(ClassObject c, StreamWriter writer)
			{
				writer.WriteLine($"class {c.TypeName}");
                writer.WriteLine("{");
                writer.WriteLine("public:");

				MemberGetterWriter getterWriter = new(writer);
				MemberWriter memberWriter = new(writer);

                // write public getters
                foreach (var m in c.Members)
                {
                    m.Write(getterWriter);
                }
                writer.WriteLine();
                writer.WriteLine("private:");

                // write private members
				foreach (var m in c.Members)
				{
					m.Write(memberWriter);
				}

                writer.WriteLine("};");
                writer.WriteLine();
			}

			public static void Write(List<ClassObject> classes, StreamWriter writer)
			{
				classes.Reverse();

				HashSet<string> writtenClasses = new();
				foreach (var c in classes)
				{
					if (writtenClasses.Contains(c.TypeName)) continue;
					Write(c, writer);
					writtenClasses.Add(c.TypeName);
				}
			}
		}

		#endregion

        #region MemberBook

        private sealed class MyMemberBook : MemberBook
        {
            public void HandoutReferences()
            {
                // Handout StreamReferences to classes, compounds and arrays taking care of equality of these objects.
                // Note: Strings are a bit of a special case since we also will collect the names of members and classes.

                Dictionary<object, StreamReference> referencesForClassesDict = new();
                foreach (var c in Classes)
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
                foreach (var a in Arrays)
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

        public static void Write(EPlatform platform, object data, string dataFilename, string codeFilename, string relocFilename)
        {
            // Analyze Data.Root and generate a list of 'Code.Class' objects from this.
            IMemberGenerator genericMemberGenerator = new GenericMemberGenerator();

            Reflector reflector = new(genericMemberGenerator);

            MyMemberBook book = new();
            reflector.Analyze(data, book);

            // Determine the size of every 'Code.Class'
            foreach (var c in book.Classes)
	            c.CombineBooleans();

            // Sort the members on every 'Code.Class' so that alignment of data is solved.
            for (var i=0; i<2; ++i)
            {
	            foreach (var c in book.Classes)
		            c.SortMembers(new SortByMemberAlignment());
	            foreach (var c in book.Classes)
		            c.DetermineAlignment();
            }

            // The StringTable to collect (and collapse duplicate) all strings, only allow lowercase
            StringTable stringTable = new();

            // Write out every 'Code.ClassObject' to the DataStream.
            CppDataStream dataStream = new(platform);
            DataStreamWriter dataStreamWriter = new(stringTable, dataStream);
            dataStreamWriter.Open();
            {
                var bookRoot = book.Classes[0];
                bookRoot.Write(dataStreamWriter);
            }
            dataStreamWriter.Close();

            // Finalize the DataStream and obtain a database of the position of the
            // IReferenceableMember objects in the DataStream.
            FileInfo dataFileInfo = new(dataFilename);
            FileStream dataFileStream = new(dataFileInfo.FullName, FileMode.Create);
            var dataFileStreamWriter = EndianUtils.CreateBinaryStream(dataFileStream, platform);
            FileInfo relocFileInfo = new(relocFilename);
            FileStream relocFileStream = new(relocFileInfo.FullName, FileMode.Create);
            var relocFileStreamWriter = EndianUtils.CreateBinaryStream(relocFileStream, platform);
            dataStream.Finalize(dataFileStreamWriter, stringTable);
            dataFileStreamWriter.Close();
            dataFileStream.Close();
            relocFileStreamWriter.Close();
            relocFileStream.Close();

            // Generate the c++ code using the CppCodeWriter.
            FileInfo codeFileInfo = new(codeFilename);
            var codeFileStream = codeFileInfo.Create();
            StreamWriter codeFileStreamWriter = new(codeFileStream);
            CppCodeWriter codeWriter = new ();
			CppCodeWriter.Write(book.Enums, codeFileStreamWriter);
			CppCodeWriter.Write(book.Classes, codeFileStreamWriter);
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
			private readonly MemoryStream _mDataStream = new();
			private readonly IBinaryStream _mDataWriter;

			private readonly Dictionary<StreamReference, List<StreamOffset>> _mPointers = new();

			internal DataBlock(EPlatform platform, int alignment)
            {
                Alignment = alignment;
                // TODO Optimization
                // Using MemoryStream with many DataBlock instances is not optimal, this could be
                // seriously optimized
				Reference = StreamReference.NewReference;
				_mDataWriter = EndianUtils.CreateBinaryStream(_mDataStream, platform);
			}

            internal StreamReference Reference { get; private set; }
            public int Alignment { get; }

			private long Position => _mDataStream.Position;

            internal int Size => (int)_mDataStream.Length;

            internal void End()
            {
                var gap = CMath.Align32(Size, Alignment) - Size;

                // Write actual data to reach the size alignment requirement

                var gap4 = gap / 4;
                for (var i = 0; i < gap4; ++i)
                {
                    _mDataWriter.Write((uint)0xCDCDCDCD);
                }

                gap -= (gap4 * 4);
                while (gap > 0)
                {
                    _mDataWriter.Write((byte)0xCD);
                    gap -= 1;
                }
            }

            internal void AlignTo(int alignment)
            {
                _mDataWriter.Position = CMath.Align(_mDataWriter.Position, alignment);
            }

            private bool IsAligned(int alignment)
            {
                return CMath.IsAligned(_mDataWriter.Position, alignment);
            }

            internal void Write(float v)
			{
				Debug.Assert(IsAligned(sizeof(float)));
				_mDataWriter.Write(v);
			}
			internal void Write(double v)
			{
				Debug.Assert(IsAligned(sizeof(double)));
				_mDataWriter.Write(v);
			}
			internal void Write(sbyte v)
			{
				_mDataWriter.Write(v);
			}
			internal void Write(short v)
			{
				Debug.Assert(IsAligned(sizeof(short)));
				_mDataWriter.Write(v);
			}
			internal void Write(int v)
			{
				Debug.Assert(IsAligned(sizeof(int)));
				_mDataWriter.Write(v);
			}
			internal void Write(long v)
			{
				Debug.Assert(IsAligned(sizeof(long)));
				_mDataWriter.Write(v);
			}
			internal void Write(byte v)
			{
				_mDataWriter.Write(v);
			}
			internal void Write(ushort v)
			{
				Debug.Assert(IsAligned(sizeof(ushort)));
				_mDataWriter.Write(v);
			}
			internal void Write(uint v)
			{
				Debug.Assert(IsAligned(sizeof(uint)));
				_mDataWriter.Write(v);
			}
			internal void Write(ulong v)
			{
				Debug.Assert(IsAligned(sizeof(ulong)));
				_mDataWriter.Write(v);
			}
			internal void Write(byte[] data, int index, int count)
			{
				_mDataWriter.Write(data, index, count);
			}
			internal void Write(string str)
			{
				_mDataWriter.Write(str);
			}

			internal void Write(StreamReference v)
			{
				Debug.Assert(IsAligned(sizeof(uint)));

				if (_mPointers.ContainsKey(v))
				{
					_mPointers[v].Add(new StreamOffset(Position));
				}
				else
				{
					List<StreamOffset> offsets = new() { new StreamOffset(Position) };
                    _mPointers.Add(v, offsets);
				}

				_mDataWriter.Write(v.ID);
			}

			internal Hash160 ComputeHash()
			{
				var dataHash = HashUtility.compute(_mDataStream);
				return dataHash;
			}

			internal void ReplaceReference(StreamReference oldRef, StreamReference newRef)
			{
				if (Reference == oldRef)
					Reference = newRef;

				if (_mPointers.ContainsKey(oldRef))
				{
					List<StreamOffset> oldOffsets = _mPointers[oldRef];
					_mPointers.Remove(oldRef);

					// Modify data
					StreamOffset currentPos = new(_mDataStream.Position);
					foreach (StreamOffset o in oldOffsets)
					{
						_mDataWriter.Seek(o);
						_mDataWriter.Write(0);
					}
					_mDataWriter.Seek(currentPos);

					// Update pointer and offsets
					if (_mPointers.ContainsKey(newRef))
					{
						List<StreamOffset> newOffsets = _mPointers[newRef];
						foreach (StreamOffset o in oldOffsets)
							newOffsets.Add(o);
					}
					else
					{
						_mPointers.Add(newRef, oldOffsets);
					}
				}
			}

			internal void WriteTo(IBinaryStream outData, IDictionary<StreamReference, StreamOffset> dataOffsetDataBase)
			{
                StreamUtils.Align(outData, Alignment);

				StreamOffset currentPos = new(_mDataStream.Position);
				foreach (KeyValuePair<StreamReference, List<StreamOffset>> k in _mPointers)
				{
					if (dataOffsetDataBase.TryGetValue(k.Key, out StreamOffset outDataOffset))
					{
						foreach (StreamOffset o in k.Value)
						{
							// Seek to the position that has the 'StreamReference'
							_mDataWriter.Seek(o);

                            // Write the relative (signed) offset
                            int offset = (int)(outDataOffset.Offset - o.Offset);
                            // TODO: Assert when the offset is out of bounds (-2GB < offset < 2GB)
                            _mDataWriter.Write(offset);
						}
					}
				}
				_mDataWriter.Seek(currentPos);

				// Write data
				outData.Write(_mDataStream.GetBuffer(), 0, (int)_mDataStream.Length);
			}
		}

		#endregion
		#region Fields

		private readonly List<DataBlock> _mData = new();
		private readonly Stack<DataBlock> _mStack = new();

		private EPlatform Platform { get; set; }
		private DataBlock Current { get; set; } = null;

		#endregion
		#region Constructor

		public CppDataStream(EPlatform platform)
        {
            Platform = platform;
        }

		#endregion
		#region Methods

		public StreamReference BeginBlock(int alignment)
		{
			if (Current != null)
				_mStack.Push(Current);

			Current = new(Platform, alignment);
			_mData.Add(Current);
			return Current.Reference;
		}

        public void EndBlock()
        {
            Current.End();
            if (_mStack.Count > 0)
                Current = _mStack.Pop();
            else
                Current = null;
        }

        public void AlignTo(int alignment)
        {
            Current.AlignTo(alignment);
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
			foreach (var d in _mData)
				finalDataDataBase.Add(d.Reference, d);

			// For all blocks, calculate Hash
			// Collapse identical blocks, and when a collapse has occurred we have
			// to re-iterate again since a collapse changes the Hash of a block.
			bool collapse = true;
			while (collapse)
			{
				Dictionary<StreamReference, List<StreamReference>> duplicateDataBase = new();
				Dictionary<Hash160, StreamReference> dataHashDataBase = new();

				foreach (var d in _mData)
				{
					var hash = d.ComputeHash();
					if (dataHashDataBase.ContainsKey(hash))
					{
						// Encountering a block of data which has a duplicate.
						// After the first iteration it might be the case that
						// they have the same 'Reference' since they are collapsed.
						var newRef = dataHashDataBase[hash];
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

				foreach (var p in duplicateDataBase)
				{
					foreach (var r in p.Value)
					{
						foreach (var d in _mData)
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

            var offset = dataWriter.Position;
            foreach (var k in finalDataDataBase)
            {
                offset = CMath.Align(offset, k.Value.Alignment);
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
