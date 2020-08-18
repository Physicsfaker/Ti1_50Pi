using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{
    public enum Valve : byte //адресса клапанов
    {
        V1 = 0x03,
        V2 = 0x08,
        V3 = 0x0A,
        V4 = 0x09,
        V5 = 0x02,
        V6 = 0x05,
        VProm = 0x07,      //promport
        VLap = 0x04        //Обратный клапан
    }

    public static class Valves
    {
        private static int _respLenght = 5;
        public static bool Switch(Valve device, Act action)
        {
            byte[] command = new byte[6];

            command[0] = 0xAB; //маска сети

            switch (device) //выбор устройства
            {
                case Valve.V1:
                case Valve.V5:
                case Valve.V6:
                case Valve.VProm:
                case Valve.VLap:
                    { command[1] = 0x3A; break; }
                case Valve.V2:
                case Valve.V3:
                case Valve.V4:
                    { command[1] = 0x26; break; }
            }

            command[2] = 2;   //команды
            command[3] = 2;   //для режима управление
            command[4] = (byte)device;

            if (action == Act.Open) //выбор состояния устройства
            {
                command[5] = 0xFF;
                if (device == Valve.VLap) command[5] = 0x00;
            }
            if (action == Act.Close)
            {
                command[5] = 0x00;
                if (device == Valve.VLap) command[5] = 0xFF;
            }

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) return true;
            else return false;
        }
    }
}
