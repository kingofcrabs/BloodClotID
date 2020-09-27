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

        protected Dictionary<int, AnalysisResult> analysisResults = new Dictionary<int, AnalysisResult>();
        protected CalibrationInfo calibInfo = new CalibrationInfo();
        protected Size bkImgSize;

        Point ptStart;
        Circle selected = null;
        List<Circle> roi_Circles = new List<Circle>();
        List<int> resultIndexs = new List<int>(); 
        bool canMove = false;

        public void UpdateBackGroundImage(string file,Size containerSize)
        {
            if (!File.Exists(file))
                throw new Exception("没找到图片！");

            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                bkImgSize = new Size(bmpTemp.Size.Width, bmpTemp.Size.Height);
                Background = RenderHelper.CreateBrush(bmpTemp, containerSize);
            }
            roi_Circles = CreateROIs();
          
            
        }

        private List<Circle> CreateROIs()
        {
            Point ptTopLeft = new Point(1426, 1079);
            Point ptBottomRight = new Point(4615, 3111);
            double width = ptBottomRight.X - ptTopLeft.X;
            double height = ptBottomRight.Y - ptTopLeft.Y;
            double widthUnit = width / 11;
            double heightUnit = height / 7;
            int radius = 88;
            int ID = 1;
            List<Circle> circles = new List<Circle>();
            for (int row = 0; row< 8; row++)
            {
                for (int col = 0; col< 12; col++)
                {
                    double x = ptTopLeft.X + col * widthUnit;
                    double y = ptTopLeft.Y + row * heightUnit;
                    Point ptCenter = new Point(x,y);
                    circles.Add(new Circle(ptCenter, radius));
                }
            }
            return circles;
        }

        public void AdapteToUI()
        {
            //CoordinationHelper coordHelper = CreateCoordHelper();
            //roi_Circles.Clear();
            //foreach(var circle in calibInfo.circles)
            //{
            //    roi_Circles.Add(coordHelper.ToUI(circle));
            //}
            InvalidateVisual();
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
        public void SetResult(Dictionary<int,AnalysisResult> eachWellResult)
        {
            analysisResults = eachWellResult;
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
            
           
            Pen redPen = new Pen(Brushes.Red, 2);
            Pen blackPen = new Pen(Brushes.Black, 1);
           
            int circleID = 1;
            drawingContext.DrawRectangle(null, blackPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            CoordinationHelper coordinationHelper = new CoordinationHelper(new Size(this.ActualWidth, this.ActualHeight), bkImgSize);
            for (int i = 0; i < roi_Circles.Count; i++)
            {
                Circle circle = roi_Circles[i];
                circle = coordinationHelper.ToUI(circle);
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Brush brush = circle == selected ? new SolidColorBrush(blue) : null;

                Size sz = new Size(circle.ptCenter.X, circle.ptCenter.Y);
                Point ptCenter = circle.ptCenter;
                Pen pen = blackPen;
              
                drawingContext.DrawEllipse(brush, pen, ptCenter, circle.radius, circle.radius);
                circleID++;
            }

            
            if (analysisResults != null)
            {
                
                for (int i = 0; i < analysisResults.Count; i++)// (AnalysisResult analysisResult in analysisResults)
                {
                    var result = analysisResults[i+1]; //ID, not index
                    Circle circle = roi_Circles[i];
                    for (int j = 0; j < 4; j++)
                    {
                        Point startPtOffset = result.RotateRectPoints[j];
                        Point endPtOffset = result.RotateRectPoints[(j + 1) % 4];

                        var translateStartOffset = coordinationHelper.ToUI(startPtOffset);//ConvertCoordImage2RealRelative(startPtOffset, bkImgSize);
                        var translateEndOffset = coordinationHelper.ToUI(endPtOffset);

                        var ptCenter = coordinationHelper.ToUI(circle.ptCenter);
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

        //internal void LeftMoseDown(Point pt, bool bAdd)
        //{
        //    if (LeftCtrlDown())
        //    {
        //        ptStart = pt;
        //        canMove = true;
        //        return;
        //    }

        //    if (bAdd)
        //    {
        //        ptStart = pt;
        //    }
        //    else
        //        SelectCircleAtPosition(pt);
        //}

        double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            return Math.Sqrt(xx * xx + yy * yy);
        }

      


       
        CoordinationHelper CreateCoordHelper()
        {
            return new CoordinationHelper(new Size(this.ActualWidth,this.ActualHeight),bkImgSize);
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
