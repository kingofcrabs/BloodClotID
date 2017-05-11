using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utility;

namespace BloodClotID
{
    class Transformer
    {
        Grid grid;
        TransformGroup tg = new TransformGroup();
        public Transformer(Grid grid2Transform)
        {
            grid = grid2Transform;
        }


        public Point TransformBack(Point pt)
        {
            int totalCnt = tg.Children.Count;
            if (totalCnt == 0)
                return pt;
            Point adjustPt = new Point(pt.X,pt.Y);
            for (int i = 0; i < totalCnt; i++)
            {
                var transform = tg.Children[totalCnt - 1 - i];
                adjustPt = transform.Inverse.Transform(adjustPt);
            }
            return adjustPt;
        }
        public void DoTransform()
        {
            if (AcquireInfo.Instance.IsHorizontal)
                return;
            tg.Children.Clear();
            Rotate90Degree();
            Move2Original();
            Shrink();
            
            grid.RenderTransform = tg;
        }

        private void Shrink()
        {
            double ratio = grid.ActualWidth / grid.ActualHeight;
            tg.Children.Add(new ScaleTransform(ratio, ratio));
        }

        private void Move2Original()
        {
            double xOffset = (grid.ActualWidth - grid.ActualHeight) / 2;
            tg.Children.Add(new TranslateTransform(-xOffset, xOffset));
        }

        private void Rotate90Degree()
        {
            var transform = new RotateTransform(90, grid.ActualWidth / 2, grid.ActualHeight / 2);
            tg.Children.Add(transform);
            //grid.RenderTransform = new MatrixTransform(transform);
        }
    }
}
