using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Euresys.MultiCam;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Vision_Login
{
    public class SetCamera
    {
        private const int BUFFER_COUNT = 10;
        private UInt32 currentSurface;
        private MC.CALLBACK? multiCamCallback;
        private UInt32 channel;
        private UInt32 channelForConversion;
        private bool isActived;
        private bool isOpened;
        private UInt32 _width = 0;
        private UInt32 _height = 0;
        private UInt32 _bpp = 8;
        private byte[]? imageBuffer = null;
        private byte[]? colorImageBuffer = null;
        private IntPtr convertBuffer;

        private EventWaitHandle grabDone = new EventWaitHandle(false, EventResetMode.AutoReset);

        // ★ WPF용 이벤트 추가
        public event Action<byte[], int, int>? OnFrameReceived;

        public SetCamera()
        {
            MC.OpenDriver();
            channel = 0;
            isActived = false;
            isOpened = false;
        }

        ~SetCamera()
        {
            if (isOpened == true)
                Close();

            MC.CloseDriver();
        }

        public void Open(uint driverIndex, string connector, string topology, string camfilePath)
        {
            //정수근
            //MC.SetParam(MC.CONFIGURATION, "ErrorHandling", "MSGBOX");
            //MC.SetParam(MC.CONFIGURATION, "ErrorLog", "error.log");


            //MC.SetParam(MC.BOARD + driverIndex, "BoardTopology", topology);


            //MC.Create("CHANNEL", out channel);
            //MC.SetParam(channel, "DriverIndex", driverIndex);
            //MC.SetParam(channel, "Connector", connector);
            //MC.SetParam(channel, "CamFile", camfilePath);

            //Int32 bufferSize = 0, bufferPitch = 0;
            //string format;
            //MC.GetParam(channel, "Hactive_Px", out _width);
            //MC.GetParam(channel, "Vactive_Ln", out _height);
            //MC.GetParam(channel, "ColorFormat", out format);
            //if (format == "RGB24")
            //    _bpp = 24;
            //else
            //    _bpp = 8;

            //MC.GetParam(channel, "BufferSize", out bufferSize);
            //MC.GetParam(channel, "BufferPitch", out bufferPitch);
            //imageBuffer = new byte[bufferSize];

            //colorImageBuffer = new byte[bufferSize * 3];
            //convertBuffer = Marshal.AllocHGlobal(colorImageBuffer.Length);
            //Marshal.Copy(colorImageBuffer, 0, convertBuffer, colorImageBuffer.Length);

            //MC.Create(MC.DEFAULT_SURFACE_HANDLE, out channelForConversion);
            //MC.SetParam(channelForConversion, "SurfaceAddr", convertBuffer);
            //MC.SetParam(channelForConversion, "SurfaceSize", bufferSize * 3);
            //MC.SetParam(channelForConversion, "SurfacePitch", bufferPitch * 3);
            //MC.SetParam(channelForConversion, "SurfaceSizeX", _width);
            //MC.SetParam(channelForConversion, "SurfaceSizeY", _height);
            //MC.SetParam(channelForConversion, "SurfaceColorFormat", 1);
            //MC.SetParam(channelForConversion, "SurfaceColorComponentsOrder", 2);

            //MC.SetParam(channel, "SurfaceCount", BUFFER_COUNT);
            //multiCamCallback = new MC.CALLBACK(MultiCamCallback);
            //MC.RegisterCallback(channel, multiCamCallback, channel);
            //MC.SetParam(channel, MC.SignalEnable + MC.SIG_START_ACQUISITION_SEQUENCE, "ON");
            //MC.SetParam(channel, MC.SignalEnable + MC.SIG_SURFACE_PROCESSING, "ON");
            //MC.SetParam(channel, MC.SignalEnable + MC.SIG_END_CHANNEL_ACTIVITY, "ON");
            //MC.SetParam(channel, MC.SignalEnable + MC.SIG_ACQUISITION_FAILURE, "ON");

            //MC.SetParam(channel, "ChannelState", "READY");

            //isOpened = true;
        }

        public void Close()
        {
            if (isActived == true)
                Stop();

            if (channel != 0)
            {
                MC.Delete(channel);
                channel = 0;
            }

            isOpened = false;
            Marshal.FreeHGlobal(convertBuffer);
        }

        public void Start()
        {
            MC.SetParam(channel, "ChannelState", "ACTIVE");
            isActived = true;
        }

        public void Stop()
        {
            //정수근
            //MC.SetParam(channel, "ChannelState", "IDLE");
            isActived = false;
        }

        public void ExecuteForceTrig() => SetValueString("ForceTrig", "TRIG");
        public void GetValueString(string node, out string value) => MC.GetParam(channel, node, out value);
        public void GetValueInteger(string node, out int value) => MC.GetParam(channel, node, out value);
        public void GetValueDouble(string node, out double value) => MC.GetParam(channel, node, out value);
        public void SetValueString(string node, string value) => MC.SetParam(channel, node, value);
        public void SetValueInteger(string node, int value) => MC.SetParam(channel, node, value);
        public void SetValueDouble(string node, double value) => MC.SetParam(channel, node, value);

        private void MultiCamCallback(ref MC.SIGNALINFO signalInfo)
        {
            switch (signalInfo.Signal)
            {
                case MC.SIG_START_ACQUISITION_SEQUENCE:
                    Debug.WriteLine("SIG_START_ACQUISITION_SEQUENCE");
                    break;
                case MC.SIG_SURFACE_PROCESSING:
                    ProcessingCallback(signalInfo);
                    break;
                case MC.SIG_END_CHANNEL_ACTIVITY:
                    Debug.WriteLine("SIG_END_CHANNEL_ACTIVITY");
                    break;
                case MC.SIG_ACQUISITION_FAILURE:
                    Debug.WriteLine("SIG_ACQUISITION_FAILURE");
                    break;
                default:
                    throw new Euresys.MultiCamException("Unknown signal");
            }
        }

        private void ProcessingCallback(MC.SIGNALINFO signalInfo)
        {
            UInt32 currentChannel = (UInt32)signalInfo.Context;
            currentSurface = signalInfo.SignalInfo;

            Int32 bufferSize;
            IntPtr bufferAddress;

            MC.GetParam(currentChannel, "BufferSize", out bufferSize);
            MC.GetParam(currentSurface, "SurfaceAddr", out bufferAddress);
            if (imageBuffer == null) return;
            Marshal.Copy(bufferAddress, imageBuffer, 0, bufferSize);

            grabDone.Set();

            // ★ 복사본 만들어서 이벤트 발생 (imageBuffer 덮어쓰기 방지)
            byte[] frameCopy = new byte[bufferSize];
            Array.Copy(imageBuffer, frameCopy, bufferSize);
            OnFrameReceived?.Invoke(frameCopy, (int)_width, (int)_height);
        }

        public bool IsOpened => isOpened;
        public bool IsActived => isActived;
        public EventWaitHandle GrabDone => grabDone;
        public int Width => (int)_width;
        public int Height => (int)_height;
        public int Bpp => (int)_bpp;
        public byte[] Buffer => imageBuffer ?? Array.Empty<byte>();
        public byte[] ColorBuffer
        {
            get
            {
                MC.ConvertSurface(currentSurface, channelForConversion);
                return colorImageBuffer ?? Array.Empty<byte>();
            }
        }
    }
}
