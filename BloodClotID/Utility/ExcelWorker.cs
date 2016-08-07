using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace BloodClotID
{
    public class ExcelWorker
    {
        public void CreateExcelFile(string fileName,Dictionary<string,List<int>> assay_Results)
        {
            //create  
            object Nothing = System.Reflection.Missing.Value;
            var app = new Application();
            app.Visible = false;
            Workbook workBook = app.Workbooks.Add(Nothing);
            Worksheet worksheet = (Worksheet)workBook.Sheets[1];
            worksheet.Name = "TodayWork";
            //headline  
            worksheet.Cells[1, 1] = "序号";
            int cnt = assay_Results.First().Value.Count;
            for(int i = 0; i< cnt; i++)
            {
                worksheet.Cells[2+i,1] = (i+1).ToString();
            }
            worksheet.Cells[1, 2] = "编号";
            int colID = 3;
            int rowID = 2;
            foreach(var pair in assay_Results)
            {
                rowID = 2;
                worksheet.Cells[1, colID] = pair.Key;
                foreach(var result in pair.Value)
                {
                    worksheet.Cells[rowID, colID] = result;
                    rowID++;
                }
                colID++;
            }

            rowID = cnt + 4;
            colID--;
            worksheet.Cells[rowID, colID] = "时间";
            worksheet.Cells[rowID++, colID+1] = DateTime.Now.ToString("yyyyMMdd_hh:mm:ss");
            worksheet.Cells[rowID++, colID] = "检验人";
            worksheet.Cells[rowID++, colID] = "复核人";
            worksheet.SaveAs(fileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);
            workBook.Close(false, Type.Missing, Type.Missing);
            app.Quit();
        }
    }
}
