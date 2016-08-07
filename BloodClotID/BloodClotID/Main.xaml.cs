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
        Thread _progressThread = null;
        Loader loaderWindow;
        IImageAcquirer imgAcquirer;
        SettingWindow settingWindow;
        
        ObservableCollection<PieSegment> pieCollection = new ObservableCollection<PieSegment>();
        public MainWindow()
        {
            
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
           

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                imgAcquirer = ImageAcquirerFactory.CreateImageAcquirer(GlobalVars.Vendor);
                pictureContainers.PreviewMouseLeftButtonDown += PictureContainers_PreviewMouseLeftButtonDown;
                   pictureContainers.PreviewMouseLeftButtonUp += PictureContainers_PreviewMouseLeftButtonUp;
                this.Closed += MainWindow_Closed;
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
        }

     

     

        #region mouse event handler
        private void PictureContainers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void PictureContainers_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(pictureContainers);
            ResultCanvas resultCanvas;
            Point adjustPt = new Point(pt.X, pt.Y);
            if(pt.X > pic1.ActualWidth)
            {
                adjustPt.X -= pic1.ActualWidth;
                if (pt.Y <= pic2.ActualHeight)
                {
                    resultCanvas = pic2;
                }
                else
                {
                    adjustPt.Y -= pic2.ActualHeight;
                    resultCanvas = pic4;
                }
            }
            else
            {
                if (pt.Y <= pic1.ActualHeight)
                {
                    resultCanvas = pic1;
                }
                else
                {
                    adjustPt.Y -= pic1.ActualHeight;
                    resultCanvas = pic3;
                }
            }
            resultCanvas.LeftMoseDown(adjustPt,false);
        }
        #endregion


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

        private void Analysis()
        {
            Dictionary<int, ResultCanvas> dict = new Dictionary<int, ResultCanvas>() { };
            dict.Add(1, pic1);
            dict.Add(2, pic2);
            dict.Add(3, pic3);
            dict.Add(4, pic4);
            foreach(var pair in dict)
            {
                var result = Analyzer.Instance.AnalysisPlate(pair.Key);
                pair.Value.SetResult(result);
            }
            
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
                {
                    canvas.UpdateBackGroundImage(file,id);
                    canvas.LoadCalib(id);
                }
                    
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
           
            btnNext.IsEnabled = AcquireInfo.Instance.curPlate != AcquireInfo.Instance.GetTotalPlateCnt();//if not last one, allow user press next.
            RefreshImage();
            Analysis();
            HideLoader();
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
            settingWindow = new SettingWindow();
            settingWindow.setOk += SettingWindow_setOk;
            settingWindow.ShowDialog();
            //FolderHelper.CreateAcquiredImageFolder();
            
            //Debug.WriteLine(result.Count);
        }

        private void SettingWindow_setOk(object sender, EventArgs e)
        {
            FolderHelper.CreateAcquiredImageFolder();
            btnTakePhote.IsEnabled = true;
            pieCollection.Clear();
            pieCollection.Add(new PieSegment { Color = Colors.Green, Value = 0, Name = "已完成" });
            pieCollection.Add(new PieSegment { Color = Colors.Yellow, Value = AcquireInfo.Instance.GetTotalPlateCnt(), Name = "未完成" });
            chart1.Data = pieCollection;
        }


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
