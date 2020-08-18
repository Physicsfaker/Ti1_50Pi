using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{
    public static class TemperatureSensor
    {
        private static int _respLenght = 8;
        public static int State()
        {
            byte[] command = new byte[4];

            command[0] = 0xAB; //маска сети
            command[1] = 0x21; 
            command[2] = 0x00;   //команды
            command[3] = 0x00;   //для запроса состояния

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer == null) return -999;
            return (int)answer[5];
        }
    }
}
