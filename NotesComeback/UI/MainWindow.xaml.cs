using Domain;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UI
{
    public partial class MainWindow : Window
    {
        private Guid? _selectedCategoryId = null;
        private ICollectionView? _notesView;

        public MainWindow()
        {
            InitializeComponent();

            if (AppServices.NotesRepo == null || AppServices.CategoriesRepo == null)
                throw new InvalidOperationException("Сервисы не инициализированы. Вызовите AppServices.Initialize(...) при старте приложения.");

            _notesView = CollectionViewSource.GetDefaultView(AppServices.NotesRepo.Notes);
            _notesView.Filter = NoteFilter;

            NotesDataGrid.ItemsSource = _notesView;

            AppServices.NotesRepo.Notes.CollectionChanged += Repo_CollectionChanged;
            AppServices.CategoriesRepo.Categories.CollectionChanged += Repo_CollectionChanged;

            foreach (var n in AppServices.NotesRepo.Notes)
                AttachNotePropertyChanged(n);

            RefreshCategoryFilter();
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void Repo_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == AppServices.NotesRepo.Notes)
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

                SafeRefreshNotesView();
            }

            if (sender == AppServices.CategoriesRepo.Categories)
            {
                RefreshCategoryFilter();
            }
        }

        private void AttachNotePropertyChanged(Note note) => note.PropertyChanged += Note_PropertyChanged;
        private void DetachNotePropertyChanged(Note note) => note.PropertyChanged -= Note_PropertyChanged;

        private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Note.IsFavorite))
            {
                SafeRefreshNotesView();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => NotesDataGrid.Items.Refresh()),
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private IEnumerable<Category> UsedCategories =>
            AppServices.CategoriesRepo.Categories
                .Where(cat => AppServices.NotesRepo.Notes.Any(n => n.Categories.Any(c => c.Id == cat.Id)))
                .ToList();

        private void RefreshCategoryFilter()
        {
            if (CategoryFilterComboBox.SelectedItem is Category selectedCat)
                _selectedCategoryId = selectedCat.Id;
            else
                _selectedCategoryId = null;

            var items = new List<object> { new ComboBoxItem { Content = "Все категории" } };
            foreach (var cat in UsedCategories) items.Add(cat);
            CategoryFilterComboBox.ItemsSource = items;

            if (_selectedCategoryId != null)
            {
                var restored = items.OfType<Category>().FirstOrDefault(c => c.Id == _selectedCategoryId);
                if (restored != null)
                {
                    CategoryFilterComboBox.SelectedItem = restored;
                    return;
                }
            }

            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private bool NoteFilter(object obj)
        {
            if (obj is not Note n) return false;

            if (_selectedCategoryId != null)
            {
                if (!n.Categories.Any(c => c.Id == _selectedCategoryId.Value))
                    return false;
            }

            if (FavoritesFilterCheckBox.IsChecked == true && !n.IsFavorite)
                return false;

            return true;
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CategoryFilterComboBox.SelectedItem;
            if (selected is Category cat) _selectedCategoryId = cat.Id;
            else _selectedCategoryId = null;

            SafeRefreshNotesView();
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
            // Если мы дошли сюда — клик был не по CheckBox, открываем окно
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
                    AppServices.NotesRepo.Remove(note);
                    RefreshCategoryFilter();
                }
            }
            else
            {
                MessageBox.Show("Выберите заметку для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StatisticsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new StatisticsWindow();
            window.ShowDialog();
        }

        private void FavoritesFilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            SafeRefreshNotesView();
        }

        private void SafeRefreshNotesView()
        {
            _notesView?.Refresh();
        }

        private void NotesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Используем Dispatcher, чтобы Refresh произошёл после завершения редактирования
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _notesView?.Refresh();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
