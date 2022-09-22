using System;
using System.Reflection;
using System.Collections.Generic;

namespace GameData
{
    using GameCore;
    using DataBuildSystem;

    /// <summary>
    /// This compiler will compile C# files into an assembly and instantiate a root object.
    /// All IDataCompilers objects are transparently routed to the main dataCompilerServer.
    /// </summary>
    
    public enum EDataUnit
    {
        External,
        Embed,
    }

    public class DataUnit : IExternalObjectProvider
    {
        private readonly Filename mAsmFilename;
        private readonly List<Filename> mSrcFilenames;
        private readonly List<Filename> mIncludeFilenames;
        private bool mValid = true;
        private EDataCompilerStatus mStatus = EDataCompilerStatus.NONE;

        private DataUnit(string _dllfilename)
        {
            mAsmFilename = new Filename(Environment.expandVariables(_dllfilename));
            mSrcFilenames = new List<Filename>();
            mIncludeFilenames = new List<Filename>();
        }

        public DataUnit(string _dllfilename, Filename _filename)
            : this(_dllfilename)
        {
            if (_filename.Extension == ".cs")
                mSrcFilenames.Add(_filename);
            else
                mIncludeFilenames.Add(_filename);
        }

        public DataUnit(string _dllfilename, Filename[] _files)
            : this(_dllfilename)
        {
            for (int i = 0; i < _files.Length; ++i)
            {
                string filename = Environment.expandVariables(_files[i]);
                if (filename.EndsWith(".cs"))
                    mSrcFilenames.Add(new Filename(filename));
                else
                    mIncludeFilenames.Add(new Filename(filename));
            }
        }

        public EDataCompilerStatus status { get { return mStatus; } }


        public object extobject
        {
            get
            {
                return null;
            }
        }
    }
}

