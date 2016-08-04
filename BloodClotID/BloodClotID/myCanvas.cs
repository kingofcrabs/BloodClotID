using System;
using System.Collections.Generic;
using System.Globalization;
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
using Utility;

namespace BloodClotID
{
    public class RenderCanvas : Canvas
    {
       
        List<Circle> hilightCircles = new List<Circle>();
        protected CalibrationInfo calibInfo = new CalibrationInfo();

        public void UpdateBackGroundImage(string file)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(file);
            Background = RenderHelper.CreateBrushFromFile(file);
        }


        public void HilightCircle(List<int> ids, int cameraID)
        {
            LoadCalib(cameraID);
            hilightCircles = calibInfo.circles.Where(x => ids.Contains(x.id)).ToList();
            InvalidateVisual();
        }

    
        internal void SaveCalib(int cameraID)
        {
            string sFile = FolderHelper.GetCalibFile(cameraID);
            calibInfo.size = new Size(this.ActualWidth, this.ActualHeight);
            SerializeHelper.SaveCalib(calibInfo, sFile);
            MessageBox.Show(string.Format("已经保存到{0}！", sFile));
        }

        public void LoadCalib(int cameraID)
        {
            string sFile = FolderHelper.GetCalibFile(cameraID);
            if (!System.IO.File.Exists(sFile))
            {
                throw new Exception(string.Format("配置文件不存在于{0}！", sFile));
            }
            calibInfo = SerializeHelper.LoadCalib(sFile);
            hilightCircles = calibInfo.circles;
            InvalidateVisual();
        }
    }


    public class ResultCanvas : RenderCanvas
    {
        
        Point ptStart;
        public delegate void circleCnt(int cnt);
        public event circleCnt onCircleCntChanged;
        public void AddCircle(Circle newCircle)
        {
            calibInfo.circles.Add(newCircle);
            if (onCircleCntChanged != null)
                onCircleCntChanged(calibInfo.circles.Count);
            this.InvalidateVisual();
        }

      

        public void RemoveCorrespondingCircle(Point pt)
        {
            Point translatePt = ConvertCoord2Real(pt);
            calibInfo.circles.RemoveAll(c => c.IsPointInside(translatePt));
            this.InvalidateVisual();
        }

        private Point ConvertCoord2Calib(Point pt)
        {
            if (calibInfo.size.Width == 0)
                return pt;
            double xRatio = this.ActualWidth / calibInfo.size.Width;
            double yRatio = this.ActualHeight / calibInfo.size.Height;
            return new Point(pt.X / xRatio, pt.Y / yRatio);
        }


        private Point ConvertCoord2Real(Point pt)
        {
            if (calibInfo.size.Width == 0)
                return pt;
            double xRatio = this.ActualWidth / calibInfo.size.Width;
            double yRatio = this.ActualHeight / calibInfo.size.Height;
            return new Point(pt.X * xRatio, pt.Y * yRatio);
        }

        private void ConvertCoord2Real(Circle circle, ref Point ptCenter, ref Size sz)
        {
            ptCenter = ConvertCoord2Real(circle.ptCenter);
            sz = ConvertCoord2Real(new Size(circle.radius, circle.radius));
        }

        private Size ConvertCoord2Real(Size sz)
        {
            if (calibInfo.size.Width == 0)
                return sz;
            double xRatio = this.ActualWidth / calibInfo.size.Width;
            double yRatio = this.ActualHeight / calibInfo.size.Height;
            return new Size(sz.Width * xRatio, sz.Height * yRatio);
        }

        public void SelectCircleAtPosition(Point pt)
        {
            Point ptTranslate = ConvertCoord2Calib(pt);
            ClearSelectFlag();
            Circle firstMatch = calibInfo.circles.Find(x => x.IsPointInside(ptTranslate));
            if (firstMatch != null)
                firstMatch.Selected = true;
            InvalidateVisual();

        }

        public void ClearSelectFlag()
        {
            if (calibInfo.circles == null)
                return;
                
            for (int i = 0; i < calibInfo.circles.Count; i++)
                calibInfo.circles[i].Selected = false;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            int circleID = 1;
            if (calibInfo.circles == null)
                return;
            foreach (Circle circle in calibInfo.circles)
            {
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Color color = circle.Selected ? blue : green;
                Brush brush = new SolidColorBrush(color);
                Point ptCenter = circle.ptCenter;
                Size sz = new Size(circle.ptCenter.X, circle.ptCenter.Y);
                ConvertCoord2Real(circle, ref ptCenter, ref sz);

                drawingContext.DrawEllipse(brush, new Pen(Brushes.Black, 1), ptCenter,sz.Width, sz.Height);
                FormattedText formattedText = new FormattedText(circleID.ToString(), CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Tahoma"), 20, Brushes.Red);
                drawingContext.DrawText(formattedText, ptCenter);
                circleID++;
            }
        }

        

        internal void LeftMoseDown(Point pt, bool bAdd)
        {
            if (bAdd)
                ptStart = pt;
            else
                SelectCircleAtPosition(pt);
        }

        double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            return Math.Sqrt(xx * xx + yy * yy);
        }

        internal void LeftMouseUp(Point pt)
        {
            if (GetDistance(pt, ptStart) < 30)
                return;
            AddCircle(new Circle(ptStart, pt));
        }

        internal void OnKey(System.Windows.Input.Key key)
        {
            Circle circleSelected = calibInfo.circles.Find(x => x.Selected);
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
                    calibInfo.circles.Remove(circleSelected);
                    break;
                default:
                    break;
            }
            InvalidateVisual();
        }



      
    }
}
