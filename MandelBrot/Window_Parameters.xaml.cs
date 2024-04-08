using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MandelBrot
{
    /// <summary>
    /// Logique d'interaction pour Window_Parameters.xaml
    /// </summary>
    public partial class Window_Parameters : Window
    {
        private int minIterations = 20;
        private int maxIterations = 500;
        private bool flag_MinIterations;
        private bool flag_MaxIterations;
        public int GetMinIterations()
        { return minIterations; }
        public int GetMaxIterations()
        { return maxIterations; }
        public Window_Parameters(int minIterations, int maxIterations)
        {
            InitializeComponent();
            this.minIterations = minIterations;
            this.maxIterations = maxIterations;
            TextBox_MinIterations.Text = minIterations.ToString();
            TextBox_MaxIterations.Text = maxIterations.ToString();
        }

        private void TextBox_MinIterations_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextBox_MinIterations.Text.Replace('.', ','), out minIterations))
            {
                TextBox_MinIterations.Background = Brushes.White;
                flag_MinIterations = true;
                return;
            }
            TextBox_MinIterations.Background = Brushes.Red;
            flag_MinIterations = false;
        }

        private void TextBox_MaxIterations_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TextBox_MaxIterations.Text.Replace('.', ','), out maxIterations))
            {
                TextBox_MaxIterations.Background = Brushes.White;
                flag_MaxIterations = true;
                return;
            }
            TextBox_MaxIterations.Background = Brushes.Red;
            flag_MaxIterations = false;
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if ((flag_MinIterations && flag_MaxIterations) == false)
                return;
            DialogResult = true;
            Hide();
        }
    }
}
