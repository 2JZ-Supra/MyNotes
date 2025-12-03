using Domain;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace UI
{
    public partial class MainWindow : Window
    {
        private Guid? _selectedCategoryId = null;
        private ICollectionView? _notesView;

        // Локальная коллекция UI
        private ObservableCollection<Note> Notes { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            if (AppServices.NotesRepo == null || AppServices.CategoriesRepo == null)
                throw new InvalidOperationException("Сервисы не инициализированы.");

            // Подписываемся на события репозиториев
            AppServices.NotesRepo.NotesChanged += NotesRepo_NotesChanged;
            AppServices.CategoriesRepo.CategoriesChanged += CategoriesRepo_CategoriesChanged;

            // Загружаем начальные данные из БД
            LoadNotesFromRepo();

            _notesView = CollectionViewSource.GetDefaultView(Notes);
            _notesView.Filter = NoteFilter;

            NotesDataGrid.ItemsSource = _notesView;

            foreach (var n in Notes)
                AttachNotePropertyChanged(n);

            RefreshCategoryFilter();
            CategoryFilterComboBox.SelectedIndex = 0;
        }


        // Загрузка коллекции заметок из репозитория
        private void LoadNotesFromRepo()
        {
            Notes.Clear();
            var loaded = AppServices.NotesRepo.GetAll();
            foreach (var n in loaded)
                Notes.Add(n);
        }

        private void NotesRepo_NotesChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadNotesFromRepo();
                _notesView?.Refresh();
                RefreshCategoryFilter();
            });
        }

        private void CategoriesRepo_CategoriesChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => RefreshCategoryFilter());
        }

        // свойства заметок
        private void AttachNotePropertyChanged(Note note) =>
            note.PropertyChanged += Note_PropertyChanged;

        private void DetachNotePropertyChanged(Note note) =>
            note.PropertyChanged -= Note_PropertyChanged;

        private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Note.IsFavorite))
            {
                SafeRefreshNotesView();
            }
            else
            {
                NotesDataGrid.Items.Refresh();
            }
        }

        private void SafeRefreshNotesView() =>
            _notesView?.Refresh();


        // категории
        private IEnumerable<Category> UsedCategories =>
            AppServices.CategoriesRepo.GetAll()
                .Where(cat => Notes.Any(n => n.Categories.Any(c => c.Id == cat.Id)))
                .ToList();

        private void RefreshCategoryFilter()
        {
            if (CategoryFilterComboBox.SelectedItem is Category selectedCat)
                _selectedCategoryId = selectedCat.Id;
            else
                _selectedCategoryId = null;

            var items = new List<object>
            {
                new ComboBoxItem { Content = "Все категории" }
            };

            foreach (var cat in UsedCategories)
                items.Add(cat);

            CategoryFilterComboBox.ItemsSource = items;

            if (_selectedCategoryId != null)
            {
                var restored = items
                    .OfType<Category>()
                    .FirstOrDefault(c => c.Id == _selectedCategoryId);

                if (restored != null)
                {
                    CategoryFilterComboBox.SelectedItem = restored;
                    return;
                }
            }

            CategoryFilterComboBox.SelectedIndex = 0;
        }


        // фильтр заметок
        private bool NoteFilter(object obj)
        {
            if (obj is not Note n) return false;

            if (_selectedCategoryId != null &&
                !n.Categories.Any(c => c.Id == _selectedCategoryId.Value))
                return false;

            if (FavoritesFilterCheckBox.IsChecked == true &&
                !n.IsFavorite)
                return false;

            return true;
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CategoryFilterComboBox.SelectedItem;

            if (selected is Category cat)
                _selectedCategoryId = cat.Id;
            else
                _selectedCategoryId = null;

            SafeRefreshNotesView();
        }


        // создание, редактирование, удаление

        private void CreateNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new NoteWindow { Owner = this };

            if (window.ShowDialog() == true)
                RefreshCategoryFilter();
        }

        private void NotesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note note)
            {
                var win = new NoteWindow(note) { Owner = this };

                if (win.ShowDialog() == true)
                {
                    NotesDataGrid.Items.Refresh();
                    RefreshCategoryFilter();
                }
            }
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is not Note note)
            {
                MessageBox.Show("Выберите заметку.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить \"{note.Title}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AppServices.NotesRepo.Remove(note);
            }
        }


        private void FavoritesFilterCheckBox_Changed(object sender, RoutedEventArgs e) =>
            SafeRefreshNotesView();

        private void NotesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _notesView?.Refresh();
            }));
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            new StatisticsWindow().ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) =>
            Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
