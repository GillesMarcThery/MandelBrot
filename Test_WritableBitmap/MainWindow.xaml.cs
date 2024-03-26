using System;
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

namespace Test_WritableBitmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random random = new Random();
        private readonly byte[] buffer = new byte[3 * 500 * 300];
        private readonly WriteableBitmap bitmap = new WriteableBitmap(500, 300, 96, 96, PixelFormats.Bgr24, null);
        public MainWindow()
        {
            InitializeComponent();
            image.Source = bitmap;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 200; i += 3) { buffer[i] = 155; }
            for (var y = 0; y < 300; y++)
            {
                for (var x = 0; x < 500; x++)
                {
                    var i = 3 * (500 * y + x);
                    //buffer[i++] = (byte)random.Next(200);
                    //buffer[i++] = (byte)random.Next(200);
                    //buffer[i++] = (byte)random.Next(200);
                    buffer[i++] = 0;
                    buffer[i++] = 190;
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, 500, 300), buffer, 3 * 500, 0);
        }
    }
}