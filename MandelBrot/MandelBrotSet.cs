using System.Diagnostics;
using System.Windows;

namespace MandelBrot
{
    public struct ResultDivergenceCalculation(double module, double divergence)
    {
        public double module = module;
        public double divergence = divergence;
    }
    public struct MandelBrotHorizontalLine(Point p, double x_end, double divergence)
    {
        public double x_start = p.X;
        public double y = p.Y;
        public double x_end = x_end;
        public double divergence = divergence;
        public readonly override string ToString()
        {
            return x_start + " -->" + x_end + " ; y= " + y + " ; d= " + divergence;
        }
    }

    internal class MandelBrotSet
    {
        public List<MandelBrotHorizontalLine> mandelBrotLines = [];
        public Point Top_Left_pix;
        public Point bottom_Right_pix;

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
        public void FillCollection(MandelBrot_Navigation navigation, Size s, int max_iterations)
        {
            mandelBrotLines.Clear();
            Debug.WriteLine("Canvas q=" + s.Height / s.Width);
            Debug.WriteLine("Rectangle q=" + navigation.Height / navigation.Width);

            Top_Left_pix = Real2Pixel(navigation.TopLeft, navigation.CurrentSelection, s);
            bottom_Right_pix = Real2Pixel(navigation.BottomRight, navigation.CurrentSelection, s);

            for (double y = Top_Left_pix.Y; y <= bottom_Right_pix.Y; y++)
            {
                Point firstPointOfLine_Pixel = new Point(Top_Left_pix.X, y);
                //Debug.WriteLine("y= " + y+ " ; "+firstPointOfLine_Pixel.Y);
                MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(firstPointOfLine_Pixel, 0, DivergenceCalculation(firstPointOfLine_Pixel, navigation.CurrentSelection, s, max_iterations).divergence);
                for (double x = Top_Left_pix.X; x < bottom_Right_pix.X; x++)
                {
                    Point p1 = new Point(x, y);
                    ResultDivergenceCalculation r = DivergenceCalculation(p1, navigation.CurrentSelection, s, max_iterations);
                    if (r.divergence != tmp.divergence)
                    {
                        tmp.x_end = p1.X;
                        if (tmp.divergence != max_iterations)
                            mandelBrotLines.Add(tmp);
                        tmp = new MandelBrotHorizontalLine(p1, 0, r.divergence);
                    }
                }
                tmp.x_end = bottom_Right_pix.X;
                if (tmp.divergence != max_iterations)
                    mandelBrotLines.Add(tmp);
            }
        }
    }
}
