using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class SettingWindow : Window
    {
        PanelViewModel panelVM;
        public event EventHandler setOk;
        public SettingWindow()
        {
            InitializeComponent();
            List<AssayGroup> assayGroups = GlobalVars.ReadGroups();
            InitTreeview(assayGroups);
            panelVM.UpdateState(new ObservableCollection<string>() { "H5R-6" });
        }

        private void InitTreeview(List<AssayGroup> assayGroups)
        {
            panelVM = PanelViewModel.CreateViewModel(assayGroups);
            treeview.ItemsSource = new ObservableCollection<PanelViewModel>() { panelVM };
            this.treeview.Focus();
            
            panelVM.PropertyChanged += panelVM_PropertyChanged;
        }

        private void panelVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var assays = panelVM.GetAssays().ToList();
            AcquireInfo.Instance.assays = assays;
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
            this.Close();
        }

        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red: Brushes.Black;
            txtInfo.Foreground = brush;
        }

        private void CheckSettings()
        {
            if (AcquireInfo.Instance.assays == null || AcquireInfo.Instance.assays.Count == 0)
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
            AcquireInfo.Instance.SetLayout((bool)rdbHorizontalLayout.IsChecked);
            if (setOk != null)
                setOk(this, null);
            
        }
    }
}
