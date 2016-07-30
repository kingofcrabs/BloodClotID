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
        protected List<Circle> circles = new List<Circle>();
        List<Circle> hilightCircles = new List<Circle>();


        public void UpdateBackGroundImage(string file)
        {
            //Background = RenderHelper.CreateBrushFromFile(file);
        }


        public void HilightCircle(List<int> ids, int cameraID)
        {
            LoadConfig(cameraID);
            hilightCircles = circles.Where(x => ids.Contains(x.id)).ToList();
            InvalidateVisual();
        }

        private string GetConfigFolder()
        {
            string sFolder = FolderHelper.GetExeParentFolder() + "\\Config\\";
            if (!System.IO.Directory.Exists(sFolder))
                System.IO.Directory.CreateDirectory(sFolder);
            return sFolder;
        }

        private string GetConfigFile(int cameraID)
        {
            return GetConfigFolder() + string.Format("ROIs_{0}.xml",cameraID);
        }

        internal void SaveConfig(int cameraID)
        {
            string sFile = GetConfigFile(cameraID);
            SerializeHelper.SaveConfig(circles, sFile);
            MessageBox.Show(string.Format("已经保存到{0}！", sFile));
        }

        internal void LoadConfig(int cameraID)
        {
            string sFile = GetConfigFile(cameraID);
            if (!System.IO.File.Exists(sFile))
            {
                throw new Exception(string.Format("配置文件不存在于{0}！", sFile));
            }
            SerializeHelper.LoadConfig(ref circles, sFile);
            hilightCircles = circles;
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
        
        Point ptStart;
        public delegate void circleCnt(int cnt);
        public event circleCnt onCircleCntChanged;
        public void AddCircle(Circle newCircle)
        {
            circles.Add(newCircle);
            if (onCircleCntChanged != null)
                onCircleCntChanged(circles.Count);
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
            int circleID = 1;
            foreach (Circle circle in circles)
            {
                Color green = Color.FromArgb(128, 0, 128, 50);
                Color blue = Color.FromArgb(128, 0, 50, 128);
                Color color = circle.bSelected ? blue : green;
                Brush brush = new SolidColorBrush(color);
                drawingContext.DrawEllipse(brush, new Pen(Brushes.Black, 1), circle.ptCenter, circle.radius, circle.radius);
                FormattedText formattedText = new FormattedText(circleID.ToString(), CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Tahoma"), 20, Brushes.Red);
                drawingContext.DrawText(formattedText, circle.ptCenter);
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
