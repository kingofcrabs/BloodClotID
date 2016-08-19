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
        public ReportWindow()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGenerateExcel_Click(object sender, RoutedEventArgs e)
        {

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
            int totalAssayCnt = AcquireInfo.Instance.assays.Count;
            var results = Report.Instance.AssayResults;
            int i = 0;
            foreach (var pair in results)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = pair.Key;
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                i++;
            }
            
            dataGridView.RowHeadersWidth = 80;
            Dictionary<int, List<string>> sampleID_allTestResults = new Dictionary<int, List<string>>();
            foreach(var pair in results)
            {
                for(i = 0; i< pair.Value.Count;i++)
                {
                    int sampleID = i + 1;
                    if (sampleID_allTestResults.ContainsKey(sampleID))
                        sampleID_allTestResults[sampleID].Add(pair.Value[i].ToString());
                    else
                        sampleID_allTestResults.Add(sampleID, new List<string> { pair.Value[i].ToString() });
                }
            }
            for (i = 0; i < AcquireInfo.Instance.totalSample; i++)
            {
                int sampleID = i + 1;
                dataGridView.Rows.Add(sampleID_allTestResults[sampleID].ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("样品{0}", i + 1);
            }
        }
    }
}
