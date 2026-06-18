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
    /// TCPIP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TCPIP : Window
    {
        public TCPIP()
        {
            InitializeComponent();
        }

        // 창 드래그 이동
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                DragMove();
        }

        // 닫기
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Serial Connect
        private void BtnSerialConnect_Click(object sender, RoutedEventArgs e)
        {
            string? portNumber = (cmbPortNumber.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
            string? baudRate = (cmbBaudRate.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
            MessageBox.Show($"Serial 연결 시도: {portNumber} @ {baudRate}", "Serial Connect");
        }

        // Serial Disconnect
        private void BtnSerialDisconnect_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Serial 연결 해제", "Serial Disconnect");
        }

        // TCP/IP Connect
        private void BtnTcpConnect_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"TCP/IP 연결 시도: {txtIP.Text}:{txtPort.Text}", "TCP/IP Connect");
        }

        // TCP/IP Disconnect
        private void BtnTcpDisconnect_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TCP/IP 연결 해제", "TCP/IP Disconnect");
        }
    }
}
