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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BloodClotID
{
    /// <summary>
    /// loader.xaml 的交互逻辑
    /// </summary>
    public partial class Loader : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        public Loader()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += Timer_Tick;
            timer.Start();
            this.Closed += Loader_Closed;
            stopWatch.Start();

        }

        private void Loader_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            stopWatch.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int seconds = (int)(stopWatch.ElapsedMilliseconds / 1000);
            int tenthSeconds = (int)((stopWatch.ElapsedMilliseconds - seconds * 1000) / 100);
            lblTimeUsed.Content = string.Format("{0}.{1}s", seconds, tenthSeconds);
        }


    }

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Normal, EmptyDelegate);
        }

    }
}
