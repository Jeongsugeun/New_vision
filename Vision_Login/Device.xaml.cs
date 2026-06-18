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
    public class DeviceItem
    {
        public int Num { get; set; }
        public string Name { get; set; } = "";
    }

    public partial class Device : Window
    {
        public Device()
        {
            InitializeComponent();
            LoadDeviceList();
        }

        private void LoadDeviceList()
        {
            var items = new List<DeviceItem>();
            for (int i = 1; i <= 100; i++)
                items.Add(new DeviceItem { Num = i, Name = $"TEST{i}" });

            lvDevices.ItemsSource = items;
        }

        // 더블클릭 시 Number, Name 출력
        private void lvDevices_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvDevices.SelectedItem is DeviceItem item)
            {
                txtNumber.Text = item.Num.ToString("D3");
                txtName.Text = item.Name;
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;

            var confirm = new ConfirmWindow(
                "Load",
                "디바이스를 불러오시겠습니까?",
                "",
                "\uED25",    // Load 아이콘
                "#4A9BB8"    // 청회색
            );
            confirm.Owner = this;
            confirm.ShowDialog();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;

            var confirm = new ConfirmWindow(
                "Save",
                "디바이스를 저장하시겠습니까?",
                "",
                "\uE74E",    // Save 아이콘
                "#64A18F"    // 청록색
            );
            confirm.Owner = this;
            confirm.ShowDialog();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => this.Close();

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }

        private void lvDevices_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = lvDevices.ActualWidth - 26; // 스크롤바 + 여백
            if (totalWidth <= 0) return;

            colNum.Width = totalWidth * 0.1;
            colName.Width = totalWidth * 0.9;
        }

        private void lvDevices_Loaded(object sender, RoutedEventArgs e)
        {
            double totalWidth = lvDevices.ActualWidth - 26;
            if (totalWidth <= 0) return;

            colNum.Width = totalWidth * 0.1;
            colName.Width = totalWidth * 0.9;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
