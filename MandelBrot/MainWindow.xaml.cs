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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        double iteration_max = 80;
        MandelBrot_Navigation navigation = new();
        //Point coinSuperieurGaucheInitial = new(-2, 1.2);
        //Point coinInferieurDroitInitial = new(0.5, -1.2);
        //Point coinSuperieurGauche = new(-2, 1.2);
        //Point coinInferieurDroit = new(0.5, -1.2);
        class MandelBrot_Navigation
        {
            Point coinSuperieurGaucheInitial = new(-2, 1.2);
            Point coinInferieurDroitInitial = new(0.5, -1.2);
            public Point temporaryUpperLeftCorner;
            List<Rect> myCollection = [];
            public int index = 0;
            public MandelBrot_Navigation()
            {
                myCollection.Add(new Rect(coinSuperieurGaucheInitial, coinInferieurDroitInitial));
            }
            public void Add_Rectangle(Point p1, Point p2)
            {
                if (p1 != p2)
                    myCollection.Add(new Rect(p1, p2));
            }
            public void Supp_Rectangle(Point p1, Point p2)
            {
                myCollection.Remove(new Rect(p1, p2));
                if (index == myCollection.Count) index--;
            }
            public Point CoinSuperieurGauche
            {
                get { return new Point(myCollection[index].X, myCollection[index].Y); }
            }
            public Point CoinInferieurDroit
            {
                get
                {
                    Rect r = myCollection[index];
                    return new Point(r.X + r.Width, r.Y + r.Height);
                }
            }
            public int Rewind
            {
                set { index = 0; }
            }
            public int End
            {
                set { index = myCollection.Count - 1; }
            }
            public int Previous
            {
                set { if (index != 0) index--; }
            }
            public int Next
            {
                set { if (index != myCollection.Count - 1) index++; }
            }
            public double Width
            {
                get
                {
                    Rect r = myCollection[index];
                    return r.Width;
                }
            }
            public double Height
            {
                get
                {
                    Rect r = myCollection[index];
                    return r.Height;
                }
            }
        }
        struct ZZZZ(double x_start, double y, double x_end, double divergence)
        {
            public double x_start = x_start;
            public double y = y;
            public double x_end = x_end;
            public double divergence = divergence;
        }
        List<ZZZZ> maCollection = [];
        public MainWindow()
        {
            InitializeComponent();            
            timer.Tick += timer_Tick;
        }

        void DrawPixel(double x, double y, Brush b)
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            Line canvasLine = new Line
            {
                //Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                Stroke = b,
                X1 = x + deltaX,
                Y1 = y + deltaY,
                X2 = x + deltaX + 1,
                Y2 = y + deltaY + 1,
                StrokeThickness = 1
            };
            _ = myCanvas.Children.Add(canvasLine);
        }
        void DrawLine(Point p1, Point p2, Brush b)
        {
            double deltaX = myCanvas.ActualWidth / 2;
            double deltaY = myCanvas.ActualHeight / 2;
            Line canvasLine = new Line
            {
                Stroke = b,
                X1 = p1.X + deltaX,
                Y1 = p1.Y + deltaY,
                X2 = p2.X + deltaX,
                Y2 = p2.Y + deltaY,
                StrokeThickness = 1
            };
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
        private void myCanvas_Loaded(object sender, RoutedEventArgs e)
        {

        }
        int WriteOnCanvas(double x, double y, string text)
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
        struct Result_Calcul_Divergence(double module, double divergence)
        {
            public double module = module;
            public double divergence = divergence;
        }
        Result_Calcul_Divergence Calcul_Divergence(double x_pix, double y_pix, double x_min, double y_max, double largeur, double hauteur)
        {
            double c_r = 0;
            double c_i = 0;
            double z_r = 0;
            double z_i = 0;
            double i = 0;
            double module;

            if (largeur / hauteur >= myCanvas.ActualWidth / myCanvas.ActualHeight)
            {
                c_r = x_min + (largeur / myCanvas.ActualWidth) * x_pix + largeur / 2;
                c_i = y_max + (hauteur / myCanvas.ActualWidth) * y_pix - hauteur / 2;
            }
            else
            {
                c_r = x_min + (largeur / myCanvas.ActualHeight) * x_pix + largeur / 2;
                c_i = y_max + (hauteur / myCanvas.ActualHeight) * y_pix - hauteur / 2;
            }

            do
            {
                double tmp = z_r;
                z_r = z_r * z_r - z_i * z_i + c_r;
                z_i = 2 * z_i * tmp + c_i;
                i++;
                module = z_r * z_r + z_i * z_i;
            }
            while (module < 4 && i < iteration_max);

            return new Result_Calcul_Divergence(module, i);
        }
        //void Render1()
        //{
        //    double largeur = coinInferieurDroit.X - navigation.CoinSuperieurGauche.X;
        //    double hauteur = navigation.CoinSuperieurGauche.Y - coinInferieurDroit.Y;
        //    myCanvas.Children.Clear();
        //    DrawXAxis();
        //    DrawYAxis();
        //    for (double x = -myCanvas.ActualWidth / 2; x < myCanvas.ActualWidth / 2; x++)
        //        for (double y = -myCanvas.ActualHeight / 2; y < myCanvas.ActualHeight / 2; y++)
        //        {
        //            Result_Calcul_Divergence r = Calcul_Divergence(x, y, navigation.CoinSuperieurGauche.X, navigation.CoinSuperieurGauche.Y, largeur, hauteur);
        //            DrawPixel(x, y, new SolidColorBrush(Color.FromRgb(0, 0, (byte)(100 + (byte)(r.divergence - 1) * (255 - 100) / (iteration_max - 1)))));
        //        }
        //}
        void Render()
        {
            myCanvas.Children.Clear();
            this.Title = "Render in progress...";
            foreach (ZZZZ z in maCollection)
                DrawLine(new Point(z.x_start, z.y), new Point(z.x_end, z.y), new SolidColorBrush(Color.FromRgb(0, 0, (byte)(100 + (byte)(z.divergence - 1) * (255 - 100) / (iteration_max - 1)))));

            this.Title = "Render done";
        }
        /// <summary>
        ///  Si la divergence est au max, on pourrait ne rien mettre et laisser le background color...
        /// </summary>
        void FillCollection()
        {
            double width = navigation.Width;
            double height = navigation.Height;
            maCollection.Clear();
            this.Title = "Calculation in progress...";
            for (double y = myCanvas.ActualHeight / 2; y > -myCanvas.ActualHeight / 2; y--)
            {
                ZZZZ tmp = new ZZZZ(-myCanvas.ActualWidth / 2, y, 0, Calcul_Divergence(-myCanvas.ActualWidth / 2, y, navigation.CoinSuperieurGauche.X, navigation.CoinSuperieurGauche.Y, width, height).divergence);

                for (double x = -myCanvas.ActualWidth / 2; x < myCanvas.ActualWidth / 2; x++)
                {
                    Result_Calcul_Divergence r = Calcul_Divergence(x, y, navigation.CoinSuperieurGauche.X, navigation.CoinSuperieurGauche.Y, width, height);
                    if (r.divergence != tmp.divergence)
                    {
                        tmp.x_end = x - 1;
                        if (tmp.divergence != iteration_max)
                            maCollection.Add(tmp);
                        tmp = new ZZZZ(x, y, 0, r.divergence);
                    }
                }
                tmp.x_end = -1 + myCanvas.ActualWidth / 2;
                if (tmp.divergence != iteration_max)
                    maCollection.Add(tmp);
            }
            this.Title = "Calculation done";
        }
        private void myCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Title = "Resizing ...";
            timer.IsEnabled = true;
            timer.Stop();
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            double width = navigation.Width;
            double height = navigation.Height;

            timer.IsEnabled = false;
            //Resize ended (based on 500 ms debaounce time
            this.Title = "Resize done.";
            FillCollection();
            Render();
            WriteOnCanvas(2, 2, Math.Round(myCanvas.ActualWidth).ToString() + " x " + Math.Round(myCanvas.ActualHeight).ToString() + " {" + (myCanvas.ActualWidth / myCanvas.ActualHeight).ToString("#.###") + "}");

            Label_ReelDef.Content = width.ToString("E3") + " x " + height.ToString("E3") + " {" + (width / height).ToString("#.###") + "}";
            Label_Count.Content = maCollection.Count.ToString();
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
            double module = Calcul_Divergence(x_pix, y_pix, navigation.CoinSuperieurGauche.X, navigation.CoinSuperieurGauche.Y, width, height).module;
            double iter = Calcul_Divergence(x_pix, y_pix, navigation.CoinSuperieurGauche.X, navigation.CoinSuperieurGauche.Y, width, height).divergence;
            double x_reel, y_reel;

            if (width / height >= myCanvas.ActualWidth / myCanvas.ActualHeight)
            {
                x_reel = navigation.CoinSuperieurGauche.X + (width / myCanvas.ActualWidth) * x_pix + width / 2;
                y_reel = navigation.CoinSuperieurGauche.Y + (height / myCanvas.ActualWidth) * y_pix - height / 2;
            }
            else
            {
                x_reel = navigation.CoinSuperieurGauche.X + (width / myCanvas.ActualHeight) * x_pix + width / 2;
                y_reel = navigation.CoinSuperieurGauche.Y + (height / myCanvas.ActualHeight) * y_pix - height / 2;
            }
            Label_PixelsXY.Content = "x = " + Math.Round(x_pix).ToString() + " ; y = " + Math.Round(y_pix).ToString();
            Label_ReelXY.Content = "x = " + x_reel.ToString("E") + " ; y = " + y_reel.ToString("E");
            Label_Divergence.Content = " div = " + iter.ToString() + " mod = " + module.ToString();
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            Point p = Pixel2Real(position);

            navigation.temporaryUpperLeftCorner= new Point(p.X, p.Y);   
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
                p.X = navigation.CoinSuperieurGauche.X + (width / myCanvas.ActualWidth) * x_pix + width / 2;
                p.Y = navigation.CoinSuperieurGauche.Y + (height / myCanvas.ActualWidth) * y_pix - height / 2;
            }
            else
            {
                p.X = navigation.CoinSuperieurGauche.X + (width / myCanvas.ActualHeight) * x_pix + width / 2;
                p.Y = navigation.CoinSuperieurGauche.Y + (height / myCanvas.ActualHeight) * y_pix - height / 2;
            }
            return p;
        }

        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            Point p = Pixel2Real(position);

            navigation.Add_Rectangle(navigation.temporaryUpperLeftCorner, p);
            double width = navigation.Width;
            double height = navigation.Height;
            FillCollection();
            Render();
            Label_ReelDef.Content = width.ToString("E3") + " x " + height.ToString("E3") + " {" + (width / height).ToString("#.###") + "}";
            Label_Count.Content = maCollection.Count.ToString();
        }
    }
}