using Mono.Posix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*ANALYZER and AMPLIFIER
#define SWITCH_CATHODE_DELAY 250 //длительность удержания кнопки для смены катода
UNS_16 analyzaer_Iem = 100; //ток эмиссии
UNS_16 analyzaer_Ua = 450; //Напряжение Ua
UNS_16 analyzaer_Us = 120; //Напряжение Us
UNS_8 working_cathode = 1;  //последений рабочий катод (НУЖНО СОХРАНЯТЬ НА ФЛЭШ!!!!)
UNS_8 cathode_mode = 0;     //текущий рабочий катод: 0-выкл; 1-первый; 2-второй;
UNS_16 cathode_switch_observer = 0; //счетчик при большом накоплении которого идет переключени на другой катод
UNS_8 cathode_button_press = 0;
/************************/

namespace Ti_150Pi.HardwareClasses
{
    public enum AnalyzerParam : int
    {
        IEM = 0,
        UA = 1,
        US = 2
    }

    public enum Cathode : int
    {
        S1,
        S2
    }

    public static class Cathodes
    {
        public static ushort analyzaer_Iem = 100; //ток эмиссии
        public static ushort analyzaer_Ua = 450; //Напряжение Ua
        public static ushort analyzaer_Us = 120;

        private static int _respLenght = 5;

        public static bool AnalyzerInit() //первоначальная настройка Анализатора, запустить в самом начале при включении
        {
            bool iemOk = false;
            bool uaOk = false;
            bool usOk = false;

            byte[] answer = new byte[_respLenght];

            byte[] command = new byte[7];
            command[0] = 0xAB; //маска сети
            command[1] = (byte)Board.analyzer;
            command[2] = 1;   //команды
            command[3] = 3; //длинна команды

            command[4] = 0; command[5] = (byte)(analyzaer_Iem & 0xFF); command[6] = (byte)((analyzaer_Iem >> 8) & 0xFF); //IEM

            answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) iemOk = true;
            else iemOk = false;

            command[4] = 1; command[5] = (byte)(analyzaer_Ua & 0xFF); command[6] = (byte)((analyzaer_Ua >> 8) & 0xFF); //UA
            answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) uaOk = true;
            else uaOk = false;

            command[4] = 2; command[5] = (byte)(analyzaer_Us & 0xFF); command[6] = (byte)((analyzaer_Us >> 8) & 0xFF); //US
            answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) usOk = true;
            else usOk = false;

            if (iemOk && uaOk && usOk) return true;
            else return false;
        }

        public static byte[] AnalyzerInit(AnalyzerParam param, ushort value) //ручная установка токов и напряжений
        {
            byte[] answer = new byte[_respLenght];
            byte[] command = new byte[7];
            command[0] = 0xAB; //маска сети
            command[1] = (byte)Board.analyzer;
            command[2] = 1;   //команды
            command[3] = 3; //длинна команды
            command[4] = (byte)param;
            command[5] = (byte)(value & 0xFF);
            command[6] = (byte)((value >> 8) & 0xFF); //US

            answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) return answer;
            else return null;
        }

        public static byte[] SwitchAnalyzer(Mode mode)
        {
            byte[] command = new byte[6];

            command[0] = 0xAB; //маска сети
            command[1] = (byte)Board.analyzer;
            command[2] = 0x02;   //команды
            command[3] = 0x02;   //для режима управление
            command[4] = 0x00;

            if (mode == Mode.On) command[5] = 0xFF; //выбор состояния устройства
            if (mode == Mode.Off) command[5] = 0x00;

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) return answer;
            else return null;
        }

        public static byte[] SwitchCathodes(Cathode cathode, Mode mode) //dodelat'
        {
            byte[] command = new byte[6];

            command[0] = 0xAB; //маска сети
            command[1] = (byte)Board.analyzer;
            command[2] = 0x02;   //команды
            command[3] = 0x02;   //для режима управление

            switch (cathode)
            {
                case Cathode.S1: { command[4] = 0x01; break; }
                case Cathode.S2: { command[4] = 0x02; break; }
            }

            if (mode == Mode.On) command[5] = 0xFF; //выбор состояния устройства
            if (mode == Mode.Off) command[5] = 0x00;

            byte[] answer = DevicesSerial.TryToUse(command, _respLenght);
            if (answer != null) return answer;
            else return null;
        }
    }
}
