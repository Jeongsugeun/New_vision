using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Vision_Login
{
    /// <summary>
    /// ConfirmWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string title, string message, string subMessage,
                             string icon = "\uE7E8", string iconColor = "#FF4C4C")
        {
            InitializeComponent();

            // 텍스트 설정
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            TxtSubMessage.Text = subMessage;

            // 아이콘 및 색상 설정
            TxtIcon.Text = icon;
            TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconColor));

            var effect = (DropShadowEffect)TxtIcon.Effect;
            effect.Color = (Color)ColorConverter.ConvertFromString(iconColor);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }
    }
}
