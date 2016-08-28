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


namespace BloodClotID
{
    /// <summary>
    /// Interaction logic for AcquisitionWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : BaseUserControl
    {
        Thread _progressThread = null;
        Loader loaderWindow;
        IImageAcquirer imgAcquirer;
        ObservableCollection<PieSegment> pieCollection = new ObservableCollection<PieSegment>();
        public event EventHandler onReportReady;
        Transformer transformer;
        Window container;
        public AnalysisWindow(Window parent)
        {
            InitializeComponent();
            try
            {
                this.container = parent;
                imgAcquirer = ImageAcquirerFactory.CreateImageAcquirer(GlobalVars.Vendor);
                transformer = new Transformer(picturesContainer);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            picturesContainer.SizeChanged += PictureContainers_SizeChanged;
            picturesContainer.PreviewMouseLeftButtonUp += PictureContainers_PreviewMouseLeftButtonUp;
            picturesContainer.PreviewMouseMove += picturesContainer_PreviewMouseMove;
            parent.Closed += Parent_Closed;
            
        }

        void picturesContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Point pt = e.GetPosition(lblOriginal);
            //SetInfo(string.Format("x:{0}y:{1}",pt.X,pt.Y), false);
        }

        private void PictureContainers_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            transformer.DoTransform();
        }


        public override void Initialize()
        {
            if (bInitialized)
                return;
            pieCollection.Clear();
            pieCollection.Add(new PieSegment { Color = Colors.Green, Value = 0, Name = "已完成" });
            pieCollection.Add(new PieSegment { Color = Colors.Yellow, Value = AcquireInfo.Instance.GetTotalPlateCnt(), Name = "未完成" });
            chart1.Data = pieCollection;
            if (!AcquireInfo.Instance.IsHorizontal)
            {
                Application.Current.MainWindow.Width = 1040;
                Application.Current.MainWindow.Height = 1000;
            }
            base.Initialize();
        }
       
        private void Parent_Closed(object sender, EventArgs e)
        {
            imgAcquirer.Stop();
        }

        

        #region mouse event handler
        private void PictureContainers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(lblOriginal);
            if(!AcquireInfo.Instance.IsHorizontal)
            {
                pt = transformer.TransformBack(pt);
            }
            ResultCanvas resultCanvas;
            Point adjustPt = new Point(pt.X, pt.Y);
            if (pt.X > pic1.ActualWidth)
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
            resultCanvas.LeftMoseUp(adjustPt, false);
        }

      
        #endregion

        
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
            AcquireInfo.Instance.NextPlate();
            btnNext.IsEnabled = false;
        }

    

        #region take photo

        private void Analysis()
        {
            Analyzer.Instance.Reset();
            List<int> plateIDs = new List<int>() { 1, 2, 3, 4 };
            Parallel.ForEach(plateIDs, x => AnalysisPlate(x));
            ShowResult();
            transformer.DoTransform();
            HighlightWell(Analyzer.Instance.Results);
            Report.Instance.AddResult(AcquireInfo.Instance.CurrentAssay, Analyzer.Instance.Results);
        }

        private void HighlightWell(List<int> eachRowPositions)
        {
            Dictionary<int, ResultCanvas> dict = new Dictionary<int, ResultCanvas>() { };
            dict.Add(1, pic1);
            dict.Add(2, pic2);
            dict.Add(3, pic3);
            dict.Add(4, pic4);
            for(int rowIndex = 0; rowIndex < eachRowPositions.Count; rowIndex++)
            {
                var colIndex = eachRowPositions[rowIndex] - 1;
                int plateID = 1;
                int indexInPlate = 0;
                Analyzer.Instance.CalculatePlateAndPosition(rowIndex, colIndex,ref plateID, ref indexInPlate);
                dict[plateID].Highlight(indexInPlate);
            }
        }

        private void AnalysisPlate(int plateID)
        {
            Dictionary<int, ResultCanvas> dict = new Dictionary<int, ResultCanvas>() { };
            dict.Add(1, pic1);
            dict.Add(2, pic2);
            dict.Add(3, pic3);
            dict.Add(4, pic4);
            var result = Analyzer.Instance.AnalysisPlate(plateID);
            
            dict[plateID].SetResult(result);
        }

      

        private void ShowResult()
        {
            var tbl3 = new DataTable("template");
            tbl3.Columns.Add("Seq", typeof(string));
            tbl3.Columns.Add("Result", typeof(string));
            for (int i = 0; i < Analyzer.Instance.Results.Count; i++)
            {
                object[] objs = new object[2] { i + AcquireInfo.Instance.BatchStartID, Analyzer.Instance.Results[i] };
                tbl3.Rows.Add(objs);
            }
            lvResult.ItemsSource = tbl3.DefaultView;
        }

        private void RefreshImage()
        {
            int id = 1;
            foreach (var uiElement in picturesContainer.Children)
            {
                string file = FolderHelper.GetImagePath(id);
                ResultCanvas canvas = (ResultCanvas)uiElement;
                if (File.Exists(file))
                {
                    canvas.UpdateBackGroundImage(file, id);
                    canvas.LoadCalib(id);
                }

                else
                    canvas.Background = null;
                id++;
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
                imgAcquirer.TakePhoto();
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            Debug.WriteLine("take photo:" + watcher.Elapsed.Milliseconds);
            btnNext.IsEnabled = AcquireInfo.Instance.curPlateID != AcquireInfo.Instance.GetTotalPlateCnt();//if not last one, allow user press next.
            RefreshImage();
            Debug.WriteLine("refresh:" + watcher.Elapsed.Milliseconds);
            Analysis();
            Debug.WriteLine("analysis:" + watcher.Elapsed.Milliseconds);
            this.IsEnabled = true;
            UpdateProgress();
            SetInfo(string.Format("分析完成。用时{0:f1}秒。",watcher.Elapsed.TotalMilliseconds/1000.0), false);
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
            pieCollection[0].Value = AcquireInfo.Instance.curPlateID;
            int notFinishedCnt = AcquireInfo.Instance.GetTotalPlateCnt() - AcquireInfo.Instance.curPlateID;
            pieCollection[1].Value = notFinishedCnt;
            this.Refresh();
            if (notFinishedCnt == 0)
            {
                if (onReportReady != null)
                    onReportReady(this, null);
            }
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
