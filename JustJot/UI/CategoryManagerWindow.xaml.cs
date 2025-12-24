using Domain;
using Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    public partial class CategoryManagerWindow : Window
    {
        private readonly CategoryService _categoryService;
        private List<Category> _categories;

        public CategoryManagerWindow(CategoryService categoryService)
        {
            InitializeComponent();
            _categoryService = categoryService;
            _categories = new List<Category>();

            LoadCategories();
        }

        private void LoadCategories()
        {
            _categories = _categoryService.GetCategories(null);
            CategoriesItemsControl.ItemsSource = _categories;
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

                MessageBox.Show("Категория успешно добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int categoryId)
            {
                var category = _categories.FirstOrDefault(c => c.Id == categoryId);
                if (category == null) return;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию \"{category.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _categoryService.DeleteCategory(categoryId);
                        LoadCategories();

                        MessageBox.Show("Категория успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}