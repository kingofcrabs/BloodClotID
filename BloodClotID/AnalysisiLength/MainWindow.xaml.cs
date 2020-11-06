using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalysisiLength
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetRectWidthAndHeight(Mat img,string folder,int index)
        {
           
            Mat smallImage = img.Clone();
            //CvInvoke.Resize(srcImage, smallImage, new Size(srcImage.Size.Width / 4, srcImage.Size.Height/4));
            CvInvoke.Threshold(smallImage, smallImage, 120, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            smallImage.Save(@"d:\test\binary.jpg");
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hiearch = new Mat();
            CvInvoke.FindContours(smallImage, contours, hiearch, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
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
            Mat smallColorImg = new Mat();
            CvInvoke.CvtColor(smallImage, smallColorImg, ColorConversion.Gray2Bgr);
            CvInvoke.DrawContours(smallColorImg, contours, maxIndex, new MCvScalar(0, 255, 0), 2);
            
            var rotateRect = CvInvoke.MinAreaRect(contours[maxIndex]);
            Mat mapMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(rotateRect.Center, rotateRect.Angle, 1, mapMatrix);
            var vertices = rotateRect.GetVertices();
            Mat colorImg = new Mat();
            CvInvoke.CvtColor(img, colorImg, ColorConversion.Gray2Bgr);
            PlatePositon platePosition;
            SetPlatePosition(vertices, out platePosition);
            for(int col = 0; col < PlatePositon.ColCnt; col++)
            {
                for(int row = 0; row < PlatePositon.RowCnt; row++)
                {
                    var pt = platePosition.GetAffinePosition(row, col);
                    var ptEnd = platePosition.GetAffinePosition(row+1, col+1);
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point((int)pt.X, (int)pt.Y),
                        new System.Drawing.Size((int)(ptEnd.X - pt.X), (int)(ptEnd.Y - pt.Y)));
                    CvInvoke.Rectangle(colorImg, rect, new MCvScalar(0, 0, 255), 2);
                    //CvInvoke.Circle(colorImg, new System.Drawing.Point((int)pt.X, (int)pt.Y),
                    //    PlatePositon.radius, new MCvScalar(0, 0, 255), 2);
                }
            }
            colorImg.Save(folder + string.Format("{0}.jpg", index + 1));

            //for(int i =0; i< 4; i++)
            //{
            //    var ptStart = vertices[i];
            //    var ptEnd = vertices[(i + 1) % 4];
            //    CvInvoke.Line(colorImg, new System.Drawing.Point((int)ptStart.X, (int)ptStart.Y),
            //        new System.Drawing.Point((int)ptEnd.X,(int)ptEnd.Y), new MCvScalar(0, 0, 255), 1);
            //}
            //
            //Debug.WriteLine( string.Format("width:{0}, height :{1}", rotateRect.Size.Width,rotateRect.Size.Height));

        }

        private void SetPlatePosition(PointF[] vertices, out PlatePositon platePosition)
        {
            double totalX = 0; 
            double totalY = 0;

            foreach(var pt in vertices)
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

                if(pt.X < avgX)
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

            platePosition = new PlatePositon(ptTopLeft, ptTopRight, ptBottomLeft);
        }

        void GetSubImage()
        {

            //Mat img = new Mat(@"d:\test\test.jpg", ImreadModes.Grayscale);
            string initFolder = @"D:\projects\BloodClotID.git\Pictures\";
            string dstFolder = @"D:\projects\BloodClotID.git\trunk\BloodClotID\BloodClotID\bin\TestImages\";
            var files = Directory.EnumerateFiles(initFolder, "*.jpg");
            System.Drawing.Point ptStart = new System.Drawing.Point(1100, 700);
            System.Drawing.Size sz = new System.Drawing.Size(4920 - 1100, 3400 - 700);
            int ID = 1;
            foreach (var file in files)
            {
                Mat img = new Mat(file);

                Mat subImg = new Mat(img, new System.Drawing.Rectangle(ptStart, sz));
                subImg.Save(dstFolder + string.Format("pic{0}.jpg", ID++));
            }
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //GetSubImage();
            //return;

            string initFolder = @"D:\projects\BloodClotID.git\trunk\BloodClotID\BloodClotID\bin\TestImages\";
            var files = Directory.EnumerateFiles(initFolder, "*.jpg");
            int i = 0;
            foreach (var file in files)
            {
                if (!file.Contains("pic"))
                    continue;
                Mat img = new Mat(file,ImreadModes.Grayscale);
                GetRectWidthAndHeight(img,initFolder,i++);
            }

            //LengthAnalyzer lengthAnalyzer = new LengthAnalyzer();
            //Mat img = new Mat(@"D:\projects\BloodClotID.git\trunk\BloodClotID\BloodClotID\bin\Output\30org.jpg");
            //List<Point> ptCorners = new List<Point>();
            //var length = lengthAnalyzer.CalculateLength(img,1, ptCorners);
            //img.Save(@"d:\temp\result.jpg");
            //Debug.WriteLine(length);
        }
    }
}
