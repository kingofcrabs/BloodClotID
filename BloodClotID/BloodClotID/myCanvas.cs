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

        protected List<Circle> hilightCircles = new List<Circle>();
        protected List<AnalysisResult> analysisResults = new List<AnalysisResult>();
        protected CalibrationInfo calibInfo = new CalibrationInfo();
        protected Size bkImgSize;
        public void UpdateBackGroundImage(string file, int cameraID = 0)
        {
            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                bkImgSize = new Size(bmpTemp.Size.Width, bmpTemp.Size.Height);
            }
            CalibrationInfo calibInfo = null;
            if (cameraID != 0)
            {
                string sFile = FolderHelper.GetCalibFile(cameraID);
                calibInfo = SerializeHelper.LoadCalib(sFile);
            }
            Background = RenderHelper.CreateBrushFromFile(file, calibInfo);
        }


        //public void HilightCircle(List<int> ids, int cameraID)
        //{
        //    LoadCalib(cameraID);
        //    hilightCircles = calibInfo.circles.Where(x => ids.Contains(x.id)).ToList();
        //    InvalidateVisual();
        //}

    
        internal void SaveCalib(int cameraID)
        {
            string sFile = FolderHelper.GetCalibFile(cameraID);
            calibInfo.size = new Size(this.ActualWidth, this.ActualHeight);
            SerializeHelper.SaveCalib(calibInfo, sFile);
            SaveCPlusPlusCalib(cameraID);
            MessageBox.Show(string.Format("已经保存到{0}！", sFile));
        }

        void SaveCPlusPlusCalib(int cameraID)
        {
            double xRatio = bkImgSize.Width / calibInfo.size.Width;
            double yRatio = bkImgSize.Height / calibInfo.size.Height;
            
            double rRatio = Math.Max(xRatio, yRatio);
            ROI[] rois = new ROI[calibInfo.circles.Count];
            for (int i = 0; i < rois.Length; i++)
            {
                Circle cirlce = calibInfo.circles[i];
                var pt = ConvertCoord2Real(xRatio, yRatio, cirlce.ptCenter);
                var r = (int)(rRatio * cirlce.radius);
                rois[i] = new ROI((int)pt.X, (int)pt.Y, r);
            }
            string sFile = FolderHelper.GetCalibFileCPlusPlus(cameraID);
            List<string> strs = new List<string>();
            string sRect = Rect2String(calibInfo.rect);
            strs.Add(sRect);
            foreach(ROI roi in rois)
            {
                strs.Add(string.Format("{0} {1} {2}", roi.x, roi.y, roi.radius));
            }
            File.WriteAllLines(sFile, strs);
        }

        private string Rect2String(Rect rect)
        {
            Point ptTopLeft = rect.TopLeft;
            Point ptBottomRight = rect.BottomRight;
            return string.Format("{0} {1} {2} {3}", ptTopLeft.X, ptTopLeft.Y, ptBottomRight.X, ptBottomRight.Y);
        }

        private Point ConvertCoord2Real(double xRatio, double yRatio, Point pt)
        {
            return new Point((int)(pt.X * xRatio), (int)(pt.Y * yRatio));
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
        Circle selected = null;
        List<Circle> results = new List<Circle>();
        bool canMove = false;

        private void AddCircle(Circle newCircle)
        {
            calibInfo.circles.Add(newCircle);
        }
        private void SetRect(Rect rect)
        {
            calibInfo.rect = rect;
        }

        public void RemoveCorrespondingCircle(Point pt)
        {
            Point translatePt = ConvertCoordCalib2Real(pt);
            calibInfo.circles.RemoveAll(c => c.IsPointInside(translatePt));
            this.InvalidateVisual();
        }
        #region coordinate translation
        private Point ConvertCoord2Calib(Point pt)
        {
            if (GlobalVars.IsCalibration)
            {
                if (calibInfo.size.Width == 0)
                    return pt;
                double xRatio = this.ActualWidth / calibInfo.size.Width;
                double yRatio = this.ActualHeight / calibInfo.size.Height;
                return new Point(pt.X / xRatio, pt.Y / yRatio);
            }
            else
            {
                double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
                double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
                double xRatio = virtualActualWidth / calibInfo.size.Width;
                double yRatio = virtualActualHeight / calibInfo.size.Height;
                double xOffset = (virtualActualWidth - this.ActualWidth) * calibInfo.rect.TopLeft.X / (calibInfo.size.Width - calibInfo.rect.Width);
                double yOffset = (virtualActualHeight - this.ActualHeight) * calibInfo.rect.TopLeft.Y / (calibInfo.size.Height - calibInfo.rect.Height);
                return new Point((pt.X+xOffset) / xRatio, (pt.Y+yOffset) / yRatio);
            }
            
        }

     
        private Point ConvertCoordImage2RealRelative(Point pt,Size imgSize) //use point as size, 此处是相对位置，所以不需要减去offset
        {
            if (GlobalVars.IsCalibration)
            {
                double xRatio = this.ActualWidth / imgSize.Width;
                double yRatio = this.ActualHeight / imgSize.Height;
                return new Point(pt.X * xRatio, pt.Y * yRatio);
            }
            else
            {
                double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
                double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
                double xRatio = virtualActualWidth / imgSize.Width;
                double yRatio = virtualActualHeight / imgSize.Height;
                return new Point(pt.X * xRatio, pt.Y * yRatio);
            }
        }

        public Point ConvertCoordCalib2Real(Point pt)
        {
            if (calibInfo.size.Width == 0)
                return pt;
            double xOffset = 0;
            double yOffset = 0;

            double xRatio, yRatio;
            if(GlobalVars.IsCalibration)
            {
                xRatio = this.ActualWidth / calibInfo.size.Width;
                yRatio = this.ActualHeight / calibInfo.size.Height;
            }
            else
            {
                double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
                double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
                xOffset = (virtualActualWidth - this.ActualWidth) * calibInfo.rect.TopLeft.X/(calibInfo.size.Width - calibInfo.rect.Width) ;
                yOffset = (virtualActualHeight - this.ActualHeight) * calibInfo.rect.TopLeft.Y /(calibInfo.size.Height - calibInfo.rect.Height) ;
                xRatio = virtualActualWidth / calibInfo.size.Width;
                yRatio = virtualActualHeight / calibInfo.size.Height;
            }
            
            return new Point(pt.X * xRatio - xOffset, pt.Y * yRatio - yOffset);
        }

        private void ConvertCoordCalib2Real(Circle circle, ref Point ptCenter, ref Size sz)
        {
            ptCenter = ConvertCoordCalib2Real(circle.ptCenter);
            sz = ConvertCoordCalib2Real(new Size(circle.radius, circle.radius));
        }

        private Size ConvertCoordCalib2Real(Size sz)
        {
            if (calibInfo.size.Width == 0)
                return sz;
         
            double xRatio, yRatio;
            if (GlobalVars.IsCalibration)
            {
                xRatio = this.ActualWidth / calibInfo.size.Width;
                yRatio = this.ActualHeight / calibInfo.size.Height;
            }
            else
            {
                double virtualActualWidth = calibInfo.size.Width / calibInfo.rect.Width * this.ActualWidth;
                double virtualActualHeight = calibInfo.size.Height / calibInfo.rect.Height * this.ActualHeight;
                    xRatio = virtualActualWidth / calibInfo.size.Width;
                yRatio = virtualActualHeight / calibInfo.size.Height;
            }

            return new Size(sz.Width * xRatio,sz.Height * yRatio);
        }
        #endregion

        public void SelectCircleAtPosition(Point pt)
        {
            Point ptTranslate = ConvertCoord2Calib(pt);
            ClearSelectFlag();
            Circle firstMatch = calibInfo.circles.Find(x => x.IsPointInside(ptTranslate));
            if (firstMatch != null)
            {
                selected = firstMatch;
                canMove = true;
            }
                
            InvalidateVisual();

        }

        internal void Highlight(int indexInPlate)
        {
            results.Add(calibInfo.circles[indexInPlate]);
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
            Pen redPen = new Pen(Brushes.Red,2);
            Pen blackPen = new Pen(Brushes.Black,1);
            foreach (Circle circle in calibInfo.circles)
            {
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Brush brush = circle == selected ? new SolidColorBrush(blue) : null;
                Point ptCenter = circle.ptCenter;
                Size sz = new Size(circle.ptCenter.X, circle.ptCenter.Y);
                ConvertCoordCalib2Real(circle, ref ptCenter, ref sz);
                Pen pen = results.Contains(circle) ? redPen : blackPen;
                drawingContext.DrawEllipse(brush, pen, ptCenter, sz.Width, sz.Height);
                //FormattedText formattedText = new FormattedText(circleID.ToString(), CultureInfo.CurrentCulture,
                //FlowDirection.LeftToRight, new Typeface("Tahoma"), 20, Brushes.Red);
                //drawingContext.DrawText(formattedText, ptCenter);
                circleID++;
            }

            if(GlobalVars.IsCalibration && calibInfo.rect.Width != 0 && calibInfo.rect.Height != 0)
            {
                Brush brush = new SolidColorBrush(Colors.Blue);
                Point ptStart = ConvertCoordCalib2Real(calibInfo.rect.Location);
                Size sz = ConvertCoordCalib2Real(calibInfo.rect.Size);
                Rect rectTranslated = new Rect(ptStart, sz);
                drawingContext.DrawRectangle(null, new Pen(brush, 1), rectTranslated);
            }

            if(this.Background != null && analysisResults != null)
            {
                for(int i = 0; i< analysisResults.Count; i++)// (AnalysisResult analysisResult in analysisResults)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var result = analysisResults[i];
                        Point startPtOffset = result.RotateRectPoints[j];
                        Point endPtOffset= result.RotateRectPoints[(j + 1) % 4];
                     
                        var translateStartOffset = ConvertCoordImage2RealRelative(startPtOffset, bkImgSize);
                        var translateEndOffset = ConvertCoordImage2RealRelative(endPtOffset, bkImgSize);
                        var ptCenter = calibInfo.circles[i].ptCenter;
                        ptCenter = ConvertCoordCalib2Real(ptCenter);
                        int val = result.val;
                        FormattedText formattedText = new FormattedText(val.ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Tahoma"), 20, Brushes.Green);
                        if (j == 3)
                        {
                            var ptText = new Point(ptCenter.X, ptCenter.Y + translateEndOffset.Y);
                            drawingContext.PushTransform(new RotateTransform(-90, ptText.X, ptText.Y));
                            drawingContext.DrawText(formattedText, new Point(ptCenter.X, ptCenter.Y + translateEndOffset.Y));
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

        internal void LeftMoseUp(Point pt, bool bAdd)
        {
            if(LeftCtrlDown())
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

        internal void LeftMouseUp(Point pt,bool bAdd)
        {
            canMove = false;
            if (LeftCtrlDown())
            {
                SetRect(new Rect(ptStart, pt));
                return;
            }
            if (bAdd && GetDistance(pt, ptStart) < 30)
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
                    calibInfo.circles.Remove(selected);
                    break;
                default:
                    break;
            }
            InvalidateVisual();
        }

        internal void MoveMouse(Point pt)
        {
            if (!canMove)
                return;

            if (LeftCtrlDown())
            {
                SetRect(new Rect(ptStart, pt));
            }
            else if (selected != null)
                selected.ptCenter = pt;
            InvalidateVisual();
        }

    
    }

    
}
