﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineDll;
using Utility;
using System.Windows;
using EngineDll;
using System.Diagnostics;

namespace BloodClotID
{
    class Analyzer
    {
      
        private static Analyzer instance;
        IEngine iEngine;
        //List<AnalysisResult> results = new List<AnalysisResult>();
        private Dictionary<int, List<AnalysisResult>> plate_Result = new Dictionary<int, List<AnalysisResult>>();
        private Analyzer()
        {
            iEngine = new IEngine(); 
        }
        public List<int> Results
        {
            get
            {
                List<List<int>> eachRowWellIDs = new List<List<int>>();
                if(AcquireInfo.samplesPerPlate == 8) //horizontal
                {
                    for(int rowIndex = 0; rowIndex < 8; rowIndex++ )
                    {
                        var wellIDs = GetRowWellIDs(rowIndex, 12, 8);
                        eachRowWellIDs.Add(wellIDs);
                    }
                }
                else//vertical
                {
                    for(int rowIndex = 12; rowIndex > 0; rowIndex--)
                    {
                        int startID = (rowIndex - 1) * 8 + 1;
                        List<int> wellIDs = new List<int>();
                        for(int colIndex = 0; colIndex < 8; colIndex++)
                        {
                            wellIDs.Add(startID + colIndex);
                        }
                        eachRowWellIDs.Add(wellIDs);
                    }
                }
                return FindAllSamplePositions(eachRowWellIDs);

            }
        }

        private List<int> FindAllSamplePositions(List<List<int>> eachRowWellIDs)
        {
            List<int> positions = new List<int>();
            foreach(var wellIDs in eachRowWellIDs)
            {
                positions.Add(FindThisSamplePosition(wellIDs));
            }
            return positions.Take(AcquireInfo.Instance.CalculateSamplesThisBatch()).ToList();
        }

        private int FindThisSamplePosition(List<int> wellIDs)
        {
            List<double> vals = new List<double>();
            wellIDs.ForEach(x => vals.Add(GetCorrespondingResult(x).val));
         
            double maxDiff = 0;
            int position = 0;
            for(int i =0; i< vals.Count-1; i++)
            {
                double diff = Math.Abs(vals[i]-vals[i + 1]);
                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    position = i;
                }
            }
            return position + 2;//start from 1
            
        }

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

    

        private AnalysisResult GetCorrespondingResult(int wellID)
        {
            int colIndex, rowIndex;
            Convert(wellID, out colIndex, out rowIndex);
            int plateID = 1;
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
                if (colIndex > 7) //plate2
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
            int indexInPlate = CalculateIndexInPlate(rowIndex, colIndex);
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
            Point ptStart = ConvertCoord2Real(xRatio, yRatio, calibInfo.rect.TopLeft);
            Point ptEnd = ConvertCoord2Real(xRatio, yRatio, calibInfo.rect.BottomRight);
            MRect boundingRect = new MRect(new MPoint((int)ptStart.X, (int)ptStart.Y),
                                           new MPoint((int)ptEnd.X, (int)ptEnd.Y));
            //var tmpResults = new List<EngineDll.AnalysisResult>();
            //for(int i = 0; i< 24;i++)
            //{
            //    var sz = new MSize(2,2);
            //    RotatedRect rc = new EngineDll.RotatedRect(new MSize[4]{sz,sz,sz,sz});
            //    tmpResults.Add(new EngineDll.AnalysisResult(rc,30,2));
            //}
            //plate_Result.Add(cameraID,tmpResults);
            
            var tmpResults = iEngine.Analysis(file, rois, boundingRect).ToList();
            plate_Result.Add(cameraID, tmpResults);
            return tmpResults;
        }

        
        private Point ConvertCoord2Real(double xRatio,double yRatio,Point pt)
        {
            return new Point((int)(pt.X * xRatio), (int)(pt.Y * yRatio));
        }
    }
}
