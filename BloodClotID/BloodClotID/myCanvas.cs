using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utility;

namespace BloodClotID
{
    public class RenderCanvas : Canvas
    {
        protected Rect bound;

        protected Dictionary<int, LengthResult> lengthResults = new Dictionary<int, LengthResult>();
        protected Size bkImgSize;
        List<int> resultIDs = new List<int>();
        Dictionary<int, int> adjustResult = new Dictionary<int, int>();
        private List<Point> eachWellPt = new List<Point>();
        public event EventHandler<KeyValuePair<int, int>> onAdjustResult;
        public RenderCanvas()
        {
            //platePositon.Save();
        }
        public void UpdateBackGroundImage(string file, Size containerSize)
        {
            if (!File.Exists(file))
                throw new Exception("没找到图片！");

            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                // bmpTemp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
                bkImgSize = new Size(bmpTemp.Size.Width, bmpTemp.Size.Height);
                Background = RenderHelper.CreateBrush(bmpTemp, containerSize);
            }
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

            if (eachWellPt.Count == 0)
                return;
            const int radius = 50;
            
            for(int wellIndex =0; wellIndex < 96; wellIndex++)
            {
                var wellPt = eachWellPt[wellIndex];
                if(GetDistance(wellPt,pt) <= radius)
                {
                    int colIndex, rowIndex;
                    PlatePositon.Convert(wellIndex + 1, out colIndex, out rowIndex);
                    if (adjustResult.ContainsKey(rowIndex))
                        adjustResult[rowIndex] = colIndex;
                    else
                        adjustResult.Add(rowIndex, colIndex);

                    if (onAdjustResult != null)
                        onAdjustResult(this, new KeyValuePair<int, int>(rowIndex, colIndex));
                    break;
                }
            }
            InvalidateVisual();
        }

        internal void Highlight(int indexInPlate)
        {
            if (indexInPlate == -1)
                return;
            resultIDs.Add(indexInPlate);
        }

        internal void ClearHight()
        {
            resultIDs.Clear();
        }


        public void SetResult(List<double> lengths, List<List<Point>> eachWellCornerPts, List<Point> centerPts)
        {
            Dictionary<int, LengthResult> eachWellResult = new Dictionary<int, LengthResult>();
            int ID = 1;
            for (int i = 0; i < lengths.Count; i++)
            {
                int len = (int)Math.Round(lengths[i]);
                var rotateRectPts = eachWellCornerPts[i];
                var pt = centerPts[i];
                eachWellResult.Add(ID++, new LengthResult(pt, len, rotateRectPts));
            }
            lengthResults = eachWellResult;
            InvalidateVisual();
        }


       

        public static bool IsInDesignMode()
        {
            if (System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio"))
            {
                return true;
            }
            return false;
        }

        public void GetRowCol(int wellID, out int row, out int col)
        {
            int _row = 8;
            col = (wellID - 1) / _row;
            row = wellID - col * _row - 1;
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (IsInDesignMode())
                return;


            Pen redPen = new Pen(Brushes.Red, 2);
            Pen blackPen = new Pen(Brushes.Black, 1);


            drawingContext.DrawRectangle(null, blackPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            CoordinationHelper coordinationHelper = new CoordinationHelper(new Size(this.ActualWidth, this.ActualHeight), bkImgSize);



            if (lengthResults != null)
            {

                for (int i = 0; i < lengthResults.Count; i++)// (AnalysisResult analysisResult in analysisResults)
                {
                    var result = lengthResults[i + 1]; //ID, not index
                    //Circle circle = roi_Circles[i];
                    if (result.RotateRectPoints.Count == 4)
                    {
                        for (int j = 0; j < 4; j++)
                        {

                            Point startPtOffset = result.RotateRectPoints[j];
                            Point endPtOffset = result.RotateRectPoints[(j + 1) % 4];

                            var translateStartOffset = coordinationHelper.ToUI(startPtOffset);//ConvertCoordImage2RealRelative(startPtOffset, bkImgSize);
                            var translateEndOffset = coordinationHelper.ToUI(endPtOffset);

                            var ptCenter = coordinationHelper.ToUI(result.ptCenter);
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
                if (resultIDs.Count > 0)
                {
                    eachWellPt.Clear();
                    for (int i = 0; i< 96; i++)
                    {
                        int wellID = i+1;
                        Circle translateCircle = GetTranslateCircle(wellID, coordinationHelper);
                        eachWellPt.Add(translateCircle.ptCenter);
                        if (resultIDs.Contains(wellID) )
                            drawingContext.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Red, 2), translateCircle.ptCenter, translateCircle.radius,translateCircle.radius);
                        
                    }
                }

                if(adjustResult.Count > 0)
                {
                    foreach(var pair in adjustResult)
                    {
                        int rowIndex = pair.Key;
                        int colIndex = pair.Value;
                        int wellID = PlatePositon.GetWellID(rowIndex, colIndex);
                        Circle translateCircle = GetTranslateCircle(wellID, coordinationHelper);
                        drawingContext.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Blue, 2), translateCircle.ptCenter, translateCircle.radius, translateCircle.radius);
                    }
                }
            }
        }

        private Circle GetTranslateCircle(int wellID, CoordinationHelper coordinationHelper)
        {
            int row, col;
            GetRowCol(wellID, out row, out col);
            Point ptStart = PlatePositon.Instance.GetAffinePosition(row, col);
            Point ptEnd = PlatePositon.Instance.GetAffinePosition(row + 1, col + 1);
            Point ptCenter = new Point((ptStart.X + ptEnd.X) / 2, (ptStart.Y + ptEnd.Y) / 2);
            Circle circle = new Circle(ptCenter, PlatePositon.radius);
            var translateCircle = coordinationHelper.ToUI(circle);
            return translateCircle;
        }

        bool LeftCtrlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        internal void LeftMoseUp(Point pt)
        {
            if (!LeftCtrlDown())
                return;          
             SelectCircleAtPosition(pt);
        }

        double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            return Math.Sqrt(xx * xx + yy * yy);
        }





        CoordinationHelper CreateCoordHelper()
        {
            return new CoordinationHelper(new Size(this.ActualWidth, this.ActualHeight), bkImgSize);
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
            return len * yRatio;
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
