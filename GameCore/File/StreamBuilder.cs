using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public struct StreamFile
    {
        #region Fields

        public readonly static StreamFile Empty = new (string.Empty, 0, StreamOffset.sEmpty);

        #endregion
        #region Constructor

        public StreamFile(StreamFile file)
        {
            Filename = file.Filename;
            FileSize = file.FileSize;
            FileOffset = file.FileOffset;
        }

        public StreamFile(string filename, int size, StreamOffset offset)
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
                return FileSize == 0 && FileOffset == StreamOffset.sEmpty && Filename  == string.Empty;
            }
        }

        public string Filename { get; set; }
        public int FileSize { get; set; }
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
                var other = (StreamFile)o;
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return FileOffset.Offset.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class StreamBuilder
    {
        #region Fields

        private FileStream mDstFilestream;

        #endregion
        #region Properties

        public long Alignment { get; set; }

        #endregion
        #region Read/Write Process

        internal static long StreamAlign(long value, long alignment)
        {
            return (value + (alignment - 1)) & (~(alignment - 1));
        }

        public long ReadWriteProcess(List<StreamFile> files)
        {
            long resultingSize = 0;
            for (var fileIndex=0; fileIndex<files.Count; fileIndex++)
            {
                var file = files[fileIndex];
                mDstFilestream.Position = file.FileOffset.Offset;
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
            long offset = 0;
            for (var fileIndex=0; fileIndex<srcFiles.Count; fileIndex++)
            {
                var sfile = srcFiles[fileIndex];
                sfile.FileOffset = new StreamOffset(offset);
                srcFiles[fileIndex] = sfile;
                offset = StreamAlign(offset + srcFiles[fileIndex].FileSize, Alignment);
            }
            var fileSize = offset;

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