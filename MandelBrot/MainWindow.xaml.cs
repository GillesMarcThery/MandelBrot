using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MandelBrot.MandelBrotSet;

namespace MandelBrot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };
        MandelbrotColors mandelBrotColors;
        MandelBrotSet mandelBrotSet;
        MandelBrot_Navigation navigation = new();

        public MainWindow()
        {
            InitializeComponent();
            mandelBrotSet = new MandelBrotSet(myCanvas, 80);
            mandelBrotColors = new MandelbrotColors(80);
            timer.Tick += timer_Tick;
        }
        #region Utils
        void DrawLine(Point p1, Point p2, Brush b)
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            Line canvasLine = new Line
            {
                Stroke = b,
                X1 = p1.X + deltaX,
                Y1 = deltaY - p1.Y,
                X2 = p2.X + deltaX,
                Y2 = deltaY - p2.Y,
                StrokeThickness = 1
            };
            //Debug.WriteLine(canvasLine.X1 + ", " + canvasLine.Y1 + "-->" + canvasLine.X2 + ", " + canvasLine.Y2);
            _ = myCanvas.Children.Add(canvasLine);
        }
        void DrawXAxis()
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            Line canvasLine = new Line
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                X1 = 0,
                Y1 = deltaY,
                X2 = myCanvas.ActualWidth,
                Y2 = deltaY,
                StrokeThickness = 1
            };
            _ = myCanvas.Children.Add(canvasLine);
        }
        void DrawYAxis()
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            Line canvasLine = new Line
            {
                //Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                Stroke = Brushes.Chocolate,
                X1 = deltaX,
                Y1 = 0,
                X2 = deltaX,
                Y2 = myCanvas.ActualHeight,
                StrokeThickness = 1
            };
            _ = myCanvas.Children.Add(canvasLine);
        }
        int WriteStringOnCanvas(double x, double y, string text)
        {
            TextBlock textBlock = new TextBlock
            {
                FontSize = 12,
                Text = text,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            return myCanvas.Children.Add(textBlock);
        }
        #endregion
        void Render()
        {
            myCanvas.Children.Clear();
            this.Title = "Render in progress...";
            foreach (MandelBrotHorizontalLine z in mandelBrotSet.mandelBrotLines)
            {
                //Byte redLevel = (byte)(100 + (byte)(z.divergence - 1) * (255 - 100) / (iteration_max - 1));
                //Byte redLevel = (Byte)(255 - (byte)((z.divergence - 1) * 255 / (iteration_max - 1)));
                //DrawLine(new Point(z.x_start, z.y), new Point(z.x_end, z.y), new SolidColorBrush(Color.FromRgb(0, 0, redLevel)));
                DrawLine(new Point(z.x_start, z.y), new Point(z.x_end, z.y), mandelBrotColors.colors[(int)z.divergence]);
                //Debug.WriteLine(z.divergence);
                //Debug.WriteLine(z.x_start + ", " + z.y + "-->" + z.x_end + ", " + z.y);
            }
            WriteStringOnCanvas(2, 2, Math.Round(myCanvas.ActualWidth).ToString() + " x " + Math.Round(myCanvas.ActualHeight).ToString() + " {" + (myCanvas.ActualWidth / myCanvas.ActualHeight).ToString("#.###") + "}");
            WriteStringOnCanvas(2, 14, "p1 (" + navigation.TopLeftCorner.ToString() + ")");
            WriteStringOnCanvas(2, 26, "p2 (" + navigation.BottomRightCorner.ToString() + ")");
            WriteStringOnCanvas(2, 38, navigation.Width.ToString("E3") + " x " + navigation.Height.ToString("E3") + " {" + (navigation.Width / navigation.Height).ToString("#.###") + "}");
            WriteStringOnCanvas(2, 50, mandelBrotSet.mandelBrotLines.Count.ToString());
            this.Title = "Render done";
        }
        /// <summary>
        ///  Si la divergence est au max, on pourrait ne rien mettre et laisser le background color...
        /// </summary>

        private void myCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Title = "Resizing ...";
            timer.IsEnabled = true;
            timer.Stop();
            timer.Start();
        }
        void timer_Tick(object? sender, EventArgs e)
        {
            double width = navigation.Width;
            double height = navigation.Height;

            timer.IsEnabled = false;
            //Resize ended (based on 500 ms debaounce time
            this.Title = "Resize done.";
            mandelBrotSet.FillCollection(navigation);
            Render();
        }
        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(this);
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            double x_pix = position.X - deltaX;
            double y_pix = deltaY - position.Y;
            double width = navigation.Width;
            double height = navigation.Height;
            double module =mandelBrotSet.DivergenceCalculation(x_pix, y_pix, navigation.TopLeftCorner.X, navigation.BottomRightCorner.Y, width, height).module;
            double iter =mandelBrotSet.DivergenceCalculation(x_pix, y_pix, navigation.TopLeftCorner.X, navigation.BottomRightCorner.Y, width, height).divergence;
            double x_reel, y_reel;

            if (width / height >= myCanvas.ActualWidth / myCanvas.ActualHeight)
            {
                x_reel = navigation.TopLeftCorner.X + (width / myCanvas.ActualWidth) * x_pix + width / 2;
                y_reel = navigation.TopLeftCorner.Y + (height / myCanvas.ActualWidth) * y_pix - height / 2;
            }
            else
            {
                x_reel = navigation.TopLeftCorner.X + (width / myCanvas.ActualHeight) * x_pix + width / 2;
                y_reel = navigation.TopLeftCorner.Y + (height / myCanvas.ActualHeight) * y_pix - height / 2;
            }
            Label_PixelsXY.Content = "x = " + Math.Round(x_pix).ToString() + " ; y = " + Math.Round(y_pix).ToString();
            Label_ReelXY.Content = "x = " + x_reel.ToString("E") + " ; y = " + y_reel.ToString("E");
            Label_Divergence.Content = " div = " + iter.ToString() + " mod = " + module.ToString();
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            Point p = Pixel2Real(position);

            navigation.temporaryUpperLeftCorner = new Point(p.X, p.Y);
        }
        Point Pixel2Real(Point position)
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            double x_pix = position.X - deltaX;
            double y_pix = deltaY - position.Y;
            double width = navigation.Width;
            double height = navigation.Height;
            Point p = new();

            if (width / height >= myCanvas.ActualWidth / myCanvas.ActualHeight)
            {
                p.X = navigation.TopLeftCorner.X + (width / myCanvas.ActualWidth) * x_pix + width / 2;
                p.Y = navigation.TopLeftCorner.Y + (height / myCanvas.ActualWidth) * y_pix - height / 2;
            }
            else
            {
                p.X = navigation.TopLeftCorner.X + (width / myCanvas.ActualHeight) * x_pix + width / 2;
                p.Y = navigation.TopLeftCorner.Y + (height / myCanvas.ActualHeight) * y_pix - height / 2;
            }
            return p;
        }

        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            Point p = Pixel2Real(position);

            navigation.Add_Rectangle(navigation.temporaryUpperLeftCorner, p);
            navigation.index++;
            mandelBrotSet.FillCollection(navigation);
            Render();
        }
    }
}