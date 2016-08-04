using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineDll;
using Utility;
using System.Windows;

namespace BloodClotID
{
    class Analyzer
    {
        //public List<int> AnalysisAllPlate()
        //{
        //    int sampleThisBatch = AcquireInfo.Instance.samplesThisBatch;
        //    EngineDll.IEngine iEngine = new EngineDll.IEngine();

        //    //ROI[] rois = new ROI[24];
        //    //iEngine.Analysis()
        //}
        double xRatio;
        double yRatio;
        private List<int> AnalysisPlate(int cameraID)
        {
            var file = FolderHelper.GetImagePath(cameraID);
            var calibFile = FolderHelper.GetCalibFile(cameraID);
            CalibrationInfo calibInfo = SerializeHelper.LoadCalib(calibFile);
            Size imgSize;
            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                imgSize = new Size(bmpTemp.Size.Width,bmpTemp.Size.Height);
            }
            double xRatio = imgSize.Width / calibInfo.size.Width;
            double yRatio = imgSize.Height / calibInfo.size.Height;
            double rRatio = Math.Max(xRatio, yRatio);
            ROI[] rois = new ROI[calibInfo.circles.Count];
            for (int i = 0; i < rois.Length; i++)
            {
                Circle cirlce = calibInfo.circles[i];
                var pt = ConvertCoord2Real(xRatio, yRatio, cirlce.ptCenter);
                var r = (int)(rRatio * cirlce.radius);
                rois[i] = new ROI((int)pt.X, (int)pt.Y, r);
            }
            IEngine iEngine = new IEngine();
            return iEngine.Analysis(file, rois).ToList();
        }

        
        private Point ConvertCoord2Real(double xRatio,double yRatio,Point pt)
        {
            return new Point((int)(pt.X * xRatio), (int)(pt.Y * yRatio));
        }
    }
}
