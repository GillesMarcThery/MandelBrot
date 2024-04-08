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
    //public struct MandelBrotHorizontalLine(Point p, double x_end, double divergence)
    //{
    //    public double x_start = p.X;
    //    public double y = p.Y;
    //    public double x_end = x_end;
    //    public double divergence = divergence;
    //    public readonly override string ToString()
    //    {
    //        return x_start + " -->" + x_end + " ; y= " + y + " ; d= " + divergence;
    //    }
    //}

    internal class MandelBrotSet
    {
        //public List<MandelBrotHorizontalLine> mandelBrotLines = [];
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
        private int[]? divergences_buffer;

        //public static Point Pixel2Real1(Point point, Rectangle r, Size canvas)
        //{
        //    Point result = new();
        //    double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
        //    double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

        //    if (canvas.Height * r.Width / r.Height <= canvas.Width)
        //    {
        //        result.X = xMiddle + (r.Height / canvas.Height) * point.X;
        //        result.Y = r.BottomRight.Y + (r.Height / canvas.Height) * (point.Y + canvas.Height / 2);
        //    }
        //    else
        //    {
        //        result.X = r.TopLeft.X + (r.Width / canvas.Width) * (point.X + canvas.Width / 2);
        //        result.Y = yMiddle + (r.Width / canvas.Width) * point.Y;
        //        //result.Y = r.BottomRight.Y + (r.Width / canvas.Width) * (point.Y + canvas.Width / 2);
        //        //result.Y = BottomRight.Y + (height / canvas.Height) * (point.Y + canvas.Height / 2);
        //    }
        //    return result;
        //}
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
        //public static Point Real2Pixel1(Point point, Rectangle r, Size canvas)
        //{
        //    Point result = new();
        //    double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
        //    double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

        //    if (canvas.Height * r.Width / r.Height <= canvas.Width) // écran plus allongé que la sélection
        //    {
        //        //result.X = Math.Round((point.X - r.TopLeft.X) * (canvas.Height / r.Height) - canvas.Height / 2);
        //        result.X = Math.Round((point.X - xMiddle) * (canvas.Height / r.Height));
        //        result.Y = Math.Round((point.Y - r.BottomRight.Y) * (canvas.Height / r.Height) - canvas.Height / 2);
        //    }
        //    else
        //    {
        //        result.X = Math.Round((point.X - r.TopLeft.X) * (canvas.Width / r.Width) - canvas.Width / 2);
        //        //result.X = Math.Round((point.X - xMiddle) * (canvas.Width / r.Width));
        //        result.Y = Math.Round((point.Y - yMiddle) * (canvas.Width / r.Width));
        //    }
        //    return result;
        //}
        public static Point Real2Pixel(Point point, Rectangle r, Size s)
        {
            Point result = new();
            double yMiddle = (r.TopLeft.Y + r.BottomRight.Y) / 2;
            double xMiddle = (r.TopLeft.X + r.BottomRight.X) / 2;

            if (s.Height * r.Width / r.Height <= s.Width) // écran plus allongé que la sélection
            {
                //result.X = Math.Round((point.X - xMiddle) * ((canvas.Height - 1) / r.Height));
                //result.Y = Math.Round((point.Y - r.BottomRight.Y) * ((canvas.Height - 1) / r.Height));
                result.X = Math.Round(((point.X - xMiddle) * ((s.Height - 1) / r.Height)) + s.Width / 2);
                result.Y = Math.Round((point.Y - r.TopLeft.Y) * ((1 - s.Height) / r.Height));
            }
            else
            {
                //result.X = Math.Round((point.X - r.TopLeft.X) * ((canvas.Width - 1) / r.Width));
                //result.Y = Math.Round((point.Y - yMiddle) * ((canvas.Width - 1) / r.Width));
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
        public ResultDivergenceCalculation DivergenceCalculation(Point point, Rectangle r, Size canvas, int max_iterations)
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
        /// Divergence calculation of a specific point
        /// </summary>
        /// <param name="p">point in compex map</param>
        /// <param name="max_iterations">maximum iterations</param>
        /// <returns></returns>
        //public ResultDivergenceCalculation DivergenceCalculation1(Point p, int max_iterations)
        //{
        //    double z_r = 0;
        //    double z_i = 0;
        //    double i = 0;
        //    double module;
        //    double c_r;
        //    double c_i;

        //    c_r = p.X;
        //    c_i = p.Y;
        //    do
        //    {
        //        double tmp = z_r;
        //        z_r = z_r * z_r - z_i * z_i + c_r;
        //        z_i = 2 * z_i * tmp + c_i;
        //        i++;
        //        module = z_r * z_r + z_i * z_i;
        //    }
        //    while (module < 4 && i < max_iterations);

        //    return new ResultDivergenceCalculation(module, i);
        //}
        //public void FillCollection1(MandelBrot_Navigation navigation, Size canvas, int max_iterations)
        //{
        //    mandelBrotLines.Clear();
        //    Debug.WriteLine("Canvas q=" + canvas.Height / canvas.Width);
        //    Debug.WriteLine("Rectangle q=" + navigation.Height / navigation.Width);

        //    Point upperLeft_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, canvas);
        //    Point bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, canvas);
        //    double step_Y = navigation.Height / (upperLeft_pix.Y - bottom_Right_pix.Y);
        //    double step_X = navigation.Width / (bottom_Right_pix.X - upperLeft_pix.X);

        //    for (double y = navigation.TopLeft.Y; y > navigation.BottomRight.Y; y -= step_Y)
        //    {
        //        Point firstPointOfLine_Real = new Point(navigation.TopLeft.X, y);
        //        Point firstPointOfLine_Pixel = Real2Pixel(firstPointOfLine_Real, navigation.CurrentSelection, canvas);
        //        //Debug.WriteLine("y= " + y+ " ; "+firstPointOfLine_Pixel.Y);
        //        MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(firstPointOfLine_Pixel, 0, DivergenceCalculation1(firstPointOfLine_Real, max_iterations).divergence);
        //        for (double x = navigation.TopLeft.X; x < navigation.BottomRight.X; x += step_X)
        //        {
        //            Point currentPoint_Real = new Point(x, y);
        //            Point p1 = Real2Pixel(currentPoint_Real, navigation.CurrentSelection, canvas);
        //            ResultDivergenceCalculation r = DivergenceCalculation1(currentPoint_Real, max_iterations);
        //            if (r.divergence != tmp.divergence)
        //            {
        //                tmp.x_end = p1.X;
        //                if (tmp.divergence != max_iterations)
        //                    mandelBrotLines.Add(tmp);
        //                tmp = new MandelBrotHorizontalLine(p1, 0, r.divergence);
        //            }
        //        }
        //        tmp.x_end = bottom_Right_pix.X;
        //        if (tmp.divergence != max_iterations)
        //            mandelBrotLines.Add(tmp);
        //    }
        //}
        /// <summary>
        /// Draw an horizontal line on the buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="width">width of the image</param>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <param name="b">Color</param>
        void DrawHorizontalLineOnBuffer(byte[] buffer, int width, Point p1, Point p2, Brush b)
        {
            // Converting Brush to Color
            Color myColorFromBrush = ((SolidColorBrush)b).Color;
            //Debug.WriteLine(s.ToString()+"   DrawHorizontalLineOnBuffer Y=" + Y1);
            for (int i = (int)(3 * (width * p1.Y + p1.X)); i < (int)(3 * (width * p1.Y + p2.X)); i += 3)
            {
                try
                {
                    buffer[i] = myColorFromBrush.B;
                    buffer[i + 1] = myColorFromBrush.G;
                    buffer[i + 2] = myColorFromBrush.R;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    break;
                }
            }
        }
        /// <summary>
        /// FillCollection_Pass1 : calc Divergence_min and Divergence_max
        /// </summary>
        /// <param name="navigation">rectangle of complex map to compute</param>
        /// <param name="s">size of canvas or image</param>
        /// <param name="max_iterations">maximum of iterations</param>
        public void FillCollection_Pass1(MandelBrot_Navigation navigation, Size s, int max_iterations)
        {
            Debug.WriteLine("Canvas q=" + s.Height / s.Width);
            Debug.WriteLine("Rectangle q=" + navigation.Height / navigation.Width);

            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);
            Point firstPointOfLine_Pixel;
            Point endPointOfLine_Pixel;
            int divergence;
            Divergence_max = 0;
            Divergence_min = max_iterations;
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                firstPointOfLine_Pixel = new Point(Top_Left_pix.X, y);
                divergence = (int)DivergenceCalculation(firstPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations).divergence;
                for (double x = Top_Left_pix.X; x < bottom_Right_pix.X; x++)
                {
                    endPointOfLine_Pixel = new Point(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(endPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations);
                    if (divergence < Divergence_min) Divergence_min = divergence;
                    if (divergence > Divergence_max) Divergence_max = divergence;
                    if (r.divergence != divergence)
                    {
                        divergence = (int)r.divergence;
                    }
                }
            }
        }
        public void FillCollection_Pass10(MandelBrot_Navigation navigation, Size s, int max_iterations)
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
                    Point p = new Point(x, y);
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
        public void FillCollection1(byte[] buffer, MandelbrotColors mandelbrotColors, Size s)
        {
            int i, j = 0;
            for (int k = 0; k < 3 * (int)s.Width * (int)s.Height; k++) { buffer[k] = 0; }
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                for (double x = Top_Left_pix.X; x <= bottom_Right_pix.X; x++)
                {
                    Point p = new Point(x, y);
                    Color c = mandelbrotColors.colors[this.divergences_buffer[j] - Divergence_min].Color;
                    i = (int)(3 * ((int)s.Width * p.Y + p.X));
                    buffer[i] = c.B;
                    buffer[i + 1] = c.G;
                    buffer[i + 2] = c.R;
                    j += sizeof(int);
                }
            }
        }
        public void FillCollection(byte[] buffer, MandelBrot_Navigation navigation, MandelbrotColors mandelbrotColors, Size s, int max_iterations)
        {
            //mandelBrotLines.Clear();
            for (int i = 0; i < 3 * (int)s.Width * (int)s.Height; i++) { buffer[i] = 0; }
            Debug.WriteLine("Canvas q=" + s.Height / s.Width);
            Debug.WriteLine("Rectangle q=" + navigation.Height / navigation.Width);

            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);
            Point firstPointOfLine_Pixel;
            Point endPointOfLine_Pixel;
            int divergence;
            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                firstPointOfLine_Pixel = new Point(Top_Left_pix.X, y);
                //Debug.WriteLine("y= " + y+ " ; "+firstPointOfLine_Pixel.Y);
                divergence = (int)DivergenceCalculation(firstPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations).divergence;
                //MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(firstPointOfLine_Pixel, 0, DivergenceCalculation(firstPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations).divergence);
                for (double x = Top_Left_pix.X; x < bottom_Right_pix.X; x++)
                {
                    endPointOfLine_Pixel = new Point(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(endPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations);
                    if (r.divergence != divergence)
                    {
                        //tmp.x_end = endPointOfLine_Pixel.X - 1;
                        if (divergence != max_iterations)
                        {
                            //mandelBrotLines.Add(tmp);
                            DrawHorizontalLineOnBuffer(buffer, (int)s.Width, firstPointOfLine_Pixel, endPointOfLine_Pixel, mandelbrotColors.colors[divergence - Divergence_min]);
                        }
                        //tmp = new MandelBrotHorizontalLine(endPointOfLine_Pixel, 0, r.divergence);
                        firstPointOfLine_Pixel = endPointOfLine_Pixel;
                        divergence = (int)r.divergence;
                    }
                }
                //tmp.x_end = bottom_Right_pix.X;
                if (divergence != max_iterations)
                {
                    //mandelBrotLines.Add(tmp);
                    DrawHorizontalLineOnBuffer(buffer, (int)s.Width, firstPointOfLine_Pixel, endPointOfLine_Pixel, mandelbrotColors.colors[divergence - Divergence_min]);
                }
            }
        }
    }
}
