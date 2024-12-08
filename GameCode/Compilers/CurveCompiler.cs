using GameCore;
using DataBuildSystem;

namespace GameData
{
    public sealed class CurveCompiler : IDataCompiler, IFileIdProvider
    {
        private string mSrcFilename;
        private string mDstFilename;
        private Dependency mDependency;

        public CurveCompiler(string filename) : this(filename, filename)
        {
        }
        public CurveCompiler(string srcFilename, string dstFilename)
        {
            mSrcFilename = srcFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            mDstFilename = dstFilename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public void CompilerSignature(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            stream.Write(mDstFilename);
        }

        public void CompilerWrite(IBinaryWriter stream)
        {
            stream.Write(mSrcFilename);
            stream.Write(mDstFilename);
            mDependency.WriteTo(stream);
        }

        public void CompilerRead(IBinaryReader stream)
        {
            mSrcFilename = stream.ReadString();
            mDstFilename = stream.ReadString();
            mDependency = Dependency.ReadFrom(stream);
        }

        public void CompilerConstruct(IDataCompiler dc)
        {
            if (dc is not CurveCompiler cc) return;

            mSrcFilename = cc.mSrcFilename;
            mDstFilename = cc.mDstFilename;
            mDependency = cc.mDependency;
        }

        public IFileIdProvider CompilerFileIdProvider => this;
        public uint FileIndex { get; set; }

        public DataCompilerOutput CompilerExecute()
        {
            var result = DataCompilerResult.None;
            if (mDependency == null)
            {
                mDependency = new Dependency(EGameDataPath.Src, mSrcFilename);
                mDependency.Add(1, EGameDataPath.Dst, mDstFilename);
                result = DataCompilerResult.DstMissing;
            }
            else
            {
                var result3 = mDependency.Update(delegate(short id, State state)
                {
                    var result2 = DataCompilerResult.None;
                    if (state == State.Missing)
                    {
                        result2 = id switch
                        {
                            0 => (DataCompilerResult.SrcMissing),
                            1 => (DataCompilerResult.DstMissing),
                            _ => (DataCompilerResult.None),
                        };
                    }
                    else if (state == State.Modified)
                    {
                        result2 |= id switch
                        {
                            0 => (DataCompilerResult.SrcChanged),
                            1 => (DataCompilerResult.DstChanged),
                            _ => (DataCompilerResult.None)
                        };
                    }

                    return result2;
                });

                if (result3 == DataCompilerResult.UpToDate)
                {
                    result = DataCompilerResult.UpToDate;
                    return new DataCompilerOutput(result, new[] { mDstFilename }, this);
                }
            }

            try
            {
                // Execute the actual purpose of this compiler
                File.Copy(Path.Join(BuildSystemCompilerConfig.SrcPath, mSrcFilename), Path.Join(BuildSystemCompilerConfig.DstPath, mDstFilename), true);

                // Execution is done, update the dependency to reflect the new state
                result = mDependency.Update(null);
            }
            catch (Exception)
            {
                result = (DataCompilerResult)(result | DataCompilerResult.Error);
            }

            // The result returned here is the result that 'caused' this compiler to execute its action and not the 'new' state.
            return new DataCompilerOutput(result, new[] { mDstFilename }, this);
        }
    }

    public class CurvePoint
    {
        public double mX;
        public double mY;
    }

    public class CurveControlPoint
    {
        public CurvePoint mControlPoint = new CurvePoint();
        public CurvePoint mTangentBegin = new CurvePoint();
        public CurvePoint mTangentEnd = new CurvePoint();
    }

    public sealed class Curve
    {
        #region Fields

        public double mMinX;
        public double mMaxX;
        public double mMinY;
        public double mMaxY;

        public int mNumControlPoints;
        public CurveControlPoint[] mControlPoints;
        public int mInterpolationFlag;

        public int mNumCurvePoints;
        public CurvePoint[] mCurve;

        #endregion
        #region Save Binary

        public void SaveBinary(string filename)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                FileStream fileStream;
                if (!fileInfo.Exists)
                    fileStream = fileInfo.Create();
                else
                    fileStream = new FileStream(fileInfo.FullName, FileMode.Truncate, FileAccess.Write, FileShare.None);
                BinaryWriter writer = new BinaryWriter(fileStream);

                writer.Write((float)mMinX);
                writer.Write((float)mMaxX);
                writer.Write((float)mMinY);
                writer.Write((float)mMaxY);

                writer.Write(mNumCurvePoints);
                foreach (CurvePoint p in mCurve)
                {
                    writer.Write((float)p.mX);
                    writer.Write((float)p.mY);
                }

                writer.Close();
                fileStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace + " - " + e.Message);
            }
        }

        #endregion
        #region Load Text

        private static string sGetWordSimple(string strText, ref int iStartingPoint)
        {
            string strReturn = "";
            int I = iStartingPoint;
            while (I < strText.Length && (strText[I] == ' ' || strText[I] == '	')) { I++; }
            while (I < strText.Length && strText[I] != ' ' && strText[I] != '	')
            {
                strReturn += strText[I];
                I++;
            }
            iStartingPoint = I;
            return strReturn;
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
                    string line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    //string legendX = line;		// X legend.

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    mMinX = Double.Parse(line);	// X min.

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    mMaxX = Double.Parse(line);	// X max.

                    // The Y information.
                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    //string legendY = line;		// Y legend.

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    mMinY = Double.Parse(line);	// Y min.

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    mMaxY = Double.Parse(line);	// Y max.

                    // The control points.
                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }

                    mNumControlPoints = (int)Double.Parse(line);	// Number of control points.
                    mControlPoints = new CurveControlPoint[mNumControlPoints];

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    for (int I = 0; I < mNumControlPoints; I++)		// Load each control point.
                    {
                        mControlPoints[I] = new CurveControlPoint();

                        int iPos = 0;
                        string strTemp1 = sGetWordSimple(line, ref iPos);
                        strTemp1 += " ";
                        strTemp1 += sGetWordSimple(line, ref iPos);

                        string strTemp2 = sGetWordSimple(line, ref iPos);
                        strTemp2 += " " + sGetWordSimple(line, ref iPos);

                        string strTemp3 = sGetWordSimple(line, ref iPos);
                        strTemp3 += " " + sGetWordSimple(line, ref iPos);

                        iPos = 0;
                        mControlPoints[I].mControlPoint.mX = Double.Parse(sGetWordSimple(strTemp1, ref iPos));
                        mControlPoints[I].mControlPoint.mY = Double.Parse(sGetWordSimple(strTemp1, ref iPos));

                        iPos = 0;
                        mControlPoints[I].mTangentBegin.mX = Double.Parse(sGetWordSimple(strTemp2, ref iPos));
                        mControlPoints[I].mTangentBegin.mY = Double.Parse(sGetWordSimple(strTemp2, ref iPos));

                        iPos = 0;
                        mControlPoints[I].mTangentEnd.mX = Double.Parse(sGetWordSimple(strTemp3, ref iPos));
                        mControlPoints[I].mTangentEnd.mY = Double.Parse(sGetWordSimple(strTemp3, ref iPos));

                        line = reader.ReadLine();
                    }

                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    //int interpolate = (int)Double.Parse(line);	// Interpolation.

                    // The points.
                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    mNumCurvePoints = (int)Double.Parse(line);	// Number of points.

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }

                    mCurve = new CurvePoint[mNumCurvePoints];

                    for (int I = 0; I < mNumCurvePoints; I++)		// Load each point.
                        mCurve[I] = new CurvePoint();

                    for (int I = 0; I < mNumCurvePoints; I++)		// Load each point.
                    {
                        int iPos = 0;
                        string strTemp1 = sGetWordSimple(line, ref iPos);
                        strTemp1 += " ";
                        strTemp1 += sGetWordSimple(line, ref iPos);

                        iPos = 0;
                        mCurve[I].mX = Double.Parse(sGetWordSimple(strTemp1, ref iPos));
                        mCurve[I].mY = Double.Parse(sGetWordSimple(strTemp1, ref iPos));

                        line = reader.ReadLine();
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
                e.ToString();
                return false;
            }
            return true;
        }

        #endregion
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
