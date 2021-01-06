using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BloodClotID
{
    class LengthAnalyzer
    {
        public double CalculateLength(Mat srcImg,int wellID, List<System.Windows.Point> cornerPts)
        {
            Mat cloneImg = srcImg.Clone();
            CvInvoke.CvtColor(cloneImg, cloneImg, Emgu.CV.CvEnum.ColorConversion.Bgr2Hls);
            Mat[] channels = cloneImg.Split();
            Mat hue = channels[0];
            Mat light = channels[1];
            Mat saturation = channels[2];


            CvInvoke.Threshold(saturation, saturation, 120, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
           
            VectorOfPoint white_pixels = new VectorOfPoint();
            CvInvoke.FindNonZero(saturation, white_pixels);
            if (white_pixels.Size < 100)
            {
                cornerPts = null;
                return 0;
            }



            Mat binary = saturation.Clone();
            CvInvoke.Threshold(hue, hue, 15, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            CvInvoke.BitwiseAnd(hue, binary, binary);
            CvInvoke.Canny(binary, binary, 255, 255, 5, true);


            VectorOfPoint contour = FindMaxContour(binary);
         
            if (contour.Size < 30)
            {
                cornerPts = null;
                return 0;
            }

            var rotatedRect = CvInvoke.MinAreaRect(contour);
            
            var vertices = rotatedRect.GetVertices();
            double maxDistance = 0;

            int height = cloneImg.Rows;
            int width = cloneImg.Cols;
            int xCenter = width / 2;
            int yCenter = height / 2;

            for (int i = 0; i < 4; i++)
            {
                var pt1 = vertices[i];
                var pt2 = vertices[(i + 1) % 4];
                CvInvoke.Line(srcImg, new Point((int)pt1.X, (int)pt1.Y),new Point((int)pt2.X, (int)pt2.Y), new MCvScalar(0, 255, 0), 2);
                var distance = GetDistance(pt1.X, pt1.Y, pt2.X, pt2.Y);
                if (distance > maxDistance)
                    maxDistance = distance;
                cornerPts.Add(new System.Windows.Point((vertices[i].X - xCenter), (vertices[i].Y - yCenter)));
            }
          
            return maxDistance;
        }

        private double GetDistance(float x1, float y1, float x2, float y2)
        {
            double xx = (x1 - x2) * (x1 - x2);
            double yy = (y1 - y2) * (y1 - y2);
            return Math.Sqrt(xx + yy);
        }

        VectorOfPoint FindMaxContour(Mat src)
        {
            
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(src, contours, null,RetrType.Ccomp,ChainApproxMethod.ChainApproxSimple);
            int height = src.Rows;
            int width = src.Cols;
            int maxSize = 0;
            VectorOfPoint maxContour = new VectorOfPoint();
            float toobig = (height + width) * 1.5f;
            for (int i = 0; i < contours.Size; i++)
            {
                int contourSize = contours[i].Size;
                if (contourSize > toobig)
                    continue;
                if (contourSize > maxSize)
                {
                    maxSize = contourSize;
                    maxContour = contours[i];
                }
            }
            return maxContour;
        }
        private void SetPlatePosition(System.Drawing.PointF[] vertices)
        {
            double totalX = 0;
            double totalY = 0;

            foreach (var pt in vertices)
            {
                totalX += pt.X;
                totalY += pt.Y;
            }
            double avgX = totalX / 4;
            double avgY = totalY / 4;

            System.Windows.Point ptTopLeft = new System.Windows.Point();
            System.Windows.Point ptTopRight = new System.Windows.Point();
            System.Windows.Point ptBottomLeft = new System.Windows.Point();
            foreach (var pt in vertices)
            {
                int xoffset = 35;
                int yoffset = 8;
                var winPt = new System.Windows.Point(pt.X, pt.Y);
                if (pt.X > avgX && pt.Y < avgY)
                {
                    ptTopRight = winPt;
                    ptTopRight.X -= xoffset;
                    ptTopRight.Y += yoffset;
                }

                if (pt.X < avgX)
                {
                    if (pt.Y < avgY)
                    {
                        ptTopLeft = winPt;
                        ptTopLeft.X += xoffset;
                        ptTopLeft.Y += yoffset;
                    }
                    else
                    {
                        ptBottomLeft = winPt;
                        ptBottomLeft.X += xoffset;
                        ptBottomLeft.Y -= yoffset;
                    }

                }
            }

            PlatePositon.Instance.Set(ptTopLeft, ptTopRight, ptBottomLeft);
        }
        internal List<double> GetEachWellLength(string file,
            List<List<System.Windows.Point>> eachWellCornerPts,List<System.Windows.Point> centerPts)
        {
            Mat img = new Mat(file);
            //CvInvoke.Flip(img, img, FlipType.Horizontal);
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(img, grayImage, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(grayImage, grayImage, 120, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hiearch = new Mat();
            CvInvoke.FindContours(grayImage, contours, hiearch, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            List<int> eachSize = new List<int>();
            List<double> variances = new List<double>();
            List<VectorOfPoint> allContours = new List<VectorOfPoint>();
            VectorOfVectorOfPoint roundContours = new VectorOfVectorOfPoint();

            int maxIndex = 0;
            int maxLen = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                if (contours[i].Size > maxLen)
                {
                    maxLen = contours[i].Size;
                    maxIndex = i;
                }
            }


            var rotateRect = CvInvoke.MinAreaRect(contours[maxIndex]);
            Mat mapMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(rotateRect.Center, rotateRect.Angle, 1, mapMatrix);
            var vertices = rotateRect.GetVertices();


            for (int i = 0; i < 4; i++)
            {
                var pt1 = vertices[i];
                var pt2 = vertices[(i + 1) % 4];
                CvInvoke.Line(img, new Point((int)pt1.X, (int)pt1.Y), new Point((int)pt2.X, (int)pt2.Y), new MCvScalar(0, 255, 0), 2);
   
            }
           // img.Save("d:\\test\\contour.jpg");


            SetPlatePosition(vertices);
            int ID = 1;
            List<double> lengths = new List<double>();
            
            for (int col = 0; col < PlatePositon.ColCnt; col++)
            {
                for (int row = 0; row < PlatePositon.RowCnt; row++)
                {
                    var pt = PlatePositon.Instance.GetAffinePosition(row, col);
                    var ptEnd = PlatePositon.Instance.GetAffinePosition(row + 1, col + 1);
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point((int)pt.X, (int)pt.Y),
                        new System.Drawing.Size((int)(ptEnd.X - pt.X), (int)(ptEnd.Y - pt.Y)));
                    List<System.Windows.Point> cornerPts = new List<System.Windows.Point>();
                    Mat subMat = new Mat(img, rect);
                    subMat.Save(string.Format("d:\\test\\sub{0}.jpg",ID++));
                    lengths.Add(CalculateLength(subMat, col + 1, cornerPts));
                    eachWellCornerPts.Add(cornerPts);
                    centerPts.Add(new System.Windows.Point((pt.X + ptEnd.X) / 2, (pt.Y + ptEnd.Y) / 2));
                }
            }

            //colorImg.Save(folder + string.Format("{0}.jpg", index + 1));
            return lengths;


            
        
        }
    }


}
