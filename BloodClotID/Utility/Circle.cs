using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Utility
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
}
