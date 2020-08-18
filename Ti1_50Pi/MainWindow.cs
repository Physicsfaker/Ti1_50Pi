using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ti_150Pi.HardwareClasses;
using System.IO.Ports;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO;
using Ti_150Pi.HardwareClasses.Other;
using Ti_150Pi;
using System.Collections.Specialized;
using OxyPlot;
using OxyPlot.Series;
using Encoder = Ti_150Pi.HardwareClasses.Encoder;
using System.Windows.Forms.DataVisualization.Charting;
using Chart = Ti_150Pi.Chart;
using OxyPlot.Axes;

namespace Ti1_50Pi
{
    public partial class MainWindow : Form
    {
        private List<Button> _MainButtons = new List<Button>();
        private int _targetButton = 0;
        private int _TargetButton
        {
            get { return _targetButton; }
            set
            {
                if (value == _MainButtons.Count) { _targetButton = _MainButtons.Count - 1; return; }
                else if (value < 0) { _targetButton = 0; return; }
                _targetButton = value;
            }
        }
        private bool _buttonSelected = false;

        private Chart CurentChart;

        public MainWindow()
        {
            InitializeComponent();
            CurentChart = new Chart(plotView1, Chart.Form.mVolt);

            DeviceManager.DataFromTi.CollectionChanged += UpdateView;
            DeviceManager.Init();
            Keyboard.Notify += Buttons_Notify;
            Encoder.RotateEncoderNotify += ControlSetFocus;

            _MainButtons.Add(buttonMainWin1);
            _MainButtons.Add(buttonMainWin2);
            _MainButtons.Add(buttonMainWin3);
            _MainButtons.Add(buttonMainWin4);
            _MainButtons.Add(buttonMainWin5);
            _MainButtons.Add(buttonMainWin6);
            _MainButtons.Add(buttonMainWin7);
            _MainButtons.Add(buttonMainWin8);
            SelectButton(_MainButtons[_targetButton], true);

        }

        public void UpdateView(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                TiInfo newData = (TiInfo)e.NewItems[0];
                DeviceManager.DataFromTi.RemoveAt(0);

                labelP1.Text = $"P1  {newData.P1:F1}";
                labelP3.Text = $"P3  {newData.P3:F1}";
                CurentChart.Add(newData.flow);

                if (newData.systemMessage != null || newData.systemMessage != "") stripStatusLabel1.Text = newData.systemMessage;
            }
        }

        private void SelectButton(Button target, bool selected)
        {
            if (selected)
            {
                target.BackColor = Color.Cyan;
                target.ForeColor = Color.Black;
            }
            else
            {
                target.BackColor = Color.CornflowerBlue;
                target.ForeColor = Color.White;
            }
        }

        public void ControlSetFocus(Encoder.EncDirection direction)
        {
            if (!_buttonSelected)
            {
                SelectButton(_MainButtons[_TargetButton], false);
                if (direction == Encoder.EncDirection.Left) { _TargetButton--; CurentChart.ChangeTimeScale(true); }
                else { _TargetButton++; CurentChart.ChangeTimeScale(false); }
                SelectButton(_MainButtons[_TargetButton], true);
            }
        }

        private bool buttonDelay = false;
        private void Buttons_Notify(Buttons but)
        {
            if (buttonDelay) return;

            buttonDelay = true;
            System.Threading.Timer timer300Ms = new System.Threading.Timer(new TimerCallback((object obj) => { buttonDelay = false; }), null, 300, Timeout.Infinite);

            switch (but)
            {
                case Buttons.S: { DeviceManager.CommandsFromUser.Add(Commands.S); break; }
                case Buttons.V1: { DeviceManager.CommandsFromUser.Add(Commands.V1); break; }
                case Buttons.V2: { DeviceManager.CommandsFromUser.Add(Commands.V2); break; }
                case Buttons.V3: { DeviceManager.CommandsFromUser.Add(Commands.V3); break; }
                case Buttons.V4: { DeviceManager.CommandsFromUser.Add(Commands.V4); break; }
                case Buttons.V5: { DeviceManager.CommandsFromUser.Add(Commands.V5); break; }
                case Buttons.V6: { DeviceManager.CommandsFromUser.Add(Commands.V6); break; }
                case Buttons.N1: { DeviceManager.CommandsFromUser.Add(Commands.N1); break; }
                case Buttons.N2: { DeviceManager.CommandsFromUser.Add(Commands.N2); break; }
                case Buttons.ChangeObj: { break; }
                case Buttons.Menu: { _buttonSelected = false; break; }
                case Buttons.Obnul: { break; }
                case Buttons.Start: { break; }
                case Buttons.Enc: { _MainButtons[_targetButton].PerformClick(); break; }
                default: break;
            }
            //labelLastComm.Text = "Last Command = "/* + but.ToString()*/;
        }
        private void close_button_Click(object sender, EventArgs e)
        {
            DevicesSerial.DevicesSerialClouse();
            Thread.Sleep(1000);
            this.Close();
            Application.Exit();
        }

        private void buttonMainWin1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void buttonMainWin2_Click(object sender, EventArgs e)
        {

        }
    }
}
