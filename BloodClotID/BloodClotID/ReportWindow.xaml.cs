using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utility;

namespace BloodClotID
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ReportWindow : BaseUserControl
    {
        Window parent;
        public ReportWindow(Window parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            parent.Close();
        }

        private void btnGenerateExcel_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            SetInfo(string.Format("正在创建Excel文件，请稍候"), false);
            this.Refresh();
            string path = FolderHelper.GetOutputFolder() + DateTime.Now.ToString("YYMMDDhhmmss") + ".xlsx";
            ExcelWriter.Save("testUser");
            SetInfo(string.Format("Excel文件已经创建于：{0}",path),false);
            this.IsEnabled = true;
            btnExit.IsEnabled = true;
        }

        private void SetInfo(string message, bool error = true)
        {
            txtInfo.Text = message;
            var brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Foreground = brush;
        }

        public override void Initialize()
        {
            if (bInitialized)
                return;
            InitDataGrid();
            base.Initialize();
        }

        private void InitDataGrid()
        {
           
            var results = GlobalVars.Instance.Result;
            DataGridViewTextBoxColumn columnID = new DataGridViewTextBoxColumn();
            columnID.HeaderText = "样品ID";
            dataGridView.Columns.Add(columnID);
          
            DataGridViewTextBoxColumn columnPosition = new DataGridViewTextBoxColumn();
            columnPosition.HeaderText = "判定位置";
            dataGridView.Columns.Add(columnPosition);

           

            foreach(var pair in results)
            {
                object[] objects = new string[2] { string.Format("样品{0}", pair.Key + 1), pair.Value.ToString() };
                dataGridView.Rows.Add(objects);
            }
        }
    }
}
