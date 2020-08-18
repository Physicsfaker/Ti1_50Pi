using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi.HardwareClasses
{
    public enum Board : byte
    {
        amplifier = 0x18,
        analyzer = 0x1A,
        dvKlapBoard = 0x3A,
        forsKlapBoard = 0x26
    }
    public enum Mode
    {
        On = 1,
        Off = 0
    }

    public enum Act
    {
        Open = 1,
        Close = 0
    }

    public enum Buttons
    {
        S,
        V1,
        V2,
        V3,
        V4,
        V5,
        V6,
        N1,
        N2,
        ChangeObj,
        Menu,
        Obnul,
        Start,
        Enc
    }
}
