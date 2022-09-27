using System;
using System.Collections.Generic;
using System.IO;

namespace GameData
{
    public class Curves : Resource, ICompound
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

        public override void consolidate()
        {
            List<FileId> fileIdList = new List<FileId>();
            foreach(CurveCompiler cc in mCompilers)
                cc.collect(fileIdList);
            fileIds = fileIdList.ToArray();
        }
        
        public Array Values { get { return fileIds; } }
    }

    public class Curve : Resource, IAtom
    {
        private readonly CurveCompiler mCompiler;
    	private FileId fileId;
    	
        public Curve(string filename)
        {
			mCompiler = new CurveCompiler(filename);
        }
        
        public override void consolidate()
        {
            List<FileId> fileIds = new List<FileId>();
            mCompiler.collect(fileIds);
            if (fileIds.Count == 1)
                fileId = fileIds[0];
            else
                fileId = new FileId();
        }
        
        public object Value { get { return fileId; } }
    }

    public class CurveIF
    {
        #region Fields

        public double mMinX;
        public double mMaxX;
        public double mMinY;
        public double mMaxY;

        public class Point
        {
            public double mX;
            public double mY;
        }

        public class ControlPoint
        {
            public Point mControlPoint = new Point();
            public Point mTangentBegin = new Point();
            public Point mTangentEnd = new Point();
        }

        public int mNumControlPoints;
        public ControlPoint[] mControlPoints;
        public int mInterpolationFlag;

        public int mNumCurvePoints;
        public Point[] mCurve;

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
                foreach (Point p in mCurve)
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
                    mControlPoints = new ControlPoint[mNumControlPoints];

                    line = reader.ReadLine();
                    while (line[0] == ';')
                    {
                        line = reader.ReadLine();
                    }
                    for (int I = 0; I < mNumControlPoints; I++)		// Load each control point.
                    {
                        mControlPoints[I] = new ControlPoint();

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

                    mCurve = new Point[mNumCurvePoints];

                    for (int I = 0; I < mNumCurvePoints; I++)		// Load each point.
                        mCurve[I] = new Point();

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

    /// <summary>
    /// Compiling data into a binary representation from a .crv file:
    /// </summary>
    public class CurveCompiler : Compiler
    {
        #region Fields

        private readonly Filename mSrcFilename;
        public FileId mFileId;

        #endregion
        #region Constructor

        public CurveCompiler(string filename)
        {
            mSrcFilename = new Filename(filename);
        }

        #endregion
        #region init / compile / resolve / collect

        public override void init(DependencyChecker dc)
        {

        }

        public override bool compile(DependencyChecker dc)
        {
            if (dc.isModified(mSrcFilename))
            {
                CurveIF c = new CurveIF();
                if (c.LoadText(FileCommander.SrcPath + mSrcFilename))
                {
                    if (FileCommander.createDirectoryOnDisk(FileCommander.DstPath, mSrcFilename))
                    {
                        c.SaveBinary(FileCommander.DstPath + mSrcFilename);

                        DepFile depFile = DepFile.sCreate(mSrcFilename, mSrcFilename);
                        if (depFile == null)
                            return false;

                        if (depFile.init(Config.SrcPath, Config.DstPath))
                            if (depFile.save(Config.DstPath))
                                return dc.add(depFile);
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool resolve(BigfileBuilder bfb)
        {
        	int index;
            bfb.indexOf(mSrcFilename, out index);
            mFileId = new FileId(index);
            return true;
        }

        public override void collect(List<FileId> compiledFileIds)
        {
            compiledFileIds.Add(mFileId);
        }

        #endregion
    }
}
