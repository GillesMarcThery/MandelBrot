using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MandelBrot
{
    public struct ResultDivergenceCalculation(double module, int divergence)
    {
        public double module = module;
        public int divergence = divergence;
    }
    struct PixelPosition
    {
        public int X;
        public int Y;
        public PixelPosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public PixelPosition(double x, double y)
        {
            this.X = (int)x;
            this.Y = (int)y;
        }
        public PixelPosition(Point p)
        {
            this.X = (int)p.X;
            this.Y = (int)p.Y;
        }
        public override readonly string ToString() { return X + ", " + Y + ")"; }
    }
    internal class MandelBrotSet
    {
        public PixelPosition Top_Left_pix;
        public PixelPosition bottom_Right_pix;
        public int Width
        {
            get { return bottom_Right_pix.X - Top_Left_pix.X + 1; }
        }
        public int Height
        {
            get { return bottom_Right_pix.Y - Top_Left_pix.Y + 1; }
        }
        public int Divergence_min;
        public int Divergence_max;
        public int Divergences_amplitude
        {
            get { return Divergence_max - Divergence_min + 1; }
        }
        private int[] divergences_buffer = [];
        private int threads_controler = 0;
        public static Point Pixel2Real(PixelPosition point, Rectangle r, Size canvas)
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
        public static PixelPosition Real2Pixel(Point point, Rectangle r, Size s)
        {
            PixelPosition result = new();
            double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
            double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

            if (s.Height * r.Width / r.Height <= s.Width) // écran plus allongé que la sélection
            {
                result.X = (int)(((point.X - xMiddle) * ((s.Height - 1) / r.Height)) + s.Width / 2);
                result.Y = (int)((point.Y - r.TopLeft.Y) * ((1 - s.Height) / r.Height));
            }
            else
            {
                result.X = (int)((point.X - r.TopLeft.X) * ((s.Width - 1) / r.Width));
                result.Y = (int)(s.Height / 2 - ((point.Y - yMiddle) * ((s.Width - 1) / r.Width)));
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
        public static ResultDivergenceCalculation DivergenceCalculation(PixelPosition point, Rectangle r, Size canvas, int max_iterations)
        {
            double z_r = 0;
            double z_i = 0;
            int i = 0;
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);
            //divergences_buffer = new int[sizeof(int) * Width * Height];
            divergences_buffer = new int[Width * Height];
            int i = 0;
            Divergence_max = 0;
            Divergence_min = max_iterations;
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                for (double x = Top_Left_pix.X; x <= bottom_Right_pix.X; x++)
                {
                    PixelPosition p = new(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(p, navigation.CurrentSelection, s, max_iterations);
                    if (r.divergence < Divergence_min) Divergence_min = r.divergence;
                    if (r.divergence > Divergence_max) Divergence_max = r.divergence;
                    if (r.divergence == max_iterations)
                        divergences_buffer[i] = -1;
                    else
                        divergences_buffer[i] = r.divergence;
                    i++;
                }
            }
            stopwatch.Stop();
            Debug.WriteLine("FillCollection_Pass1 : " + stopwatch.ElapsedMilliseconds);
        }
        public void Calc_Div_Buffer_Top(MandelBrot_Navigation navigation, Size s, int max_iterations)
        {
            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);
            divergences_buffer = new int[Width * Height];
            int q = Height / 12;
            int r = Height % 12;
            int blocSize = Width * q;
            PixelPosition p1, p2;
            Thread myThread;
            threads_controler = 0;
            int total_thread = 12;
            Divergence_max = 0;
            Divergence_min = max_iterations;
            //Thread[] myThread1 = new Thread[6];

            //p1 = new(Top_Left_pix.X, Top_Left_pix.Y);
            //p2 = new(bottom_Right_pix.X, Top_Left_pix.Y + q);
            //myThread = new Thread(() => Calc_Div_Buffer(1, navigation.CurrentSelection, p1, p2, s, max_iterations));
            //myThread.Start();
            for (int i = 1; i <= 12; i++)
            {
                int temp = i;
                //p1 = new(Top_Left_pix.X, Top_Left_pix.Y + (i - 1) * q + 1);
                //p2 = new(bottom_Right_pix.X, Top_Left_pix.Y + q * i);
                PixelPosition p1_temp = new(Top_Left_pix.X, Top_Left_pix.Y + (i - 1) * q); ;
                PixelPosition p2_temp = new(bottom_Right_pix.X, Top_Left_pix.Y + q * i - 1);
                myThread = new Thread(() => Calc_Div_Buffer(temp, blocSize, navigation.CurrentSelection, p1_temp, p2_temp, s, max_iterations));
                myThread.Start();
            }
            if (r != 0)
            {
                p1 = new(Top_Left_pix.X, Top_Left_pix.Y + 12 * q);
                p2 = new(bottom_Right_pix.X, bottom_Right_pix.Y);
                myThread = new Thread(() => Calc_Div_Buffer(13, blocSize, navigation.CurrentSelection, p1, p2, s, max_iterations));
                myThread.Start();
                total_thread++;
            }
            while (threads_controler < total_thread)
            {
                //Debug.WriteLine("Waiting ");
            }
        }
        public void Calc_Div_Buffer(int index, int blocSize, Rectangle rect, PixelPosition p1, PixelPosition p2, Size canvas, int max_iterations)
        {
            int w = ((int)p2.X - (int)p1.X + 1);
            int h = ((int)p2.Y - (int)p1.Y + 1);
            int i = (index - 1) * blocSize;
            Debug.WriteLine("Calc_Div_Buffer " + index + " start --> p1 = " + p1.ToString() + " end --> p2 = " + p2.ToString() + " ; i = " + i + " ; w = " + w + " ; h = " + h);

            for (double y = p1.Y; y <= p2.Y; y++)
            {
                for (double x = p1.X; x <= p2.X; x++)
                {
                    PixelPosition p = new(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(p, rect, canvas, max_iterations);
                    if (r.divergence < Divergence_min) Divergence_min = r.divergence;
                    if (r.divergence > Divergence_max) Divergence_max = r.divergence;
                    if (r.divergence == max_iterations)
                        divergences_buffer[i] = -1;
                    else
                        divergences_buffer[i] = r.divergence;
                    i++;
                }
            }
            threads_controler++;
            //Debug.WriteLine("Calc_Div_Buffer " + index + " finished");
        }
        public void FillCollection(byte[] buffer, MandelbrotColors mandelbrotColors, Size s)
        {
            int i, j = 0;
            Color c;
            for (int k = 0; k < 3 * s.Width * s.Height; k++) { buffer[k] = 0; }
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                for (double x = Top_Left_pix.X; x <= bottom_Right_pix.X; x++)
                {
                    Point p = new(x, y);
                    if (divergences_buffer[j] == -1)
                        c = Colors.Black;
                    else
                    {
                        int d = divergences_buffer[j];
                        try
                        {
                            c = mandelbrotColors.colors[d - Divergence_min].Color;
                        }
                        catch (Exception e)
                        {
                            int k;
                            using (StreamWriter writer = new StreamWriter("./tmp.csv"))
                            {
                                int count = 0;

                                for (k = 0; k < divergences_buffer.Length; k ++)
                                {
                                    writer.Write(divergences_buffer[k] + ";");
                                    count++;
                                    if (count == 296) { writer.Write(Environment.NewLine); count = 0; }
                                }
                            }
                            Environment.Exit(0);
                        }
                    }
                    i = (int)(3 * (s.Width * p.Y + p.X));
                    buffer[i] = c.B;
                    buffer[i + 1] = c.G;
                    buffer[i + 2] = c.R;
                    j++;
                }
            }
        }
    }
}
