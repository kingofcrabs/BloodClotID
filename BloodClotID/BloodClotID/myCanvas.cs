using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BloodClotID
{

    [Serializable]
    public class Circle
    {
        public Point ptCenter;
        public double radius;
        public bool bSelected;
        public int id;
        public Circle(Point ptStart, Point ptEnd)
        {
            // TODO: Complete member initialization
            double x = (ptStart.X + ptEnd.X) / 2.0;
            double y = (ptStart.Y + ptEnd.Y) / 2.0;
            ptCenter = new Point(x, y);
            radius = GetDistance(ptStart, ptEnd) / 2;
            bSelected = false;
        }
        public Circle()
        {

        }
        public Circle(Point ptEnd, double r, int id)
        {
            ptCenter = ptEnd;
            radius = r;
            bSelected = false;
            this.id = id;
        }

        private double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            double dis = Math.Sqrt(xx * xx + yy * yy);
            return dis;
        }

        public bool IsPointInside(Point ptTest)
        {
            return GetDistance(ptCenter, ptTest) <= radius;
        }

        public override string ToString()
        {
            return string.Format("{0:0.00}_{1:0.00};{2:0.00}", ptCenter.X, ptCenter.Y, radius);
        }
    }

    public class RenderCanvas : Canvas
    {
        BitmapImage _img = null;
        List<Circle> circles = new List<Circle>();
        List<Circle> hilightCircles = new List<Circle>();
        //public void SetBkGroundImage(string sFile)
        //{
        //    _img = new BitmapImage();
        //    _img.BeginInit();
        //    _img.CacheOption = BitmapCacheOption.OnLoad;
        //    _img.UriSource = new Uri(sFile, UriKind.Absolute);
        //    _img.EndInit();
        //    ImageBrush imageBrush = new ImageBrush();
        //    imageBrush.ImageSource = _img;
        //    this.Background = imageBrush;
        //}

        public void UpdateBackGroundImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            System.Drawing.Bitmap cloneBitmpa = (System.Drawing.Bitmap)bitmap.Clone();
            using (MemoryStream memory = new MemoryStream())
            {
                cloneBitmpa.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = bitmapImage;
            Background = imgBrush;
            this.InvalidateVisual();
        }


        public void HilightCircle(List<int> ids)
        {
            LoadConfig();
            hilightCircles = circles.Where(x => ids.Contains(x.id)).ToList();
            InvalidateVisual();
        }

        private string GetConfigFolder()
        {
            string sFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Config\\";
            if (!System.IO.Directory.Exists(sFolder))
                System.IO.Directory.CreateDirectory(sFolder);
            return sFolder;
        }

        private string GetConfigFile()
        {
            return GetConfigFolder() + "ROIs.xml";
        }
        internal void SaveConfig()
        {
            string sFile = GetConfigFile();
            SerializeHelper.SaveConfig(circles, sFile);
            MessageBox.Show(string.Format("已经保存到{0}！", sFile));
        }

        internal void LoadConfig()
        {
            string sFile = GetConfigFile();
            if (!System.IO.File.Exists(sFile))
            {
                throw new Exception(string.Format("配置文件不存在于{0}！", sFile));
            }
            SerializeHelper.LoadConfig(ref circles, sFile);
            InvalidateVisual();
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (Circle circle in hilightCircles)
            {
                drawingContext.DrawEllipse(null, new Pen(Brushes.Black, 1), circle.ptCenter, circle.radius, circle.radius);
            }
        }
    }


    public class CalibCanvas : RenderCanvas
    {
        List<Circle> circles = new List<Circle>();
        
        Point ptStart;

        public void AddCircle(Circle newCircle)
        {
            circles.Add(newCircle);
            this.InvalidateVisual();
        }

      

        public void RemoveCorrespondingCircle(Point pt)
        {
            circles.RemoveAll(c => c.IsPointInside(pt));
            this.InvalidateVisual();
        }


        public void SelectCircleAtPosition(Point pt)
        {
            ClearSelectFlag();
            Circle firstMatch = circles.Find(x => x.IsPointInside(pt));
            if (firstMatch != null)
                firstMatch.bSelected = true;
            InvalidateVisual();

        }

        public void ClearSelectFlag()
        {
            for (int i = 0; i < circles.Count; i++)
                circles[i].bSelected = false;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (Circle circle in circles)
            {
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Color color = circle.bSelected ? blue : green;
                Brush brush = new SolidColorBrush(color);
                drawingContext.DrawEllipse(brush, new Pen(Brushes.Black, 1), circle.ptCenter, circle.radius, circle.radius);
            }
        }

        internal void LeftMoseDown(Point pt, bool bAdd)
        {
            if (bAdd)
                ptStart = pt;
            else
                SelectCircleAtPosition(pt);
        }

        internal void LeftMouseUp(Point pt)
        {
            AddCircle(new Circle(ptStart, pt));
        }

        internal void OnKey(System.Windows.Input.Key key)
        {
            Circle circleSelected = circles.Find(x => x.bSelected);
            if (circleSelected == null)
                return;
            switch (key)
            {
                case Key.NumPad8: //up
                    circleSelected.ptCenter.Y--;
                    break;
                case Key.NumPad2:
                    circleSelected.ptCenter.Y++;
                    break;
                case Key.NumPad4:
                    circleSelected.ptCenter.X--;
                    break;
                case Key.NumPad6:
                    circleSelected.ptCenter.X++;
                    break;
                case Key.Add:
                    circleSelected.radius++;
                    break;
                case Key.Subtract:
                    circleSelected.radius--;
                    break;
                case Key.Delete:
                    circles.Remove(circleSelected);
                    break;
                default:
                    break;
            }
            InvalidateVisual();
        }



      
    }
}
