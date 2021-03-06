﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using Utility;

namespace BloodClotID
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : BaseUserControl
    {

        GroupViewModel groupViewModel;
        DateTime lastChangeTime = DateTime.Now;

        public SettingWindow()
        {
            InitializeComponent();
            this.Loaded += SettingWindow_Loaded;
          
        }

        private void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<AssayGroup> assayGroups = GlobalVars.ReadGroups();
            groupViewModel = new GroupViewModel(assayGroups);
            this.DataContext = groupViewModel;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckSettings();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }
            base.OnSwitch(Stage.Analysis);
            
        }

        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red: Brushes.Black;
            txtInfo.Foreground = brush;
        }

        private void CheckSettings()
        {
            AcquireInfo.Instance.assayName = groupViewModel.SelectedGroup.SelectedAssay;
            if (AcquireInfo.Instance.assayName == null)
                throw new Exception("请先选择测试项目！");

            string smpCnt = txtSampleCnt.Text;
            int val = 0;
            bool bok = int.TryParse(smpCnt, out val);
            if(!bok)
            {
                throw new Exception("样品数必须为数字！");
            }
            if (val <= 0)
                throw new Exception("样品数必须大于0！");
            AcquireInfo.Instance.SetSampleCount(val);
            AcquireInfo.Instance.IsHI = true;
            AcquireInfo.Instance.HasControl = (bool)rdbControl.IsChecked;
            bool isHorizontal = (bool)rdbHorizontalLayout.IsChecked;
            AcquireInfo.Instance.SetLayout(isHorizontal);
            PlatePositon.SetAlignment(isHorizontal);

        }
    }
}
