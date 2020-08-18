using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ti_150Pi
{
    public class Chart
    {
        public enum Form
        {
            mVolt,
            Flow
        }

        //if (Chart.Points.Count >= 2700) Chart.Points.RemoveAt(0); //2700 


        public PlotModel MyModel;
        private List<DataPoint> _Points;
        private Form ChartForm;
        private double _start = 0;
        private LineSeries _lineserie;
        private LinearAxis _flowAxis;
        private LinearAxis _timeAxis;
        private LinearAxis _inverseTimeAxis;
        private Stopwatch _timeWatch = new Stopwatch();

        private int _reversTimeMs = 150000;
        //private int _scale;
        public int Scale { get => Scale; private set => Scale = value; }

        public Chart(PlotView plotView, Form form)
        {
            Scale = _reversTimeMs;
            MyModel = new PlotModel { };
            _Points = new List<DataPoint>();



            switch (form)
            {
                case Form.mVolt:
                    {
                        _lineserie = new LineSeries()
                        {
                            ItemsSource = _Points,
                            DataFieldX = "Time",
                            DataFieldY = "mV",
                            StrokeThickness = 2,
                            MarkerSize = 0,
                            LineStyle = LineStyle.Solid,
                            Color = OxyColors.Blue,
                            MarkerType = MarkerType.None
                        };

                        Func<double, string> mvLabelFormatter =
                        mv => $"{mv:E1}";

                        _flowAxis = new LinearAxis
                        {
                            Title = "mV",
                            //AxisTitleDistance = 1,
                            //AxisTickToLabelDistance = 1,
                            Position = AxisPosition.Left,
                            LabelFormatter = mvLabelFormatter,
                            //StringFormat = "{0.0:E}",
                            //StringFormat = "0.00",
                            //AbsoluteMinimum = 0,
                            //AbsoluteMaximum = 25000,
                            IsZoomEnabled = false,
                            IsPanEnabled = false
                        };
                        ChartForm = Form.mVolt;
                        break;
                    }
                case Form.Flow:
                    {
                        _lineserie = new LineSeries()
                        {
                            ItemsSource = _Points,
                            DataFieldX = "Time",
                            DataFieldY = "Flow",
                            StrokeThickness = 2,
                            MarkerSize = 0,
                            LineStyle = LineStyle.Solid,
                            Color = OxyColors.Blue,
                            MarkerType = MarkerType.None
                        };

                        Func<double, string> flowLabelFormatter =
                        milliseconds => $"{(int)(milliseconds - _start) / 1000 / 60}:{(int)(milliseconds - _start) / 1000 % 60:D2}";

                        _flowAxis = new LinearAxis
                        {
                            Title = "Flow",
                            Position = AxisPosition.Left,
                            LabelFormatter = flowLabelFormatter,
                            //StringFormat = "0.00",
                            //AbsoluteMinimum = 0,
                            //AbsoluteMaximum = 25000,
                            IsZoomEnabled = false,
                            IsPanEnabled = false
                        };
                        ChartForm = Form.mVolt;
                        break;
                    }
            }

            Func<double, string> TimelabelFormatter =
            milliseconds => $"{(int)(milliseconds - _start) / 1000 / 60}:{(int)(milliseconds - _start) / 1000 % 60:D2}";

            _timeAxis = new LinearAxis()
            {
                Title = "Time",
                Position = AxisPosition.Bottom,
                LabelFormatter = TimelabelFormatter,
                AxisTickToLabelDistance = 1,
                //StringFormat = $"{_Points[_Points.Count-1].X / 1000 / 60}:{_Points[_Points.Count - 1].X / 1000}",
                //IntervalLength = 10,
                //MinorIntervalType = DateTimeIntervalType.Milliseconds,
                //IntervalType = DateTimeIntervalType.Milliseconds,
                //AbsoluteMinimum = 0,
                //AbsoluteMaximum = 150,
                //Minimum = 0.3,
                //MajorStep = 0.3
                Minimum = 0,
                Maximum = Scale,
            };

            Func<double, string> InverseTimelabelFormatter =
            milliseconds => $"{(int)(milliseconds) / 1000 / 60}:{(int)(milliseconds) / 1000 % 60:D2}";

            _inverseTimeAxis = new LinearAxis()
            {
                Title = "Time",
                Position = AxisPosition.Bottom,
                //LabelFormatter = TimelabelFormatter,
                AxisTickToLabelDistance = 1,
                LabelFormatter = InverseTimelabelFormatter,
                //IntervalLength = 10,
                //MinorIntervalType = DateTimeIntervalType.Milliseconds,
                //IntervalType = DateTimeIntervalType.Milliseconds,
                //AbsoluteMinimum = 0,
                //AbsoluteMaximum = 72,
                //Minimum = 0.3,
                //MajorStep = 0.3
                EndPosition = 0,
                StartPosition = 1,
                Minimum = 0,
                Maximum = Scale,
                IsAxisVisible = false
            };

            MyModel.Axes.Add(_flowAxis);
            MyModel.Axes.Add(_timeAxis);
            MyModel.Axes.Add(_inverseTimeAxis);
            MyModel.Series.Add(_lineserie);
            MyModel.ZoomAllAxes(15);

            _Points.Clear();
            plotView.Model = MyModel;
            MyModel.InvalidatePlot(true);
        }

        public void Add(double newData)
        {
            if (_Points.Count == 0)
            {
                _timeWatch.Start();
                _start = _timeWatch.ElapsedMilliseconds;
            }

            var timeAtNow = _timeWatch.ElapsedMilliseconds;
            _Points.Add(new OxyPlot.DataPoint((timeAtNow - _start), newData));

            if (_Points.Count > 0)
            {
                if (_Points[_Points.Count - 1].X > Scale)
                {
                    _timeAxis.IsAxisVisible = false;
                    _inverseTimeAxis.IsAxisVisible = true;
                }
                else
                {
                    _timeAxis.IsAxisVisible = true;
                    _inverseTimeAxis.IsAxisVisible = false;
                }


                if (_Points[_Points.Count - 1].X > Scale)
                {
                    _timeAxis.Maximum = _Points[_Points.Count - 1].X;
                    _timeAxis.Minimum = _Points[_Points.Count - 1].X - Scale;
                }
                else
                {
                    _timeAxis.Maximum = Scale;
                    _inverseTimeAxis.Maximum = Scale;
                }

            }

            Update();
        }

        public void Update() { MyModel.InvalidatePlot(true); }

        public void ChangeTimeScale(bool x)
        {
            if (x)
            {
                if (Scale + 30000 < 28800000 )
                    Scale += 30000;
            }
            else 
            {
                Scale -= 30000;
                if (Scale - 30000 < 30000) Scale = 30000;
            }
            //_timeAxis.Maximum = _Points[_Points.Count - 1].X;
            //_timeAxis.Minimum = _Points[_Points.Count - 1].X - _scale;
            //Update();
        }
    }
}
