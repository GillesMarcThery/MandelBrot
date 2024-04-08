using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup.Localizer;
using System.Windows.Media;

namespace MandelBrot
{
    public enum ColorMethod
    {
        Random,
        Red,
        Green,
        Blue,
        Yellow,
        Gray,
        ProgressiveTest
    }
    internal class MandelbrotColors
    {
        public List<SolidColorBrush> colors = [];
        private int total_Colors; // field

        public int TotalColors   // property
        {
            set { total_Colors = value; }  // set method
        }
        public MandelbrotColors(int n, ColorMethod method, double factor)
        {
            this.total_Colors = n;
            Picker(method, factor);
        }
        public void ChangeColors(int n, ColorMethod method, double factor)
        {
            total_Colors = n;
            Picker(method, factor);
        }
        public void Picker(ColorMethod cm, double factor)
        {
            switch (cm)
            {
                case ColorMethod.Random:
                    Random();
                    break;
                case ColorMethod.Red:
                    ProgressiveRed(factor);
                    break;
                case ColorMethod.Green:
                    ProgressiveGreen(factor);
                    break;
                case ColorMethod.Blue:
                    ProgressiveBlue(factor);
                    break;
                case ColorMethod.Gray:
                    ProgressiveGray(factor);
                    break;
                case ColorMethod.Yellow:
                    ProgressiveYellow(factor);
                    break;
                case ColorMethod.ProgressiveTest:
                    ProgressiveTest(factor);
                    break;
            }
        }
        public void Random()
        {
            colors.Clear();
            Random rnd = new();
            for (int i = 0; i < total_Colors; i++)
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255))));
        }
        public void ProgressiveRed(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 1; i <= total_Colors; i++)
            {
                //Debug.WriteLine("ProgressiveRed " + i + " ; " + (byte)((255 - factor) + step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)((255 - factor) + step * i), 0, 0)));
            }
        }
        public void ProgressiveGreen(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 0; i < total_Colors; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb(0, (byte)((255 - factor) + step * i), 0)));
            }
        }
        public void ProgressiveBlue(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 0; i < total_Colors; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb(0, 0, (byte)((255 - factor) + step * i))));
            }
        }
        public void ProgressiveGray(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 0; i < total_Colors; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)((255 - factor) + step * i), (byte)((255 - factor) + step * i), (byte)((255 - factor) + step * i))));
            }
        }
        public void ProgressiveYellow(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 0; i < total_Colors; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)((255 - factor) + step * i), (byte)(0.6 * ((255 - factor) + step * i)), 0)));
            }
        }
        public void ProgressiveTest(double factor)
        {
            colors.Clear();
            double step = factor / total_Colors;

            for (int i = 0; i < total_Colors; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb(0, (byte)((255 - factor) + step * i), (byte)((255 - factor) + step * i))));
            }
        }
    }
}
