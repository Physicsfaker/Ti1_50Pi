using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*Write запись командных байтов с лева на право */
namespace Ti_150Pi.HardwareClasses
{

    public enum Led
    {
        V1,
        V2,
        V3,
        V4,
        V5,
        V6,
        N1Green,
        N1Red,
        N1Orange,
        N2,
        Katod1Green,
        Katod1Red,
        Katod2Green,
        Katod2Red,
        Obnul,
        Start,
        BadLeak,
        GoodLeak,
        Manual
    }

    public static class Leds
    {
        private static I2C _dev74;
        private static I2C _dev75;

        private static ushort _state74 = 0xFFC0;
        private static ushort _state75 = 0x0007;

        public static void Init()
        {
            _dev74 = new I2C(0x74);
            _dev75 = new I2C(0x75);

            _dev74.Write(6, 0x0000);
            _dev74.Write(2, _state74);

            _dev75.Write(6, 0xFBF8); //0x0100
            _dev75.Write(2, _state75);
            //_dev75.Read(2);
        }

        public static void Switch(Led device, Mode mode)
        {
            if (I2C.I2CGlobalError) return;
            switch (device)
            {
                case Led.V1:
                    {
                        if (mode == Mode.On) _state74 &= 0xFBFF;
                        if (mode == Mode.Off) _state74 |= 0x0400;
                        break; //V1
                    }
                case Led.V2:
                    {
                        if (mode == Mode.On) _state74 &= 0xFFBF;
                        if (mode == Mode.Off) _state74 |= 0x0040;
                        break; //V2
                    }
                case Led.V3:
                    {
                        if (mode == Mode.On) _state74 &= 0xFDFF;
                        if (mode == Mode.Off) _state74 |= 0x0200;
                        break; //V3
                    }
                case Led.V4:
                    {
                        if (mode == Mode.On) _state74 &= 0xFEFF;
                        if (mode == Mode.Off) _state74 |= 0x0100;
                        break; //V4
                    }
                case Led.V5:
                    {
                        if (mode == Mode.On) _state74 &= 0xFF7F;
                        if (mode == Mode.Off) _state74 |= 0x0080;
                        break; //V5
                    }
                case Led.V6:
                    {
                        if (mode == Mode.On) _state74 &= 0xF7FF;
                        if (mode == Mode.Off) _state74 |= 0x0800;
                        break; //V6
                    }
                case Led.N1Green:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFCF;    //N1 выкл.
                        if (mode == Mode.On) { Switch(Led.N1Red, Mode.Off); _state74 |= 0x0010; }   //N1 зелен.
                        break;
                    }
                case Led.N1Red:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFCF;    //N1 выкл.
                        if (mode == Mode.On) { Switch(Led.N1Green, Mode.Off); _state74 |= 0x0020; }   //N1 красн.
                        break;
                    }
                case Led.N1Orange:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFCF;    
                        if (mode == Mode.On) { Switch(Led.N1Green, Mode.Off); _state74 |= 0x0030; }   //N1 красн.
                        break;
                    }
                case Led.N2:
                    {
                        if (mode == Mode.On) _state74 &= 0xEFFF;
                        if (mode == Mode.Off) _state74 |= 0x1000;
                        break; //N2
                    }
                case Led.Katod1Green:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFFC;   //K1 выкл.
                        if (mode == Mode.On) { Switch(Led.Katod1Green, Mode.Off); _state74 |= 0x0001; }   //K1 зелен.
                        break;
                    }
                case Led.Katod1Red:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFFC;   //K1 выкл.
                        if (mode == Mode.On) { Switch(Led.Katod1Red, Mode.Off); _state74 |= 0x0002; }  //K1 красн.
                        break;
                    }
                case Led.Katod2Green:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFF3;   //K1 выкл.
                        if (mode == Mode.On) { Switch(Led.Katod2Green, Mode.Off); _state74 |= 0x0004; }   //K2 зелен.
                        break;
                    }
                case Led.Katod2Red:
                    {
                        if (mode == Mode.Off) _state74 &= 0xFFF3;   //K1 выкл.
                        if (mode == Mode.On) { Switch(Led.Katod2Red, Mode.Off); _state74 |= 0x0008; }   //K2 зелен.
                        break;
                    }
                case Led.Obnul:
                    {
                        if (mode == Mode.On) _state75 &= 0x00FD;
                        if (mode == Mode.Off) _state75 |= 0x0002;   //0
                        break;
                    }
                case Led.Start:
                    {
                        if (mode == Mode.On) _state75 &= 0x00FB;
                        if (mode == Mode.Off) _state75 |= 0x0004;
                        break;          //start
                    }
                case Led.BadLeak:
                    {
                        if (mode == Mode.On) _state74 &= 0xDFFF;
                        if (mode == Mode.Off) _state74 |= 0x2000;
                        break;  //ТЕЧЬ КРАСН. левый
                    }
                case Led.GoodLeak:
                    {
                        if (mode == Mode.On) _state75 &= 0x00FE;
                        if (mode == Mode.Off) _state75 |= 0x0001;
                        break;          //ТЕЧЬ ЗЕЛЕН. прав
                    }
                case Led.Manual:
                    {
                        if (mode == Mode.On) _state74 &= 0x3FFF;
                        if (mode == Mode.Off) _state74 |= 0xC000;
                        break; //ручной режим
                    }
                default: return;
            }

            switch (device)
            {
                case Led.Obnul: case Led.Start: case Led.GoodLeak: { _dev75.Write(2, _state75); break; }
                default: { _dev74.Write(2, _state74); break; }
            }
        }
    }
}
