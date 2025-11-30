using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Domain;

namespace UI
{
    public partial class MainWindow : Window
    {
        // Храним id выбранной категории (null = Все категории)
        private Guid? _selectedCategoryId = null;
        private ICollectionView? _notesView;

        public MainWindow()
        {
            InitializeComponent();

            // Предполагаем, что DataStore.Notes это ObservableCollection<Note>
            _notesView = CollectionViewSource.GetDefaultView(DataStore.Notes);
            _notesView.Filter = NoteFilter;

            // ItemsSource должен быть view чтобы фильтр применялся
            NotesDataGrid.ItemsSource = _notesView;

            // Подписка на изменение коллекций
            DataStore.Notes.CollectionChanged += DataStore_CollectionChanged;
            DataStore.Categories.CollectionChanged += DataStore_CollectionChanged;

            // Подпишемся на уже существующие заметки (чтобы отлавливать изменение IsFavorite)
            foreach (var n in DataStore.Notes.OfType<Note>())
                AttachNotePropertyChanged(n);

            RefreshCategoryFilter();

            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void DataStore_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Если изменились заметки — подпишемся на новые и отпишемся от удалённых
            if (sender == DataStore.Notes)
            {
                if (e.OldItems != null)
                {
                    foreach (Note oldN in e.OldItems.OfType<Note>())
                        DetachNotePropertyChanged(oldN);
                }

                if (e.NewItems != null)
                {
                    foreach (Note newN in e.NewItems.OfType<Note>())
                        AttachNotePropertyChanged(newN);
                }

                _notesView?.Refresh();
            }

            // Если изменились категории — обновляем фильтр списка категорий
            if (sender == DataStore.Categories)
            {
                RefreshCategoryFilter();
            }
        }

        private void AttachNotePropertyChanged(Note note)
        {
            note.PropertyChanged += Note_PropertyChanged;
        }

        private void DetachNotePropertyChanged(Note note)
        {
            note.PropertyChanged -= Note_PropertyChanged;
        }

        private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // когда изменился избранный статус — обновляем фильтр (и DataGrid)
            if (e.PropertyName == nameof(Note.IsFavorite))
            {
                Dispatcher.Invoke(() => { _notesView?.Refresh(); });
            }
            else
            {
                // для других свойств при необходимости можно только обновить грид
                Dispatcher.Invoke(() => NotesDataGrid.Items.Refresh());
            }
        }

        // Категории, используемые хотя бы в одной заметке
        private IEnumerable<Category> UsedCategories =>
            DataStore.Categories
                     .Where(cat => DataStore.Notes.Any(n => n.Categories.Any(c => c.Id == cat.Id)))
                     .ToList();

        private void RefreshCategoryFilter()
        {
            // Сохраняем выбранную категорию
            if (CategoryFilterComboBox.SelectedItem is Category selectedCat)
                _selectedCategoryId = selectedCat.Id;
            else
                _selectedCategoryId = null;

            var items = new List<object>();
            items.Add(new ComboBoxItem { Content = "Все категории" });

            foreach (var cat in UsedCategories)
                items.Add(cat);

            CategoryFilterComboBox.ItemsSource = items;

            // Восстанавливаем выбор
            if (_selectedCategoryId != null)
            {
                var restored = items.OfType<Category>().FirstOrDefault(c => c.Id == _selectedCategoryId);
                if (restored != null)
                {
                    CategoryFilterComboBox.SelectedItem = restored;
                    return;
                }
            }

            // Иначе выбираем "Все категории"
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private bool NoteFilter(object obj)
        {
            if (obj is not Note n) return false;

            // Фильтр по категории (если выбрана конкретная категория)
            if (_selectedCategoryId != null)
            {
                if (!n.Categories.Any(c => c.Id == _selectedCategoryId.Value))
                    return false;
            }

            // Фильтр по избранному чекбоксу сверху
            if (FavoritesFilterCheckBox.IsChecked == true && !n.IsFavorite)
                return false;

            return true;
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CategoryFilterComboBox.SelectedItem;

            if (selected is Category cat)
            {
                _selectedCategoryId = cat.Id;
            }
            else
            {
                _selectedCategoryId = null;
            }

            _notesView?.Refresh();
        }

        private void CreateNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new NoteWindow();
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                RefreshCategoryFilter();
            }
        }

        private void NotesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note note)
            {
                var nw = new NoteWindow(note);
                nw.Owner = this;

                if (nw.ShowDialog() == true)
                {
                    NotesDataGrid.Items.Refresh();
                    RefreshCategoryFilter();
                }
            }
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note note)
            {
                var result = MessageBox.Show(
                    $"Удалить заметку \"{note.Title}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DataStore.Notes.Remove(note);
                    RefreshCategoryFilter();
                }
            }
            else
            {
                MessageBox.Show("Выберите заметку для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Note CreateNoteWithDate(string title, string content, DateTime date)
        {
            var note = new Note(title, content);

            // Находим закрытое поле CreatedAt
            var field = typeof(Note).GetField("<CreatedAt>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (field != null)
                field.SetValue(note, date);

            return note;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            // для теста
            DataStore.Notes.Clear();

            DataStore.Notes.Add(CreateNoteWithDate("Январь", "Тест", new DateTime(2025, 1, 15)));
            DataStore.Notes.Add(CreateNoteWithDate("Январь2", "Тест", new DateTime(2027, 1, 16)));
            DataStore.Notes.Add(CreateNoteWithDate("Февраль", "Тест", new DateTime(2026, 2, 10)));
            DataStore.Notes.Add(CreateNoteWithDate("Март", "Тест", new DateTime(2025, 3, 5)));
            DataStore.Notes.Add(CreateNoteWithDate("Апрель1", "Тест", new DateTime(2024, 4, 12)));
            DataStore.Notes.Add(CreateNoteWithDate("Апрель2", "Тест", new DateTime(2024, 4, 12)));
            DataStore.Notes.Add(CreateNoteWithDate("Апрель3", "Тест", new DateTime(2025, 4, 13)));
            DataStore.Notes.Add(CreateNoteWithDate("Май", "Тест", new DateTime(2025, 5, 22)));

            // если нужно можно выставить избранные для проверки
            if (DataStore.Notes.Count > 0)
                DataStore.Notes[0].IsFavorite = true;
        }

        private void StatisticsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new StatisticsWindow();
            window.ShowDialog();
        }

        private void FavoritesFilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _notesView?.Refresh();
        }
    }
}
