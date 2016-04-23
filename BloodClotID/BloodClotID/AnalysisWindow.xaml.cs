using CameraControl;
using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Permissions;
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

namespace BloodClotID
{
    /// <summary>
    /// AnalysisWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AnalysisWindow : Window
    {
        ImageAcquirer imageAcquirer = new ImageAcquirer();
        IEngine iEngine = new IEngine();
        public AnalysisWindow()
        {
            InitializeComponent();
            imageAcquirer.onFinished += imageAcquirer_onFinished;
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            StartAcquire();
        }

        private void SetInfo(string s, System.Windows.Media.Brush color)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = color;
        }

        private void SetInfo(string s, bool hasError = true)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = hasError ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }
        #region refresh helper
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        #endregion

        #region acquisition
        void StartRealAcquire()
        {
            System.Threading.Thread t1 = new System.Threading.Thread
              (delegate()
              {
                  imageAcquirer.Start(FolderHelper.GetLatestImagePath(), 1);
              });
            t1.Start();
        }


        private void StartAcquire()
        {
            EnableButtons(false);
            string extraHint = imageAcquirer.IsInitialed ? "" : "第一次初始化较慢，";
            SetInfo(string.Format("开始采集，{0}请耐心等待。", extraHint), false);
            
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            if (!ConfigValues.UseTestImage)
            {
                imageAcquirer.SetExposureTime(30);
                StartRealAcquire();
            }
            else
            {
                //simulate delay
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                    DoEvents();
                }
                
                imageAcquirer_onFinished("");
            }
        }

        void imageAcquirer_onFinished(string errMsg)
        {
            this.Dispatcher.Invoke(
             (Action)delegate()
             {
                 EnableButtons(true);
                 if (errMsg != "")
                 {
                     renderCanvas.Children.Clear();
                     SetInfo(errMsg + "请重新连接相机线。关闭程序，再来一次。", System.Windows.Media.Brushes.Red);
                     return;
                 }
                 RefreshImage();
                 SetInfo("开始分析", false);
                 DoEvents();
                 Analysis();
             });

        }

        private void Analysis()
        {
            throw new NotImplementedException();
        }

    
        private void RefreshImage()
        {
            try
            {
                string sFile = FolderHelper.GetLatestImagePath();
                System.Drawing.Bitmap latestImage = new System.Drawing.Bitmap(sFile);
                renderCanvas.UpdateBackGroundImage(latestImage);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, System.Windows.Media.Brushes.Red);
            }
        }
        private void EnableButtons(bool bEnable)
        {
            btnAnalysis.IsEnabled = bEnable;
        }
        #endregion
    }
}
