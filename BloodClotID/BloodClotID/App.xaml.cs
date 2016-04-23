using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BloodClotID
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            bool isCalibration = e.Args.Length > 0;
            if(isCalibration)
            {
                CalibWindow calibWindow = new CalibWindow();
                calibWindow.Show();
            }
            else
            {
                AnalysisWindow analysisWindow = new AnalysisWindow();
                analysisWindow.Show();
            }
        }

     

    }
}
