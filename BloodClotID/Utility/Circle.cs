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

    public class AnalysisResult
    {
        public List<Point> RotateRectPoints;
        public int val;
        public AnalysisResult(int v, List<Point> pts)
        {
            val = v;
            RotateRectPoints = pts;
        }
        //public double radius;
    }

    //public ref class AnalysisResult
    //{
    //public: 
    //    RotatedRect^ rect;
    //    int val;
    //    double radius;
    //    AnalysisResult(RotatedRect^ rc, int v,double r)
    //    {
    //        rect = rc;
    //        val = v;
    //        radius = r;
    //    }
    //};

    //public ref class ROI
    //{
    //public:
    //    int x;
    //    int y;
    //    int radius;
		
    //    ROI(int xx, int yy, int rr)
    //    {
    //        x = xx;
    //        y = yy;
    //        radius = rr;
    //    }
    //};
    //public ref class MPoint
    //{
    //public:
    //    int x;
    //    int y;
    //    MPoint(int xx, int yy)
    //    {
    //        x = xx;
    //        y = yy;
    //    }

    //};

    //public ref class MRect
    //{
    //public:
    //    MPoint^ ptStart;
    //    MPoint^ ptEnd;
    //    MRect(MPoint^ ptS, MPoint^ ptE)
    //    {
    //        ptStart = gcnew MPoint(ptS->x,ptS->y);
    //        ptEnd = gcnew MPoint(ptE->x,ptE->y);
    //    }
    //};

    //public ref class MSize
    //{
    //public:
    //    int x;
    //    int y;
	
    //    MSize(int xx, int yy)
    //    {
    //        x = xx;
    //        y = yy;
    //    }
    //};

    //public ref class RotatedRect
    //{
    //public:
    //    array<MSize^>^ points;
    //    RotatedRect(array<MSize^>^ pts)
    //    {
    //        points = gcnew array<MSize^>(pts->Length);
    //        for (int i = 0; i < points->Length; i++)
    //        {
    //            points[i] = gcnew MSize(pts[i]->x,pts[i]->y);
    //        }
    //    }
    //};

    
}
