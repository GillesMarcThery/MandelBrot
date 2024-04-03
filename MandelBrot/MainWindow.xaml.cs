﻿using System.Diagnostics;
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
        public MainWindow()
        {
            InitializeComponent();
            mandelBrotSet = new MandelBrotSet();
            mandelBrotColors = new MandelbrotColors(80);
            myImage.Source = bitmap;
            timer.Tick += timer_Tick;
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
        void DrawHorizontalLineOnBuffer(byte[] buffer, Size s, Point p1, Point p2, Brush b)
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
        int DrawWhiteRectangle(Point upperLeft, Point bottomRight)
        {
            //double width = bottomRight.X - upperLeft.X;
            //double height = upperLeft.Y - bottomRight.Y;
            //if (width < 0 || height < 0) return 0;

            //double deltaX = myCanvas.ActualWidth / 2;
            //double deltaY = myCanvas.ActualHeight / 2;
            //System.Windows.Shapes.Rectangle rect = new()
            //{
            //    Stroke = Brushes.White,
            //    Width = bottomRight.X - upperLeft.X,
            //    Height = upperLeft.Y - bottomRight.Y,
            //    StrokeThickness = 1
            //};
            ////Debug.WriteLine(canvasLine.X1 + ", " + canvasLine.Y1 + "-->" + canvasLine.X2 + ", " + canvasLine.Y2);
            //Canvas.SetLeft(rect, upperLeft.X + deltaX);
            //Canvas.SetTop(rect, deltaY - upperLeft.Y);
            //return myCanvas.Children.Add(rect);
            return 1;
        }
        #endregion
        void Render1()
        {
            int width = (int)myDock.ActualWidth;
            int height = (int)myDock.ActualHeight;
            buffer = new byte[3 * width * height];
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

            this.Title = "Render in progress...";
            foreach (MandelBrotHorizontalLine z in mandelBrotSet.mandelBrotLines)
            {
                DrawHorizontalLineOnBuffer(buffer, new Size(Math.Round(myDock.ActualWidth), Math.Round(myDock.ActualHeight)), new Point(z.x_start, z.y), new Point(z.x_end, z.y), mandelBrotColors.colors[(int)z.divergence]);
            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, 3 * width, 0);
            myImage.Source = bitmap;
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
        }
        void timer_Tick(object? sender, EventArgs e)
        {
            timer.IsEnabled = false;

            int width = (int)myDock.ActualWidth;
            int height = (int)myDock.ActualHeight;

            //Resize ended (based on 500 ms debaounce time
            this.Title = "Resize done.";
            mandelBrotSet.FillCollection(navigation, new Size(width, height), 80);
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
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            double module = mandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, 80).module;
            double iter = mandelBrotSet.DivergenceCalculation(position, navigation.CurrentSelection, canvas, 80).divergence;

            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            Label_PixelsXY.Content =  Math.Round(position.X).ToString() + " ; " + Math.Round(position.Y).ToString();
            Label_ReelXY.Content = "r = " + p.X.ToString("E") + " ; i = " + p.Y.ToString("E");
            Label_Divergence.Content = " div = " + iter.ToString() + " mod = " + module.ToString();

            navigation.temporaryBottomRight = new Point(p.X, p.Y);
            if (navigation.status == Status.Capture)
            {
                Point upperLeft = MandelBrotSet.Real2Pixel(navigation.temporaryTopLeft, navigation.CurrentSelection, canvas);
                Debug.WriteLine(upperLeft.X + ", " + upperLeft.Y + "  --->   " + position.X + ", " + position.Y);
                //if (id_Selection_Rectangle != 0) myCanvas.Children.RemoveAt(id_Selection_Rectangle);
                //id_Selection_Rectangle = DrawWhiteRectangle(upperLeft, pointOnCanvas);
            }
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            position.Y -= Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            navigation.temporaryTopLeft = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);
            navigation.status = Status.Capture;
        }

        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            position.Y -= Math.Round(menu.ActualHeight + toolBarTray_navigation.ActualHeight);
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            Point p = MandelBrotSet.Pixel2Real(position, navigation.CurrentSelection, canvas);

            navigation.status = Status.None;
            id_Selection_Rectangle = 0;
            navigation.Add_Rectangle(navigation.temporaryTopLeft, p);
            navigation.index++;
            mandelBrotSet.FillCollection(navigation, canvas, 80);
            Render1();
        }
        #region navigation
        private void Button_Rewind_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            if (navigation.Rewind())
            {
                mandelBrotSet.FillCollection(navigation, canvas, 80);
                Render1();
            }
        }

        private void Button_Previous_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            if (navigation.Previous())
            {
                mandelBrotSet.FillCollection(navigation, canvas, 80);
                Render1();
            }
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            if (navigation.Next())
            {
                mandelBrotSet.FillCollection(navigation, canvas, 80);
                Render1();
            }
        }

        private void Button_End_Click(object sender, RoutedEventArgs e)
        {
            Size canvas = new(myDock.ActualWidth, myDock.ActualHeight);
            if (navigation.End())
            {
                mandelBrotSet.FillCollection(navigation, canvas, 80);
                Render1();
            }
        }
    }
    #endregion
}