using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{
    public enum PressureMeter : byte //адресса датчиков давления
    {
        P1 = 0x03,
        P3 = 0x08
    }
    public static class PressureMeters
    {
        private static int[] _pressure_hex = new int[] {
            100, 6000, 10048, 12224, 14784,
            16896, 19456, 21952, 23744, 27200, 33344,
            36928, 38400, 40064, 42170, 43728, 49152, 
            54208, 58944, 60096, 62656, 63360, 64448, 
            64789, 65132, 65472};

        private static double[] _pressure_pascal = new double[] {
            0.01, 0.1, 2.1, 4.0, 7.0, 10.0, 15.0, 
            20.0, 25.0, 35.0, 60.0, 80.0, 90.0, 100.0, 
            130.0, 200.0, 300.0, 500.0, 800.0, 1000.0, 
            2000.0, 3000.0, 5700.0, 10000.0, 50000.0, 100000.0};

        private static int _respLenght = 8;
        private static int _pressValue = 0;

        public static double State(PressureMeter device)
        {
            byte[] command = new byte[4];

            command[0] = 0xAB; //маска сети

            switch (device) //выбор устройства
            {
                case PressureMeter.P1:
                    { command[1] = 0x12; break; }

                case PressureMeter.P3:
                    { command[1] = 0x11; break; }
            }

            command[2] = 0x00;   //команды
            command[3] = 0x00;   //для запроса состояния

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer == null) return -999.9;

            _pressValue = (answer[5] | answer[6] << 8);

            if (_pressValue < 100) _pressValue = 100;
            if (_pressValue > 65471) _pressValue = 65471;

            double pressure = 0;

            for (int i = 1; i < 26; i++)
            {
                if (_pressValue < _pressure_hex[i])
                {
                    pressure = (((_pressure_pascal[i] - _pressure_pascal[i - 1]) / 
                        (_pressure_hex[i] - _pressure_hex[i - 1])) * _pressValue + (_pressure_pascal[i - 1] - 
                        ((_pressure_pascal[i] - _pressure_pascal[i - 1]) / (_pressure_hex[i] - _pressure_hex[i - 1])) * 
                        _pressure_hex[i - 1]));
                    break;
                }
            }
            if (pressure > 99851.0) pressure = 100000;

            return pressure;
        }
    }
}
