using CameraControl;
using System;
using System.Collections.Generic;
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

namespace BloodClotID.Camera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread _progressThread = null;
        Loader loaderWindow;
        IImageAcquirer imgAcquirer;

        public MainWindow()
        {
            
            InitializeComponent();
            try
            {
                imgAcquirer = ImageAcquirerFactory.CreateImageAcquirer(ConfigValues.Vendor);
                imgAcquirer.onFinished += ImgAcquirer_onFinished;
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            
        }

        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Foreground = brush;
        }

        #region take photo
     
        private void ImgAcquirer_onFinished(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(
             (Action)delegate ()
             {
                 HideLoader();
                 rootGrid.IsEnabled = true;
                 string sErrMsg = ((MyEventArgs)e).ErrMsg;
                 if (sErrMsg != "")
                 {
                     foreach (var uiElement in pictureContainers.Children)
                     {
                         ((Grid)uiElement).Background = null;
                     }

                     SetInfo(sErrMsg + "请重新连接相机线,再来一次。");
                     return;
                 }
                 var FinishedPhoto = ((MyEventArgs)e).FinishedPhoto;
                 if(FinishedPhoto == FinishedPhoto.All)
                 {
                     RefreshImage();
                     //DoMeasure();
                 }
                 

             });
        }

        private void DoMeasure()
        {
            throw new NotImplementedException();
        }

        private void RefreshImage()
        {
            
        }

        private void btnTakePhote_Click(object sender, RoutedEventArgs e)
        {
            ShowLoader();
            rootGrid.IsEnabled = false;
            this.Refresh();

            imgAcquirer.Start(FolderHelper.GetAcquiredImageFolder() + "1.jpg", 1);
            imgAcquirer.Start(FolderHelper.GetAcquiredImageFolder() + "2.jpg", 2);

        }




        #endregion
        #region loader

        protected void ShowLoader()
        {
            if (_progressThread != null)
                return;
            double x = this.Left + this.Width / 2 ;
            double y = this.Top + this.Height / 2;
            _progressThread = new Thread(() =>
            {
                loaderWindow = new Loader();
                loaderWindow.Show();
                loaderWindow.Left = x - loaderWindow.Width/2;
                loaderWindow.Top = y - loaderWindow.Height/2;
                Dispatcher.Run();
            });
            
            _progressThread.SetApartmentState(ApartmentState.STA);
            _progressThread.Start();
        }

        protected void HideLoader()
        {
            if (_progressThread == null)
                return;
            loaderWindow.Dispatcher.InvokeShutdown();
            _progressThread = null;
        }

        #endregion

        //private void ShowPhotos(TwoCamera fourCamera)
        //{
        //    string imgFolder = FolderHelper.GetImageFolder();
        //    int cameraIndex = 0;
        //    foreach(var uiElement in pictureContainers.Children)
        //    {
        //        ((Grid)uiElement).Background = RenderHelper.CreateBrushFromStream(fourCamera[cameraIndex]);
        //        cameraIndex++;
        //    }
        //}

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

        #region
        private void btnPrepare_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.ShowDialog();
            settingWindow.setOk += SettingWindow_setOk;
        }

        private void SettingWindow_setOk()
        {
            btnTakePhote.IsEnabled = true;
        }
        #endregion
    }


}
