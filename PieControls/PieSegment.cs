using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;

namespace PieControls
{
    public class PieSegment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string name;
        double value;
        Brush gradientBrush;
        Brush solidBrush;
        Color color;

        public double Value
        {
            get 
            {
                return value;
            }
            set 
            {
                this.value = value;
                onPropertyChanged(this, "Value");
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;                
                gradientBrush = new LinearGradientBrush(MakeSecondColor(color, 50), color, 45);
                solidBrush = new SolidColorBrush(color);
                gradientBrush.Freeze();
                solidBrush.Freeze();
                onPropertyChanged(this, "Color");
            }
        }

        //difference should be a maximum value of 100
        Color MakeSecondColor(Color color, uint difference)
        {
            difference = difference > 100 ? 100 : difference;
            byte r = GetNewColorByte(color.R, difference);
            byte g = GetNewColorByte(color.G, difference);
            byte b = GetNewColorByte(color.B, difference);
            return Color.FromRgb(r, g, b);
        }

        //This method ensures that bytes never overflow to avoid drastic change in color
        byte GetNewColorByte(byte oldByte, uint difference)
        {
            if (oldByte + difference > 255)
            {
                return (byte)(oldByte - difference);
            }
            else
            {
                return (byte)(oldByte + difference);
            }
        }

        public Brush GradientBrush
        {
            get { return gradientBrush; }
        }
        public Brush SolidBrush
        {
            get { return solidBrush; }
        }

        public string Name 
        { 
            get 
            { 
                return name; 
            } 
            set 
            { 
                name = value; 
                onPropertyChanged(this, "Name"); 
            } 
        }

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
