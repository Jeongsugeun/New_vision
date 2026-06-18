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
using System.ComponentModel;

namespace Vision_Login
{
    /// <summary>
    /// IO.xaml에 대한 상호 작용 논리
    /// </summary>
    public class IOItem : INotifyPropertyChanged
    {
        public string No { get; set; } = "";
        public string Name { get; set; } = "";

        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    OnPropertyChanged(nameof(IsOn));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class IO : Window
    {
        public IO()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => this.Close();

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }

        private void IOStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IOItem item)
            {
                item.IsOn = !item.IsOn;
            }
        }
    }
}
