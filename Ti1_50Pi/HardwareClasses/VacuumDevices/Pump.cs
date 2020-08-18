using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{

    public static class Pump
    {
        private static int _respLenght = 5;
        public static bool Switch(Mode mode)
        {
            byte[] command = new byte[6];

            command[0] = 0xAB; //маска сети
            command[1] = 0x3A;
            command[2] = 0x02;   //команды
            command[3] = 0x02;   //для режима управление
            command[4] = 0x00;

            if (mode == Mode.On) //выбор состояния устройства
            {
                command[5] = 0xFF;
            }
            if (mode == Mode.Off)
            {
                command[5] = 0x00;
            }

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) return true;
            else return false;
        }
    }
}
