using FCFS.Library;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Plottable;
using System.Collections.ObjectModel;
using System.Windows;

namespace FCFS.UI.Views
{
    /// <summary>
    /// Interaction logic for Calculate.xaml
    /// </summary>
    public partial class Calculate : Window
    {
        public Calculate()
        {
            InitializeComponent();
            AvgCTBlock.Text = $"Completion Time: {string.Format("{0:0.00}", MainWindow.Data.AvgCT)} ms";
            AvgTTBlock.Text = $"Turnaround Time: {string.Format("{0:0.00}", MainWindow.Data.AvgTT)} ms";
            AvgWTBlock.Text = $"Waiting Time: {string.Format("{0:0.00}", MainWindow.Data.AvgWT)} ms";
            AvgRTBlock.Text = $"Response Time: {string.Format("{0:0.00}", MainWindow.Data.AvgRT)} ms";

            Closed += (sender, e) => GC.Collect();

            var pes = MainWindow.Data.GetProcesses;
            DataTable.ItemsSource = pes;
            DrawBigChart(pes);
            DrawSmallChart(pes);
        }

        void DrawBigChart(ObservableCollection<Process> processes)
        {
            BigChart.Configuration.DoubleClickBenchmark = false;
            BigChart.Configuration.Quality = QualityMode.High;
            BigChart.RightClicked -= BigChart.DefaultRightClickEvent;
            var plot = BigChart.Plot;
            plot.BottomAxis.Ticks(false);
            plot.BottomAxis.Grid(false);
            plot.TopAxis.Ticks(true);
            plot.TopAxis.Grid(true);
            plot.TopAxis.Label("Time (ms)", bold: true);
            plot.TopAxis.SetBoundary(0);
            plot.TopAxis.MinimumTickSpacing(1);
            plot.YAxis.Label("PID", bold: true);
            plot.YAxis.Grid(true);
            plot.YAxis.TickLabelNotation(invertSign: true);
            plot.YAxis.MinimumTickSpacing(1);
            var barSeries = plot.AddBarSeries();
            barSeries.XAxisIndex = plot.TopAxis.AxisIndex;
            var lCPU = plot.AddBar(new double[] { 0 });
            lCPU.Label = "CPU Burst Time";
            lCPU.FillColor = System.Drawing.Color.LimeGreen;
            var lIO = plot.AddBar(new double[] { 0 });
            lIO.Label = "IO Burst Time";
            lIO.FillColor = System.Drawing.Color.Moccasin;
            uint maxTime = 0;
            List<string> pid = new();
            List<double> pos = new();
            HashSet<double> pos2 = new();
            HashSet<string> times = new();
            foreach (var p in processes)
            {
                pid.Add(p.ID.ToString());
                pos.Add(-p.Index);
                pos2.Add(0);
                pos2.Add(p.ArrivalTime);
                foreach (var time in p.CompletionTimes)
                    pos2.Add(time);
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                    pos2.Add(p.CompletionTimes[i] - p.CPUBurstTimes[i]);
                for (int i = 0; i < p.IOBurstTimes.Count; i++)
                    pos2.Add(p.CompletionTimes[i] + p.IOBurstTimes[i]);
                times.Add("0");
                times.Add(p.ArrivalTime.ToString());
                foreach (var time in p.CompletionTimes)
                    times.Add(time.ToString());
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                    times.Add((p.CompletionTimes[i] - p.CPUBurstTimes[i]).ToString());
                for (int i = 0; i < p.IOBurstTimes.Count; i++)
                    times.Add((p.CompletionTimes[i] + p.IOBurstTimes[i]).ToString());
                if (maxTime < p.CompletionTime)
                    maxTime = p.CompletionTime;
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                {
                    Bar bar = new()
                    {
                        Value = p.CompletionTimes[i],
                        ValueBase = p.CompletionTimes[i] - p.CPUBurstTimes[i],
                        FillColor = System.Drawing.Color.LimeGreen,
                        Position = -p.Index,
                        IsVertical = false,
                        Thickness = .8,
                        LineWidth = .5F,
                    };
                    barSeries.Bars.Add(bar);
                }
                for (int i = 0; i < p.IOBurstTimes.Count; i++)
                {
                    Bar bar = new()
                    {
                        Value = p.CompletionTimes[i] + p.IOBurstTimes[i],
                        ValueBase = p.CompletionTimes[i],
                        FillColor = System.Drawing.Color.Moccasin,
                        Position = -p.Index,
                        IsVertical = false,
                        Thickness = .8,
                        LineWidth = .5F,
                    };
                    barSeries.Bars.Add(bar);
                }
            }
            plot.YAxis.SetBoundary(-(processes.Count + 1.2), -0.3);
            plot.TopAxis.SetBoundary(0, maxTime);
            plot.TopAxis.SetZoomInLimit(MainWindow.Data.AvgRT / (processes.Count / 8 == 0 ? 1 : processes.Count / 8));
            plot.YAxis.SetZoomInLimit(processes.Count / 5);
            plot.YAxis.ManualTickPositions(pos.ToArray(), pid.ToArray());
            plot.TopAxis.ManualTickPositions(pos2.ToArray(), times.ToArray());
            var legend = plot.Legend(true, location: Alignment.LowerCenter);
            legend.Orientation = Orientation.Horizontal;
            legend.AntiAlias = true;
            legend.FontBold = false;
            legend.FontSize = 16;
        }

        void DrawSmallChart(ObservableCollection<Process> processes)
        {
            SmallChart.Configuration.DoubleClickBenchmark = false;
            SmallChart.Configuration.Quality = QualityMode.High;
            SmallChart.RightClicked -= SmallChart.DefaultRightClickEvent;
            var plot = SmallChart.Plot;
            plot.BottomAxis.Ticks(false);
            plot.BottomAxis.Grid(false);
            plot.TopAxis.Ticks(true);
            plot.TopAxis.Grid(true);
            plot.TopAxis.Label("Time (ms)", bold: true);
            plot.TopAxis.SetBoundary(0);
            plot.XAxis.SetBoundary(0);
            plot.TopAxis.MinimumTickSpacing(1);
            plot.XAxis.MinimumTickSpacing(1);
            plot.YAxis.Label("PID", bold: true);
            plot.YAxis.Grid(false);
            plot.YAxis.MinimumTickSpacing(1);
            var barSeries = plot.AddBarSeries();
            barSeries.XAxisIndex = plot.TopAxis.AxisIndex;
            uint maxTime = 0;
            HashSet<double> pos = new();
            HashSet<string> times = new();
            foreach (var p in processes)
            {
                pos.Add(0);
                foreach (var time in p.CompletionTimes)
                    pos.Add(time);
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                    pos.Add(p.CompletionTimes[i] - p.CPUBurstTimes[i]);
                times.Add("0");
                foreach (var time in p.CompletionTimes)
                    times.Add(time.ToString());
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                    times.Add((p.CompletionTimes[i] - p.CPUBurstTimes[i]).ToString());
                if (maxTime < p.CompletionTime)
                    maxTime = p.CompletionTime;
                for (int i = 0; i < p.CompletionTimes.Count; i++)
                {
                    Bar bar = new()
                    {
                        Value = p.CompletionTimes[i],
                        ValueBase = p.CompletionTimes[i] - p.CPUBurstTimes[i],
                        FillColor = System.Drawing.Color.LimeGreen,
                        Position = 0,
                        IsVertical = false,
                        Thickness = 1,
                        LineWidth = .5F,
                    };
                    barSeries.Bars.Add(bar);
                    var txt = plot.AddText(p.ID.ToString(), p.CompletionTimes[i] - ((double)p.CPUBurstTimes[i] / 2), 0);
                    txt.Alignment = Alignment.MiddleCenter;
                    txt.Color = System.Drawing.Color.Black;
                    txt.FontBold = true;
                }
                plot.TopAxis.ManualTickPositions(pos.ToArray(), times.ToArray());
                plot.TopAxis.Label("Time (ms)", bold: true);
                plot.TopAxis.SetBoundary(0, maxTime);
                plot.XAxis.SetBoundary(0, maxTime);
                double limit = MainWindow.Data.AvgTT / ((processes.Count / 3) == 0 ? 1 : (processes.Count / 3));
                plot.TopAxis.SetZoomInLimit(limit);
                plot.XAxis.SetZoomInLimit(limit);
                plot.TopAxis.MinimumTickSpacing(1);
                plot.YAxis.Grid(true);
                plot.YAxis.Ticks(false);
                plot.YAxis.SetBoundary(-0.5, 0.5);
                plot.YAxis.SetZoomInLimit(1);
            }
        }
    }
}
