using Nikon;
using PieControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Utility;


namespace BloodClotID
{
    /// <summary>
    /// Interaction logic for AcquisitionWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : BaseUserControl
    {
        Thread _progressThread = null;
        Loader loaderWindow;
        NikonDevice _device;
        AutoResetEvent waitDeviceAdded = new AutoResetEvent(false);
        ObservableCollection<PieSegment> pieCollection = new ObservableCollection<PieSegment>();
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event EventHandler onReportReady;
        Transformer transformer;
        Window container;
        NikonManager nikonManager;
        public AnalysisWindow(Window parent)
        {
            log.Info("AnalysisWindow");
            InitializeComponent();
            try
            {
                this.container = parent;
                btnAcquire.IsEnabled = false;
                nikonManager = new NikonManager("Type0011.md3");
                nikonManager.DeviceAdded += NikonManager_DeviceAdded;
                transformer = new Transformer(picturesContainer);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            this.SizeChanged += AnalysisWindow_SizeChanged;
            if (!AcquireInfo.Instance.IsHorizontal)
            {
                parent.MaxWidth = 1920;
            }
            pic1.PreviewMouseLeftButtonUp += pic1_PreviewMouseLeftButtonUp;
            parent.Closed += Parent_Closed;
        }

        private void NikonManager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            Console.WriteLine("=> {0}, {1}", sender.Id, sender.Name);
            waitDeviceAdded.Set();
            if (_device == null)
            {
                // Save device
                _device = device;

            }
            _device.CaptureComplete += _device_CaptureComplete;
            _device.ImageReady += _device_ImageReady;
            btnAcquire.IsEnabled = true;
        }

        void _device_ImageReady(NikonDevice sender, NikonImage image)
        {
            string dts = DateTime.Now.ToString("HHmmss");
            string filename = FolderHelper.GetImagePath();
            filename += dts + ".jpg";
            // Save captured image to disk
            using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                s.Write(image.Buffer, 0, image.Buffer.Length);
            }

            File.Copy(filename, FolderHelper.GetLatestImagePath(),true);
        }


        private void _device_CaptureComplete(NikonDevice sender, int data)
        {
            RefreshImage();
        }

        void pic1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        
        void AnalysisWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("new size :{0}-{1}", picturesContainer.ActualWidth, picturesContainer.ActualHeight);
            
        }

        private void FitNewSize()
        {
            pic1.AdapteToUI();
        }

      

        public override void Initialize()
        {
            if (bInitialized)
                return;
            
            
            if (!AcquireInfo.Instance.IsHorizontal)
            {
                Application.Current.MainWindow.Width = 1040;
                Application.Current.MainWindow.Height = 1000;
            }
            base.Initialize();
        }
       
        private void Parent_Closed(object sender, EventArgs e)
        {
            if (nikonManager != null)
                nikonManager.Shutdown();
        }

        

        #region mouse event handler
        private void PictureContainers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(lblOriginal);
          
        }

      
        #endregion

        
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
            AcquireInfo.Instance.NextPlate();
            btnNext.IsEnabled = false;
        }

    

        #region take photo



        private void HighlightWell(List<int> eachRowPositions)
        {
            pic1.ClearHight();
            for (int rowIndex = 0; rowIndex < eachRowPositions.Count; rowIndex++)
            {
                pic1.Highlight(eachRowPositions[rowIndex]);
            }
        }

 

        private void ShowResult()
        {
            var tbl3 = new DataTable("template");
            tbl3.Columns.Add("Seq", typeof(string));
            tbl3.Columns.Add("Result", typeof(string));
            //for (int i = 0; i < OldAnalyzer.Instance.Results.Count; i++)
            //{
            //    object[] objs = new object[2] { i + AcquireInfo.Instance.BatchStartID, OldAnalyzer.Instance.Results[i] };
            //    tbl3.Rows.Add(objs);
            //}
            lvResult.ItemsSource = tbl3.DefaultView;
        }

        private void RefreshImage()
        {
            string file = FolderHelper.GetLatestImagePath();
            
            if (File.Exists(file))
            {
                pic1.UpdateBackGroundImage(file,new Size(picturesContainer.ActualWidth,picturesContainer.ActualHeight));
            }
        }

        private void btnTakePhote_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch watcher = new Stopwatch();
            watcher.Start();
            SetInfo("正在拍照，请稍候。",false);
            this.IsEnabled = false;
            this.Refresh();
        
            Debug.WriteLine("update progress:" + watcher.Elapsed.Milliseconds);
            try
            {
                bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
                if(!bUseTestImage)
                {
                    
                    SetInfo("初始化完成！", false);
                    _device.Capture();
                }
                Debug.WriteLine("take photo:" + watcher.Elapsed.Milliseconds);
                btnNext.IsEnabled = AcquireInfo.Instance.curPlateID != AcquireInfo.Instance.GetTotalPlateCnt();//if not last one, allow user press next.
                RefreshImage();
                //OldAnalyzer.Instance.Analysis();
                //pic1.SetResult(OldAnalyzer.Instance.AnalysisResults);
                UpdateProgress();
                SetInfo(string.Format("分析完成。用时{0:f1}秒。", watcher.Elapsed.Milliseconds/1000.0), false);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            this.IsEnabled = true;
        }

       



#endregion
        #region loader

        protected void ShowLoader()
        {
            if (_progressThread != null)
                return;
            Window mainWindow = UIHelper.TryFindParent<Window>(this);
            double x = mainWindow.Left + mainWindow.Width/3;
            double y = mainWindow.Top + mainWindow.Height / 2;
            _progressThread = new Thread(() =>
            {
                loaderWindow = new Loader();
                loaderWindow.Show();
                loaderWindow.Left = x - loaderWindow.Width / 2;
                loaderWindow.Top = y - loaderWindow.Height / 2;
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
        #region helper functions
        private void UpdateProgress()
        {
            string assayName = AcquireInfo.Instance.CurrentAssay;
            lblProgress.Content = string.Format("{0}:{1}-{2}", assayName, AcquireInfo.Instance.BatchStartID, AcquireInfo.Instance.BatchEndID);
            var prgInfo =  string.Format( "进度：{0}/{1}", AcquireInfo.Instance.curPlateID, AcquireInfo.Instance.GetTotalPlateCnt());
            this.Refresh();
        }
        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Foreground = brush;
        }
        Visibility Bool2Visibility(bool bVisible)
        {
            return bVisible ? Visibility.Visible : Visibility.Hidden;
        }
      
        #endregion

    }
}
