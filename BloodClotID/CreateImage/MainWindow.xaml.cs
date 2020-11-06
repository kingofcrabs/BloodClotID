using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace CreateImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateBitmap();
        }

        private void CreateBitmap()
        {
            //6016 4016
            Bitmap bmp = new Bitmap(6016, 4016);
            //Point ptTopLeft = new Point(1426, 1079);
            //Point ptBottomRight = new Point(4615, 3111);
            Point ptTopLeft = new  Point(1426, 1079);
            Point ptBottomRight = new Point(4615, 3111);
            double width = ptBottomRight.X - ptTopLeft.X;
            double height = ptBottomRight.Y - ptTopLeft.Y;
            double widthUnit = width / 11;
            double heightUnit = height / 7;
            
            Random rnd = new Random((int)DateTime.Now.Ticks);
            
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new RectangleF(0, 0, bmp.Width, bmp.Height));
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 12; col++)
                    {
                        //5400 
                        double x = ptTopLeft.X + col * widthUnit;
                        double y = ptTopLeft.Y + row * heightUnit;
                        //Point ptCenter = new Point(x,y);
                        //Point ptStart = new Point(x - radius, y - radius);
                        var val = rnd.Next(100);
                        float ratio = val / 100.0f;
                        g.FillRectangle(Brushes.Red, new RectangleF((float)x, (float)y, 20.0f, 88* ratio));
                    }
                }

                //for (int y = 0; y < 8; y++)
                //{
                //    for (int x = 0; x < 12; x++)
                //    {

                //        int startX = x * 500 + 200;
                //        int startY = y * 500 + 100;

                //        circles.Add(new Circle(new Point(startX, startY), 130));
                //    }
                //}
            }
            bmp.Save("D:\\test.jpg");
           

        }
    }
}
