using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utility;
using System.Windows;
using System.Diagnostics;
using System.IO;
using EngineDll;

namespace BloodClotID
{
    class Analyzer
    {
      
        private static Analyzer instance;
        IEngine iEngine = new IEngine();

        //List<AnalysisResult> results = new List<AnalysisResult>();
        private Dictionary<int, List<AnalysisResult>> plate_Result;
        static private object locker = new object();
        private Analyzer()
        {
            plate_Result = new Dictionary<int, List<AnalysisResult>>();
        }
        public List<int> Results
        {
            get
            {
                int cnt = AcquireInfo.Instance.CalculateSamplesThisBatch();
                List<List<int>> eachRowWellIDs = new List<List<int>>();
                if(AcquireInfo.samplesPerPlate == 8) //horizontal
                {
                    cnt = Math.Min(cnt, 8);
                    for(int rowIndex = 0; rowIndex < cnt; rowIndex++ )
                    {
                        var wellIDs = GetRowWellIDs(rowIndex, 12, 8);
                        eachRowWellIDs.Add(wellIDs);
                    }
                }
                else//vertical
                {
                    cnt = Math.Min(cnt, 12);
                    for (int rowIndex = 0; rowIndex < cnt; rowIndex++)
                    {
                        int startID = rowIndex * 8 + 1;
                        List<int> wellIDs = new List<int>();
                        for(int colIndex = 0; colIndex < 8; colIndex++)
                        {
                            wellIDs.Add(startID + colIndex);
                        }
                        wellIDs.Reverse();
                        eachRowWellIDs.Add(wellIDs);
                    }
                }
                return FindAllSamplePositions(eachRowWellIDs);

            }
        }

        private List<int> FindAllSamplePositions(List<List<int>> eachRowWellIDs)
        {
            List<int> positions = new List<int>();
            //try
            //{
            foreach (var wellIDs in eachRowWellIDs)
            {
                positions.Add(FindThisSamplePosition(wellIDs));
            }
            //}
            //catch(Exception ex)
            //{
            //    throw new Exception("invalid position!");
            //}
            
            return positions.Take(AcquireInfo.Instance.CalculateSamplesThisBatch()).ToList();
        }

        private int FindThisSamplePosition(List<int> wellIDs)
        {
            List<double> vals = new List<double>();
            wellIDs.ForEach(x => vals.Add(GetCorrespondingResult(x).val));
            //int  
            //double maxDiff = 0;
            //int position = 0;
            //for(int i =0; i< vals.Count-1; i++)
            //{
            //    double diff = Math.Abs(vals[i]-vals[i + 1]);
            //    if (diff > maxDiff)
            //    {
            //        maxDiff = diff;
            //        position = i;
            //    }
            //}
            if (AcquireInfo.Instance.IsHI)
            {
                return ParseHIPosition(vals);
            }
            return  2;//start from 1
            
        }

        private int ParseHIPosition(List<double> vals) //血凝
        {
            //find last, search the first well < 0.25 * L_last;
            double max = vals.Last();
            double threshold = max * 0.50;
            for(int i = vals.Count - 2; i >= 1 ; i--)
            {
                double tmpVal = vals[i];
                if (tmpVal < threshold)
                    return i+1;
            }
            return 1;
        }


        //private int ParseHIPosition(List<double> vals) //血凝抑制
        //{
        //    if (vals.TrueForAll(x => x == 0))
        //        return 1;

        //    List<string> sVals = new List<string>();
        //    vals.ForEach(x => sVals.Add(x.ToString()));
        //    File.WriteAllLines("d:\\test.txt", sVals);
        //    double max = vals.Max();
        //    int maxIndex = vals.IndexOf(max);
        //    List<double> bigVals = new List<double>();
        //    int lastMaxIndex = 0;
        //    for(int i = 0; i< vals.Count; i++)
        //    {
        //        double val = vals[i]; 
        //        if( val / max > 0.85){
        //            bigVals.Add(val);
        //            if( i != vals.Count -1)
        //                lastMaxIndex = i;
        //        }
        //    }
          
        //    double bigAvg = bigVals.Average();
        //    for (int i = lastMaxIndex; i < vals.Count-1; i++ )
        //    {
        //        double v = vals[i];
        //        if(v/max <= 0.75)
        //            return i;
        //    }
             
        //    throw new Exception("无法分析阳性位置！");
        //}

        private List<int> GetRowWellIDs(int rowIndex,int samplesPerRow, int samplesPerColumn)
        {
            List<int> ids = new List<int>();
            for (int colIndex = 0; colIndex< samplesPerRow;colIndex++)
            {
                int wellID = colIndex * samplesPerColumn + rowIndex + 1;
             
                ids.Add(wellID);
                //lengths.Add(GetCorrespondingResult(wellID).val);
            }
            return ids;
        }
        public void CalculatePlateAndPosition(int rowIndex, int colIndex,ref int plateID, ref int indexInPlate)
        {
            int wellID = CalculateWellID(rowIndex, colIndex);
            CalculatePlateAndPosition(wellID, ref plateID, ref indexInPlate);
        }
        private int CalculateWellID(int rowIndex, int colIndex)
        {
            bool isHorizontal = AcquireInfo.Instance.IsHorizontal;

            if(!isHorizontal)
            {
                Swap<int>(ref rowIndex,ref colIndex);
            }
            var wellID = colIndex * 8 + rowIndex + 1;
            var idInRow = (wellID-1) % 8 + 1;
            var withoutLastRow = wellID - idInRow;
            if(!isHorizontal)
            {
                idInRow = 9 - idInRow; //1->8 2->7 3->6
                wellID = withoutLastRow + idInRow;
            }
                
            return wellID;
        }
        void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
        private void CalculatePlateAndPosition(int wellID,ref int plateID, ref int indexInPlate)
        {
            int colIndex, rowIndex;
            Convert(wellID, out colIndex, out rowIndex);
            plateID = 1;
            if (rowIndex > 3)
            {
                rowIndex -= 4;
                if (colIndex > 5)//plate4
                {

                    plateID = 4;
                }
                else//plate3
                {
                    plateID = 3;
                }
            }
            else
            {
                if (colIndex > 5) //plate2
                {
                    plateID = 2;
                }
                else//plate1
                {
                    plateID = 1;
                }
            }
            if (colIndex > 5)
                colIndex -= 6;
            indexInPlate = CalculateIndexInPlate(rowIndex, colIndex);
        }

        private AnalysisResult GetCorrespondingResult(int wellID)
        {
            int plateID = 1;
            int indexInPlate = 0;
            CalculatePlateAndPosition(wellID, ref plateID, ref indexInPlate);
            return plate_Result[plateID][indexInPlate];

        }
        public static void Convert(int wellID, out int col, out int row)
        {
            int _row = 8;
            col = (wellID - 1) / _row;
            row = wellID - col * _row - 1;
        }
        private int CalculateIndexInPlate(int rowIndex, int colIndex)
        {
            return colIndex * 4 + rowIndex;
        }

        public Size ImageSize { get; set; }

        internal void Reset()
        {
            plate_Result.Clear();
        }

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
            var file = FolderHelper.GetImagePath(cameraID);
            var calibFile = FolderHelper.GetCalibFile(cameraID);
            CalibrationInfo calibInfo = SerializeHelper.LoadCalib(calibFile);

            using (var bmpTemp = new System.Drawing.Bitmap(file))
            {
                ImageSize = new Size(bmpTemp.Size.Width, bmpTemp.Size.Height);
            }
        
            //preapare rois
            MROI[] rois = new MROI[calibInfo.circles.Count];
            for (int i = 0; i < rois.Length; i++)
            {
                Circle cirlce = calibInfo.circles[i];
                rois[i] = new MROI((int)cirlce.ptCenter.X, (int)cirlce.ptCenter.Y, (int)cirlce.radius);
            }

            var tmpResults = new List<EngineDll.MAnalysisResult>();
            for (int i = 0; i < 24; i++)
            {
                var pt = new MPoint(2, 2);
                RotatedRect rc = new EngineDll.RotatedRect(new MPoint[4] { pt, pt, pt, pt });
                tmpResults.Add(new EngineDll.MAnalysisResult(rc, 30, 2));
            }
            tmpResults = iEngine.Analysis(file, rois).ToList();
            List<AnalysisResult> internalResults = new List<AnalysisResult>();
            tmpResults.ForEach(x => internalResults.Add(Convert2InternalResult(x)));
            FilterHorizontalFakes(internalResults);
            if (plate_Result == null)
            {
                Debug.WriteLine("interesting!");
            }
            lock (locker)
            {
                plate_Result.Add(cameraID, internalResults);
            }
            
            return internalResults;
            
        }

        private void FilterHorizontalFakes(List<AnalysisResult> results)
        {
            for(int i = 0; i< results.Count; i++)
            {
                if(IsFakeHorizontal(results[i]))
                {
                    results[i].val = 0;
                }
            }
        }

        private bool IsFakeHorizontal(AnalysisResult analysisResult)
        {
            double maxX = analysisResult.RotateRectPoints.Max(pt => pt.X);
            double minX = analysisResult.RotateRectPoints.Min(pt => pt.X);
            double maxY = analysisResult.RotateRectPoints.Max(pt => pt.Y);
            double minY = analysisResult.RotateRectPoints.Max(pt => pt.Y);
            double lenX = maxX - minX;
            double lenY = maxY - minY;
            return analysisResult.val > 80 && lenX > lenY;
        }

        private AnalysisResult Convert2InternalResult(MAnalysisResult x)
        {
            List<Point> pts = new List<Point>();
            for(int i = 0; i< x.rect.points.Length; i++)
                pts.Add(new Point(x.rect.points[i].x,x.rect.points[i].y));
            AnalysisResult result = new AnalysisResult(x.val, pts);
            return result;
        }

        private List<AnalysisResult> ReadResult(string resultFile)
        {
            List<string> strs = File.ReadAllLines(resultFile).ToList();
            List<AnalysisResult> results = new List<AnalysisResult>();
            foreach(string s in strs)
            {
                List<string> subStrs = s.Split(' ').ToList();
                int len = int.Parse(subStrs[0]);
                List<Point> rotatedRect = new List<Point>();
                for(int ptIndex = 0; ptIndex< 4;ptIndex++)
                {
                    int x = int.Parse(subStrs[ptIndex * 2 + 1]);
                    int y = int.Parse(subStrs[ptIndex * 2 + 2]);
                    rotatedRect.Add(new Point(x, y));
                }
                AnalysisResult result = new AnalysisResult(len,rotatedRect);
                results.Add(result);
            }
            return results;
        }

        
        private Point ConvertCoord2Real(double xRatio,double yRatio,Point pt)
        {
            return new Point((int)(pt.X * xRatio), (int)(pt.Y * yRatio));
        }
    }
}
