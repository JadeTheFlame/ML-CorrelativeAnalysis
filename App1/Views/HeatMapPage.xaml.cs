using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Linq;
using ViewModels;
using Windows.UI.Xaml.Controls;

namespace MachineLearning
{
    public sealed partial class HeatMapPage : Page
    {
        public HeatMapPage()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (System.Exception e)
            {

                throw;
            }
            this.DataContext = new HeatMapPageViewModel();

            Loaded += Page_Loaded;
        }

        private HeatMapPageViewModel ViewModel => DataContext as HeatMapPageViewModel;

        // Reference plot at
        // https://tryolabs.com/blog/2017/03/16/pandas-seaborn-a-guide-to-handle-visualize-data-elegantly/
        // Minor difference, since we did not compensate missing Age values.

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Read data
            var matrix = await ViewModel.LoadCorrelationData();

            // Prepare diagram
            var plotModel = PrepareDiagram(matrix.data.Select(element => element.Key).ToArray());

            // Populate diagram
            var data = new double[matrix.data.Count, matrix.data.Count];
            int x = 0;
            foreach (var columnStart in matrix.data)
            {
                int y = 0;
                foreach (var columnEnd in matrix.data.Reverse())
                {
                    var seriesA = columnStart.Value;
                    var seriesB = columnEnd.Value;

                    var value = Statistics.Pearson(seriesA, seriesB);

                    data[x, y] = value;
                    data[(matrix.data.Count - 1) - y, (matrix.data.Count - 1) - x] = value;
                    ++y;
                }

                data[x, (matrix.data.Count - 1) - x] = 1;
                ++x;
            }

            (plotModel.Series[0] as HeatMapSeries).Data = data;

            // Update diagram
            Diagram.InvalidatePlot();
        }

        private PlotModel PrepareDiagram(string[] labels)
        {
            var foreground = OxyColors.SteelBlue;

            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground,
                TextColor = foreground,
                TitleColor = foreground,
                SubtitleColor = foreground
            };

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "HorizontalAxis",
                ItemsSource = labels,
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "VerticalAxis",
                ItemsSource = labels.Reverse(),
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            });

            plotModel.Axes.Add(new LinearColorAxis
            {
                // Pearson color scheme from blue over white to red.
                Palette = OxyPalettes.BlueWhiteRed31,
                Position = AxisPosition.Top,
                Minimum = -1,
                Maximum = 1,
                TicklineColor = OxyColors.Transparent
            });

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 4,
                Y0 = 0,
                Y1 = 4,
                XAxisKey = "HorizontalAxis",
                YAxisKey = "VerticalAxis",
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.12,
                LabelFormatString = ".00"
            };

            plotModel.Series.Add(heatMapSeries);

            try
            {
                Diagram.Model = plotModel;
            }
            catch (System.Exception)
            {
            }

            return plotModel;
        }
    }
}
