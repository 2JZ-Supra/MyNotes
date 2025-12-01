using Domain;
using Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UI
{
    public partial class StatisticsWindow : Window
    {

        private readonly Brush[] _palette = new Brush[]
        {
            Brushes.CadetBlue, Brushes.Coral, Brushes.SteelBlue, Brushes.MediumSeaGreen,
            Brushes.Orange, Brushes.MediumPurple, Brushes.Goldenrod, Brushes.SlateGray,
            Brushes.Tomato, Brushes.OliveDrab, Brushes.MediumVioletRed, Brushes.CornflowerBlue
        };

        // Для дебаунса / предотвращения множественных отрисовок
        private bool _drawScheduled = false;
        private Dictionary<string, int>? _pendingData = null;

        public StatisticsWindow()
        {
            InitializeComponent();
            Loaded += StatisticsWindow_Loaded;
            SizeChanged += StatisticsWindow_SizeChanged;
        }

        private bool _categoryCanvasDrawn = false;

        private void StatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshStatistics();
            InitFilters();
            RefreshStatistics();

            // Подпишемся на LayoutUpdated для корректного центрирования сразу
            CategoryBarCanvas.LayoutUpdated += CategoryBarCanvas_LayoutUpdated;
        }

        private void CategoryBarCanvas_LayoutUpdated(object? sender, EventArgs e)
        {
            // Рисуем только один раз после измерения Canvas
            if (_categoryCanvasDrawn) return;
            if (CategoryBarCanvas.ActualWidth <= 0) return;

            RefreshCategoryStatistics();
            _categoryCanvasDrawn = true;

            // После первого удачного центрирования — отписываемся
            CategoryBarCanvas.LayoutUpdated -= CategoryBarCanvas_LayoutUpdated;
        }

        private void InitFilters()
        {
            // Годы заметок
            var years = AppServices.NotesRepo.Notes
                .Select(n => n.CreatedAt.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            if (!years.Any())
                years.Add(DateTime.Now.Year);

            YearFromCombo.ItemsSource = years;
            YearToCombo.ItemsSource = years;

            YearFromCombo.SelectedIndex = 0;
            YearToCombo.SelectedIndex = years.Count - 1;

            // Месяцы — теперь MonthItem
            var monthNames = Enumerable.Range(1, 12)
                .Select(m => new MonthItem
                {
                    Number = m,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();

            MonthFromCombo.ItemsSource = monthNames;
            MonthToCombo.ItemsSource = monthNames;

            MonthFromCombo.DisplayMemberPath = "Name";
            MonthToCombo.DisplayMemberPath = "Name";

            MonthFromCombo.SelectedIndex = 0;
            MonthToCombo.SelectedIndex = 11;
        }




        private void StatisticsWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Просто обновляем статистику — реальная отрисовка будет дебаунситься
            RefreshStatistics();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshStatistics();
        }

        public class MonthItem
        {
            public int Number { get; set; }
            public string Name { get; set; } = "";
        }

        private IEnumerable<Note> ApplyFilters(IEnumerable<Note> notes)
        {
            if (YearFromCombo.SelectedItem is int yearFrom &&
                YearToCombo.SelectedItem is int yearTo &&
                MonthFromCombo.SelectedItem is MonthItem monthFrom &&
                MonthToCombo.SelectedItem is MonthItem monthTo)
            {
                return notes.Where(n =>
                    n.CreatedAt.Year >= yearFrom &&
                    n.CreatedAt.Year <= yearTo &&
                    n.CreatedAt.Month >= monthFrom.Number &&
                    n.CreatedAt.Month <= monthTo.Number
                );
            }

            return notes;
        }



        private void RefreshStatistics()
        {
            NotesCountTextBlock.Text = $"Заметок: {AppServices.NotesRepo.Notes.Count}";
            CategoriesCountTextBlock.Text = $"Категорий: {AppServices.CategoriesRepo.Categories.Count}";

            var filtered = ApplyFilters(AppServices.NotesRepo.Notes);

            var byMonth = filtered
                .GroupBy(n => n.CreatedAt.Month)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    g => g.Count()
                );

            // Подготовим данные для отложенной отрисовки
            _pendingData = byMonth;

            // Если отрисовка уже запланирована — ничего не делаем (выполнится одна отрисовка).
            if (_drawScheduled) return;

            _drawScheduled = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Если данных нет — всё очистим и выйдем
                    if (_pendingData == null || !_pendingData.Any() || _pendingData.Values.Sum() == 0)
                    {
                        PieCanvas.Children.Clear();
                        LegendPanel.Children.Clear();
                        LegendPanel.Children.Add(new TextBlock { Text = "Нет заметок для отображения.", Margin = new Thickness(8) });
                        return;
                    }

                    DrawPieChart(_pendingData);
                }
                finally
                {
                    // сбрасываем флаг, следующий RefreshStatistics сможет снова планировать отрисовку
                    _drawScheduled = false;
                }
            }), DispatcherPriority.Loaded);
        }

        private void DrawPieChart(Dictionary<string, int> data)
        {
            // Очистка перед рисованием
            PieCanvas.Children.Clear();
            LegendPanel.Children.Clear();

            // Берём реальные доступные размеры (может быть 0 если ещё не измерено)
            double availW = PieCanvas.ActualWidth;
            double availH = PieCanvas.ActualHeight;

            // Если Actual ещё не установлен — используем окно как ориентир
            if (availW <= 0) availW = Math.Max(300, this.ActualWidth - 360);
            if (availH <= 0) availH = Math.Max(300, this.ActualHeight - 160);

            // Выбираем квадратную область: размер = min(availW, availH), но не меньше 300
            double size = Math.Max(300, Math.Min(availW, availH));
            PieCanvas.Width = size;
            PieCanvas.Height = size;

            // Центр и радиус зависят от этого квадратного размера
            double centerX = size / 2.0;
            double centerY = size / 2.0;
            double radius = Math.Max(10, (size / 2.0) - 24); // запас для подписей

            int total = data.Values.Sum();
            double startAngle = -90.0;
            int colorIndex = 0;

            foreach (var kv in data)
            {
                string label = kv.Key;
                int value = kv.Value;
                double sweep = (double)value / total * 360.0;
                if (value <= 0) { startAngle += sweep; colorIndex++; continue; }

                double startRad = DegreesToRadians(startAngle);
                double endRad = DegreesToRadians(startAngle + sweep);

                Point startPt = new Point(centerX + radius * Math.Cos(startRad), centerY + radius * Math.Sin(startRad));
                Point endPt = new Point(centerX + radius * Math.Cos(endRad), centerY + radius * Math.Sin(endRad));
                bool isLargeArc = sweep > 180.0;

                var sg = new StreamGeometry();
                using (var ctx = sg.Open())
                {
                    ctx.BeginFigure(new Point(centerX, centerY), true, true);
                    ctx.LineTo(startPt, true, true);
                    ctx.ArcTo(endPt, new Size(radius, radius), 0.0, isLargeArc, SweepDirection.Clockwise, true, true);
                    ctx.LineTo(new Point(centerX, centerY), true, true);
                }
                sg.Freeze();

                var fillBrush = _palette[colorIndex % _palette.Length];
                var path = new Path
                {
                    Data = sg,
                    Fill = fillBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.2
                };
                PieCanvas.Children.Add(path);

                // Процент в середине слайса
                double midAngle = startAngle + sweep / 2.0;
                double midRad = DegreesToRadians(midAngle);
                double labelRadius = radius * 0.58;
                double labelX = centerX + labelRadius * Math.Cos(midRad);
                double labelY = centerY + labelRadius * Math.Sin(midRad);

                double percent = Math.Round((double)value / total * 100, 1);
                var sliceText = new TextBlock
                {
                    Text = $"{percent}%",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = Math.Max(12, Math.Min(18, radius / 12.0)),
                    Foreground = Brushes.Black
                };
                sliceText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var textSize = sliceText.DesiredSize;
                Canvas.SetLeft(sliceText, labelX - textSize.Width / 2);
                Canvas.SetTop(sliceText, labelY - textSize.Height / 2);
                PieCanvas.Children.Add(sliceText);

                // Легенда справа
                var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4) };
                var rect = new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = fillBrush,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                var legendText = new TextBlock
                {
                    Text = $"{label}: {value} ({percent}%)",
                    VerticalAlignment = VerticalAlignment.Center
                };
                legendItem.Children.Add(rect);
                legendItem.Children.Add(legendText);
                LegendPanel.Children.Add(legendItem);

                startAngle += sweep;
                colorIndex++;
            }
        }

        private bool _filtersExpanded = false;

        private void FiltersHeader_Click(object sender, MouseButtonEventArgs e)
        {
            _filtersExpanded = !_filtersExpanded;

            FiltersPanel.Visibility = _filtersExpanded
                ? Visibility.Visible
                : Visibility.Collapsed;

            FiltersArrow.Text = _filtersExpanded ? "▲" : "▼";
        }


        private double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshCategoryStatistics()
        {
            CategoryBarCanvas.Children.Clear();
            CategoryLegendPanel.Children.Clear();

            var groups = AppServices.NotesRepo.Notes
                .SelectMany(n => n.Categories)
                .GroupBy(c => c.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            if (!groups.Any())
            {
                CategoryBarCanvas.Children.Add(new TextBlock
                {
                    Text = "Нет заметок для отображения",
                    Margin = new Thickness(8),
                    FontWeight = FontWeights.Bold
                });
                return;
            }

            // Сортировка
            if (CategorySortCombo.SelectedIndex == 0)
                groups = groups.OrderByDescending(g => g.Count).ToList();
            else
                groups = groups.OrderBy(g => g.Count).ToList();

            double barWidth = 60;
            double barSpacing = 20;
            double maxHeight = 250;
            double maxValue = groups.Max(g => g.Count);

            CategoryBarCanvas.Height = maxHeight + 40;

            // Получаем ScrollViewer родитель
            if (!(CategoryBarCanvas.Parent is ScrollViewer scrollViewer)) return;

            // Полная ширина всех столбцов
            double totalWidth = groups.Count * barWidth + (groups.Count - 1) * barSpacing;

            // Видимая ширина ScrollViewer
            double visibleWidth = scrollViewer.ActualWidth;
            if (visibleWidth <= 0)
            {
                scrollViewer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                visibleWidth = scrollViewer.ActualWidth > 0 ? scrollViewer.ActualWidth : 400;
            }

            // Центрирование или скролл
            double startX;
            if (totalWidth < visibleWidth)
            {
                CategoryBarCanvas.Width = visibleWidth;
                startX = (visibleWidth - totalWidth) / 2.0;
            }
            else
            {
                CategoryBarCanvas.Width = totalWidth;
                startX = 0;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                var item = groups[i];
                double barHeight = (item.Count / maxValue) * maxHeight;

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = _palette[i % _palette.Length],
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, startX + i * (barWidth + barSpacing));
                Canvas.SetTop(rect, maxHeight - barHeight);
                CategoryBarCanvas.Children.Add(rect);

                var label = new TextBlock
                {
                    Text = item.Category,
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center
                };
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double labelX = startX + i * (barWidth + barSpacing) + barWidth / 2 - label.DesiredSize.Width / 2;
                double labelY = maxHeight + 4;
                Canvas.SetLeft(label, labelX);
                Canvas.SetTop(label, labelY);
                CategoryBarCanvas.Children.Add(label);

                var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4) };
                var colorRect = new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = rect.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                var legendText = new TextBlock
                {
                    Text = $"{item.Category} ({item.Count})",
                    VerticalAlignment = VerticalAlignment.Center
                };
                legendItem.Children.Add(colorRect);
                legendItem.Children.Add(legendText);
                CategoryLegendPanel.Children.Add(legendItem);
            }
        }


        private void CategorySortCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshCategoryStatistics();
        }
    }
}
