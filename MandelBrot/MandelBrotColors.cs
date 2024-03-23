using System.Windows.Media;

namespace MandelBrot
{
    internal class MandelbrotColors
    {
        public List<SolidColorBrush> colors = [];
        public MandelbrotColors(int max_iterations)
        {
            Random rnd = new Random();
            for (int i = 0; i < max_iterations; i++)
                colors.Add(new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255))));
        }
    }
}
