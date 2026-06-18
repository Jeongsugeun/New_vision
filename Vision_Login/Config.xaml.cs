using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Vision_Login
{
    /// <summary>
    /// Config.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnHardware_Click(object sender, RoutedEventArgs e)
        {
            Hardware hard = new Hardware();
            hard.Owner = this.Owner;
            hard.Show();
            this.Close();
        }

        private void btnIO_Click(object sender, RoutedEventArgs e)
        {
            IO io = new IO();
            io.Owner = this.Owner;
            io.Show();
            this.Close();
        }

        private void btnCalibration_Click(object sender, RoutedEventArgs e)
        {
            Calibration calibration = new Calibration();
            calibration.Owner = this.Owner;
            calibration.Show();
            this.Close();
        }
    }
}
