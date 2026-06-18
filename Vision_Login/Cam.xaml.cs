using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;       // OpenFileDialog
using System.Windows.Media.Imaging; // BitmapImage
using System.Windows.Media.Effects;

namespace Vision_Login
{
    // ↓ Cam 클래스 밖으로 꺼냄
    public class LogItem
    {
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public partial class Cam : Window
    {
        // Transform 관련 필드
        private ScaleTransform _scaleTransform = new ScaleTransform(1, 1);
        private TranslateTransform _translateTransform = new TranslateTransform(0, 0);

        // 드래그 관련 필드
        private bool _isDragging = false;
        private Point _lastMousePos;

        private SetCamera _camera = new SetCamera();
        private const string CAM_FILE_PATH = @"C:\vwork\CamFile\Euresis_MC_A500M-35_3TAP_8bit_CC1.cam";

        //조명 관련
        private LightController _light = new LightController();
        private bool _bLiveMode = false;

        private int _totalOk = 0;
        private int _totalNg = 0;
        private int _totalUnits = 0;

        private bool isAuto = true;
        private Grid? _selectedGrid = null; // 현재 선택된 행
        private ObservableCollection<LogItem> _logs { get; set; } = new();
        public Cam()
        {
            InitializeComponent();
            LoadLogs();

            //조명값
            // COM 번호와 Baud Rate는 기존 INI에서 읽던 값을 그대로 사용
            _light.Open("COM1", 19200);

            // 프로그램 시작 시 자동으로 카메라 열기
            Loaded += Cam_Loaded;

            // 프로그램 종료 시 자동으로 카메라 닫기
            Closing += Cam_Closing;

            // 처음 시작 시 AUTO → CONFIG, EXIT 비활성화
            BtnConfig.IsEnabled = false;
            BtnConfig.Opacity = 0.4;
            BtnExit.IsEnabled = false;
            BtnExit.Opacity = 0.4;
            BtnDevice.IsHitTestVisible = false;  // 클릭 막기
            Btn_NameChange.IsHitTestVisible = false;
            Btn_IP.IsHitTestVisible = false;
            cmbUserLevel.IsHitTestVisible = false; // 클릭 막기

            Btn_Grab.IsEnabled = false;
            Btn_Grab.Opacity = 0.7;
            Btn_Live.IsEnabled = false;
            Btn_Live.Opacity = 0.7;
            Btn_Test.IsEnabled = false;
            Btn_Test.Opacity = 0.7;
            Btn_Settings.IsEnabled = false;
            Btn_Settings.Opacity = 0.7;
            Btn_Load.IsEnabled = false;
            Btn_Load.Opacity = 0.7;
            Btn_Save.IsEnabled = false;
            Btn_Save.Opacity = 0.7;
            Btn_Measure.IsEnabled = false;
            Btn_Measure.Opacity = 0.7;

            // 초기 게이지 0.0% 설정
            TxtPassRate.Text = "0.0%";
            TxtPassRate.Foreground = new SolidColorBrush(Colors.White);
            GaugePath.Stroke = new SolidColorBrush(Color.FromRgb(112, 173, 71));

            // 카운터 초기화
            TxtTotalUnits.Text = "0";
            TxtTotalOk.Text = "0";
            TxtTotalNg.Text = "0";

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

                // AUTO → 활성화
                BtnConfig.IsEnabled = false;
                BtnConfig.Opacity = 0.4;
                BtnExit.IsEnabled = false;
                BtnExit.Opacity = 0.4;
                BtnDevice.IsHitTestVisible = false;
                Btn_NameChange.IsHitTestVisible = false;
                Btn_IP.IsHitTestVisible = false;
                cmbUserLevel.IsHitTestVisible = false;

                Btn_Grab.IsEnabled = false;
                Btn_Grab.Opacity = 0.7;
                Btn_Live.IsEnabled = false;
                Btn_Live.Opacity = 0.7;
                Btn_Test.IsEnabled = false;
                Btn_Test.Opacity = 0.7;
                Btn_Settings.IsEnabled = false;
                Btn_Settings.Opacity = 0.7;
                Btn_Load.IsEnabled = false;
                Btn_Load.Opacity = 0.7;
                Btn_Save.IsEnabled = false;
                Btn_Save.Opacity = 0.7;
                Btn_Measure.IsEnabled = false;
                Btn_Measure.Opacity = 0.7;
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
                BtnDevice.IsHitTestVisible = true;
                Btn_NameChange.IsHitTestVisible = true;
                Btn_IP.IsHitTestVisible = true;
                cmbUserLevel.IsHitTestVisible = true;

                Btn_Grab.IsEnabled = true;
                Btn_Grab.Opacity = 1;
                Btn_Live.IsEnabled = true;
                Btn_Live.Opacity = 1;
                Btn_Test.IsEnabled = true;
                Btn_Test.Opacity = 1;
                Btn_Settings.IsEnabled = true;
                Btn_Settings.Opacity = 1;
                Btn_Load.IsEnabled = true;
                Btn_Load.Opacity = 1;
                Btn_Save.IsEnabled = true;
                Btn_Save.Opacity = 1;
                Btn_Measure.IsEnabled = true;
                Btn_Measure.Opacity = 1;
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

        private void LoadLogs()
        {
            _logs.Add(new() { Time = "16:34:00.000", Status = "Mode : AutoRun" });
            _logs.Add(new() { Time = "16:34:01.000", Status = "1 Ready ON" });
            _logs.Add(new() { Time = "16:34:02.356", Status = "Mode : Stop" });
            _logs.Add(new() { Time = "16:34:03.554", Status = "1 Ready OFF" });
            _logs.Add(new() { Time = "16:34:04.432", Status = "All Reset" });
            _logs.Add(new() { Time = "16:34:05.765", Status = "Open Server socket" });

            LogList.ItemsSource = _logs;
        }

        public void AddLog(string status)
        {
            _logs.Insert(0, new()
            {
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                Status = status
            });
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

        // 양품 추가 시 호출
        public void AddOk()
        {
            _totalOk++;
            UpdateGauge();
        }

        // 불량 추가 시 호출
        public void AddNg()
        {
            _totalNg++;
            UpdateGauge();
        }

        private void UpdateGauge()
        {
            int total = _totalOk + _totalNg;
            if (total == 0) return;

            double rate = (double)_totalOk / total * 100.0;
            UpdatePassRateUI(rate);
        }

        private void UpdatePassRateUI(double rate)
        {
            TxtPassRate.Text = $"{rate:F1}%";

            Color gaugeColor;
            if (rate >= 70)
                gaugeColor = Color.FromRgb(57, 255, 100);   // 형광 초록
            else if (rate >= 40)
                gaugeColor = Color.FromRgb(255, 165, 0);    // 주황
            else
                gaugeColor = Color.FromRgb(220, 53, 53);    // 빨강

            GaugePath.Stroke = new SolidColorBrush(gaugeColor);
            TxtPassRate.Foreground = new SolidColorBrush(gaugeColor);

            double angle = rate / 100.0 * 359.99;
            double radians = (angle - 90) * Math.PI / 180.0;  // 12시 방향이 시작

            double cx = 90.0;
            double cy = 90.0;
            double r = 74.0;

            double endX = cx + r * Math.Cos(radians);
            double endY = cy + r * Math.Sin(radians);

            GaugeArc.Point = new Point(endX, endY);
            GaugeArc.IsLargeArc = angle > 180;
        }

        private void Btn_Config_Click(object sender, RoutedEventArgs e)
        {
            Config popup = new Config();
            popup.Owner = this;
            popup.ShowDialog();
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
        private void Btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            // 카운터 초기화
            _totalOk = 0;
            _totalNg = 0;
            _totalUnits = 0;

            // TOTAL 텍스트 초기화
            TxtTotalOk.Text = "0";
            TxtTotalNg.Text = "0";
            TxtTotalUnits.Text = "0";

            // 게이지 초기화
            TxtPassRate.Text = "0.0%";
            TxtPassRate.Foreground = new SolidColorBrush(Colors.White);
            GaugePath.Stroke = new SolidColorBrush(Color.FromRgb(112, 173, 71));

            // 게이지 호 초기화 (빈 원으로)
            GaugeArc.Point = new Point(90, 15);
            GaugeArc.IsLargeArc = false;

            AddLog($"PASS RATE Reset");
        }

        private Device? _deviceWindow = null;

        private void btnDevice_Click(object sender, RoutedEventArgs e)
        {
            //if (isAuto) return;

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

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Cam1 cam1 = new Cam1();
            cam1.Owner = this.Owner;

            // Cam1이 완전히 렌더링된 후에 현재 창 숨기기
            cam1.ContentRendered += (s, args) =>
            {
                this.Hide();
            };

            cam1.Show();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "이미지 파일 선택",
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif|모든 파일|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgCam1.Source = bitmap;

                // Transform 초기화
                TransformGroup tg = new TransformGroup();
                tg.Children.Add(_scaleTransform);
                tg.Children.Add(_translateTransform);
                imgCam1.RenderTransform = tg;
            }
        }

        // 마우스 휠 - 확대/축소
        private void imgCam1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double newScale = _scaleTransform.ScaleX * zoomFactor;

            // 최소/최대 배율 제한
            newScale = Math.Max(0.1, Math.Min(newScale, 20.0));

            _scaleTransform.ScaleX = newScale;
            _scaleTransform.ScaleY = newScale;
        }

        private Grid? _cam1Grid = null;

        // 마우스 버튼 누를 때 - 드래그 시작
        private void imgCam1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid ?? (sender as Image)?.Parent as Grid;
            if (grid == null) return;

            _isDragging = true;
            _lastMousePos = e.GetPosition(grid);
            grid.CaptureMouse();          // ← Border 안 Grid가 캡처
            _cam1Grid = grid;
        }

        // 마우스 버튼 뗄 때 - 드래그 종료
        private void imgCam1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _cam1Grid?.ReleaseMouseCapture();
            _cam1Grid = null;
        }

        // 마우스 이동 - 드래그 중 이미지 이동
        private void imgCam1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _cam1Grid == null) return;

            Point currentPos = e.GetPosition(_cam1Grid);
            double dx = currentPos.X - _lastMousePos.X;
            double dy = currentPos.Y - _lastMousePos.Y;

            _translateTransform.X += dx;
            _translateTransform.Y += dy;

            _lastMousePos = currentPos;
        }

        // 윈도우 로드 완료 후 카메라 시작
        private void Cam_Loaded(object? sender, RoutedEventArgs e)
        {
            // connector "A" = DualBase A채널, topology는 cam 파일 확인 필요
            // 3TAP이면 "MONO" 또는 "FULL" → 실제 보드에 맞게 수정
            _camera.Open(0, "M", "MONO", CAM_FILE_PATH);

            if (!_camera.IsOpened)
            {
                //정수근
                //MessageBox.Show("카메라를 열 수 없습니다.", "카메라 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ★ 프레임 수신 시 imgCam1 업데이트
            _camera.OnFrameReceived += (buf, w, h) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var bmp = BitmapSource.Create(
                        w, h, 96, 96,
                        System.Windows.Media.PixelFormats.Gray8,
                        null, buf, w);
                    bmp.Freeze();
                    imgCam1.Source = bmp;
                });
            };

            AddLog("Camera Open OK");
        }

        // 윈도우 종료 시 카메라 정리
        private void Cam_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _light.LightOff();
            _light.Close();
            _camera.Stop();
            _camera.Close();
        }

        private void LiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_bLiveMode)
            {
                _bLiveMode = true;
                _light.SetLightValue(1, 200);
                _light.SetLightValue(2, 200);

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(_scaleTransform);
                tg.Children.Add(_translateTransform);
                imgCam1.RenderTransform = tg;

                _camera.Start();
                AddLog("Live ON");
            }
            else
            {
                _bLiveMode = false;
                _camera.Stop();
                _light.LightOff();
                AddLog("Live OFF");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _light.Close();
            base.OnClosed(e);
        }

        private Border? _selectedCamBorder;
        private TextBlock? _selectedCamTitle;

        private void CamBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClearSelectedCamBorder();

            _selectedCamBorder = sender as Border;

            if (_selectedCamBorder == null)
                return;

            _selectedCamBorder.BorderThickness = new Thickness(3);

            _selectedCamBorder.BorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
        {
            new GradientStop((Color)ColorConverter.ConvertFromString("#00D8FF"), 0.0),
            new GradientStop((Color)ColorConverter.ConvertFromString("#00C8FF"), 0.45),
            new GradientStop((Color)ColorConverter.ConvertFromString("#007EA3"), 1.0)
        }
            };

            _selectedCamBorder.Effect = new DropShadowEffect
            {
                Color = (Color)ColorConverter.ConvertFromString("#00D8FF"),
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.95
            };

            _selectedCamTitle = FindChild<TextBlock>(_selectedCamBorder);

            if (_selectedCamTitle != null)
            {
                _selectedCamTitle.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#00C8FF"));

                _selectedCamTitle.FontWeight = FontWeights.Bold;
            }

            Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearSelectedCamBorder();
                _selectedCamBorder = null;
                _selectedCamTitle = null;
            }
        }

        private void ClearSelectedCamBorder()
        {
            if (_selectedCamBorder != null)
            {
                _selectedCamBorder.BorderBrush = Brushes.Gray;
                _selectedCamBorder.BorderThickness = new Thickness(1);
                _selectedCamBorder.Effect = null;
            }

            if (_selectedCamTitle != null)
            {
                _selectedCamTitle.Foreground = Brushes.White;
                _selectedCamTitle.FontWeight = FontWeights.Normal;
            }
        }

        private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                T? childResult = FindChild<T>(child);

                if (childResult != null)
                    return childResult;
            }

            return null;
        }
    }
}