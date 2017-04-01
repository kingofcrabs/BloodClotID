using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utility;


namespace BloodClotID
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        SettingWindow settingWindow = new SettingWindow();
        AnalysisWindow analysisWindow;
        ReportWindow reportWindow;
        Stage curStage = Stage.Preapare;
        Stage reachedStage = Stage.Preapare;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            settingWindow.stageSwitched += SettingWindow_stageSwitched;

        }

        private void SettingWindow_stageSwitched(object sender, SwitchEventArgs e)
        {
            btnAnalysis.IsEnabled = true;
          
                
            SwitchTo(Stage.Analysis);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            userControlContainer.Children.Add(settingWindow);
            analysisWindow = new AnalysisWindow(this);
            reportWindow = new ReportWindow();

            analysisWindow.onReportReady += AnalysisWindow_onReportReady;
            FolderHelper.CreateAcquiredImageFolder();
        }

        private void AnalysisWindow_onReportReady(object sender, EventArgs e)
        {
            btnReport.IsEnabled = true;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HelpCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void HelpCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #region stage change
        private void btnPrepare_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(Stage.Preapare);
        }
        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(Stage.Report);
        }
        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(Stage.Analysis);
        }
        #endregion
        private void SwitchTo(Stage dstStage)
        {
            if (dstStage == curStage)
                return;
            Dictionary<Stage, BaseUserControl> stage_UserControl = new Dictionary<Stage, BaseUserControl>();
            stage_UserControl.Add(Stage.Analysis, analysisWindow);
            stage_UserControl.Add(Stage.Preapare, settingWindow);
            stage_UserControl.Add(Stage.Report, reportWindow);
            userControlContainer.Children.Clear();
            curStage = dstStage;
            var dstUserControl = stage_UserControl[dstStage];
           
            dstUserControl.Initialize();
            userControlContainer.Children.Add(dstUserControl);
            
            if (curStage > reachedStage)
                reachedStage = curStage;
            dstUserControl.IsEnabled = curStage == reachedStage;
        }
       
        Visibility Bool2Visibility(bool bVisible)
        {
            return bVisible ? Visibility.Visible : Visibility.Hidden;
        }
    }


    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.ContextIdle, EmptyDelegate);
        }
    }

}
