using System.Diagnostics;
using System.Windows.Media;

namespace MandelBrot
{
    internal class MandelbrotColors
    {
        public List<SolidColorBrush> colors = [];
        private int max_iterations; // field

        public int MaxIterations   // property
        {
            set { max_iterations = value; }  // set method
        }
        public MandelbrotColors(int max_iterations)
        {
            this.max_iterations = max_iterations;

        }
        public void Random()
        {
            colors.Clear();
            Random rnd = new Random();
            for (int i = 0; i < max_iterations; i++)
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255))));
        }
        public void RandomRed()
        {
            colors.Clear();
            Random rnd = new Random();
            for (int i = 0; i < max_iterations; i++)
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), 0, 0)));
        }
        public void ProgressiveRed()
        {
            colors.Clear();
            double step = 156.0 / max_iterations;

            for (int i = 0; i < max_iterations; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)(50 + step * i), 0, 0)));
            }
        }
        public void ProgressiveGreen()
        {
            colors.Clear();
            double step = 156.0 / max_iterations;

            for (int i = 0; i < max_iterations; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb(0, (byte)(50 + step * i), 0)));
            }
        }
        public void ProgressiveBlue()
        {
            colors.Clear();
            double step = 156.0 / max_iterations;

            for (int i = 0; i < max_iterations; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb(0, 0, (byte)(50 + step * i))));
            }
        }
        public void ProgressiveGray()
        {
            colors.Clear();
            double step = 156.0 / max_iterations;

            for (int i = 0; i < max_iterations; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)(50 + step * i), (byte)(50 + step * i), (byte)(50 + step * i))));
            }
        }
        public void ProgressiveTest()
        {
            colors.Clear();
            double step = 256.0 / max_iterations;

            for (int i = 0; i < max_iterations; i++)
            {
                //Debug.WriteLine(i + " ; " + (byte)(step * i));
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)(step * i), (byte)(step * i),50)));
            }
        }
    }
}
