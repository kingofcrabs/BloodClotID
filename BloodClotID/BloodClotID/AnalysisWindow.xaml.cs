using Emgu.CV;
using Nikon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utility;


namespace BloodClotID
{
    /// <summary>
    /// Interaction logic for AcquisitionWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : BaseUserControl
    {

        NikonDevice _device;
        AutoResetEvent waitDeviceAdded = new AutoResetEvent(false);
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event EventHandler onReportReady;
        List<int> eachRowSelectedWellIndexs = new List<int>();
        Window container;
        NikonManager nikonManager;
        public AnalysisWindow(Window parent)
        {
            log.Info("AnalysisWindow");
            InitializeComponent();
            try
            {
                this.container = parent;
                pic1.onAdjustResult += Pic1_onAdjustResult;
                if(GlobalVars.UseTestImage)
                {
                    btnAcquire.IsEnabled = true;
                }
                else
                {
                    btnAcquire.IsEnabled = false;
                    nikonManager = new NikonManager("Type0008.md3");
                    nikonManager.DeviceAdded += NikonManager_DeviceAdded;
                }
              
                //transformer = new Transformer(picturesContainer);
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
            else
            {

            }
            pic1.PreviewMouseLeftButtonUp += pic1_PreviewMouseLeftButtonUp;
            parent.Closed += Parent_Closed;
        }

        private void Pic1_onAdjustResult(object sender, KeyValuePair<int, int> e)
        {
            eachRowSelectedWellIndexs[e.Key] = e.Value;
            ShowResult();
        }

        private void NikonManager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            Debug.WriteLine("=> {0}, {1}", sender.Id, sender.Name);
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
            //RefreshImage();
        }

        void pic1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pic1.LeftMoseUp(e.GetPosition(this));
        }

        
        void AnalysisWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("new size :{0}-{1}", picturesContainer.ActualWidth, picturesContainer.ActualHeight);
            
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
        public void HighlightWell()
        {
            pic1.ClearHight();
            for (int rowIndex = 0; rowIndex < eachRowSelectedWellIndexs.Count; rowIndex++)
            {
                pic1.Highlight( PlatePositon.GetWellID(rowIndex, eachRowSelectedWellIndexs[rowIndex]));
            }
            pic1.InvalidateVisual();
        }

 

        private void ShowResult()
        {
            var tbl3 = new DataTable("template");
            tbl3.Columns.Add("Seq", typeof(string));
            tbl3.Columns.Add("Result", typeof(string));
            for (int i = 0; i < eachRowSelectedWellIndexs.Count; i++)
            {
                object[] objs = new object[2] { i + AcquireInfo.Instance.curPlateID, eachRowSelectedWellIndexs[i]+1 };
                tbl3.Rows.Add(objs);
            }
            lvResult.ItemsSource = tbl3.DefaultView;
        }

        private void RefreshImage(string file)
        {
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
                bool bUseTestImage = GlobalVars.UseTestImage;
                string file = FolderHelper.GetLatestImagePath();
                if (!bUseTestImage)
                {
                    SetInfo("初始化完成！", false);
                    _device.Capture();
                }
                else
                {

                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                    // Set filter for file extension and default file extension 
                    dlg.DefaultExt = ".jpg";
                    dlg.InitialDirectory = FolderHelper.GetAcquiredImageRootFolder();
                    dlg.Filter = "image files (.jpg)|*.jpg";

                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                        file = dlg.FileName;
                }
                Debug.WriteLine("take photo:" + watcher.Elapsed.Milliseconds);
                bool isLastOne = AcquireInfo.Instance.curPlateID == AcquireInfo.Instance.GetTotalPlateCnt();
                btnNext.IsEnabled = !isLastOne;//if not last one, allow user press next.
            
                string newFile = GetROI(file);
               
       
                RefreshImage(newFile);
                LengthAnalyzer analyzer = new LengthAnalyzer();
                List<List<System.Windows.Point>> eachWellCornerPts = new List<List<System.Windows.Point>>();
                List<System.Windows.Point> centerPts = new List<Point>();
            
                List<double> lengths = analyzer.GetEachWellLength(newFile,
                   eachWellCornerPts, centerPts); //AcquireInfo.Instance.CalculateSamplesThisBatch(),AcquireInfo.Instance.IsHorizontal,
                
                pic1.SetResult(lengths, eachWellCornerPts,centerPts);
                eachRowSelectedWellIndexs = HighLightAnalyzer.Instance.Go(lengths);
                HighlightWell();
                ShowResult();
                GlobalVars.Instance.SetResult(eachRowSelectedWellIndexs);
                UpdateProgress();
                SetInfo(string.Format("分析完成。用时{0:f1}秒。", watcher.Elapsed.Milliseconds/1000.0), false);
                string tempImage = SaveSnapShot(picturesContainer);
                AcquireInfo.Instance.SnapShot = tempImage;
                AcquireInfo.Instance.OriginalImage = newFile;
                if (isLastOne)
                {
                    //btnReport.IsEnabled = true;
                    if (onReportReady != null)
                        onReportReady(this, null);
                }
                    
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            this.IsEnabled = true;
        }

        private string SaveSnapShot(Grid grid)
        {
           
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap(
                                       (int)grid.ActualWidth*2,
                                       (int)grid.ActualHeight*2,
                                       192d,
                                       192d,
                                       PixelFormats.Default);

            targetBitmap.Render(grid);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            string imgPath = FolderHelper.CurrentAcquiredImageFolder + "snapshort.jpg";
            // save file to disk
            using (FileStream fs = File.Open(imgPath, FileMode.OpenOrCreate))
            {
                encoder.Save(fs);
            }
            return imgPath;
        }

        private string GetROI(string file)
        {
            System.Drawing.Point ptStart = new System.Drawing.Point(1100, 700);
            System.Drawing.Size sz = new System.Drawing.Size(4920 - 1100, 3400 - 700);
            if (!AcquireInfo.Instance.IsHorizontal)
            {
                ptStart = new System.Drawing.Point(1700,120);
                sz = new System.Drawing.Size(4300-1700, 3920 - 120);
            }
        
            Mat img = new Mat(file);
            Mat subImg = new Mat(img, new System.Drawing.Rectangle(ptStart, sz));
            string newFile = GetName(file);
            
            if (File.Exists(newFile)) 
                File.Delete(newFile);

            CvInvoke.Flip(subImg, subImg, Emgu.CV.CvEnum.FlipType.Horizontal);
            subImg.Save(newFile);
            return newFile;
        }

        private string GetName(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            int ID = 1;
            do
            {
                string newFileName = fileInfo.Directory + string.Format("\\tmp{0}.jpg", ID++);
                if (!File.Exists(newFileName))
                    return newFileName;
            } while (true);
        }

        #endregion

        #region helper functions
        private void UpdateProgress()
        {
            string assayName = AcquireInfo.Instance.CurrentAssay;
            lblProgress.Content = string.Format("{0}", assayName);
            var prgInfo =  string.Format( "进度：{0}/{1}", AcquireInfo.Instance.curPlateID, AcquireInfo.Instance.GetTotalPlateCnt());
            if(AcquireInfo.Instance.curPlateID == AcquireInfo.Instance.GetTotalPlateCnt())
            {
                SetInfo("全部完成！");
            }
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

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
