using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MandelBrot
{
    public struct ResultDivergenceCalculation(double module, double divergence)
    {
        public double module = module;
        public double divergence = divergence;
    }

    internal class MandelBrotSet
    {
        public Point Top_Left_pix;
        public Point bottom_Right_pix;
        public int Width
        {
            get { return (int)bottom_Right_pix.X - (int)Top_Left_pix.X + 1; }
        }
        public int Height
        {
            get { return (int)bottom_Right_pix.Y - (int)Top_Left_pix.Y + 1; }
        }
        public int Divergence_min;
        public int Divergence_max;
        public int Divergences_amplitude
        {
            get { return Divergence_max - Divergence_min + 1; }
        }
        private int[] divergences_buffer = [];
        public static Point Pixel2Real(Point point, Rectangle r, Size canvas)
        {
            Point result = new();
            double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
            double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

            if (canvas.Height * r.Width / r.Height <= canvas.Width)
            {
                result.X = xMiddle + ((r.Height * (point.X - canvas.Width / 2)) / (canvas.Height - 1));
                result.Y = r.TopLeft.Y + ((r.Height * point.Y) / (1 - canvas.Height));
            }
            else
            {
                result.X = r.TopLeft.X + ((r.Width * (point.X)) / (canvas.Width - 1));
                result.Y = yMiddle - ((r.Width * (point.Y - canvas.Height / 2)) / (canvas.Width - 1));
            }
            return result;
        }
        public static Point Real2Pixel(Point point, Rectangle r, Size s)
        {
            Point result = new();
            double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
            double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

            if (s.Height * r.Width / r.Height <= s.Width) // écran plus allongé que la sélection
            {
                result.X = Math.Round(((point.X - xMiddle) * ((s.Height - 1) / r.Height)) + s.Width / 2);
                result.Y = Math.Round((point.Y - r.TopLeft.Y) * ((1 - s.Height) / r.Height));
            }
            else
            {
                result.X = Math.Round((point.X - r.TopLeft.X) * ((s.Width - 1) / r.Width));
                result.Y = Math.Round(s.Height / 2 - ((point.Y - yMiddle) * ((s.Width - 1) / r.Width)));
            }
            return result;
        }
        /// <summary>
        /// Divergence calculation of a specific point
        /// </summary>
        /// <param name="point">point in pixels</param>
        /// <param name="r">current selection</param>
        /// <param name="canvas">canvas</param>
        /// <param name="max_iterations">maximum iterations</param>
        /// <returns></returns>
        public static ResultDivergenceCalculation DivergenceCalculation(Point point, Rectangle r, Size canvas, int max_iterations)
        {
            double z_r = 0;
            double z_i = 0;
            double i = 0;
            double module;
            double c_r;
            double c_i;

            Point p = Pixel2Real(point, r, canvas);
            c_r = p.X;
            c_i = p.Y;
            do
            {
                double tmp = z_r;
                z_r = z_r * z_r - z_i * z_i + c_r;
                z_i = 2 * z_i * tmp + c_i;
                i++;
                module = z_r * z_r + z_i * z_i;
            }
            while (module < 4 && i < max_iterations);

            return new ResultDivergenceCalculation(module, i);
        }
        /// <summary>
        /// FillCollection_Pass1 : calc Divergence_min and Divergence_max
        /// </summary>
        /// <param name="navigation">rectangle of complex map to compute</param>
        /// <param name="s">size of canvas or image</param>
        /// <param name="max_iterations">maximum of iterations</param>
        public void FillCollection_Pass1(MandelBrot_Navigation navigation, Size s, int max_iterations)
        {
            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);
            divergences_buffer = new int[sizeof(int) * Width * Height];

            int i = 0;
            Divergence_max = 0;
            Divergence_min = max_iterations;
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                for (double x = Top_Left_pix.X; x <= bottom_Right_pix.X; x++)
                {
                    Point p = new(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(p, navigation.CurrentSelection, s, max_iterations);
                    if (r.divergence < Divergence_min) Divergence_min = (int)r.divergence;
                    if (r.divergence > Divergence_max) Divergence_max = (int)r.divergence;
                    divergences_buffer[i] = (int)r.divergence;
                    //if ((int)r.divergence != max_iterations)
                    //    divergences_buffer[i] = (int)r.divergence;
                    //else
                    //    divergences_buffer[i] = -1;
                    i += sizeof(int);
                }
            }
        }
        public void FillCollection(byte[] buffer, MandelbrotColors mandelbrotColors, Size s)
        {
            int i, j = 0;
            for (int k = 0; k < 3 * (int)s.Width * (int)s.Height; k++) { buffer[k] = 0; }
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                for (double x = Top_Left_pix.X; x <= bottom_Right_pix.X; x++)
                {
                    Point p = new(x, y);
                    Color c = mandelbrotColors.colors[this.divergences_buffer[j] - Divergence_min].Color;
                    i = (int)(3 * ((int)s.Width * p.Y + p.X));
                    buffer[i] = c.B;
                    buffer[i + 1] = c.G;
                    buffer[i + 2] = c.R;
                    j += sizeof(int);
                }
            }
        }
    }
}
