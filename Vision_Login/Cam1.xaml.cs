using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vision_Login
{
    // ↓ Cam 클래스 밖으로 꺼냄
    public class LogItem1
    {
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public partial class Cam1 : Window
    {
        private bool isAuto = true;
        private Grid? _selectedGrid = null; // 현재 선택된 행
        private ObservableCollection<LogItem1> _logs = new();

        public Cam1()
        {
            InitializeComponent();

            // 처음 시작 시 AUTO → CONFIG, EXIT 비활성화
            BtnConfig.IsEnabled = false;
            BtnConfig.Opacity = 0.4;
            BtnExit.IsEnabled = false;
            BtnExit.Opacity = 0.4;
            BtnDevice.IsHitTestVisible = false;  // 클릭 막기
            cmbUserLevel.IsHitTestVisible = false; // 클릭 막기


            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateNow();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateNow();
        }

        private void UpdateNow()
        {
            TxtCurrentTime.Text = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
        }

        private void Btn_Toggle_Click(object sender, RoutedEventArgs e)
        {
            isAuto = !isAuto;

            if (sender is not Button button) return;

            var icon = (TextBlock)button.Template.FindName("Btn_Icon", button);
            var label = (TextBlock)button.Template.FindName("Btn_Label", button);

            if (isAuto)
            {
                icon.Text = "\uE895";
                label.Text = "AUTO";

                // AUTO → 비활성화
                BtnConfig.IsEnabled = false;
                BtnConfig.Opacity = 0.4;
                BtnExit.IsEnabled = false;
                BtnExit.Opacity = 0.4;
                BtnDevice.IsHitTestVisible = false;  // 클릭 막기
                cmbUserLevel.IsHitTestVisible = false;  // 클릭 막기
            }
            else
            {
                icon.Text = "\uEDA4";
                label.Text = "MANUAL";

                // MANUAL → 활성화
                BtnConfig.IsEnabled = true;
                BtnConfig.Opacity = 1.0;
                BtnExit.IsEnabled = true;
                BtnExit.Opacity = 1.0;
                BtnDevice.IsHitTestVisible = true;   // 클릭 허용
                cmbUserLevel.IsHitTestVisible = true;   // 클릭 허용
            }
        }

        private void RotateAndSetEnabled(Button button, bool enable)
        {
            // Button의 RenderTransform에서 RotateTransform 직접 가져오기
            var rotate = button.RenderTransform as RotateTransform;
            if (rotate == null)
            {
                rotate = new RotateTransform(0);
                button.RenderTransform = rotate;
                button.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var storyboard = new System.Windows.Media.Animation.Storyboard();

            var rotateAnim = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(0.4))
            };

            System.Windows.Media.Animation.Storyboard.SetTarget(rotateAnim, rotate);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(rotateAnim,
                new PropertyPath(RotateTransform.AngleProperty));

            storyboard.Children.Add(rotateAnim);

            storyboard.Completed += (s, e) =>
            {
                button.IsEnabled = enable;
                button.Opacity = enable ? 1.0 : 0.4;
            };

            storyboard.Begin();
        }

        private void LogItem_Click(object sender, MouseButtonEventArgs e)
        {
            // 이전 선택 색 초기화
            if (_selectedGrid != null)
            {
                var prevBorder = (Border)_selectedGrid.Children[0];
                prevBorder.Background = Brushes.Transparent;
            }

            // 새로운 선택 색 적용
            var grid = (Grid)sender;
            var border = (Border)grid.Children[0];
            border.Background = new SolidColorBrush(Color.FromRgb(60, 80, 120)); // 파란색 계열
            _selectedGrid = grid;
        }

        //Log 우클릭시 복사 기능
        private void LogItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;

            // DataContext에서 LogItem 가져오기
            if (grid.DataContext is LogItem item)
            {
                string copyText = $"{item.Time}\t{item.Status}";
                Clipboard.SetText(copyText);
            }
        }

        private void Btn_Config_Click(object sender, RoutedEventArgs e)
        {
            //OK 추가
            //_totalOk++;
            //_totalUnits++;
            //TxtTotalOk.Text = _totalOk.ToString();
            //TxtTotalUnits.Text = _totalUnits.ToString();
            //AddLog($"GOOD (OK: {_totalOk})");
            //UpdateGauge();
        }

        private void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            var confirm = new ConfirmWindow(
                   "SYSTEM EXIT",
                   "프로그램을 종료하시겠습니까?",
                   "저장되지 않은 데이터는 손실될 수 있습니다."
               );
            confirm.Owner = this;

            if (confirm.ShowDialog() == true)
                Application.Current.Shutdown();

            //NG 추가
            //_totalNg++;
            //_totalUnits++;
            //TxtTotalNg.Text = _totalNg.ToString();
            //TxtTotalUnits.Text = _totalUnits.ToString();
            //AddLog($"NG (NG: {_totalNg})");
            //UpdateGauge();
        }

        private Device? _deviceWindow = null;

        private void btnDevice_Click(object sender, RoutedEventArgs e)
        {
            if (isAuto) return;

            if (_deviceWindow != null && _deviceWindow.IsVisible)
            {
                _deviceWindow.Activate();
                return;
            }

            _deviceWindow = new Device();
            _deviceWindow.Owner = this;
            _deviceWindow.Show();
        }

        private void btnVisionPlus_Click(object sender, RoutedEventArgs e)
        {
            if (isAuto) return;

            Name name = new Name();
            name.Owner = this;
            name.ShowDialog();
        }

        private void BtnIPPort_Click(object sender, RoutedEventArgs e)
        {
            if (isAuto) return;

            TCPIP Tcpip = new TCPIP();
            Tcpip.Owner = this;
            Tcpip.ShowDialog();
        }

        private void btnCam1Settings_Click(object sender, RoutedEventArgs e)
        {
            Cam cam = new Cam();
            cam.Owner = this.Owner;

            cam.ContentRendered += (s, args) =>
            {
                this.Hide();
            };

            cam.Show();
        }

        private void LiveButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!_bLiveMode)
            //{
            //    _bLiveMode = true;

            //    // 조명 켜기
            //    _light.SetLightValue(1, 200);
            //    _light.SetLightValue(2, 200);

            //    // Transform 적용 (드래그/줌 가능하게)
            //    TransformGroup tg = new TransformGroup();
            //    tg.Children.Add(_scaleTransform);
            //    tg.Children.Add(_translateTransform);
            //    imgCam1.RenderTransform = tg;

            //    // ★ Live 시작
            //    _camera.StartLive();

            //    AddLog("Live ON");
            //}
            //else
            //{
            //    _bLiveMode = false;

            //    // ★ Live 정지
            //    _camera.StopLive();

            //    // 조명 끄기
            //    _light.LightOff();

            //    AddLog("Live OFF");
            //}
        }

        private void SectionToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggle) return;

            // 토글 버튼 이름으로 대응하는 StackPanel 찾기
            string detailsName = toggle.Name switch
            {
                "AlignToggle" => "AlignDetails",
                "Find1Toggle" => "Find1Details",
                "Find2Toggle" => "Find2Details",
                "Find3Toggle" => "Find3Details",
                "Find4Toggle" => "Find4Details",
                _ => ""
            };

            if (string.IsNullOrEmpty(detailsName)) return;

            // VisualTree에서 이름으로 찾기
            var details = FindName(detailsName) as StackPanel;
            if (details == null) return;

            // IsChecked=True → 펼침, False → 접힘
            details.Visibility = toggle.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
