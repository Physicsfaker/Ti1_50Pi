using Mono.Posix;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ti_150Pi.HardwareClasses;
using Ti_150Pi.HardwareClasses.Other;
using Unosquare.RaspberryIO;

namespace Ti_150Pi
{
    //#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    //#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

    public struct TiInfo
    {
        public Commands currentState;
        public float flow;
        public double P1;
        public double P3;
        public int temperature;
        public string systemMessage; //podumat' nad tipom

        public TiInfo(Commands currentState, float flow, double P1, double P3, int temperature, string systemMessage)
        {
            this.currentState = currentState;
            this.flow = flow;
            this.P1 = P1;
            this.P3 = P3;
            this.temperature = temperature;
            this.systemMessage = systemMessage;
        }
        //public static bool operator ==(TiInfo c1, TiInfo c2)
        //{
        //    if (c1.currentState == c2.currentState && c1.flow == c2.flow && c1.P1 == c2.P1 && c1.P3 == c2.P3 && c1.temperature == c2.temperature && c1.systemMessage == c2.systemMessage) return true;
        //    else return false;
        //}

        //public static bool operator !=(TiInfo c1, TiInfo c2)
        //{
        //    if (c1.currentState == c2.currentState && c1.flow == c2.flow && c1.P1 == c2.P1 && c1.P3 == c2.P3 && c1.temperature == c2.temperature && c1.systemMessage == c2.systemMessage) return false;
        //    else return true;
        //}
    }

    public enum Commands
    {
        NON,
        //Buttons ->
        S,
        V1,
        V2,
        V3,
        V4,
        V5,
        V6,
        Vlap,
        VProm,
        N1,
        N2,
        ChangeObj,
        Menu,
        Obnul,
        Start,
        //States ->
        Launching,
        Manual
    }

    public static class DeviceManager
    {
        public enum Status
        {
            On,
            Off,
            Error,
            StartWork
        }

        public struct FullTiState
        {
            public bool CurrentS1;
            public Status S1;
            public Status S2;
            public Status V1;
            public Status V2;
            public Status V3;
            public Status V4;
            public Status V5;
            public Status V6;
            public Status Vlap;
            public Status VProm;
            public Status N1;
            public Status N2;
            public Status P1;
            public Status P2;
            public Status Tmp;
            public Status Amplifier;
            public int CurrentAmpMka;
            public int CurrentAmpUa;
            public int CurrentAmpUs;
        }

        private static Ti _ti1_50;
        public static FullTiState GlobalState;
        private static TiInfo _newinfo = new TiInfo(Commands.NON, 0, 0, 0, 26, "Work");

        public static ObservableCollection<Commands> CommandsFromUser { get; set; } = new ObservableCollection<Commands>();
        public static ObservableCollection<TiInfo> DataFromTi { get; set; } = new ObservableCollection<TiInfo>();


        private static int _amp_P1_P3_OtherCycl = 1;
        private static int _Amp_P1_P3_OtherCycl { get { return _amp_P1_P3_OtherCycl; } set { if (value > 3 || value < 0) _amp_P1_P3_OtherCycl = 1; else _amp_P1_P3_OtherCycl = value; } }
        private static bool _first = true;

        public static void Init()
        {
            if (_ti1_50 != null) return;
            _ti1_50 = new Ti(new LaunchingState());
            Leds.Init();
            Keyboard.Init();

            #region Узнаем что с течеком при включении или перезапуске программы
            GlobalState = new FullTiState();
            GlobalState.CurrentS1 = true;
            for (int i = 0; i < 11; i++) DeviceStatusObserver(ref _newinfo);
            _newinfo.temperature = TemperatureSensor.State();
            Cathodes.AnalyzerInit();
            #endregion

            System.Threading.Timer timer111Ms = new System.Threading.Timer(new TimerCallback(DevicesStateTreatment), null, 0, 111);
        }

        private static async void DevicesStateTreatment(object obj)
        {
            await Task.Run(() =>
            {
                switch (_Amp_P1_P3_OtherCycl)
                {
                    case 1: { AmplifierStatusObserver(ref _newinfo);  break; }
                    case 2:
                        {
                            if (_first) { _newinfo.P1 = PressureMeters.State(PressureMeter.P1); _first = false; }
                            else { _newinfo.P3 = PressureMeters.State(PressureMeter.P3); _first = true; }
                            break;
                        }
                    case 3:
                        {
                            if (_first) DeviceStatusObserver(ref _newinfo);
                            else
                            {
                                Commands newCom = Commands.NON;
                                if (CommandsFromUser.Count > 0)
                                {
                                    newCom = CommandsFromUser[0];
                                    CommandsFromUser.RemoveAt(0);
                                    if (!(_newinfo.currentState == newCom))
                                    {
                                        switch (newCom)
                                        {
                                            case Commands.Launching: { _ti1_50.State = new LaunchingState(); break; }
                                            case Commands.Manual: { _ti1_50.State = new ManualState(); break; }
                                            default: break;
                                        }
                                    }
                                }
                                _ti1_50.Execution(newCom, ref _newinfo);
                            }
                            break;
                        }
                }
                _Amp_P1_P3_OtherCycl++;
                if (I2C.I2CGlobalError) _newinfo.systemMessage = SysMessages.I2CError;
                DataFromTi.Add(_newinfo);
            });
        }

        private static uint _forsKlapBoardErrors = 0;
        private static uint _analyzerBoardErrors = 0;
        private static uint _amplifBoardErrors = 0;
        private static uint _dvKlapBoardErrors = 0;
        private static uint _baseBoardsStateCycl = 1;
        private static bool _n1Flash = true;

        private static uint _BaseBoardsStateCycl { get { return _baseBoardsStateCycl; } set { if (value > 3 || value < 0) _baseBoardsStateCycl = 1; else _baseBoardsStateCycl = value; } }

        private static void AmplifierStatusObserver(ref TiInfo info)
        {
            byte[] answer;
            answer = Boards.State(Board.amplifier);
            if (answer == null) { _amplifBoardErrors++; if (_amplifBoardErrors == 3) info.systemMessage = SysMessages.AmplifierrBoardError; return; } //ошибка платы;
            else //obrabotka otveta
            {
                _amplifBoardErrors = 0;
                info.flow = answer[5] | answer[6] << 8 | answer[7] << 16 | answer[8] << 24;
            }
        }

        private static void DeviceStatusObserver(ref TiInfo info)
        {
            byte[] answer;
            switch (_BaseBoardsStateCycl)
            {
                case 1: //плата ФорсКлап
                    {
                        answer = Boards.State(Board.forsKlapBoard);
                        if (answer == null) { _forsKlapBoardErrors++; if (_forsKlapBoardErrors == 3) info.systemMessage = SysMessages.ForceKlapBoardError; break; } //ошибка платы;
                        else //obrabotka otveta
                        {
                            _forsKlapBoardErrors = 0;

                            GlobalState.V3 = (answer[4] & 0x01 << 2) > 0 ? Status.On : Status.Off;
                            GlobalState.V4 = (answer[4] & 0x01 << 1) > 0 ? Status.On : Status.Off;
                            GlobalState.V2 = (answer[4] & 0x01) > 0 ? Status.On : Status.Off;

                            if (GlobalState.V3 == Status.Off) Leds.Switch(Led.V3, Mode.Off);
                            else Leds.Switch(Led.V3, Mode.On);

                            if (GlobalState.V4 == Status.Off) Leds.Switch(Led.V4, Mode.Off);
                            else Leds.Switch(Led.V4, Mode.On);

                            if (GlobalState.V2 == Status.Off) Leds.Switch(Led.V2, Mode.Off);
                            else Leds.Switch(Led.V2, Mode.On);
                        }
                        break;
                    }
                case 2: //палата ДвКлап
                    {
                        answer = Boards.State(Board.dvKlapBoard);
                        if (answer == null) { _dvKlapBoardErrors++; if (_dvKlapBoardErrors == 3) info.systemMessage = SysMessages.DvKlapBoardError; break; } //ошибка платы;
                        else //obrabotka otveta
                        {
                            _dvKlapBoardErrors = 0;

                            GlobalState.VProm = (answer[4] & 0x01 << 7) > 0 ? Status.On : Status.Off;

                            if ((answer[4] & 0x01 << 5) == 0 && (answer[4] & 0x01 << 6) > 0) GlobalState.V6 = Status.Error;
                            if ((answer[4] & 0x01 << 5) > 0 && (answer[4] & 0x01 << 6) > 0) GlobalState.V6 = Status.On;
                            if ((answer[4] & 0x01 << 5) == 0 && (answer[4] & 0x01 << 6) == 0) GlobalState.V6 = Status.Off;


                            if (GlobalState.V6 == Status.StartWork) GlobalState.V6 = (answer[4] & 0x01 << 5) > 0 ? Status.On : Status.Off;

                            GlobalState.Vlap = (answer[4] & 0x01 << 4) > 0 ? Status.On : Status.Off;
                            GlobalState.V1 = (answer[4] & 0x01 << 3) > 0 ? Status.On : Status.Off;
                            GlobalState.V5 = (answer[4] & 0x01 << 2) > 0 ? Status.On : Status.Off;

                            GlobalState.N2 = (answer[4] & 0x01) > 0 ? Status.On : Status.Off;

                            GlobalState.N1 = (answer[4] & 0x01 << 1) > 0 ? Status.StartWork : Status.Off;
                            GlobalState.N1 = (answer[5] & 0x01 << 1) > 0 ? Status.On : GlobalState.N1;
                            GlobalState.N1 = (answer[5] & 0x01 << 2) > 0 ? Status.Error : GlobalState.N1;

                            if (GlobalState.V1 == Status.Off) Leds.Switch(Led.V1, Mode.Off);
                            else Leds.Switch(Led.V1, Mode.On);

                            if (GlobalState.V5 == Status.Off) Leds.Switch(Led.V5, Mode.Off);
                            else Leds.Switch(Led.V5, Mode.On);

                            if (GlobalState.N2 == Status.Off) Leds.Switch(Led.N2, Mode.Off);
                            else Leds.Switch(Led.N2, Mode.On);

                            if (GlobalState.V6 != Status.On) Leds.Switch(Led.V6, Mode.Off);
                            else Leds.Switch(Led.V6, Mode.On);

                            if (GlobalState.N1 == Status.Off) Leds.Switch(Led.N1Green, Mode.Off);
                            else if (GlobalState.N1 == Status.Error) { Leds.Switch(Led.N1Red, Mode.On); }
                            else if (GlobalState.N1 == Status.On) { Leds.Switch(Led.N1Green, Mode.On);}
                            else if (GlobalState.N1 == Status.StartWork)
                            {
                                if (_n1Flash) { Leds.Switch(Led.N1Orange, Mode.On); }
                                else { Leds.Switch(Led.N1Orange, Mode.Off); }
                                _n1Flash = !_n1Flash;
                            }

                            info.systemMessage = $"N1 =" + GlobalState.N1.ToString();
                        }
                        break;
                    }
                case 3: //Плата питания анализатора 
                    {
                        answer = Boards.State(Board.analyzer);
                        if (answer == null) { _analyzerBoardErrors++; if (_analyzerBoardErrors == 3) info.systemMessage = SysMessages.AnalyzerBoardError; break; } //ошибка платы;
                        else //obrabotka otveta
                        {
                            _analyzerBoardErrors = 0;


                            GlobalState.S1 = (answer[5] & 0x01 << 1) > 0 ? Status.On : Status.Off;     //катод 1 вкл
                            GlobalState.S1 = (answer[5] & 0x01 << 2) > 0 ? Status.On : Status.Off;     //катод 2 вкл
                            if (GlobalState.CurrentS1)
                            {
                                GlobalState.S1 = (answer[5] & 0x01 << 4) > 0 ? Status.StartWork : GlobalState.S1;
                                GlobalState.S1 = (answer[4] & 0x01 << 5) > 0 ? Status.Error : GlobalState.S1;     //обрыв катода 1
                            }
                            else
                            {
                                GlobalState.S2 = (answer[5] & 0x01 << 4) > 0 ? Status.StartWork : GlobalState.S2;
                                GlobalState.S2 = (answer[4] & 0x01 << 5) > 0 ? Status.Error : GlobalState.S2;     //обрыв катода 2
                            }

                            GlobalState.Amplifier = (answer[5] & 0x01) > 0 ? Status.On : Status.Off;          //высковолт. источ. вкл.
                            if ((answer[4] & 0x01 << 3) > 0) _analyzerBoardErrors++;     //нет напряжения питания катодов (типа ошибка самой платы)
                            if (_analyzerBoardErrors == 3) GlobalState.Amplifier = Status.Error;

                            GlobalState.CurrentAmpMka = answer[6] | answer[7] << 8;    //заданный ток эмиссии мкА
                            GlobalState.CurrentAmpUa = answer[8] | answer[9] << 8;    //напряжение Ua
                            GlobalState.CurrentAmpUs = answer[10] | answer[11] << 8;  //напряжение Us

                            //info.systemMessage = $"mkA = {GlobalState.CurrentAmpMka}; Ua = {GlobalState.CurrentAmpUa}; Us = {GlobalState.CurrentAmpUs}";
                        }
                        break;
                    }
                default: break;

            }
            if ((_forsKlapBoardErrors + _analyzerBoardErrors + _amplifBoardErrors + _dvKlapBoardErrors) > 11) info.systemMessage = SysMessages.InternalRS485Error;
            _BaseBoardsStateCycl++;
        }
    }



    //сами состояния
    public class LaunchingState : ITiState
    {
        public void Execution(Commands butt, ref TiInfo inf)
        {
            if (DeviceManager.GlobalState.Vlap == DeviceManager.Status.Off) Valves.Switch(Valve.VLap, Act.Close);

            switch (butt)
            {
                case Commands.S:
                    break;
                case Commands.V1:
                    {
                        if (DeviceManager.GlobalState.V1 == DeviceManager.Status.Off) { Valves.Switch(Valve.V1, Act.Open); }
                        else { Valves.Switch(Valve.V1, Act.Close); }
                        break;
                    }
                case Commands.V2:
                    {
                        if (DeviceManager.GlobalState.V2 == DeviceManager.Status.Off) { Valves.Switch(Valve.V2, Act.Open); }
                        else { Valves.Switch(Valve.V2, Act.Close); }
                        break;
                    }
                case Commands.V3:
                    {
                        if (DeviceManager.GlobalState.V3 == DeviceManager.Status.Off) { Valves.Switch(Valve.V3, Act.Open); }
                        else { Valves.Switch(Valve.V3, Act.Close); }
                        break;
                    }
                case Commands.V4:
                    {
                        if (DeviceManager.GlobalState.V4 == DeviceManager.Status.Off) { Valves.Switch(Valve.V4, Act.Open); }
                        else { Valves.Switch(Valve.V4, Act.Close); }
                        break;
                    }
                case Commands.V5:
                    {
                        if (DeviceManager.GlobalState.V5 == DeviceManager.Status.Off) { Valves.Switch(Valve.V5, Act.Open); }
                        else { Valves.Switch(Valve.V5, Act.Close); }
                        break;
                    }
                case Commands.V6:
                    {
                        if (DeviceManager.GlobalState.V6 == DeviceManager.Status.Off) { Valves.Switch(Valve.V6, Act.Open); }
                        else { Valves.Switch(Valve.V6, Act.Close); }
                        break;
                    }
                case Commands.VProm:
                    {
                        if (DeviceManager.GlobalState.VProm == DeviceManager.Status.Off) { Valves.Switch(Valve.VProm, Act.Open); }
                        else { Valves.Switch(Valve.VProm, Act.Close); }
                        break;
                    }
                case Commands.Vlap:
                    {
                        if (DeviceManager.GlobalState.Vlap == DeviceManager.Status.Off) { Valves.Switch(Valve.VLap, Act.Close); }
                        else { Valves.Switch(Valve.VLap, Act.Open); }
                        break;
                    }
                case Commands.N1:
                    if (DeviceManager.GlobalState.N1 == DeviceManager.Status.Off) { TurbomolecularPump.Switch(Mode.On); }
                    else { TurbomolecularPump.Switch(Mode.Off); }
                    break;
                case Commands.N2:
                    if (DeviceManager.GlobalState.N2 == DeviceManager.Status.Off) { Pump.Switch(Mode.On); }
                    else { Pump.Switch(Mode.Off); }
                    break;
                case Commands.ChangeObj:
                    break;
                case Commands.Menu:
                    break;
                case Commands.Obnul:
                    break;
                case Commands.Start:
                    break;
            }
        }
    }
    public class ManualState : ITiState
    {
        public void Execution(Commands butt, ref TiInfo inf)
        {

        }
    }

    public interface ITiState { void Execution(Commands butt, ref TiInfo inf); }
    public class Ti
    {
        public ITiState State { get; set; }
        public Ti(ITiState st) { State = st; }
        public void Execution(Commands butt, ref TiInfo inf) { State.Execution(butt, ref inf); }
    }
}
