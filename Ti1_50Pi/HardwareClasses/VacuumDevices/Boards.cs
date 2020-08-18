using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{
    public static class Boards
    {
        public static byte[] State(Board device)
        {
            int respLenght = 8;
            byte[] command = new byte[4];
            command[0] = 0xAB; //маска сети
            command[1] = (byte)device;  //адрес платы
            command[2] = 0x00;   //команды
            command[3] = 0x00;   //для запроса состояния

            switch (device)
            {
                case Board.amplifier: { respLenght = 10; break; }
                case Board.analyzer: { respLenght = 17; break; }
                case Board.forsKlapBoard: { respLenght = 8; break; }
                case Board.dvKlapBoard: { respLenght = 11; break; }
            }

            byte[] answer = DevicesSerial.TryToUse(command, respLenght);

            if (answer != null) return answer;
            else return null;
        }
    }
}
