using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        Canvas myCanvas;
        int max_iterations = 80;

        public List<MandelBrotHorizontalLine> mandelBrotLines = [];
        public MandelBrotSet(Canvas myCanvas, int max_iterations)
        {
            this.myCanvas = myCanvas;
            this.max_iterations = max_iterations;
        }
        public ResultDivergenceCalculation DivergenceCalculation(double x_pix, double y_pix, double x_min, double y_min, double largeur, double hauteur)
        {
            double z_r = 0;
            double z_i = 0;
            double i = 0;
            double module;
            double c_r;
            double c_i;

            //if (largeur / hauteur >= myCanvas.ActualWidth / myCanvas.ActualHeight)
            //{
            //    c_r = x_min + (largeur / myCanvas.ActualWidth) * x_pix + largeur / 2;
            //    c_i = y_max + (hauteur / myCanvas.ActualWidth) * y_pix - hauteur / 2;
            //}
            //else
            //{
            //    c_r = x_min + (largeur / myCanvas.ActualHeight) * x_pix + largeur / 2;
            //    c_i = y_max + (hauteur / myCanvas.ActualHeight) * y_pix - hauteur / 2;
            //}

            if (myCanvas.ActualHeight*largeur/hauteur <myCanvas.ActualWidth)
            {
                c_r = x_min + (hauteur / myCanvas.ActualHeight) * (x_pix + myCanvas.ActualHeight / 2);
                c_i = y_min + (hauteur / myCanvas.ActualHeight) * (y_pix + myCanvas.ActualHeight / 2);
            }
            else
            {
                c_r = x_min + (largeur / myCanvas.ActualWidth) * (x_pix + myCanvas.ActualWidth / 2);
                c_i = y_min + (hauteur / myCanvas.ActualWidth) * (y_pix + myCanvas.ActualWidth / 2);
            }

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
        public void FillCollection(MandelBrot_Navigation navigation)
        {
            double width = navigation.Width;
            double height = navigation.Height;
            mandelBrotLines.Clear();
            for (double y = myCanvas.ActualHeight / 2; y > -myCanvas.ActualHeight / 2; y--)
            {
                MandelBrotHorizontalLine tmp = new MandelBrotHorizontalLine(-myCanvas.ActualWidth / 2, y, 0, DivergenceCalculation(-myCanvas.ActualWidth / 2, y, navigation.TopLeftCorner.X, navigation.BottomRightCorner.Y, width, height).divergence);

                for (double x = -myCanvas.ActualWidth / 2; x < myCanvas.ActualWidth / 2; x++)
                {
                    ResultDivergenceCalculation r = DivergenceCalculation(x, y, navigation.TopLeftCorner.X, navigation.BottomRightCorner.Y, width, height);
                    if (r.divergence != tmp.divergence)
                    {
                        tmp.x_end = x - 1;
                        if (tmp.divergence != max_iterations)
                            mandelBrotLines.Add(tmp);
                        tmp = new MandelBrotHorizontalLine(x, y, 0, r.divergence);
                    }
                }
                tmp.x_end = -1 + myCanvas.ActualWidth / 2;
                if (tmp.divergence != max_iterations)
                    mandelBrotLines.Add(tmp);
            }
        }
    }
}
