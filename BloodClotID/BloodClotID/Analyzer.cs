using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineDll;
using Utility;
using System.Windows;
using EngineDll;

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
        private static Analyzer instance;
        List<AnalysisResult> results = new List<AnalysisResult>();
        private Analyzer()
        {

        }
        public List<AnalysisResult> Results
        {
            get
            {
                return results;
            }
            set
            {
                results = value;
            }
        }

        public Size ImageSize { get; set; }
        static public Analyzer Instance
        {
            get
            {
                if (instance == null)
                    instance = new Analyzer();
                return instance;
            }
        }


        public List<AnalysisResult> AnalysisPlate(int cameraID)
        {
            if (cameraID == 1)
                results.Clear();

            var file = FolderHelper.GetImagePath(cameraID);
            var calibFile = FolderHelper.GetCalibFile(cameraID);
            CalibrationInfo calibInfo = SerializeHelper.LoadCalib(calibFile);

            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                ImageSize = new Size(bmpTemp.Size.Width,bmpTemp.Size.Height);
            }
            double xRatio = ImageSize.Width / calibInfo.size.Width;
            double yRatio = ImageSize.Height / calibInfo.size.Height;
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
            var tmpResults = iEngine.Analysis(file, rois).ToList();
            results.AddRange(tmpResults);
            return tmpResults;
        }

        
        private Point ConvertCoord2Real(double xRatio,double yRatio,Point pt)
        {
            return new Point((int)(pt.X * xRatio), (int)(pt.Y * yRatio));
        }
    }
}
