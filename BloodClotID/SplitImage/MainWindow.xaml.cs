using Emgu.CV;
using System;
using System.Collections.Generic;
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

namespace SplitImage
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

        private void btnSplit_Click(object sender, RoutedEventArgs e)
        {
            btnSplit.IsEnabled = false;
            SplitImages(@"D:\test\test.jpg");
            btnSplit.IsEnabled = true;
        }


        private void SplitImages(string filePath)
        {
           
            Mat srcImg = new Mat(filePath);
            srcImg.Save(@"D:\test\odd.jpg");
            Point ptTopLeft = new Point(1426, 1079);
            Point ptBottomRight = new Point(4615, 3111);
            double width = ptBottomRight.X - ptTopLeft.X;
            double height = ptBottomRight.Y - ptTopLeft.Y;
            double widthUnit = width / 11;
            double heightUnit = height / 7;
            int radius = 88;
            int ID = 1;
            for(int row = 0; row < 8; row++)
            {
                for(int col = 0; col < 12; col++)
                {
                    double x = ptTopLeft.X + col * widthUnit;
                    double y = ptTopLeft.Y + row * heightUnit;
                    //Point ptCenter = new Point(x,y);
                    Point ptStart = new Point(x - radius/2, y-10);
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)ptStart.X,(int)ptStart.Y,radius,radius+15);
                    Mat subImage = new Mat(srcImg, rect);
                    subImage.Save(string.Format("d:\\test\\{0}.jpg", ID++));
                }
            }
            //1426,1079   4615,1076  
            //1429,3111   4615,3106

            //System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 100, 100);
            //var roi = new Mat(dst, rect);
            //unsafe
            //{
            //    byte* head = (byte*)roi.DataPointer;
            //    for(int r = 0; r< roi.Rows; r++)
            //    {

            //        for(int c = 0; c< roi.Cols; c++)
            //        {

            //            byte val = *head;
            //        double distance = GetDistance(c, r, xx, yy); // > innerRadius
            //            head++;
            //        }
            //    }
            //}


        }

    
    }
}
