using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

/*WriteAddressWord запись командных байтов с лева на право */
namespace Ti_150Pi.HardwareClasses
{
    public class I2C
    {
        public static bool I2CGlobalError { get; private set; } = false;
        private II2CDevice _device;

        static I2C()
        {
            Pi.Init<BootstrapWiringPi>();
        }

        public I2C(byte adress)
        {
            _device = Pi.I2C.AddDevice(adress);
        }

        public void Write(int adress, ushort command) //for leds
        {
            try
            {
                _device.WriteAddressWord(adress, command);
                I2CGlobalError = false;
            }
            catch (Unosquare.RaspberryIO.Abstractions.Native.HardwareException)
            {
                I2CGlobalError = true;
            }
        }

        public byte[] Read(int adress)
        {
            try
            {
                var data = _device.ReadAddressWord(adress);
                I2CGlobalError = false;
                return BitConverter.GetBytes(data);
            }
            catch (Unosquare.RaspberryIO.Abstractions.Native.HardwareException)
            {
                I2CGlobalError = true;
                return null;
            }
        }
    }

}
