using System.Drawing;
using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class CurveDataFile : IDataFile, ISignature
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public CurveDataFile() : this(string.Empty, string.Empty)
        {
        }
        public CurveDataFile(string filename) : this(filename, filename)
        {
        }

        private CurveDataFile(string srcFilename, string dstFilename)
        {
            mSrcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            mDstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public Hash160 Signature { get; set; }

        public void BuildSignature(IWriter stream)
        {
            GameCore.BinaryWriter.Write(stream,"CurveDataFile");
            GameCore.BinaryWriter.Write(stream,mSrcFilename);
        }

        public void SaveState(IWriter stream)
        {
            GameCore.BinaryWriter.Write(stream,mSrcFilename);
            GameCore.BinaryWriter.Write(stream,mDstFilename);
            mDependency.WriteTo(stream);
        }

        public void LoadState(IBinaryReader stream)
        {
            GameCore.BinaryReader.Read(stream, out mSrcFilename);
            GameCore.BinaryReader.Read(stream, out mDstFilename);
            mDependency = Dependency.ReadFrom(stream);
        }

        public void CopyConstruct(IDataFile dc)
        {
            if (dc is not CurveDataFile cc) return;

            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public string CookedFilename => mDstFilename;
        public object CookedObject => new DataFile(this, "curve_t");

        public DataCookResult Cook(List<IDataFile> additionalDataFiles)
        {
            var result = DataCookResult.None;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.GameDataSrcPath, mSrcFilename);
                mDependency.Add(1, EGameDataPath.GameDataDstPath, mDstFilename);
                result = DataCookResult.DstMissing;
            }
            else
            {
                var result3 = mDependency.Update(delegate(ushort id, State state)
                {
                    var result2 = DataCookResult.None;
                    if (state == State.Missing)
                    {
                        result2 = id switch
                        {
                            0 => (DataCookResult.SrcMissing),
                            1 => (DataCookResult.DstMissing),
                            _ => (DataCookResult.None),
                        };
                    }
                    else if (state == State.Modified)
                    {
                        result2 |= id switch
                        {
                            0 => (DataCookResult.SrcChanged),
                            1 => (DataCookResult.DstChanged),
                            _ => (DataCookResult.None)
                        };
                    }

                    return result2;
                });

                if (result3 == DataCookResult.UpToDate)
                {
                    result = DataCookResult.UpToDate;
                    return result;
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemConfig.DstPath, mDstFilename), true);

                //

                // Execution is done, update the dependency to reflect the new state
                result = mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCookResult)(result | DataCookResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return result;
        }
    }

    public class Curve_Point
    {
        public double mX;
        public double mY;
    }

    public class Curve_Control_Point
    {
        public Curve_Point mControlPoint = new Curve_Point();
        public Curve_Point mTangentBegin = new Curve_Point();
        public Curve_Point mTangentEnd = new Curve_Point();
    }

    public sealed class Curve
    {
        public double MinX;
        public double MaxX;
        public double MinY;
        public double MaxY;

        public int NumControlPoints;
        public Curve_Control_Point[] ControlPoints;
        public int InterpolationFlag;

        public int NumCurvePoints;
        public Curve_Point[] CurvePoints;

        private static bool ReadCurveControlPoint(StreamReader reader, out Curve_Control_Point point)
        {
            point = new Curve_Control_Point();
            var line = reader.ReadLine();
            if (line == null)
                return false;

            // We need to read 6 double values:
            // - ControlPoint X/Y
            // - TangentBegin X/Y
            // - TangentEnd X/Y
            //
            // The format is:
            // ControlPointX, ControlPointY, TangentBeginX, TangentBeginY, TangentEndX, TangentEndY
            //

            var span = line.AsSpan();
            var begin = 0;
            for (int i = 0; i < 6; i++)
            {
                while (begin < span.Length && (span[begin] == ' ' || span[begin] == '\t'))
                    begin++;

                var end = begin;
                while (end < span.Length && span[end] != ',')
                    end++;

                var value = double.Parse(span.Slice(begin, end - begin));

                switch (i)
                {
                    case 0:
                        point.mControlPoint.mX = value;
                        break;
                    case 1:
                        point.mControlPoint.mY = value;
                        break;
                    case 2:
                        point.mTangentBegin.mX = value;
                        break;
                    case 3:
                        point.mTangentBegin.mY = value;
                        break;
                    case 4:
                        point.mTangentEnd.mX = value;
                        break;
                    case 5:
                        point.mTangentEnd.mY = value;
                        break;
                }

                begin = end + 1;
            }


            return true;
        }

        private static bool ReadCurvePoint(StreamReader reader, out Curve_Point point)
        {
            point = new Curve_Point();
            var line = reader.ReadLine();
            if (line == null)
                return false;

            // We need to read 2 double values:
            // - Point X/Y
            //
            // The format is:
            // PointX, PointY
            //

            var span = line.AsSpan();
            var begin = 0;
            for (int i = 0; i < 2; i++)
            {
                while (begin < span.Length && (span[begin] == ' ' || span[begin] == '\t'))
                    begin++;

                var end = begin;
                while (end < span.Length && span[end] != ',')
                    end++;

                var value = double.Parse(span.Slice(begin, end - begin));

                switch (i)
                {
                    case 0:
                        point.mX = value;
                        break;
                    case 1:
                        point.mY = value;
                        break;
                }

                begin = end + 1;
            }

            return true;
        }
        private static string ReadLine(StreamReader reader)
        {
            string line;
            do
            {
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return string.Empty;
            } while (line[0] == ';');

            return line;
        }

        private static double ReadDouble(StreamReader reader)
        {
            var line = ReadLine(reader);
            return double.Parse(line);
        }

        public bool LoadText(string filename)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                if (fileInfo.Exists)
                {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamReader reader = new StreamReader(fileStream);

                    // The X information.
                    ReadLine(reader); // legend (ignored)
                    MinX = ReadDouble(reader); // min.
                    MaxX = ReadDouble(reader); // max.

                    // The Y information.
                    ReadLine(reader); //  legend (ignored)
                    MinY = ReadDouble(reader); // min.
                    MaxY = ReadDouble(reader); // max.

                    // The control points.
                    NumControlPoints = (int)ReadDouble(reader); // Number of control points.
                    ControlPoints = new Curve_Control_Point[NumControlPoints];
                    for (int i = 0; i < NumControlPoints; i++)
                    {
                        ReadCurveControlPoint(reader, out var point);
                        ControlPoints[i] = point;
                    }

                    ReadDouble(reader); // Interpolation (ignored)

                    // The curve itself
                    NumCurvePoints = (int)ReadDouble(reader); // Number of curve points.
                    CurvePoints = new Curve_Point[NumCurvePoints];
                    for (int i = 0; i < NumCurvePoints; i++)
                    {
                        ReadCurvePoint(reader, out var point);
                        CurvePoints[i] = point;
                    }

                    reader.Close();
                    fileStream.Close();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace + " - " + e.Message);
                return false;
            }

            return true;
        }
    }

#if CURVE_COMPILER
    public class Curves
    {
        private readonly CurveCompiler[] mCompilers;
        private FileId[] fileIds;

        public Curves(params string[] filenames)
        {
            int n = filenames.Length;
            mCompilers = new CurveCompiler[n];
            for (int i=0; i<n; i++)
                mCompilers[i] = new CurveCompiler(filenames[i]);
        }

        public void consolidate()
        {
            List<FileId> fileIdList = new List<FileId>();
            foreach(CurveCompiler cc in mCompilers)
                cc.collect(fileIdList);
            fileIds = fileIdList.ToArray();
        }

        public Array Values { get { return fileIds; } }
    }

    public class Curve
    {
        private readonly CurveCompiler mCompiler;
    	private FileId fileId;

        public Curve(string filename)
        {
			mCompiler = new CurveCompiler(filename);
        }

        public void consolidate()
        {
            List<FileId> fileIds = new List<FileId>();
            mCompiler.collect(fileIds);
            if (fileIds.Count == 1)
                fileId = fileIds[0];
            else
                fileId = new FileId(null);
        }

        public object Value { get { return fileId; } }
    }

#endif
}
