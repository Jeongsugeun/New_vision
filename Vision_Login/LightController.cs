using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace Vision_Login
{
    internal class LightController : IDisposable  // ← IDisposable 추가
    {
        // ── 필드 ──────────────────────────────────────
        private SerialPort? _port;
        private const byte STX = 0x02;
        private const byte ETX = 0x03;

        // ── 포트 열기 ─────────────────────────────────
        public bool Open(string comPortName, int baudRate)
        {
            try
            {
                _port = new SerialPort(comPortName, baudRate, Parity.None, 8, StopBits.One);
                _port.Open();
                SendCommand("CCHA1");  // 조명 컨트롤러 초기화
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Serial Open Error: {ex.Message}");
                return false;
            }
        }

        // ── 채널 밝기 설정 (channel: 1~4, value: 0~998) ──
        public void SetLightValue(int channel, int value)
        {
            SendCommand($"WCH{channel}{value:D3}");
        }

        // ── 조명 끄기 ─────────────────────────────────
        public void LightOff()
        {
            SetLightValue(1, 0);
            SetLightValue(2, 0);
        }

        // ── 포트 닫기 ─────────────────────────────────
        public void Close()
        {
            if (_port?.IsOpen == true)
            {
                SendCommand("CCHA0");
                _port.Close();
            }
        }

        // ── 패킷 전송: STX + 명령 + ETX ───────────────
        private void SendCommand(string cmd)
        {
            if (_port == null || !_port.IsOpen) return;

            byte[] ascii = Encoding.ASCII.GetBytes(cmd.ToUpper());
            byte[] packet = new byte[ascii.Length + 2];
            packet[0] = STX;
            Buffer.BlockCopy(ascii, 0, packet, 1, ascii.Length);
            packet[packet.Length - 1] = ETX;

            _port.Write(packet, 0, packet.Length);
        }

        // ── IDisposable ───────────────────────────────
        public void Dispose() => Close();
    }
}