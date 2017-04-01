using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected Rect bound;
        
        protected List<AnalysisResult> analysisResults = new List<AnalysisResult>();
        protected CalibrationInfo calibInfo = new CalibrationInfo();
        protected Size bkImgSize;

        Point ptStart;
        Circle selected = null;
        List<Circle> roi_Circles = new List<Circle>();
        List<int> resultIndexs = new List<int>(); 
        bool canMove = false;

        public void UpdateBackGroundImage(string file, int cameraID = 0)
        {
            if (!File.Exists(file))
                throw new Exception("没找到图片！");

            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                bkImgSize = new Size(bmpTemp.Size.Width, bmpTemp.Size.Height);
            }
            calibInfo.circles.Clear();
            roi_Circles.Clear();
            if (cameraID != 0)
            {
                string sFile = FolderHelper.GetCalibFile(cameraID);
                calibInfo = SerializeHelper.LoadCalib(sFile);
                
                foreach(var circle in calibInfo.circles)
                {
                    CoordinationHelper coordHelper = CreateCoordHelper();
                    roi_Circles.Add(coordHelper.ToUI(circle));
                }
            }
            Background = RenderHelper.CreateBrushFromFile(file, calibInfo);
        }

       
        internal void SaveCalib(int cameraID)
        {
            string sFile = FolderHelper.GetCalibFile(cameraID);
            CoordinationHelper coordHelper = new CoordinationHelper(new Size(this.ActualWidth, this.ActualHeight), bkImgSize);
            calibInfo.circles.Clear();
            foreach(var circle in roi_Circles)
            {
                calibInfo.circles.Add(coordHelper.ToReal(circle));
            }
            calibInfo.circles = calibInfo.circles.OrderBy(c => GetIndex(c)).ToList();
            SerializeHelper.SaveCalib(calibInfo, sFile);
            MessageBox.Show(string.Format("已经保存到{0}！", sFile));
        }

        private int GetIndex(Circle c)
        {
            double w = 2100;
            double h = 1400;
            double xUnit = w / 5;
            double yUnit = h / 3;
            int colIndex = (int)Math.Round(c.ptCenter.X / xUnit - 1);
            int rowIndex = (int)Math.Round(c.ptCenter.Y / yUnit - 1);
            return colIndex * 4 + rowIndex;

        }

        

        private string Rect2String(Rect rect)
        {
            Point ptTopLeft = rect.TopLeft;
            Point ptBottomRight = rect.BottomRight;
            return string.Format("{0} {1} {2} {3}", ptTopLeft.X, ptTopLeft.Y, ptBottomRight.X, ptBottomRight.Y);
        }

        public void LoadCalib(int cameraID)
        {
            string sFile = FolderHelper.GetCalibFile(cameraID);
            if (!System.IO.File.Exists(sFile))
            {
                throw new Exception(string.Format("配置文件不存在于{0}！", sFile));
            }
            calibInfo = SerializeHelper.LoadCalib(sFile);
            roi_Circles.Clear();
            calibInfo.circles.ForEach(c => roi_Circles.Add(Coord2UI(c)));
            InvalidateVisual();
        }

      

        private void AddCircle(Circle newCircle)
        {
            roi_Circles.Add(newCircle);
        }
        //private void SetRect(Rect rect)
        //{
        //    calibInfo.rect = new Rect(Coord2Real(rect.TopLeft), Coord2Real(rect.BottomRight));
        //}

        public void RemoveCorrespondingCircle(Point pt)
        {
            Point translatePt = Coord2Real(pt);
            calibInfo.circles.RemoveAll(c => c.IsPointInside(translatePt));
            this.InvalidateVisual();
        }

        #region coordination translation, only real & UI coordination now.
        private Point Coord2UI(Point pt)//go smaller
        {
            double xRatio = this.ActualWidth / bkImgSize.Width;
            double yRatio = this.ActualHeight / bkImgSize.Height;
            return new Point(pt.X * xRatio, pt.Y * yRatio);
        }

        private Circle Coord2UI(Circle circle) //go smaller
        {
            double xRatio = this.ActualWidth / bkImgSize.Width;
            double yRatio = this.ActualHeight / bkImgSize.Height;
            Point ptCenter = Coord2UI(circle.ptCenter);
            double radius = circle.radius * xRatio;
            return new Circle(ptCenter, radius);
        }

        private Point Coord2Real(Point pt) //go bigger
        {
            double xRatio = bkImgSize.Width / this.ActualWidth;
            double yRatio = bkImgSize.Height / this.ActualHeight;
            return new Point(pt.X * xRatio, pt.Y * yRatio);
        }

        private Circle Coord2Real(Circle circle) //go bigger
        {
            double xRatio = bkImgSize.Width / this.ActualWidth;
            Point ptCenter = Coord2Real(circle.ptCenter);
            double radius = circle.radius * xRatio;
            return new Circle(ptCenter, radius);
        }



        #endregion


        //#region coordinate translation
        //private Point ConvertCoord2Calib(Point pt)
        //{
        //    if (GlobalVars.IsCalibration)
        //    {
        //        if (calibInfo.size.Width == 0)
        //            return pt;
        //        double xRatio = this.ActualWidth / calibInfo.size.Width;
        //        double yRatio = this.ActualHeight / calibInfo.size.Height;
        //        return new Point(pt.X / xRatio, pt.Y / yRatio);
        //    }
        //    else
        //    {
        //        double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
        //        double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
        //        double xRatio = virtualActualWidth / calibInfo.size.Width;
        //        double yRatio = virtualActualHeight / calibInfo.size.Height;
        //        double xOffset = (virtualActualWidth - this.ActualWidth) * calibInfo.rect.TopLeft.X / (calibInfo.size.Width - calibInfo.rect.Width);
        //        double yOffset = (virtualActualHeight - this.ActualHeight) * calibInfo.rect.TopLeft.Y / (calibInfo.size.Height - calibInfo.rect.Height);
        //        return new Point((pt.X + xOffset) / xRatio, (pt.Y + yOffset) / yRatio);
        //    }
        //}

        //private Point ConvertCoordImage2RealRelative(Point pt, Size imgSize) //use point as size, 此处是相对位置，所以不需要减去offset
        //{
        //    if (GlobalVars.IsCalibration)
        //    {
        //        double xRatio = this.ActualWidth / imgSize.Width;
        //        double yRatio = this.ActualHeight / imgSize.Height;
        //        return new Point(pt.X * xRatio, pt.Y * yRatio);
        //    }
        //    else
        //    {
        //        double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
        //        double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
        //        double xRatio = virtualActualWidth / imgSize.Width;
        //        double yRatio = virtualActualHeight / imgSize.Height;
        //        return new Point(pt.X * xRatio, pt.Y * yRatio);
        //    }
        //}

        //public Point ConvertCoordCalib2Real(Point pt)
        //{
        //    if (calibInfo.size.Width == 0)
        //        return pt;
        //    double xOffset = 0;
        //    double yOffset = 0;

        //    double xRatio, yRatio;
        //    if (GlobalVars.IsCalibration)
        //    {
        //        xRatio = this.ActualWidth / calibInfo.size.Width;
        //        yRatio = this.ActualHeight / calibInfo.size.Height;
        //    }
        //    else
        //    {
        //        double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
        //        double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
        //        xOffset = (virtualActualWidth - this.ActualWidth) * calibInfo.rect.TopLeft.X / (calibInfo.size.Width - calibInfo.rect.Width);
        //        yOffset = (virtualActualHeight - this.ActualHeight) * calibInfo.rect.TopLeft.Y / (calibInfo.size.Height - calibInfo.rect.Height);
        //        xRatio = virtualActualWidth / calibInfo.size.Width;
        //        yRatio = virtualActualHeight / calibInfo.size.Height;
        //    }

        //    return new Point(pt.X * xRatio - xOffset, pt.Y * yRatio - yOffset);
        //}

        //private void ConvertCoordCalib2Real(Circle circle, ref Point ptCenter, ref Size sz)
        //{
        //    ptCenter = ConvertCoordCalib2Real(circle.ptCenter);
        //    sz = ConvertCoordCalib2Real(new Size(circle.radius, circle.radius));
        //}

        //private Size ConvertCoordCalib2Real(Size sz)
        //{
        //    if (calibInfo.size.Width == 0)
        //        return sz;

        //    double xRatio, yRatio;
        //    if (GlobalVars.IsCalibration)
        //    {
        //        xRatio = this.ActualWidth / calibInfo.size.Width;
        //        yRatio = this.ActualHeight / calibInfo.size.Height;
        //    }
        //    else
        //    {
        //        double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
        //        double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
        //        xRatio = virtualActualWidth / calibInfo.size.Width;
        //        yRatio = virtualActualHeight / calibInfo.size.Height;
        //    }

        //    return new Size(sz.Width * xRatio, sz.Height * yRatio);
        //}
        //#endregion

        public void SelectCircleAtPosition(Point pt)
        {
            ClearSelectFlag();
            Circle firstMatch = roi_Circles.Find(x => x.IsPointInside(pt));
            if (firstMatch != null)
            {
                selected = firstMatch;
                canMove = true;
            }
            InvalidateVisual();
        }

        internal void Highlight(int indexInPlate)
        {
            resultIndexs.Add(indexInPlate);
        }

        internal void ClearHight()
        {
            resultIndexs.Clear();
        }
        public void SetResult(List<AnalysisResult> result)
        {
            analysisResults = result;
            InvalidateVisual();
        }

        public void ClearSelectFlag()
        {
            selected = null;
            canMove = false;
        }

        public static bool IsInDesignMode()
        {
            if (System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio"))
            {
                return true;
            }
            return false;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (IsInDesignMode())
                return;
            int circleID = 1;
            if (calibInfo.circles == null)
                return;
            Pen redPen = new Pen(Brushes.Red, 2);
            Pen blackPen = new Pen(Brushes.Black, 1);
            //foreach (Circle circle in roi_Circles)
            for (int i = 0; i < roi_Circles.Count; i++)
            {
                Circle circle = roi_Circles[i];
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Brush brush = circle == selected ? new SolidColorBrush(blue) : null;

                Size sz = new Size(circle.ptCenter.X, circle.ptCenter.Y);
                Point ptCenter = circle.ptCenter;
                Pen pen = blackPen;
                if(!GlobalVars.IsCalibration)
                {
                    Circle calibCircle = calibInfo.circles[i];
                    if(resultIndexs.Contains(i))
                        pen = redPen;
                }
                drawingContext.DrawEllipse(brush, pen, ptCenter, circle.radius, circle.radius);
                circleID++;
            }

            //if (GlobalVars.IsCalibration && calibInfo.rect.Width != 0 && calibInfo.rect.Height != 0)
            //{
            //    Brush brush = new SolidColorBrush(Colors.Blue);
            //    Point ptStart = ConvertCoordCalib2Real(calibInfo.rect.Location);
            //    Size sz = ConvertCoordCalib2Real(calibInfo.rect.Size);
            //    Rect rc = new Rect(ptStart, sz);
            //    drawingContext.DrawRectangle(null, new Pen(brush, 1), rc);
            //}
            if(this.Name == "pic1")
            {
                Debug.WriteLine("debug");
            }

            
            if (this.Background != null && analysisResults != null)
            {
                CoordinationHelper coordinationHelper = new CoordinationHelper(new Size(this.ActualWidth, this.ActualHeight), bkImgSize);
                for (int i = 0; i < analysisResults.Count; i++)// (AnalysisResult analysisResult in analysisResults)
                {
                    var result = analysisResults[i];
                    for (int j = 0; j < 4; j++)
                    {
                        Point startPtOffset = result.RotateRectPoints[j];
                        Point endPtOffset = result.RotateRectPoints[(j + 1) % 4];

                        var translateStartOffset = coordinationHelper.ToUI(startPtOffset);//ConvertCoordImage2RealRelative(startPtOffset, bkImgSize);
                        var translateEndOffset = coordinationHelper.ToUI(endPtOffset);
                        var ptCenter = coordinationHelper.ToUI(calibInfo.circles[i].ptCenter);
                        int val = result.val;
                        FormattedText formattedText = new FormattedText(val.ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Tahoma"), 20, Brushes.Green);
                        if (j == 3)
                        {
                            var ptText = new Point(ptCenter.X, ptCenter.Y);
                            drawingContext.PushTransform(new RotateTransform(-90, ptText.X, ptText.Y));
                            drawingContext.DrawText(formattedText, new Point(ptCenter.X, ptCenter.Y));
                            drawingContext.Pop();
                        }
                        drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(ptCenter.X + translateStartOffset.X, ptCenter.Y + translateStartOffset.Y),
                            new Point(ptCenter.X + translateEndOffset.X, ptCenter.Y + translateEndOffset.Y));
                    }
                }
            }
        }


        bool LeftCtrlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        internal void LeftMoseDown(Point pt, bool bAdd)
        {
            if (LeftCtrlDown())
            {
                ptStart = pt;
                canMove = true;
                return;
            }

            if (bAdd)
            {
                ptStart = pt;
            }
            else
                SelectCircleAtPosition(pt);
        }

        double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            return Math.Sqrt(xx * xx + yy * yy);
        }

        internal void LeftMouseUp(Point pt, bool bAdd)
        {

            canMove = false;
            //if (LeftCtrlDown())
            //{
            //    SetRect(new Rect(ptStart, pt));
            //    return;
            //}
            if (bAdd)
            {
                AddCircle(new Circle(ptStart, pt));
            }

            InvalidateVisual();
        }


        internal void OnKey(System.Windows.Input.Key key)
        {

            if (selected == null)
                return;
            switch (key)
            {
                case Key.NumPad8: //up
                    selected.ptCenter.Y--;
                    break;
                case Key.NumPad2:
                    selected.ptCenter.Y++;
                    break;
                case Key.NumPad4:
                    selected.ptCenter.X--;
                    break;
                case Key.NumPad6:
                    selected.ptCenter.X++;
                    break;
                case Key.Add:
                    selected.radius++;
                    break;
                case Key.Subtract:
                    selected.radius--;
                    break;
                case Key.Delete:
                    roi_Circles.Remove(selected);
                    break;
                default:
                    break;
            }
            InvalidateVisual();
        }
        CoordinationHelper CreateCoordHelper()
        {
            return new CoordinationHelper(new Size(this.ActualWidth,this.ActualHeight),bkImgSize);
        }

        internal void DoubleClick(Point pt)
        {
            
            CoordinationHelper coordHelper = CreateCoordHelper();
            double len = coordHelper.ToUIYDir(60);
            pt.Y += len; //let radius 60 pixel in original image;
            AddCircle(new Circle(pt, len));
            InvalidateVisual();
        }

        internal void MoveMouse(Point pt)
        {
            if (!canMove)
                return;

            //if (LeftCtrlDown())
            //{
            //    SetRect(new Rect(ptStart, pt));
            //}
            if (selected != null)
                selected.ptCenter = pt;
            InvalidateVisual();
        }

      
    }

    public class CoordinationHelper
    {
        Size UISize;
        Size imgSize;
        public CoordinationHelper(Size UISize, Size imgSize)
        {
            this.UISize = UISize;
            this.imgSize = imgSize;
        }
        #region coordination translation, only real & UI coordination now.
        public Point ToUI(Point pt)//go smaller
        {
            double xRatio = UISize.Width / imgSize.Width;
            double yRatio = UISize.Height / imgSize.Height;
            return new Point(pt.X * xRatio, pt.Y * yRatio);
        }

        public double ToUIYDir(double len)//go smaller
        {
            double yRatio = UISize.Height / imgSize.Height;
            return len* yRatio;
        }

        public Circle ToUI(Circle circle) //go smaller
        {
            double xRatio = this.UISize.Width / imgSize.Width;
            double yRatio = this.UISize.Height / imgSize.Height;
            Point ptCenter = ToUI(circle.ptCenter);
            double radius = circle.radius * yRatio;
            return new Circle(ptCenter, radius);
        }

        public Point ToReal(Point pt) //go bigger
        {
            double xRatio = imgSize.Width / this.UISize.Width;
            double yRatio = imgSize.Height / this.UISize.Height;
            return new Point(pt.X * xRatio, pt.Y * yRatio);
        }

        public Circle ToReal(Circle circle) //go bigger
        {
            double xRatio = imgSize.Width / this.UISize.Width;
            Point ptCenter = ToReal(circle.ptCenter);
            double radius = circle.radius * xRatio;
            return new Circle(ptCenter, radius);
        }



        #endregion
    }
        
}
