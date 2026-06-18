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
using Microsoft.Win32;

namespace Vision_Login
{
    /// <summary>
    /// Hardware.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Hardware : Window
    {
        public Hardware()
        {
            InitializeComponent();

            Cb_Grabber.SelectedIndex = 0;
        }

        private void Cb_Grabber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // null 체크 추가
            if (Grid_CamFile == null || Grid_Serial == null) return;

            if (Cb_Grabber.SelectedItem is ComboBoxItem item)
            {
                string grabber = item.Content?.ToString() ?? "";

                if (grabber.Contains("Gigelink"))
                {
                    Grid_CamFile.Visibility = Visibility.Collapsed;
                    Grid_Serial.Visibility = Visibility.Visible;
                }
                else
                {
                    Grid_CamFile.Visibility = Visibility.Visible;
                    Grid_Serial.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCamFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Camfile select",
                Filter = "모든 파일 (*.*)|*.*",
                InitialDirectory = @"C:\vwork"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtCamFile.Text = dialog.FileName;
            }
        }

        private void BtnSizeXUp_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSizeX.Text, out int val))
                TxtSizeX.Text = (val + 1).ToString();
        }

        private void BtnSizeXDown_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSizeX.Text, out int val) && val > 0)
                TxtSizeX.Text = (val - 1).ToString();
        }

        private void BtnSizeYUp_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSizeY.Text, out int val))
                TxtSizeY.Text = (val + 1).ToString();
        }

        private void BtnSizeYDown_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSizeY.Text, out int val) && val > 0)
                TxtSizeY.Text = (val - 1).ToString();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            // Apply 로직 작성
        }
    }
}
