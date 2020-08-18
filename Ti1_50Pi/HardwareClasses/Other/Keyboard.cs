using Mono.Posix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Ti_150Pi.HardwareClasses.Other
{
    public static class Keyboard
    {
        public delegate void ButtonPress(Buttons button);
        public static event ButtonPress Notify;

        private static IGpioPin _interputPin;
        private static I2C _device75;

        public static void Init()
        {
            _device75 = new I2C(0x75);
            if (I2C.I2CGlobalError) return;

            _device75.Write(6, 0xFBF8); //0x0100
            _device75.Read(0);

            _interputPin = Pi.Gpio[30];
            _interputPin.PinMode = GpioPinDriveMode.Input;
            _interputPin.RegisterInterruptCallback(EdgeDetection.FallingEdge, ButtonPressCallback);
        }

        private static void ButtonPressCallback()
        {
            if (I2C.I2CGlobalError) return;
            var data = BitConverter.ToString(_device75.Read(0));

            switch (data)
            {
                case "FF-F9": { Notify(Buttons.S); break; }
                case "DF-FB": { Notify(Buttons.N1); break; }
                case "EF-FB": { Notify(Buttons.V4); break; }
                case "7F-FB": { Notify(Buttons.V5); break; }
                case "FF-E3": { Notify(Buttons.V2); break; }
                case "BF-FB": { Notify(Buttons.V1); break; }
                case "FF-DB": { Notify(Buttons.N2); break; }
                case "FF-BB": { Notify(Buttons.Start); break; }
                case "FF-FA": { Notify(Buttons.Enc); break; }
                //case "FF-FB": { Notike($"BtnV3"); break; }
                case "FF-7B": { Notify(Buttons.V6); break; }

                default: { /*System.Windows.Forms.MessageBox.Show($"Iterput  {data} \n");*/ break; }
            }

        }
    }
}
