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
        SettingWindow settingWindow = new SettingWindow();
        ObservableCollection<PieSegment> pieCollection = new ObservableCollection<PieSegment>();
        public MainWindow()
        {
            
            InitializeComponent();
            try
            {
                imgAcquirer = ImageAcquirerFactory.CreateImageAcquirer(ConfigValues.Vendor);
                this.Closed += MainWindow_Closed;
                settingWindow.setOk += SettingWindow_setOk;
           
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            imgAcquirer.Stop();
        }

        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Foreground = brush;
        }

        #region take photo
     
        //private void ImgAcquirer_onFinished(object sender, EventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(
        //     (Action)delegate ()
        //     {
        //         HideLoader();
        //         rootGrid.IsEnabled = true;
        //         string sErrMsg = ((MyEventArgs)e).ErrMsg;
        //         if (sErrMsg != "")
        //         {
        //             foreach (var uiElement in pictureContainers.Children)
        //             {
        //                 ((Grid)uiElement).Background = null;
        //             }

        //             SetInfo(sErrMsg + "请重新连接相机线,再来一次。");
        //             return;
        //         }
        //         var FinishedPhoto = ((MyEventArgs)e).FinishedPhoto;
        //         if(FinishedPhoto == FinishedPhoto.All)
        //         {
        //             btnNext.IsEnabled = AcquireInfo.Instance.curPlate != AcquireInfo.Instance.GetTotalPlateCnt();//if not last one, allow user press next.

        //             RefreshImage();
                     
        //             //DoMeasure();
        //         }
                 

        //     });
        //}
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
            AcquireInfo.Instance.NextPlate();
        }

        private void UpdateProgress()
        {
            pieCollection[0].Value = AcquireInfo.Instance.curPlate;
            pieCollection[1].Value = AcquireInfo.Instance.GetTotalPlateCnt() - AcquireInfo.Instance.curPlate;
        }

        private void DoMeasure()
        {
            throw new NotImplementedException();
        }

        private void ShowResult(List<int> results)
        {
            var tbl3 = new DataTable("template");
            tbl3.Columns.Add("Seq", typeof(string));
            tbl3.Columns.Add("Result", typeof(string));
            for (int i = 0; i < results.Count; i++)
            {
                object[] objs = new object[2] { i + 1, results[i] };
                tbl3.Rows.Add(objs);
            }
            lvResult.ItemsSource = tbl3.DefaultView;
        }

        private void RefreshImage()
        {
            int id = 1;
            foreach (var uiElement in pictureContainers.Children)
            {
                
                string file = FolderHelper.GetImagePath(id);

                ResultCanvas canvas = (ResultCanvas)uiElement;
                if (File.Exists(file))
                    canvas.UpdateBackGroundImage(file);
                else
                    canvas.Background = null;
                id++;
            }

        }

        private void btnTakePhote_Click(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
            ShowLoader();
            try
            {
                imgAcquirer.TakePhoto();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
            }
            HideLoader();
            btnNext.IsEnabled = AcquireInfo.Instance.curPlate != AcquireInfo.Instance.GetTotalPlateCnt();//if not last one, allow user press next.
            RefreshImage();
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

        #region prepare
        private void btnPrepare_Click(object sender, RoutedEventArgs e)
        {
            settingWindow.ShowDialog();
            
        }

        private void SettingWindow_setOk(object sender, EventArgs e)
        {
            FolderHelper.CreateAcquiredImageFolder();
            btnTakePhote.IsEnabled = true;
            pieCollection.Add(new PieSegment { Color = Colors.Green, Value = 0, Name = "已完成" });
            pieCollection.Add(new PieSegment { Color = Colors.Yellow, Value = AcquireInfo.Instance.GetTotalPlateCnt(), Name = "未完成" });
            chart1.Data = pieCollection;
        }

        #endregion


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
