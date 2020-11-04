using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisiLength
{
    class LengthAnalyzer
    {
        public double CalculateLength(Mat srcImg,int wellID, List<System.Windows.Point> cornerPts)
        {
            CvInvoke.CvtColor(srcImg, srcImg, Emgu.CV.CvEnum.ColorConversion.Bgr2Hls);
            Mat[] channels = srcImg.Split();
            Mat hue = channels[0];
            Mat light = channels[1];
            Mat saturation = channels[2];
            Mat binary = hue.Clone();
            CvInvoke.Threshold(hue, hue, 15, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            CvInvoke.Threshold(light, light, 130, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            CvInvoke.Threshold(saturation, saturation, 80, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);
            CvInvoke.BitwiseAnd(hue, light, hue);
            CvInvoke.BitwiseAnd(hue, saturation, binary);
            CvInvoke.Canny(binary, binary, 255, 255, 5, true);


            VectorOfPoint contour = FindMaxContour(binary);
         
            if (contour.Size < 30)
            {
                cornerPts = null;
            }

            var rotatedRect = CvInvoke.MinAreaRect(contour);
            var vertices = rotatedRect.GetVertices();
            double maxDistance = 0;

            int height = srcImg.Rows;
            int width = srcImg.Cols;
            int xCenter = width / 2;
            int yCenter = height / 2;

            for (int i = 0; i < 4; i++)
            {
                var pt1 = vertices[i];
                var pt2 = vertices[(i + 1) % 4];
                var distance = GetDistance(pt1.X, pt1.Y, pt2.X, pt2.Y);
                if (distance > maxDistance)
                    maxDistance = distance;
                cornerPts.Add(new System.Windows.Point(vertices[i].X - xCenter, vertices[i].Y - yCenter));
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

    }


}
