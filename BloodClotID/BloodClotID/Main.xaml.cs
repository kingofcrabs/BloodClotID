using CameraControl;
using PieControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Utility;
using EngineDll;

namespace BloodClotID.Camera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        SettingWindow settingWindow = new SettingWindow();
        AnalysisWindow analysisWindow;
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
            FolderHelper.CreateAcquiredImageFolder();
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

        #region prepare
        private void btnPrepare_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(Stage.Preapare);
        }

        private void SwitchTo(Stage dstStage)
        {
            if (dstStage == curStage)
                return;
            Dictionary<Stage, BaseUserControl> stage_UserControl = new Dictionary<Stage, BaseUserControl>();
            stage_UserControl.Add(Stage.Analysis, analysisWindow);
            stage_UserControl.Add(Stage.Preapare, settingWindow);
            userControlContainer.Children.Clear();
            curStage = dstStage;
            var dstUserControl = stage_UserControl[dstStage];
            dstUserControl.IsEnabled = curStage == reachedStage;
            dstUserControl.Initialize();
            userControlContainer.Children.Add(dstUserControl);
            
            if (curStage > reachedStage)
                reachedStage = curStage;
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(Stage.Analysis);
        }
        //private void SettingWindow_setOk(object sender, EventArgs e)
        //{
        //    FolderHelper.CreateAcquiredImageFolder();
        //    btnTakePhote.IsEnabled = true;
        //
        //}


        #endregion


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
            uiElement.Dispatcher.Invoke(DispatcherPriority.Normal, EmptyDelegate);
        }

    
    }

}
