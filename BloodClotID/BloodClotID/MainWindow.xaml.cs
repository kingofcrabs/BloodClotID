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

namespace BloodClotID.Camera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread _progressThread = null;
        Loader loaderWindow;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnTakePhote_Click(object sender, RoutedEventArgs e)
        {

            ShowLoader();
            rootGrid.IsEnabled = false;
            this.Refresh();
            FourCamera fourCamera = new FourCamera();
            fourCamera.TakePhote();
            ShowPhotos();
            HideLoader();
            rootGrid.IsEnabled = true;
        }

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

        private void ShowPhotos()
        {
            string imgFolder = FolderHelper.GetImageFolder();
            int cameraIndex = 0;
            foreach(var uiElement in pictureContainers.Children)
            {
                string path = imgFolder + string.Format("{0}.jpg", cameraIndex + 1);
                if (!File.Exists(path))
                    throw new Exception(string.Format("图像{0}未能找到！", cameraIndex));

                ((Grid)uiElement).Background = RenderHelper.CreateBrushFromFile(path);
                cameraIndex++;
            }
            
           
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
    }

   
}
