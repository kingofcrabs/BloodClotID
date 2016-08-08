using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PieControls
{
    /// <summary>
    /// Interaction logic for PieControl.xaml
    /// </summary> 
    public partial class PieControl : UserControl
    {
        ObservableCollection<PieSegment> values;
        Dictionary<Path, PieSegment> pathDictionary = new Dictionary<Path, PieSegment>();
        public static readonly DependencyProperty PopupBrushProperty = DependencyProperty.Register("PopupBrush", typeof(Brush), typeof(PieControl));

        public Brush PopupBrush 
        {
            get 
            {
                return (Brush)GetValue(PopupBrushProperty);
            }
            set 
            {
                SetValue(PopupBrushProperty, value);
            } 
        }

        public PieControl()
        {
            DataContext = this;
            PopupBrush = Brushes.Orange;
            InitializeComponent();
        }


        public ObservableCollection<PieSegment> Data
        {
            get { return values; }
            set
            {
                values = value;
                values.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(values_CollectionChanged);
                foreach (var v in values)
                {
                    v.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(pieSegment_PropertyChanged);
                }
                ResetPie();
            }
        }

        void AddPathToDictionary(Path path, PieSegment ps)
        {
            pathDictionary.Add(path, ps);
            path.MouseEnter += new MouseEventHandler(Path_MouseEnter);            
            path.MouseMove += new MouseEventHandler(Path_MouseMove);
        }

        void Path_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = Mouse.GetPosition(this);            
            piePopup.Margin = new Thickness(point.X - piePopup.ActualWidth / 4, point.Y - (18 + piePopup.ActualHeight), 0, 0);
        }

        private void PieControl_MouseLeave(object sender, MouseEventArgs e)
        {
            piePopup.Visibility = System.Windows.Visibility.Collapsed;
        }     

        void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            piePopup.Visibility = System.Windows.Visibility.Visible;
            PieSegment seg = pathDictionary[sender as Path];
            popupData.Text = seg.Name + " : " + ((seg.Value / Data.GetTotal()) * 100).ToString("N2") + "%";
            Point point = Mouse.GetPosition(this);
            piePopup.Margin = new Thickness(point.X - piePopup.ActualWidth / 4, point.Y - (18 + piePopup.ActualHeight), 0, 0);
        }

        void ClearPathDictionary()
        {
            foreach (Path path in pathDictionary.Keys)
            {
                path.MouseEnter -= Path_MouseEnter;
                path.MouseMove -= Path_MouseMove;
            }
            pathDictionary.Clear();
        }

        void CreatePiePathAndGeometries()
        {
            ClearPathDictionary();
            drawingCanvas.Children.Clear();
            drawingCanvas.Children.Add(piePopup);
            if (Data != null)
            {   
                double total = Data.GetTotal();
                if (total > 0)
                {
                    double angle = -Math.PI / 2;
                    foreach (PieSegment ps in Data)
                    {
                        //PieSegment ps = Data[1];
                        Geometry geometry;
                        Path path = new Path();
                        if (ps.Value == total)
                        {
                            geometry = new EllipseGeometry(new Point(this.Width / 2, this.Height / 2), this.Width / 2, this.Height / 2);
                        }
                        else
                        {
                            geometry = new PathGeometry();
                            double x = Math.Cos(angle) * Width / 2 + Width / 2;
                            double y = Math.Sin(angle) * Height / 2 + Height / 2;
                            LineSegment lingeSeg = new LineSegment(new Point(x, y), true);
                            double angleShare = (ps.Value / total) * 360;
                            angle += DegreeToRadian(angleShare);
                            x = Math.Cos(angle) * Width / 2 + Width / 2;
                            y = Math.Sin(angle) * Height / 2 + Height / 2;
                            ArcSegment arcSeg = new ArcSegment(new Point(x, y), new Size(Width / 2, Height / 2), angleShare, angleShare > 180, SweepDirection.Clockwise, false);
                            LineSegment lingeSeg2 = new LineSegment(new Point(Width / 2, Height / 2), true);
                            PathFigure fig = new PathFigure(new Point(Width / 2, Height / 2), new PathSegment[] { lingeSeg, arcSeg, lingeSeg2 }, false);
                            ((PathGeometry)geometry).Figures.Add(fig);
                        }
                        path.Fill = ps.GradientBrush;
                        path.Data = geometry;
                        AddPathToDictionary(path, ps);
                        drawingCanvas.Children.Add(path);
                    }
                }
            }
        }

        void ResetPie()
        {
            Dispatcher.Invoke(new Action(CreatePiePathAndGeometries));
        }

        void pieSegment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResetPie();
        }

        void values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ResetPie();
        }
     
        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
