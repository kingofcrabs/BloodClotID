﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utility;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace BloodClotID
{
    class HighLightAnalyzer
    {
      
        private static HighLightAnalyzer instance;
        List<double> lengths = new List<double>();

        public List<int> Go(List<double> lengths)
        {
            this.lengths = lengths;
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
            wellIDs.ForEach(x => vals.Add(lengths[x-1]));
         
            if (AcquireInfo.Instance.IsHI)
            {
                return ParseHIPosition(vals);
            }
            return  2;//start from 1
            
        }

        private int ParseHIPosition(List<double> vals)
        {
            double max = vals.Max();
            int maxIndex = vals.IndexOf(max);
            List<double> bigVals = new List<double>();
            bigVals.Add(max);
            if(maxIndex > 0)
            {
                double val = vals[maxIndex - 1]; 
                if( val / max > 0.85)
                    bigVals.Add(val);
            }
            if(maxIndex < vals.Count -1)
            {
                double val = vals[maxIndex + 1];
                if (val / max > 0.85)
                    bigVals.Add(val);
            }
            //from right search first > 0.8
            int startIndex = AcquireInfo.Instance.HasControl ? Math.Max(vals.Count - 3, 0): vals.Count - 1;
            for(int i = startIndex; i >=0; i--)
            {
                double v = vals[i];
                if (v == 0)
                    continue;
                if(v / max < 0.8)
                {
                    return Math.Max( i - 1,0);
                }
            }
            return -1;
            //throw new Exception("无法分析阳性位置！");
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
        

      
        public static void Convert(int wellID, out int col, out int row)
        {
            int _row = 8;
            col = (wellID - 1) / _row;
            row = wellID - col * _row - 1;
        }
  


        

        static public HighLightAnalyzer Instance
        {
            get
            {
                if (instance == null)
                    instance = new HighLightAnalyzer();
                return instance;
            }
        }


        

       

        
     
    }
}
