using System.Diagnostics;
using System.Windows;

namespace MandelBrot
{
    public struct ResultDivergenceCalculation(double module, double divergence)
    {
        public double module = module;
        public double divergence = divergence;
    }
    public struct MandelBrotHorizontalLine(double x_start, double y, double x_end, double divergence)
    {
        public double x_start = x_start;
        public double y = y;
        public double x_end = x_end;
        public double divergence = divergence;
    }

    internal class MandelBrotSet
    {
        public List<MandelBrotHorizontalLine> mandelBrotLines = [];
        public static Point Pixel2Real(Point point, Point UpperLeft, Point BottomRight, Size canvas)
        {
            Point result = new();
            double width = BottomRight.X - UpperLeft.X;
            double height = UpperLeft.Y - BottomRight.Y;

            if (canvas.Height * width / height < canvas.Width)
            {
                result.X = UpperLeft.X + (height / canvas.Height) * (point.X + canvas.Height / 2);
                result.Y = BottomRight.Y + (height / canvas.Height) * (point.Y + canvas.Height / 2);
            }
            else
            {
                result.X = UpperLeft.X + (width / canvas.Width) * (point.X + canvas.Width / 2);
                result.Y = BottomRight.Y + (width / canvas.Width) * (point.Y + canvas.Width / 2);
                //result.Y = BottomRight.Y + (height / canvas.Height) * (point.Y + canvas.Height / 2);
            }
            return result;
        }
        public static Point Real2Pixel(Point point, Point UpperLeft, Point BottomRight, Size canvas)
        {
            Point result = new();
            double width = BottomRight.X - UpperLeft.X;
            double height = UpperLeft.Y - BottomRight.Y;

            if (canvas.Height * width / height < canvas.Width)
            {
                result.X = Math.Round((point.X - UpperLeft.X) * (canvas.Height / height) - canvas.Height / 2);
                result.Y = Math.Round((point.Y - BottomRight.Y) * (canvas.Height / height) - canvas.Height / 2);
            }
            else
            {
                result.X = Math.Round((point.X - UpperLeft.X) * (canvas.Width / width) - canvas.Width / 2);
                result.Y = Math.Round((point.Y - BottomRight.Y) * (canvas.Width / width) - canvas.Width / 2);
                //result.Y = Math.Round((point.Y - BottomRight.Y) * (canvas.Height / height) - canvas.Height / 2);
            }
            return result;
        }
        public ResultDivergenceCalculation DivergenceCalculation(Point point, Point UpperLeft, Point BottomRight, Size canvas, int max_iterations)
        {
            double z_r = 0;
            double z_i = 0;
            double i = 0;
            double module;
            double c_r;
            double c_i;

            Point p = Pixel2Real(point, UpperLeft, BottomRight, canvas);
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
        public ResultDivergenceCalculation DivergenceCalculation1(Point p, int max_iterations)
        {
            double z_r = 0;
            double z_i = 0;
            double i = 0;
            double module;
            double c_r;
            double c_i;

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
        public void FillCollection2(MandelBrot_Navigation navigation, Size canvas, int max_iterations)
        {
            mandelBrotLines.Clear();
            for (double y = canvas.Height / 2; y > -canvas.Height / 2; y--)
            {
                MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(-canvas.Width / 2, y, 0, DivergenceCalculation(new Point(-canvas.Width / 2, y), navigation.UpperLeft, navigation.BottomRight, canvas, max_iterations).divergence);
                for (double x = -canvas.Width / 2; x < canvas.Width / 2; x++)
                {
                    ResultDivergenceCalculation r = DivergenceCalculation(new Point(x, y), navigation.UpperLeft, navigation.BottomRight, canvas, max_iterations);
                    if (r.divergence != tmp.divergence)
                    {
                        tmp.x_end = x;
                        if (tmp.divergence != max_iterations)
                            mandelBrotLines.Add(tmp);
                        tmp = new MandelBrotHorizontalLine(x, y, 0, r.divergence);
                    }
                }
                tmp.x_end = canvas.Width / 2;
                if (tmp.divergence != max_iterations)
                    mandelBrotLines.Add(tmp);
            }
        }
        public void FillCollection(MandelBrot_Navigation navigation, Size canvas, int max_iterations)
        {
            mandelBrotLines.Clear();
            double step_Y = navigation.Height / canvas.Height;
            double step_X = navigation.Width / canvas.Width;
            for (double y = navigation.UpperLeft.Y; y > navigation.BottomRight.Y; y -= step_Y)
            {
                Point p = Real2Pixel(new Point(navigation.UpperLeft.X, y), navigation.UpperLeft, navigation.BottomRight, canvas);
                Debug.WriteLine(p.X + ", " + p.Y);
                if (p.Y == 116)
                {
                    int a = 0;
                }
                if (p.Y < -canvas.Height / 2 || p.Y > canvas.Height / 2) continue;
                MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(p.X, p.Y, 0, DivergenceCalculation1(p, max_iterations).divergence);
                for (double x = navigation.UpperLeft.X; x < navigation.BottomRight.X; x += step_X)
                {
                    Point p1 = Real2Pixel(new Point(x, y), navigation.UpperLeft, navigation.BottomRight, canvas);
                    if (p1.X < -canvas.Width / 2 || p1.X > canvas.Width / 2)
                    {
                        Debug.WriteLine("Non");
                        continue;
                    }
                    ResultDivergenceCalculation r = DivergenceCalculation(p1, navigation.UpperLeft, navigation.BottomRight, canvas, max_iterations);
                    if (r.divergence != tmp.divergence)
                    {
                        tmp.x_end = p1.X;
                        if (tmp.divergence != max_iterations)
                            mandelBrotLines.Add(tmp);
                        tmp = new MandelBrotHorizontalLine(p1.X, p1.Y, 0, r.divergence);
                    }
                }
                tmp.x_end = canvas.Width / 2;
                if (tmp.divergence != max_iterations)
                    mandelBrotLines.Add(tmp);
            }
        }
    }
}
