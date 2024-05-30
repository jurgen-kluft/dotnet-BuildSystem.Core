using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameCore
{
    public struct StreamFile
    {
        public static readonly StreamFile sEmpty = new (string.Empty, 0, StreamOffset.sEmpty);

        public StreamFile(StreamFile file)
        {
            Filename = file.Filename;
            FileSize = file.FileSize;
            FileOffset = file.FileOffset;
        }

        public StreamFile(string filename, uint size, StreamOffset offset)
        {
            Filename = filename;
            FileSize = size;
            FileOffset = offset;
        }

        public bool IsEmpty
        {
            get
            {
                return FileSize == 0 && FileOffset == StreamOffset.sEmpty && Filename  == string.Empty;
            }
        }

        public string Filename { get; set; }
        public uint FileSize { get; set; }
        public StreamOffset FileOffset { get; set; }

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
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class StreamBuilder
    {
        private FileStream mDstFilestream;

        public uint Alignment { get; set; }

        private static ulong StreamAlign(ulong value, uint alignment)
        {
            return (value + (alignment - 1)) & (~(alignment - 1));
        }

        private ulong ReadWriteProcess(List<StreamFile> files)
        {
            for (var fileIndex=0; fileIndex<files.Count; fileIndex++)
            {
                var file = files[fileIndex];
                mDstFilestream.Position = (long)file.FileOffset.Offset;
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

            var resultingSize = (ulong)mDstFilestream.Length;
            Console.WriteLine("Writing {0} bytes to stream was successful ({1}Mb).", resultingSize, ((resultingSize + (1024*1024 - 1))/(1024*1024)));

            return resultingSize;
        }

        public void Build(List<StreamFile> srcFiles, string dstFile)
        {
            ulong offset = 0;
            for (var fileIndex=0; fileIndex<srcFiles.Count; fileIndex++)
            {
                var streamFile = srcFiles[fileIndex];
                streamFile.FileOffset = new StreamOffset(offset);
                srcFiles[fileIndex] = streamFile;
                offset = StreamAlign(offset + srcFiles[fileIndex].FileSize, Alignment);
            }
            var fileSize = offset;

            mDstFilestream = new FileStream(dstFile, FileMode.Create, FileAccess.Write, FileShare.Write, 8, FileOptions.WriteThrough | FileOptions.Asynchronous);
            mDstFilestream.SetLength((long)fileSize);
            mDstFilestream.Position = 0;

            ReadWriteProcess(srcFiles);

            mDstFilestream.Dispose();
            mDstFilestream.Close();
        }
    }
}
