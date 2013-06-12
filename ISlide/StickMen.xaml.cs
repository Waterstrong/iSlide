using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ISlide
{
    /// <summary>
    /// StickMen.xaml 的交互逻辑
    /// </summary>
    public partial class StickMen : Window
    {
        public StickMen()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    this.Hide();
                    break;
                default:break;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private const double offset = 30;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Left = screenWidth - this.Width - offset;
            this.Top = screenHeight - this.Height - offset;
        }
    }
}
