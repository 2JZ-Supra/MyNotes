using Domain;
using Services;
using System.Windows;

namespace UI
{
    public partial class NoteEditorWindow : Window
    {
        private readonly NoteService? _noteService;
        private readonly CategoryService? _categoryService;
        private readonly Note? _existingNote;
        private List<int> _selectedCategoryIds = new List<int>();
        private List<Category> _allCategories = new List<Category>();

        public NoteEditorWindow(NoteService? noteService, CategoryService? categoryService, Note? note = null)
        {
            InitializeComponent();
            _noteService = noteService;
            _categoryService = categoryService;
            _existingNote = note;

            if (note != null)
            {
                Title = "Редактирование заметки";
                LoadNoteData(note);
            }
            else
            {
                Title = "Новая заметка";
            }

            LoadCategories();
            UpdateCategoriesPreview();
        }

        private void LoadNoteData(Note note)
        {
            if (TitleTextBox != null)
                TitleTextBox.Text = note.Title;

            if (ContentTextBox != null)
                ContentTextBox.Text = note.Content;

            if (FavoriteCheckBox != null)
                FavoriteCheckBox.IsChecked = note.IsFavorite;

            _selectedCategoryIds = note.Categories?.Select(c => c.Id).ToList() ?? new List<int>();
        }

        private void LoadCategories()
        {
            try
            {
                if (_categoryService != null)
                {
                    _allCategories = _categoryService.GetCategories(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategoriesPreview()
        {
            var selectedCategories = _allCategories
                .Where(c => _selectedCategoryIds.Contains(c.Id))
                .ToList();

            if (selectedCategories.Any())
            {
                if (CategoriesListRun != null)
                    CategoriesListRun.Text = string.Join(", ", selectedCategories.Select(c => c.Name));

                if (CategoriesPreviewText != null)
                    CategoriesPreviewText.Visibility = Visibility.Visible;
            }
            else
            {
                if (CategoriesListRun != null)
                    CategoriesListRun.Text = "нет";

                if (CategoriesPreviewText != null)
                    CategoriesPreviewText.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_categoryService == null) return;

            var categoryWindow = new CategorySelectionWindow(_categoryService, _selectedCategoryIds)
            {
                Owner = this
            };

            if (categoryWindow.ShowDialog() == true)
            {
                _selectedCategoryIds = categoryWindow.SelectedCategoryIds;
                LoadCategories();
                UpdateCategoriesPreview();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_noteService == null) return;

            var title = TitleTextBox?.Text?.Trim();
            var content = ContentTextBox?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите заголовок заметки", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleTextBox?.Focus();
                return;
            }

            try
            {
                var isFavorite = FavoriteCheckBox?.IsChecked ?? false;

                if (_existingNote != null)
                {

                    _noteService.UpdateNote(
                        _existingNote.Id,
                        title,
                        content ?? "",
                        isFavorite,
                        _selectedCategoryIds
                    );

                    MessageBox.Show("Заметка успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {

                    _noteService.CreateNote(
                        title!,
                        content ?? "",
                        isFavorite,
                        _selectedCategoryIds
                    );

                    MessageBox.Show("Заметка успешно создана", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}