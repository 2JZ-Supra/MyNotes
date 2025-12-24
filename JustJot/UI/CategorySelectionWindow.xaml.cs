using Domain;
using Services;
using System.Windows;
using System.Windows.Input;

namespace UI
{
    public partial class CategorySelectionWindow : Window
    {
        private readonly CategoryService _categoryService;
        private readonly List<CategoryViewModel> _categoryViewModels;
        private List<int> _selectedCategoryIds;

        public List<int> SelectedCategoryIds => _selectedCategoryIds;

        private class CategoryViewModel
        {
            public Category Category { get; set; } = null!;
            public bool IsSelected { get; set; }
        }

        public CategorySelectionWindow(CategoryService categoryService, List<int>? selectedCategoryIds = null)
        {
            InitializeComponent();
            _categoryService = categoryService;
            _selectedCategoryIds = selectedCategoryIds ?? new List<int>();
            _categoryViewModels = new List<CategoryViewModel>();

            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _categoryService.GetCategories(null);
                _categoryViewModels.Clear();

                foreach (var category in categories)
                {
                    _categoryViewModels.Add(new CategoryViewModel
                    {
                        Category = category,
                        IsSelected = _selectedCategoryIds.Contains(category.Id)
                    });
                }

                CategoriesItemsControl.ItemsSource = _categoryViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewCategory();
        }

        private void NewCategoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNewCategory();
            }
        }

        private void AddNewCategory()
        {
            var categoryName = NewCategoryTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Введите название категории", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _categoryService.CreateCategory(categoryName);
                NewCategoryTextBox.Clear();
                LoadCategories();
                CategoriesItemsControl.Items.Refresh();

                MessageBox.Show("Категория успешно добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedCategoryIds = _categoryViewModels
                .Where(vm => vm.IsSelected)
                .Select(vm => vm.Category.Id)
                .ToList();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}