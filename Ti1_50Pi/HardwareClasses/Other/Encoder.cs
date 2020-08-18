using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ti1_50Pi;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Ti_150Pi.HardwareClasses
{
    public static class Encoder
    {
        private static uint _leftCount = 0;
        private static uint _rightCount = 0;
        private static uint _sensibility = 2;


        public enum EncDirection
        {
            Left,
            Right
        };

        public delegate void RotateEncoder(EncDirection direction);
        public static event RotateEncoder RotateEncoderNotify;

        static Encoder()
        {
            var pin1 = Pi.Gpio[26];
            pin1.PinMode = GpioPinDriveMode.Input;
            pin1.RegisterInterruptCallback(EdgeDetection.FallingEdge, ISRCallEncRight);

            var pin2 = Pi.Gpio[27];
            pin2.PinMode = GpioPinDriveMode.Input;
            pin2.RegisterInterruptCallback(EdgeDetection.FallingEdge, ISRCallEncLeft);
        }


        private static void ISRCallEncRight()
        {
            _rightCount++;
            if (_rightCount >= _sensibility)
            {
                RotateEncoderNotify(EncDirection.Right);
                _rightCount = 0;
            }
        }

        private static void ISRCallEncLeft()
        {
            _leftCount++;
            if (_leftCount >= _sensibility)
            {
                RotateEncoderNotify(EncDirection.Left);
                _leftCount = 0;
            }
        }
    }
}
