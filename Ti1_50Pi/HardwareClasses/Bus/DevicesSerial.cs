using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace Ti_150Pi.HardwareClasses
{
    public static class DevicesSerial
    {
        private static SerialPort _serial;

        static DevicesSerial()
        {
            _serial = new SerialPort("/dev/ttyUSB0", 115200, Parity.None, 8, StopBits.One);
            _serial.ReadTimeout = 50;
            _serial.WriteTimeout = 50;
            if (_serial.IsOpen) _serial.Close();
            _serial.Open();
        }

        public static void DevicesSerialClouse()
        {
            if (_serial.IsOpen) _serial.Close();
        }


        public static byte[] TryToUse(byte[] command, int responseLength)
        {
            if (_serial.IsOpen)
            {
                Write(command);
                return Read(responseLength);
            }
            else return null;
        }

        private static void Write(byte[] request)
        {
            try
            {
                byte contrsum = 0;
                byte[] package = new byte[request.Length + 1];
                for (int i = 0; i < request.Length; i++) package[i] = request[i];
                for (int i = 1; i < package.Length - 1; i++) contrsum -= package[i];
                package[package.Length - 1] = contrsum;
                _serial.Write(package, 0, package.Length);
                //return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Доступ к порту закрыт.");
                //return false;
            }
        }

        private static byte[] Read(int count)
        {
            byte[] tmpByte = new byte[count];
            try
            {
                int lenght = 0;
                while (lenght < count)
                {
                    tmpByte[lenght] = (byte)_serial.ReadByte();
                    lenght++;
                }

                byte contrsum = 0;
                for (int i = 1; i < tmpByte.Length - 1; i++) contrsum -= tmpByte[i];
                if (contrsum == tmpByte[tmpByte.Length - 1]) return tmpByte;
                else return null;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Доступ к порту закрыт.");
                return null;
            }
        }
    }
}
