using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MandelBrot
{
    struct ColoredPoint(int index, byte blue, byte green, byte red)
    {
        public int index = index;
        public byte Blue = blue;
        public byte Red = red;
        public byte Green = green;
    }
    struct Size
    { 
        public int Width ; 
        public int Height;
        public Size(int width, int height)
        {
            Width = width;  
            Height = height;
        }
        public Size(double width, double height)
        {
            Width = (int)width;
            Height = (int)height;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timer = new()
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };
        readonly MandelbrotColors mandelBrotColors;
        readonly MandelBrotSet mandelBrotSet;
        readonly MandelBrot_Navigation navigation = new();
        private byte[] buffer = [];
        private WriteableBitmap bitmap;
        readonly List<ColoredPoint> points = [];
        public MainWindow()
        {
            InitializeComponent();
            mandelBrotSet = new MandelBrotSet();
            mandelBrotColors = new MandelbrotColors((int)Slider_Divergence.Value, ColorMethod.Random, slider_intensity.Value);
            mandelBrotColors.Random();
            timer.Tick += Timer_Tick;
            Label_DivMax.Content = (int)Slider_Divergence.Value;
            foreach (string name in Enum.GetNames(typeof(ColorMethod)))
                ComboBox_ColorPicker.Items.Add(name);
            ComboBox_ColorPicker.SelectedIndex = 0;
            bitmap = new WriteableBitmap(10, 10, 96, 96, PixelFormats.Bgr24, null);
            Debug.WriteLine("Processor : "+Environment.ProcessorCount);
        }
        #region Utils
        ColoredPoint DrawPointOnBuffer(Size s, PixelPosition p, Brush b)
        {
            // Converting Brush to Color
            Color myColorFromBrush = ((SolidColorBrush)b).Color;
            //Debug.WriteLine(s.ToString()+"   DrawHorizontalLineOnBuffer Y=" + Y1);
            int i = 3 * (s.Width * p.Y + p.X);
            ColoredPoint cp = new(i, buffer[i], buffer[i + 1], buffer[i + 2]);
            buffer[i] = myColorFromBrush.B;
            buffer[i + 1] = myColorFromBrush.G;
            buffer[i + 2] = myColorFromBrush.R;
            return cp;
        }
        void DrawWhiteRectangle(Size s, PixelPosition upperLeft, PixelPosition bottomRight)
        {
            double width = bottomRight.X - upperLeft.X;
            double height = bottomRight.Y - upperLeft.Y;
            if (width < 0 || height < 0) return;

            points.Clear();
            // Up horizontal
            for (double x = upperLeft.X; x < bottomRight.X; x++)
                points.Add(DrawPointOnBuffer(s, new PixelPosition(x, upperLeft.Y), Brushes.White));
            // Bottom horizontal
            for (double x = upperLeft.X; x < bottomRight.X; x++)
                points.Add(DrawPointOnBuffer(s, new PixelPosition(x, bottomRight.Y), Brushes.White));
            // Left vertical
            for (double y = upperLeft.Y + 1; y < bottomRight.Y; y++)
                points.Add(DrawPointOnBuffer(s, new PixelPosition(upperLeft.X, y), Brushes.White));
            // Right vertical
            for (double y = upperLeft.Y + 1; y < bottomRight.Y; y++)
                points.Add(DrawPointOnBuffer(s, new PixelPosition(bottomRight.X, y), Brushes.White));

            bitmap.WritePixels(new Int32Rect(0, 0, s.Width, s.Height), buffer, 3 * s.Width, 0);
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
        void Render()
        {
            int width = (int)myImage.ActualWidth;
            int height = (int)myImage.ActualHeight;
            Size imageSize = new(width, height);
            // create and start a Stopwatch instance
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.Title = "Render in progress...";
            _ = Enum.TryParse<ColorMethod>(ComboBox_ColorPicker.SelectedItem.ToString(), out ColorMethod colorMethod);
            mandelBrotSet.Calc_Div_Buffer_Top(navigation, imageSize, (int)Slider_Divergence.Value);
            //mandelBrotSet.FillCollection_Pass1(navigation, imageSize, (int)Slider_Divergence.Value);
            mandelBrotColors.ChangeColors(mandelBrotSet.Divergences_amplitude, colorMethod, slider_intensity.Value);
            mandelBrotSet.FillCollection(buffer, mandelBrotColors, imageSize);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, 3 * width, 0);
            myImage.Source = bitmap;
            Label_Count.Content = navigation.index + 1 + "/" + navigation.Count();
            this.Title = "Render done";
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.ElapsedMilliseconds);
            Label_Delay.Content = stopwatch.ElapsedMilliseconds + " ms";
            Label_DivMax.ToolTip = "Min: " + mandelBrotSet.Divergence_min + " ; Max: " + mandelBrotSet.Divergence_max;
        }
        /// <summary>
        ///  Si la divergence est au max, on pourrait ne rien mettre et laisser le background color...
        /// </summary>

        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Title = "Resizing ...";
            timer.IsEnabled = true;
            timer.Stop();
            timer.Start();
            Debug.WriteLine("Size Changed");
            Label_Size.Content = (int)myImage.ActualWidth + "x" + (int)myImage.ActualHeight;
        }
        void Timer_Tick(object? sender, EventArgs e)
        {
            timer.IsEnabled = false;

            int width = (int)myImage.ActualWidth;
            int height = (int)myImage.ActualHeight;
            buffer = new byte[3 * width * height];
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

            Render();
            this.Title = "Resize done.";
        }
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PixelPosition position = new( e.GetPosition(this));
            //Debug.WriteLine(menu.ActualHeight);
            //Debug.WriteLine(toolBarTray_navigation.ActualHeight);
            //Debug.WriteLine(Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight));
            position.Y -= (int)(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            if (position.Y == canvas.Height)
                return;
            double module = MandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, (int)Slider_Divergence.Value).module;
            double iter = MandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, (int)Slider_Divergence.Value).divergence;

            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            navigation.temporaryBottomRight = new Point(p.X, p.Y);
            if (navigation.status == Status.Capture)
            {
                PixelPosition upperLeft = MandelBrotSet.Real2Pixel(navigation.temporaryTopLeft, navigation.CurrentSelection, canvas);
                Debug.WriteLine(upperLeft.X + ", " + upperLeft.Y + "  --->   " + position.X + ", " + position.Y);
                UndrawWhiteRectangle();
                DrawWhiteRectangle(canvas, upperLeft, position);
            }
            Label_PixelsXY.Content = (position.X).ToString() + " ; " + (position.Y).ToString();
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

        private void MyImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Label_PixelsXY.Content = "out";
            Label_ReelXY.Content = "out";
            Label_Divergence.Content = "out";
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PixelPosition position = new(e.GetPosition(this));
            position.Y -= (int)(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            navigation.temporaryTopLeft = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);
            navigation.status = Status.Capture;
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PixelPosition position = new(e.GetPosition(this));
            position.Y -= (int)(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myImage.ActualWidth, myImage.ActualHeight);
            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            navigation.status = Status.None;
            if (navigation.temporaryTopLeft.Equals(p))
                return;

            navigation.Add_Selection(navigation.temporaryTopLeft, p);
            navigation.index++;
            points.Clear();
            Render();
        }
        #region navigation
        private void Button_Rewind_Click(object sender, RoutedEventArgs e)
        {
            if (navigation.Rewind())
                Render();
        }

        private void Button_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (navigation.Previous())
                Render();
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (navigation.Next())
                Render();
        }

        private void Button_End_Click(object sender, RoutedEventArgs e)
        {
            if (navigation.End())
                Render();
        }

        private void Slider_Divergence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Label_DivMax != null)
            {
                Label_DivMax.Content = (int)Slider_Divergence.Value;
                Render();
            }
        }

        private void MyImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
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
            Render();
        }

        private void MyImage_KeyDown(object sender, KeyEventArgs e)
        {
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
            Render();
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            navigation.Reset_CurrentSelection();
            Render();
        }

        private void Button_Suppr_Click(object sender, RoutedEventArgs e)
        {
            navigation.Suppr_Selection();
            Render();
        }
        private void ComboBox_ColorPicker_Selected(object sender, RoutedEventArgs e)
        {
            Size myImageSize = new(myImage.ActualWidth, myImage.ActualHeight);
            if (myImageSize.Width == 0 || myImageSize.Height == 0) return;
            _ = Enum.TryParse<ColorMethod>(ComboBox_ColorPicker.SelectedItem.ToString(), out ColorMethod colorMethod);
            mandelBrotColors.Picker(colorMethod, slider_intensity.Value);
            Render();
        }

        private void Slider_intensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (myImage == null) return;
            Size myImageSize = new(myImage.ActualWidth, myImage.ActualHeight);
            if (myImageSize.Width == 0 || myImageSize.Height == 0) return;
            _ = Enum.TryParse<ColorMethod>(ComboBox_ColorPicker.SelectedItem.ToString(), out ColorMethod colorMethod);
            mandelBrotColors.Picker(colorMethod, slider_intensity.Value);
            Render();
        }
        #endregion
        #region Menu
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window_Parameters window_Parameter = new((int)Slider_Divergence.Minimum, (int)Slider_Divergence.Maximum);
            if (window_Parameter.ShowDialog() == false) return;
            Slider_Divergence.Minimum= window_Parameter.GetMinIterations();
            Slider_Divergence.Maximum= window_Parameter.GetMaxIterations();
        }
        #endregion
    }
}