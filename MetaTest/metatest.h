enum ETestEnum
{
	EnumerationA = 4294901760,
	EnumerationB = 4294901761,
	EnumerationC = 4294901762,
	EnumerationD = 4294901763,
};


class TestArrayElement
{
public:
	s32	getInt() const { return m_Int; }
	f32	getFloat() const { return m_Float; }

private:
	s32 const m_Int;
	f32 const m_Float;
};

class TestData
{
public:
	string_t	getName() const { return m_Name.str(); }
	fileid_t const&	getFile() const { return m_File; }
	array_t<f32>	getFloats() const { return m_Floats.array(); }
	array_t<s64>	getIntegerList() const { return m_IntegerList.array(); }
	array_t<TestArrayElement>	getObjectArray() const { return m_ObjectArray.array(); }
	array_t<s64>	getIntPtrArray() const { return m_IntPtrArray.array(); }

private:
	raw_string_t const m_Name;
	fileid_t const m_File;
	raw_array_t<f32> const m_Floats;
	raw_array_t<s64> const m_IntegerList;
	raw_array_t<TestArrayElement> const m_ObjectArray;
	raw_array_t<s64> const m_IntPtrArray;
};

class TestRoot
{
public:
	s32	getInt32() const { return m_Int32; }
	f32	getFloat() const { return m_Float; }
	ETestEnum	getEnum() const { return m_Enum.get(e); }
	color_t const&	getColor() const { return m_Color; }
	s64 const*	getHandle() const { return m_Handle.ptr(); }
	const TestData*	getData() const { return m_Data.ptr(); }
	bool	getBool4() const { return (m_Bool4 & 1) != 0; }
	bool	getBool3() const { return (m_Bool4 & 2) != 0; }
	bool	getBool2() const { return (m_Bool4 & 4) != 0; }
	bool	getBool1() const { return (m_Bool4 & 8) != 0; }
	s8	getInt8() const { return m_Int8; }

private:
	s32 const m_Int32;
	f32 const m_Float;
	raw_enum_t<ETestEnum, u32> const m_Enum;
	color_t const m_Color;
	raw_ptr_t<s64> const m_Handle;
	raw_ptr_t<TestData> const m_Data;
	u32 const m_Bool4;
	s8 const m_Int8;
};

