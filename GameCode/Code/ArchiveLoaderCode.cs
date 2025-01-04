namespace GameData
{
    public readonly struct ArchiveLoaderCode : ICode
    {
        public ICode[] CodeDependency =>  new ICode[1] { new FileIdCode() };

        public string[] CodeLines
        {
            get
            {
                const string code = """
                                    class archive_loader_t
                                    {
                                    public:
                                        void* load_datafile(fileid_t fileid) { return v_load_datafile(fileid); }
                                        void* load_dataunit(u32 dataunit_index) { return v_load_dataunit(dataunit_index); }

                                        template <typename T>
                                        void* get_datafile_ptr(fileid_t fileid)
                                        {
                                            return (T*)v_get_datafile_ptr(fileid);
                                        }

                                        template <typename T>
                                        void* get_dataunit_ptr(u32 dataunit_index)
                                        {
                                            return (T*)v_get_dataunit_ptr(dataunit_index);
                                        }

                                        template <typename T>
                                        void unload_datafile(T*& object)
                                        {
                                            v_unload_datafile(object);
                                        }

                                        template <typename T>
                                        void unload_dataunit(T*& object)
                                        {
                                            v_unload_dataunit(object);
                                        }

                                    protected:
                                        virtual void* v_get_datafile_ptr(fileid_t fileid)    = 0;
                                        virtual void* v_get_dataunit_ptr(u32 dataunit_index) = 0;
                                        virtual void* v_load_datafile(fileid_t fileid)       = 0;
                                        virtual void* v_load_dataunit(u32 dataunit_index)    = 0;
                                        virtual void  v_unload_datafile(fileid_t fileid)     = 0;
                                        virtual void  v_unload_dataunit(u32 dataunit_index)  = 0;
                                    };

                                    extern archive_loader_t* g_loader;
                                    """;
                return code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
