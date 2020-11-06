using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utility;

namespace Utility
{
    public class PlatePositon
    {
        public Point topLeft;
        public Point topRight;
        public Point bottomLeft;
        public const int radius = 88;
        public const int ColCnt = 12;
        public const int RowCnt = 8;
        public Point GetAffinePosition(int row, int col)
        {
            Vector vec1 = new Vector(topRight.X - topLeft.X,
                topRight.Y - topLeft.Y);
            Vector vec2 = new Vector(bottomLeft.X - topLeft.X, bottomLeft.Y - topLeft.Y);
            double ratioX;
            double ratioY;
            ratioX = ((double)col) / ColCnt;
            ratioY = ((double)row) / RowCnt;
            double x = topLeft.X;
            double y = topLeft.Y;
            x += (float)(vec1.X * ratioX);
            y += (float)(vec1.Y * ratioX);
            x += (float)(vec2.X * ratioY);
            y += (float)(vec2.Y * ratioY);
            return new Point(x, y);
        }

        public PlatePositon()
        {
            topLeft = new Point(0, 0);
            topRight = new Point(0, 0);
            bottomLeft = new Point(0, 0);
 
        }

       public  static int GetWellID(int rowIndex, int colIndex)
       {
            return colIndex * 8 + rowIndex + 1;
        }

        static PlatePositon instance;
        public static PlatePositon Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlatePositon();
                return instance;
            }
        }

        public void Set(Point tl, Point tr, Point bl)
        {
            topLeft = tl;
            topRight = tr;
            bottomLeft = bl;

        }
    }
}
