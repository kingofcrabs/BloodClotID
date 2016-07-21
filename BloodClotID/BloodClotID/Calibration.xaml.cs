﻿using System;
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
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"D:\Projects\BloodClotID.git\trunk\test.jpg");
            myCanvas.UpdateBackGroundImage(bmp);
            myCanvas.IsHitTestVisible = false;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.MouseLeftButtonUp += scrollViewer_MouseLeftButtonUp;
            this.KeyDown += MainWindow_KeyDown;
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
            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMoseDown(pt, (bool)rdbAdd.IsChecked);
        }

        void scrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((bool)rdbModify.IsChecked)
                return;

            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMouseUp(pt);
        }

        private void rdbAdd_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.ClearSelectFlag();
        }

        private void rdbModify_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.SaveConfig();

        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.LoadConfig();
        }
    }
}