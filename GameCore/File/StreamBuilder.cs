using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public struct StreamFile
    {
        #region Fields

        public readonly static StreamFile Empty = new (string.Empty, 0, StreamOffset.Empty);

        #endregion
        #region Constructor

        public StreamFile(StreamFile file)
        {
            Filename = file.Filename;
            FileSize = file.FileSize;
            FileOffset = file.FileOffset;
        }

        public StreamFile(string filename, Int32 size, StreamOffset offset)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = offset;
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return FileSize == 0 && FileOffset == StreamOffset.Empty && Filename  == string.Empty;
            }
        }

        public string Filename { get; set; }
        public Int32 FileSize { get; set; }
        public StreamOffset FileOffset { get; set; }

        #endregion
        #region Operators

        public static bool operator ==(StreamFile a, StreamFile b)
        {
            if (a.FileSize == b.FileSize && a.Filename == b.Filename && a.FileOffset == b.FileOffset)
                return true;
            return false;
        }
        public static bool operator !=(StreamFile a, StreamFile b)
        {
            if (a.FileSize != b.FileSize || a.Filename != b.Filename || a.FileOffset != b.FileOffset)
                return true;
            return false;
        }

        #endregion
        #region Comparer (IEqualityComparer)

        public override bool Equals(object o)
        {
            if (o is StreamFile)
            {
                StreamFile other = (StreamFile)o;
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return FileOffset.value.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class StreamBuilder
    {
        #region Fields

        private FileStream mDstFilestream;

        #endregion
        #region Properties

        public Int64 Alignment { get; set; }

        #endregion
        #region Read/Write Process

        internal static Int64 StreamAlign(Int64 value, Int64 alignment)
        {
            return (value + (alignment - 1)) & (~(alignment - 1));
        }

        public Int64 ReadWriteProcess(List<StreamFile> files)
        {
            Int64 resultingSize = 0;
            for (int fileIndex=0; fileIndex<files.Count; fileIndex++)
            {
                StreamFile file = files[fileIndex];
                mDstFilestream.Position = file.FileOffset.value;
                if (File.Exists(file.Filename))
                {
                    try
                    {
                        FileStream srcStream = new (file.Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        srcStream.CopyTo(mDstFilestream);
                        srcStream.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[StreamBuilder:EXCEPTION] ReadWriteProcess, {0}", e);
                    }
                }
            }

            Console.WriteLine("Writing {0} bytes to stream was successful ({1}Mb).", resultingSize, ((resultingSize + (1024*1024 - 1))/(1024*1024)));

            return resultingSize;
        }

        #endregion
        #region Build

        public void Build(List<StreamFile> srcFiles, string dstFile)
        {
            Int64 offset = 0;
            for (int fileIndex=0; fileIndex<srcFiles.Count; fileIndex++)
            {
                StreamFile sfile = srcFiles[fileIndex];
                sfile.FileOffset = new StreamOffset(offset);
                srcFiles[fileIndex] = sfile;
                offset = StreamAlign(offset + srcFiles[fileIndex].FileSize, Alignment);
            }
            Int64 fileSize = offset;

            mDstFilestream = new FileStream(dstFile, FileMode.Create, FileAccess.Write, FileShare.Write, 8, FileOptions.WriteThrough | FileOptions.Asynchronous);
            mDstFilestream.SetLength(fileSize);
            mDstFilestream.Position = 0;

            ReadWriteProcess(srcFiles);

            mDstFilestream.Dispose();
            mDstFilestream.Close();
        }

        #endregion
    }
}