using Emgu.CV;
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

namespace AnalysisiLength
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            LengthAnalyzer lengthAnalyzer = new LengthAnalyzer();
            Mat img = new Mat(@"D:\test\1.jpg");
            List<Point> ptCorners = new List<Point>();
            var length = lengthAnalyzer.CalculateLength(img,1, ptCorners);
            Debug.WriteLine(length);
        }
    }
}
