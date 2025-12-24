using Domain.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Services;
using System.Windows;

namespace UI
{
    public partial class StatisticsWindow : Window
    {
        private readonly StatisticsService _statisticsService;
        private readonly NoteService _noteService;
        private readonly CategoryService _categoryService;

        private bool _categorySortDescending = true;

        public StatisticsWindow(StatisticsService statisticsService,
                               NoteService noteService,
                               CategoryService categoryService)
        {
            InitializeComponent();
            _statisticsService = statisticsService;
            _noteService = noteService;
            _categoryService = categoryService;

            InitializeDatePickers();
            LoadCharts();
        }

        private void InitializeDatePickers()
        {
            var oneYearAgo = DateTime.Now.AddYears(-1);
            MonthFromDatePicker.SelectedDate = new DateTime(oneYearAgo.Year, oneYearAgo.Month, 1);
            MonthToDatePicker.SelectedDate = DateTime.Now;
        }

        private void LoadCharts()
        {
            UpdateMonthChart();
            UpdateCategoryChart();
        }

        #region Месячная статистика

        private void UpdateMonthChart()
        {
            try
            {
                var fromDate = MonthFromDatePicker.SelectedDate ?? DateTime.Now.AddYears(-1);
                var toDate = MonthToDatePicker.SelectedDate ?? DateTime.Now;

                if (fromDate > toDate)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var monthlyData = _statisticsService.GetNotesByMonth(fromDate, toDate);

                if (!monthlyData.Any())
                {
                    MessageBox.Show("Нет данных для отображения за выбранный период", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var chartData = monthlyData.Select(m => new
                {
                    Label = $"{m.GetMonthName()} {m.Year}",
                    MonthName = m.GetMonthName(),
                    Year = m.Year,
                    Count = m.Total,
                    Value = m.Total
                }).ToList();

                UpdateMonthLegend(chartData);

                CreateMonthPieChart(chartData, fromDate, toDate);

                UpdateMonthSummary(chartData, fromDate, toDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении диаграммы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMonthLegend(IEnumerable<dynamic> data)
        {
            var legendItems = data.Select(d => new
            {
                MonthName = d.Label,
                Total = d.Count
            }).ToList();

            MonthLegendItemsControl.ItemsSource = legendItems;
        }

        private void CreateMonthPieChart(IEnumerable<dynamic> data, DateTime fromDate, DateTime toDate)
        {
            var plotModel = new PlotModel
            {
                Title = $"Распределение заметок по месяцам\n({fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy})",
                TitleFontSize = 14,
                TitlePadding = 15
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0,
                FontSize = 11,
                InsideLabelFormat = "{2:F1}%",
                OutsideLabelFormat = "{0}: {1:F0}"
            };

            var colors = GenerateColors(data.Count());

            int colorIndex = 0;
            foreach (var item in data)
            {
                var slice = new PieSlice(item.Label, item.Count)
                {
                    IsExploded = false,
                    Fill = colors[colorIndex % colors.Count]
                };
                pieSeries.Slices.Add(slice);
                colorIndex++;
            }

            plotModel.Series.Add(pieSeries);

            MonthPlotView.Model = plotModel;
        }

        private void UpdateMonthSummary(IEnumerable<dynamic> data, DateTime fromDate, DateTime toDate)
        {
            var totalNotes = data.Sum(d => d.Count);
            var monthCount = data.Count();
            var averagePerMonth = monthCount > 0 ? (double)totalNotes / monthCount : 0;

            var maxMonth = data.OrderByDescending(d => d.Count).FirstOrDefault();
            var minMonth = data.OrderBy(d => d.Count).FirstOrDefault();

            var summary = $"Период: {fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy}\n" +
                         $"Всего заметок: {totalNotes}\n" +
                         $"Количество месяцев: {monthCount}\n" +
                         $"Среднее в месяц: {averagePerMonth:F1}\n\n";

            if (maxMonth != null && minMonth != null && totalNotes > 0)
            {
                summary += $"Наиболее продуктивный месяц:\n{maxMonth?.Label}: {maxMonth?.Count} заметок\n\n" +
                          $"Наименее продуктивный месяц:\n{minMonth?.Label}: {minMonth?.Count} заметок";
            }

            MonthSummaryText.Text = summary;
        }

        #endregion

        #region Статистика по категориям

        private void UpdateCategoryChart()
        {
            try
            {
                var categoryData = _statisticsService.GetCategoryStatistics(_categorySortDescending);

                if (!categoryData.Any())
                {
                    MessageBox.Show("Нет категорий для отображения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                UpdateCategoryLegend(categoryData);

                CreateCategoryBarChart(categoryData);

                UpdateCategorySummary(categoryData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статистики категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategoryLegend(List<CategoryStatItem> data)
        {
            var legendItems = data.Select(item => new
            {
                CategoryName = item.CategoryName,
                NoteCount = item.NoteCount,
                DisplayText = $"{item.CategoryName} ({item.NoteCount})"
            }).ToList();

            CategoryLegendItemsControl.ItemsSource = legendItems;
        }

        private void CreateCategoryBarChart(List<CategoryStatItem> data)
        {
            var plotModel = new PlotModel
            {
                Title = "Использование категорий",
                TitleFontSize = 14,
                PlotMargins = new OxyThickness(60, 40, 40, 80)
            };

            var barSeries = new BarSeries
            {
                Title = "Количество заметок",
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0}",
                FontSize = 10,
                FillColor = OxyColor.FromRgb(70, 130, 180),
                StrokeColor = OxyColors.White,
                StrokeThickness = 1
            };

            foreach (var category in data)
            {
                barSeries.Items.Add(new BarItem(category.NoteCount));
            }

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = "Категории",
                TitleFontSize = 11,
                GapWidth = 0.5
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Количество заметок",
                TitleFontSize = 11,
                Minimum = 0,
                MinimumPadding = 0.05,
                MaximumPadding = 0.1,
                MajorStep = CalculateStep(data.Max(d => d.NoteCount)),
                MinorStep = 1
            };

            foreach (var category in data)
            {
                categoryAxis.Labels.Add(TruncateString(category.CategoryName, 20));
            }

            plotModel.Axes.Add(categoryAxis);
            plotModel.Axes.Add(valueAxis);
            plotModel.Series.Add(barSeries);

            CategoryPlotView.Model = plotModel;
        }

        private void UpdateCategorySummary(List<CategoryStatItem> data)
        {
            var summary = _statisticsService.GetCategoryStatSummary();

            var summaryText = $"Всего категорий: {summary.TotalCategories}\n" +
                            $"Используемых: {summary.UsedCategories}\n" +
                            $"Неиспользуемых: {summary.UnusedCategories}\n" +
                            $"Всего заметок в категориях: {summary.TotalNotesInCategories}\n\n";

            if (summary.MostPopularCategoryCount > 0)
            {
                summaryText += $"Самая популярная категория:\n" +
                             $"{summary.MostPopularCategoryName} ({summary.MostPopularCategoryCount} заметок)";
            }
            else
            {
                summaryText += "Нет заметок в категориях";
            }

            CategorySummaryText.Text = summaryText;
        }

        #endregion

        private List<OxyColor> GenerateColors(int count)
        {
            var colors = new List<OxyColor>
            {
                OxyColor.FromRgb(65, 105, 225),
                OxyColor.FromRgb(220, 20, 60),
                OxyColor.FromRgb(34, 139, 34),
                OxyColor.FromRgb(255, 140, 0),
                OxyColor.FromRgb(138, 43, 226),
                OxyColor.FromRgb(255, 69, 0),
                OxyColor.FromRgb(50, 205, 50),
                OxyColor.FromRgb(255, 105, 180),
                OxyColor.FromRgb(30, 144, 255),
                OxyColor.FromRgb(255, 215, 0),
                OxyColor.FromRgb(0, 206, 209),
                OxyColor.FromRgb(186, 85, 211)
            };

            if (count > colors.Count)
            {
                var additionalColors = GenerateAdditionalColors(count - colors.Count);
                colors.AddRange(additionalColors);
            }

            return colors.Take(count).ToList();
        }

        private List<OxyColor> GenerateAdditionalColors(int count)
        {
            var additionalColors = new List<OxyColor>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var r = random.Next(50, 200);
                var g = random.Next(50, 200);
                var b = random.Next(50, 200);

                additionalColors.Add(OxyColor.FromRgb((byte)r, (byte)g, (byte)b));
            }

            return additionalColors;
        }

        private double CalculateStep(double maxValue)
        {
            if (maxValue <= 5) return 1;
            if (maxValue <= 10) return 2;
            if (maxValue <= 20) return 5;
            if (maxValue <= 50) return 10;
            if (maxValue <= 100) return 20;
            return Math.Ceiling(maxValue / 10);
        }

        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength - 3) + "...";
        }

        private void SortDescButton_Click(object sender, RoutedEventArgs e)
        {
            _categorySortDescending = true;
            SortDescButton.IsEnabled = false;
            SortAscButton.IsEnabled = true;
            UpdateCategoryChart();
        }

        private void SortAscButton_Click(object sender, RoutedEventArgs e)
        {
            _categorySortDescending = false;
            SortDescButton.IsEnabled = true;
            SortAscButton.IsEnabled = false;
            UpdateCategoryChart();
        }

        private void RefreshCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCategoryChart();
        }

        private void UpdateMonthChartButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateMonthChart();
        }

        private void CurrentYearButton_Click(object sender, RoutedEventArgs e)
        {
            MonthFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
            MonthToDatePicker.SelectedDate = DateTime.Now;
            UpdateMonthChart();
        }

        private void AllTimeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allNotes = _noteService.GetNotes(null);
                if (allNotes.Any())
                {
                    var earliestDate = allNotes.Min(n => n.CreatedAt);
                    MonthFromDatePicker.SelectedDate = new DateTime(earliestDate.Year, earliestDate.Month, 1);
                    MonthToDatePicker.SelectedDate = DateTime.Now;
                    UpdateMonthChart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}