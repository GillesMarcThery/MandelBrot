using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MandelBrot
{
    struct ColoredPoint(int index, byte blue, byte green, byte red)
    {
        public int index = index;
        public byte Blue = blue;
        public byte Red = red;
        public byte Green = green;
    }
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
        int id_Selection_Rectangle;
        private byte[] buffer = new byte[0];
        private WriteableBitmap bitmap;
        List<ColoredPoint> points = [];
        public MainWindow()
        {
            InitializeComponent();
            mandelBrotSet = new MandelBrotSet();
            mandelBrotColors = new MandelbrotColors(500);
            mandelBrotColors.Random();
            timer.Tick += timer_Tick;
            Label_DivMax.Content = (int)Slider_Divergence.Value;
        }
        #region Utils
        /// <summary>
        /// Draw an horizontal line on the buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="width">width of the image</param>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <param name="b">Color</param>
        void DrawHorizontalLineOnBuffer(Size s, Point p1, Point p2, Brush b)
        {
            // Converting Brush to Color
            Color myColorFromBrush = ((SolidColorBrush)b).Color;
            //Debug.WriteLine(s.ToString()+"   DrawHorizontalLineOnBuffer Y=" + Y1);
            for (int i = (int)(3 * (s.Width * p1.Y + p1.X)); i < (int)(3 * (s.Width * p1.Y + p2.X)); i += 3)
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
        ColoredPoint DrawPointOnBuffer(Size s, Point p, Brush b)
        {
            // Converting Brush to Color
            Color myColorFromBrush = ((SolidColorBrush)b).Color;
            //Debug.WriteLine(s.ToString()+"   DrawHorizontalLineOnBuffer Y=" + Y1);
            int i = (int)(3 * (s.Width * p.Y + p.X));
            ColoredPoint cp = new ColoredPoint(i, buffer[i], buffer[i + 1], buffer[i + 2]);
            buffer[i] = myColorFromBrush.B;
            buffer[i + 1] = myColorFromBrush.G;
            buffer[i + 2] = myColorFromBrush.R;
            return cp;
        }

        void DrawWhiteRectangle(Size s, Point upperLeft, Point bottomRight)
        {
            double width = bottomRight.X - upperLeft.X;
            double height = bottomRight.Y - upperLeft.Y;
            if (width < 0 || height < 0) return;

            points.Clear();
            // Up horizontal
            for (double x = upperLeft.X; x < bottomRight.X; x++)
                points.Add(DrawPointOnBuffer(s, new Point(x, upperLeft.Y), Brushes.White));
            // Bottom horizontal
            for (double x = upperLeft.X; x < bottomRight.X; x++)
                points.Add(DrawPointOnBuffer(s, new Point(x, bottomRight.Y), Brushes.White));
            // Left vertical
            for (double y = upperLeft.Y + 1; y < bottomRight.Y; y++)
                points.Add(DrawPointOnBuffer(s, new Point(upperLeft.X, y), Brushes.White));
            // Right vertical
            for (double y = upperLeft.Y + 1; y < bottomRight.Y; y++)
                points.Add(DrawPointOnBuffer(s, new Point(bottomRight.X, y), Brushes.White));

            bitmap.WritePixels(new Int32Rect(0, 0, (int)s.Width, (int)s.Height), buffer, 3 * (int)s.Width, 0);
        }
        void UndrawWhiteRectangle()
        {
            foreach (var point in points)
            {
                buffer[point.index] = point.Blue;
                buffer[point.index + 1] = point.Green;
                buffer[point.index + 2] = point.Red;
            }
        }
        #endregion
        void Render1()
        {
            int width = (int)myImage.ActualWidth;
            int height = (int)myImage.ActualHeight;
            //buffer = new byte[3 * width * height];
            //bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

            this.Title = "Render in progress...";
            //foreach (MandelBrotHorizontalLine z in mandelBrotSet.mandelBrotLines)
            //{
            //    DrawHorizontalLineOnBuffer(new Size(Math.Round(myImage.ActualWidth), Math.Round(myImage.ActualHeight)), new Point(z.x_start, z.y), new Point(z.x_end, z.y), mandelBrotColors.colors[(int)z.divergence]);
            //}
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, 3 * width, 0);
            myImage.Source = bitmap;
            Label_Count.Content = navigation.index + 1 + "/" + navigation.Count();
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
            Debug.WriteLine("Size Changed");
            Label_Size.Content = (int)myImage.ActualWidth + "x" + (int)myImage.ActualHeight;
        }
        void timer_Tick(object? sender, EventArgs e)
        {
            timer.IsEnabled = false;

            int width = (int)myImage.ActualWidth;
            int height = (int)myImage.ActualHeight;
            buffer = new byte[3 * width * height];
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

            if (width == 0) return;

            //Resize ended (based on 500 ms debaounce time
            this.Title = "Resize done.";
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, new Size(width, height), (int)Slider_Divergence.Value);
            //Render();
            Render1();
        }
        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(this);
            //Debug.WriteLine(menu.ActualHeight);
            //Debug.WriteLine(toolBarTray_navigation.ActualHeight);
            //Debug.WriteLine(Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight));
            position.Y -= Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (position.Y == (int)canvas.Height)
                return;
            double module = mandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, (int)Slider_Divergence.Value).module;
            double iter = mandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, (int)Slider_Divergence.Value).divergence;

            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            navigation.temporaryBottomRight = new Point(p.X, p.Y);
            if (navigation.status == Status.Capture)
            {
                Point upperLeft = MandelBrotSet.Real2Pixel(navigation.temporaryTopLeft, navigation.CurrentSelection, canvas);
                Debug.WriteLine(upperLeft.X + ", " + upperLeft.Y + "  --->   " + position.X + ", " + position.Y);
                UndrawWhiteRectangle();
                DrawWhiteRectangle(canvas, upperLeft, position);
                //if (id_Selection_Rectangle != 0) myCanvas.Children.RemoveAt(id_Selection_Rectangle);
                //id_Selection_Rectangle = DrawWhiteRectangle(upperLeft, pointOnCanvas);
            }
            Label_PixelsXY.Content = Math.Round(position.X).ToString() + " ; " + Math.Round(position.Y).ToString();
            if ((position.X >= mandelBrotSet.Top_Left_pix.X && position.X <= mandelBrotSet.bottom_Right_pix.X) && (position.Y >= mandelBrotSet.Top_Left_pix.Y && position.Y <= mandelBrotSet.bottom_Right_pix.Y))
            {
                Label_ReelXY.Content = "r = " + p.X.ToString("E") + " ; i = " + p.Y.ToString("E");
                Label_Divergence.Content = " div = " + iter.ToString() + " mod = " + module.ToString();
            }
            else
            {
                Label_ReelXY.Content = "out";
                Label_Divergence.Content = "out";
            }
        }

        private void myImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_PixelsXY.Content = "out";
            Label_ReelXY.Content = "out";
            Label_Divergence.Content = "out";
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            position.Y -= Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            navigation.temporaryTopLeft = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);
            navigation.status = Status.Capture;
        }

        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            position.Y -= Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            navigation.status = Status.None;
            if (navigation.temporaryTopLeft.Equals(p))
                return;
            id_Selection_Rectangle = 0;
            navigation.Add_Selection(navigation.temporaryTopLeft, p);
            navigation.index++;
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            points.Clear();
            Render1();
        }
        #region navigation
        private void Button_Rewind_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (navigation.Rewind())
            {
                mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
                Render1();
            }
        }

        private void Button_Previous_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (navigation.Previous())
            {
                mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
                Render1();
            }
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (navigation.Next())
            {
                mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
                Render1();
            }
        }

        private void Button_End_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (navigation.End())
            {
                mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
                Render1();
            }
        }

        private void Slider_Divergence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Label_DivMax != null)
            {
                Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
                Label_DivMax.Content = (int)Slider_Divergence.Value;
                mandelBrotColors.MaxIterations = (int)Slider_Divergence.Value;
                mandelBrotColors.Random();
                mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
                //Render();
                Render1();
            }
        }

        private void myImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            double dX = 0.1 * navigation.Width / 2;
            double dY = 0.1 * navigation.Height / 2;
            Rectangle r = navigation.CurrentSelection;
            if (e.Delta > 0)
            {
                r.TopLeft.X += dX;
                r.BottomRight.X -= dX;
                r.TopLeft.Y -= dY;
                r.BottomRight.Y += dY;
            }
            else
            {
                r.TopLeft.X -= dX;
                r.BottomRight.X += dX;
                r.TopLeft.Y += dY;
                r.BottomRight.Y -= dY;
            }
            navigation.Replace_Selection(r);
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            Render1();
        }

        private void myImage_KeyDown(object sender, KeyEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            double dX = 0.1 * navigation.Width / 2;
            double dY = 0.1 * navigation.Height / 2;
            Rectangle r = navigation.CurrentSelection;
            switch (e.Key)
            {
                case Key.Down:
                    r.TopLeft.Y -= dY;
                    r.BottomRight.Y -= dY;
                    break;
                case Key.Up:
                    r.TopLeft.Y += dY;
                    r.BottomRight.Y += dY;
                    break;
                case Key.Left:
                    r.TopLeft.X -= dX;
                    r.BottomRight.X -= dX;
                    break;
                case Key.Right:
                    r.TopLeft.X += dX;
                    r.BottomRight.X += dX;
                    break;
                default:
                    return;
            }
            navigation.Replace_Selection(r);
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            Render1();
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            navigation.Reset_CurrentSelection();
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            Render1();
        }

        private void Button_Suppr_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            navigation.Suppr_Selection();
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            Render1();
        }

        private void checkBox_Blue_Checked(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            mandelBrotColors.ProgressiveBlue();
            mandelBrotSet.FillCollection(buffer, navigation, mandelBrotColors, canvas, (int)Slider_Divergence.Value);
            Render1();
        }
    }
    #endregion
}