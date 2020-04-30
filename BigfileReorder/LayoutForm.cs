using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using SlimDX;

namespace BigfileFileReorder
{
    public partial class LayoutForm : Form
    {
        public LayoutForm()
        {
            InitializeComponent();
        }

        private class Point2D
        {
            private double mX;
            private double mY;

            public double x
            {
                set
                {
                    mX = value;
                }
                get
                {
                    return mX;
                }
            }
            public double y
            {
                set
                {
                    mY = value;
                }
                get
                {
                    return mY;
                }
            }

            public PointF toPointF
            {
                get
                {
                    return new PointF((float)x, (float)y);
                }
            }
        }

        private class Sector
        {
            //private Point2D mInnerP1;
            //private Point2D mOuterP1;
            //private Point2D mInnerP2;
            //private Point2D mOuterP2;

            public Sector(Point2D center, double radiusStart, double radiusEnd, double thickness)
            {

            }

            public void draw(Graphics g, Pen p)
            {
                //g.DrawPolygon(p, { mInnerP1.toPointF, mOuterP1.toPointF, mOuterP2.toPointF, mOuterP2.toPointF });
            }
        }

        private class Spiral
        {
            private Point2D mCenter = new Point2D();
            private double mSectorWidth = 3;
            private double mSectorHeight = 3;
            private double mMinRadius = 50;
            private double mMaxRadius = 460;

            private List<Sector> mSectors = new List<Sector>();

            public int compute(Graphics g)
            {
                // l = 2 * PI * r

                // angle = ( 4 / l ) * 2 * PI
                // angle = ( 4 / (2 * PI * r) ) * 2 * PI
                // angle = ( 4 / (r) )

                mCenter.x = 475;
                mCenter.y = 462;
                
                double currentAngle = 0.0;
                double currentRadius = mMinRadius;

                PointF[] poly = new PointF[4];
                poly[0].X = 0;
                poly[0].Y = (float)(mMinRadius - (mSectorHeight / 2));
                poly[1].X = 0;
                poly[1].Y = (float)(mMinRadius + (mSectorHeight / 2));

                PointF[] drawPoly = new PointF[4];

                SolidBrush sb = new SolidBrush(Color.Red);

                g.TranslateTransform((float)mCenter.x, (float)mCenter.y);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int numSectors = 0;
                while (currentRadius < mMaxRadius)
                {
                    double deltaAngle = (mSectorWidth / (2 * Math.PI * currentRadius)) * 2 * Math.PI;
                    currentAngle += deltaAngle;

                    // Rotation
                    poly[2].X = (float)((currentRadius + (mSectorHeight / 2)) * Math.Sin(currentAngle));
                    poly[2].Y = (float)((currentRadius + (mSectorHeight / 2)) * Math.Cos(currentAngle));
                    poly[3].X = (float)((currentRadius - (mSectorHeight / 2)) * Math.Sin(currentAngle));
                    poly[3].Y = (float)((currentRadius - (mSectorHeight / 2)) * Math.Cos(currentAngle));


                    sb.Color = Color.FromArgb((int)(numSectors*1.1) & 255, (int)(128 + 1.3*numSectors)&255, (int)(64 + 0.9*numSectors)&255);
                    g.FillPolygon(sb, poly);
                    ++numSectors;

                    // Increase radius
                    currentRadius = (currentAngle / (2 * Math.PI)) * mSectorWidth;

                    // Next point
                    poly[0] = poly[3];
                    poly[1] = poly[2];
                }

                g.TranslateTransform(0, 0);
                return numSectors;
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            double dvdFullTrackLength = 11800;                              // (m)
            //double dvdMostInnerRadiusDVD = 0.02;                            // (m)
            //double dvdMostOuterRadiusDVD = 0.12;                            // (m)

            double dvdFullSizeInBytes = 4.7 * 1024 * 1024 * 1024;           // 4.7 Gb
            double dvdBytesPerMeter = dvdFullSizeInBytes / dvdFullTrackLength;  // (bytes/m)

            Spiral s = new Spiral();
            int numSectors = s.compute(e.Graphics);

            int numKiloBytesPerSector = (int)((dvdFullSizeInBytes / numSectors) / 1024);
        }
    }
}
