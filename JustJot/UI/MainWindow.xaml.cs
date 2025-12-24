using Domain;
using Domain.Filters;
using Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    public partial class MainWindow : Window
    {
        private readonly NoteService _noteService;
        private readonly CategoryService _categoryService;
        private readonly StatisticsService _statisticsService;
        private List<Note>? _allNotes;
        private List<Category>? _allCategories;

        public MainWindow(NoteService noteService, CategoryService categoryService, StatisticsService statisticsService)
        {
            InitializeComponent();

            _noteService = noteService;
            _categoryService = categoryService;
            _statisticsService = statisticsService;

            NotesDataGrid.MouseDoubleClick += NotesDataGrid_MouseDoubleClick;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allNotes = _noteService.GetNotes(null);
                _allCategories = _categoryService.GetCategories(null);

                UpdateCategoryComboBox();
                DisplayNotes(_allNotes);

                EnableControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DisableControls();
            }
        }

        private void EnableControls()
        {
            NewNoteButton.IsEnabled = true;
            CategoriesButton.IsEnabled = true;
            StatisticsButton.IsEnabled = true;
            DeleteNoteButton.IsEnabled = NotesDataGrid.SelectedItem != null;
            CategoryComboBox.IsEnabled = true;
            FavoriteCheckBox.IsEnabled = true;
            NotesDataGrid.IsEnabled = true;
        }

        private void DisableControls()
        {
            NewNoteButton.IsEnabled = false;
            CategoriesButton.IsEnabled = false;
            StatisticsButton.IsEnabled = false;
            DeleteNoteButton.IsEnabled = false;
            CategoryComboBox.IsEnabled = false;
            FavoriteCheckBox.IsEnabled = false;
            NotesDataGrid.IsEnabled = false;
        }

        private void UpdateCategoryComboBox()
        {
            CategoryComboBox.Items.Clear();

            CategoryComboBox.Items.Add(new ComboBoxItem
            {
                Content = "Все категории",
                Tag = null
            });

            if (_allNotes != null && _allCategories != null)
            {
                var usedCategoryIds = _allNotes
                    .Where(note => note.Categories != null && note.Categories.Any())
                    .SelectMany(note => note.Categories.Select(c => c.Id))
                    .Distinct()
                    .ToList();

                var usedCategories = _allCategories
                    .Where(category => usedCategoryIds.Contains(category.Id))
                    .OrderBy(category => category.Name)
                    .ToList();

                foreach (var category in usedCategories)
                {
                    CategoryComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = category.Name,
                        Tag = category.Id
                    });
                }
            }

            CategoryComboBox.SelectedIndex = 0;
        }

        private void DisplayNotes(List<Note> notes)
        {
            NotesDataGrid.ItemsSource = notes;

            if (!notes.Any())
            {
                NotesDataGrid.ItemsSource = null;
            }
        }

        private void ApplyFilters()
        {
            if (_noteService == null) return;

            var filter = new NoteFilter();

            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                filter.CategoryId = (int)selectedItem.Tag;
            }

            if (FavoriteCheckBox.IsChecked == true)
            {
                filter.IsFavorite = true;
            }

            var filteredNotes = _noteService.GetNotes(filter);
            DisplayNotes(filteredNotes);
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var noteWindow = new NoteEditorWindow(_noteService, _categoryService)
            {
                Owner = this
            };

            if (noteWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void FavoriteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void FavoriteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            var categoryWindow = new CategoryManagerWindow(_categoryService)
            {
                Owner = this
            };

            categoryWindow.ShowDialog();
            LoadData();
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statisticsWindow = new StatisticsWindow(_statisticsService, _noteService, _categoryService)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                statisticsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is not Note selectedNote)
            {
                MessageBox.Show("Выберите заметку для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить заметку \"{selectedNote.Title}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _noteService.DeleteNote(selectedNote.Id);
                    LoadData();

                    MessageBox.Show("Заметка успешно удалена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NotesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteNoteButton.IsEnabled = NotesDataGrid.SelectedItem != null;
        }

        private void NotesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement source &&
                source.DataContext != null &&
                source.DataContext is Note selectedNote)
            {
                EditNote(selectedNote);
                e.Handled = true;
            }
        }

        private void EditNote(Note note)
        {
            var noteWindow = new NoteEditorWindow(_noteService, _categoryService, note)
            {
                Owner = this
            };

            if (noteWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }
    }
}