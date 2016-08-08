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
using BloodClotID.Camera;

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

        public AnalysisWindow(Window parent)
        {
            InitializeComponent();
            try
            {

                imgAcquirer = ImageAcquirerFactory.CreateImageAcquirer(GlobalVars.Vendor);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message);
            }
            pictureContainers.PreviewMouseLeftButtonDown += PictureContainers_PreviewMouseLeftButtonDown;
            pictureContainers.PreviewMouseLeftButtonUp += PictureContainers_PreviewMouseLeftButtonUp;
            parent.Closed += Parent_Closed;
            
        }
        protected override void InitializeImpl()
        {
            if (bInitialized)
                return;
            base.InitializeImpl();
            
            pieCollection.Clear();
            pieCollection.Add(new PieSegment { Color = Colors.Green, Value = 0, Name = "已完成" });
            pieCollection.Add(new PieSegment { Color = Colors.Yellow, Value = AcquireInfo.Instance.GetTotalPlateCnt(), Name = "未完成" });
            chart1.Data = pieCollection;
        }
        
        private void Parent_Closed(object sender, EventArgs e)
        {
            imgAcquirer.Stop();
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
            resultCanvas.LeftMoseDown(adjustPt, false);
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

        #region take photo

        private void Analysis()
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(AnalysisFirstPlate));
            tasks.Add(Task.Factory.StartNew(AnalysisSecondPlate));
            tasks.Add(Task.Factory.StartNew(AnalysisThirdPlate));
            tasks.Add(Task.Factory.StartNew(AnalysisForthPlate));
            tasks.ForEach(x => x.Wait());
        }

        private void AnalysisForthPlate()
        {
            var result = Analyzer.Instance.AnalysisPlate(4);
            pic4.SetResult(result);
        }

        private void AnalysisThirdPlate()
        {
            var result = Analyzer.Instance.AnalysisPlate(3);
            pic3.SetResult(result);
        }

        private void AnalysisSecondPlate()
        {
            var result = Analyzer.Instance.AnalysisPlate(2);
            pic2.SetResult(result);
        }

        private void AnalysisFirstPlate()
        {
            var result = Analyzer.Instance.AnalysisPlate(1);
            pic1.SetResult(result);
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
            UpdateProgress();
            ShowLoader();
            try
            {
                imgAcquirer.TakePhoto();
            }
            catch (Exception ex)
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
        Visibility Bool2Visibility(bool bVisible)
        {
            return bVisible ? Visibility.Visible : Visibility.Hidden;
        }
      
        #endregion

    }
}
