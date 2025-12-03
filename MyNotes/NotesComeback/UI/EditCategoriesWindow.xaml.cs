using Domain;
using Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    public partial class EditCategoriesWindow : Window
    {
        public EditCategoriesWindow()
        {
            InitializeComponent();
            RefreshCategories();
        }

        private void RefreshCategories()
        {
            CategoriesPanel.Children.Clear();

            if (AppServices.CategoriesRepo == null)
                return;

            foreach (var cat in AppServices.CategoriesRepo.GetAll())
            {
                var cb = new CheckBox
                {
                    Content = cat.Name,
                    Tag = cat,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                CategoriesPanel.Children.Add(cb);
            }
        }

        private void DeleteSelectedCategories_Click(object sender, RoutedEventArgs e)
        {
            if (AppServices.CategoriesRepo == null || AppServices.NotesRepo == null)
            {
                MessageBox.Show("Ошибка: репозитории не инициализированы.");
                return;
            }

            var selected = CategoriesPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag as Category)
                .Where(cat => cat != null)
                .Cast<Category>()
                .ToList();

            if (!selected.Any())
            {
                MessageBox.Show("Выберите категории для удаления.");
                return;
            }

            var res = MessageBox.Show(
                "Удалить выбранные категории? Это удалит их из всех заметок.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes)
                return;

            // -------- Удаление категорий --------
            foreach (var cat in selected)
            {
                // 1. Удаляем категорию из всех заметок
                foreach (var note in AppServices.NotesRepo.GetAll().ToList())
                {
                    if (note.Categories.Any(c => c.Id == cat.Id))
                    {
                        try
                        {
                            // Если есть метод SetCategories
                            note.SetCategories(
                                note.Categories.Where(c => c.Id != cat.Id)
                            );
                        }
                        catch
                        {
                            // fallback — пересобираем вручную
                            var remaining = note.Categories
                                .Where(c => c.Id != cat.Id)
                                .ToList();

                            note.Categories.Clear();
                            foreach (var rc in remaining)
                                note.Categories.Add(rc);
                        }
                    }
                }

                // 2. Удаляем категорию из репозитория
                AppServices.CategoriesRepo.Remove(cat);
            }

            // 3. Обновление UI
            RefreshCategories();
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewCategory();
            win.Owner = this;
            if (win.ShowDialog() == true)
                RefreshCategories();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
