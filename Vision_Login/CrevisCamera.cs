using System;
using System.Windows.Media.Imaging;
using System.Windows;
using Euresys.MultiCam;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Vision_Login
{
    public class CrevisCamera
    {
        private uint _channel = 0;

        public event Action<BitmapSource>? OnFrameReceived;
        public bool IsOpen { get; private set; } = false;

        private int _width = 0;
        private int _height = 0;

        // 콜백 델리게이트 - GC 수집 방지용
        private MC.CALLBACK? _callback;

        /// <summary>
        /// .cam 파일로 카메라 열기
        /// </summary>
        public bool OpenWithCamFile(string camFilePath, int driverIndex = 0, string connector = "A")
        {
            try
            {
                // 1. 드라이버 열기 (McOpenDriver)
                MC.OpenDriver();

                // 2. 채널 생성
                MC.Create("Channel", out _channel);

                // 3. 보드 인덱스 지정 (0 = 첫 번째 보드)
                MC.SetParam(_channel, "DriverIndex", driverIndex);

                // 4. 커넥터 지정
                //    "A" = BOARDTYPE_DUAL (듀얼탭)
                //    "M" = BOARDTYPE_SINGLE (싱글)
                MC.SetParam(_channel, "Connector", connector);

                // 5. .cam 파일 지정
                MC.SetParam(_channel, "CamFile", camFilePath);

                // 6. 픽셀 포맷
                MC.SetParam(_channel, "ColorFormat", "Y8");

                // 7. 이미지 크기 읽기
                MC.GetParam(_channel, "ImageSizeX", out int w);
                MC.GetParam(_channel, "ImageSizeY", out int h);
                _width = w;
                _height = h;

                // 8. 콜백 등록
                _callback = new MC.CALLBACK(OnNewFrame);
                MC.RegisterCallback(_channel, _callback, MC.SIG_SURFACE_FILLED);

                // 9. 채널 활성화
                MC.SetParam(_channel, "ChannelState", "Active");

                IsOpen = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrevisCamera] OpenWithCamFile 실패: {ex.Message}");
                IsOpen = false;
                return false;
            }
        }

        /// <summary>
        /// 라이브 시작
        /// </summary>
        public void StartLive()
        {
            if (!IsOpen) return;
            try
            {
                MC.SetParam(_channel, "ChannelState", "Active");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrevisCamera] StartLive 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 라이브 정지
        /// </summary>
        public void StopLive()
        {
            if (!IsOpen) return;
            try
            {
                MC.SetParam(_channel, "ChannelState", "Idle");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrevisCamera] StopLive 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 카메라 닫기
        /// </summary>
        public void Close()
        {
            try
            {
                StopLive();

                if (_channel != 0)
                {
                    MC.Delete(_channel);
                    _channel = 0;
                }

                MC.CloseDriver();
                IsOpen = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrevisCamera] Close 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 프레임 콜백 - CALLBACK(object, nint) 시그니처
        /// </summary>
        private void OnNewFrame(ref MC.SIGNALINFO signalInfo)
        {
            try
            {
                MC.GetParam(_channel, "ImageAddress", out nint imgPtr);
                MC.GetParam(_channel, "ImagePitch", out int pitch);

                if (imgPtr == 0) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var bitmapSource = BitmapSource.Create(
                        _width, _height,
                        96, 96,
                        System.Windows.Media.PixelFormats.Gray8,
                        null,
                        imgPtr,
                        pitch * _height,
                        pitch
                    );
                    bitmapSource.Freeze();
                    OnFrameReceived?.Invoke(bitmapSource);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrevisCamera] 프레임 처리 실패: {ex.Message}");
            }
        }
    }
}