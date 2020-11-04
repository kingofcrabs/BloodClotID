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
        Point topLeft;
        Point topRight;
        Point bottomLeft;
        Point bottomRight;
        public Point GetAffinePosition(int row, int col)
        {
            Vector vec1 = new Vector(topRight.X - topLeft.X,
                topRight.Y - topLeft.Y);
            Vector vec2 = new Vector(bottomRight.X - bottomLeft.X, bottomRight.Y - bottomLeft.X);
            double ratioX;
            double ratioY;
            ratioX = ((double)col) / 11;
            ratioY = ((double)row) / 11;
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
            bottomRight = new Point(0, 0);
        }

        public static PlatePositon Load()
        {
            string calibFile = FolderHelper.GetCalibFile();
            return SerializeHelper.LoadCalib(calibFile);
        }


        public void Save()
        {
            string calibFile = FolderHelper.GetCalibFile();
            SerializeHelper.SaveCalib(this,calibFile);
        }
    }
}
