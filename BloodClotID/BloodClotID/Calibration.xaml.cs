using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utility;

namespace BloodClotID
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CalibWindow : Window
    {
        public CalibWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                myCanvas.UpdateBackGroundImage(GetImage(1));
            }
            catch(Exception ex)
            {
                txtInfo.Text = ex.Message;
                txtInfo.Foreground = Brushes.Red;
            }
            
            myCanvas.IsHitTestVisible = false;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.MouseLeftButtonUp += scrollViewer_MouseLeftButtonUp;
            scrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;
            scrollViewer.PreviewMouseDoubleClick += scrollViewer_PreviewMouseDoubleClick;
            this.KeyDown += MainWindow_KeyDown;
        }

        void scrollViewer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            
            myCanvas.DoubleClick(pt);
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            myCanvas.MoveMouse(pt);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("key pressed:{0}", e.Key);
            if ((bool)rdbAdd.IsChecked)
                return;

            myCanvas.OnKey(e.Key);
        }

        void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Point pt = e.GetPosition(myCanvas);
            //myCanvas.LeftMoseDown(pt, (bool)rdbAdd.IsChecked);
        }

        void scrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMouseUp(pt, (bool)rdbAdd.IsChecked);
        }

        private void rdbAdd_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.ClearSelectFlag();
        }

        private void rdbModify_Click(object sender, RoutedEventArgs e)
        {

        }

        private string GetImage(int cameraID)
        {
            return FolderHelper.GetAcquiredImageRootFolder() + string.Format("{0}.jpg",cameraID);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int cameraID = cmbPlateNum.SelectedIndex + 1;
            myCanvas.SaveCalib(cameraID);

        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            int cameraID = cmbPlateNum.SelectedIndex + 1;
            myCanvas.LoadCalib(cameraID);
        }

        private void cmbPlateNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            myCanvas.UpdateBackGroundImage(GetImage(cmbPlateNum.SelectedIndex+1));
        }
    }
}
