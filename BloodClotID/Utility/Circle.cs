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
 
        //public int id;

      
        public Circle(Point ptStart, Point ptEnd)
        {
            // TODO: Complete member initialization
            double x = (ptStart.X + ptEnd.X) / 2.0;
            double y = (ptStart.Y + ptEnd.Y) / 2.0;
            ptCenter = new Point(x, y);
            radius = GetDistance(ptStart, ptEnd) / 2;
        }
        public Circle()
        {

        }
        public Circle(Point center, double r)
        {
            ptCenter = center;
            radius = r;
        }

        private double GetDistance(Point pt1, Point pt2)
        {
            double xx = pt1.X - pt2.X;
            double yy = pt1.Y - pt2.Y;
            double dis = Math.Sqrt(xx * xx + yy * yy);
            return dis;
        }
        public static bool operator ==(Circle c1, Circle c2)
        {
            return Object.Equals(c1, c2);
        }
        public static bool operator !=(Circle c1, Circle c2)
        {
            return !Object.Equals(c1, c2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
       
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            Circle that = (Circle)obj;
            int ptX = (int)this.ptCenter.X * 100;
            int ptY = (int)this.ptCenter.Y * 100;
            int thatPtX = (int)that.ptCenter.X * 100;
            int thatPtY = (int)that.ptCenter.Y * 100;
            int thatRadius = (int)that.radius * 100;
            int radius = (int)this.radius * 100;
            return ptX == thatPtX && ptY == thatPtY && radius == thatRadius;

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


    public class ROI
    {
        public int x;
        public int y;
        public int radius;
        public ROI(int xx,int yy,int rr)
        {
            x = xx;
            y = yy;
            radius = rr;
        }
    }

    public class LengthResult
    {
        public List<Point> RotateRectPoints;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
        public int val;
        public Point ptCenter;
        public LengthResult(Point pt, int len, List<Point> pts)
        {
            val = len;
            ptCenter = pt;
            RotateRectPoints = pts;
        }
        //public double radius;
    }
}
