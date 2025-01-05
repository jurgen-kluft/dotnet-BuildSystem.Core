namespace GameData
{
    public readonly struct StringTableCode : ICode
    {
        public ICode[] CodeDependency => new ICode[] { new StringCode(), new LocStrCode() };

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    struct strtable_t
                                    {
                                        inline strtable_t() {}
                                        inline strtable_t(u32 numStrings, u32 const* byteLengths, u32 const* charLengths, u32 const* offsets, const char* strings)
                                            : mMagic(0x36DF5DE5)
                                            , mNumStrings(numStrings)
                                            , mByteLengths(byteLengths)
                                            , mCharLengths(charLengths)
                                            , mOffsets(offsets)
                                            , mStrings(strings)
                                        {
                                        }
                                        inline s32      size() const { return mNumStrings; }
                                        inline string_t str(locstr_t l) const { return string_t(mByteLengths[l.id], mCharLengths[l.id], mStrings + mOffsets[l.id]); }
                                        DCORE_CLASS_PLACEMENT_NEW_DELETE
                                    protected:
                                        u32         mMagic;  // 'STRT'
                                        u32         mNumStrings;
                                        u32 const*  mHashes;
                                        u32 const*  mOffsets;
                                        u32 const*  mCharLengths;
                                        u32 const*  mByteLengths;
                                        const char* mStrings;
                                    };
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
