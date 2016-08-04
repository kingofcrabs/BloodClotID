using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoadingControl.Control
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        Timer timer = new Timer(100);
        Stopwatch watcher = new Stopwatch();
        public LoadingAnimation()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            watcher.Start();
            timer.Start();
            
            this.Unloaded += LoadingAnimation_Unloaded;
        }

        private void LoadingAnimation_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            watcher.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    int seconds = (int)(watcher.ElapsedMilliseconds / 1000);
                    txtInfo.Text = string.Format("{0}.{1}s", seconds, (watcher.ElapsedMilliseconds - 1000 * seconds) / 100);
                });
            }
            catch(Exception)
            {

            }
            
            
        }


    }
}
