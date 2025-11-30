using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Domain;

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

            foreach (var cat in DataStore.Categories)
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
            var selected = CategoriesPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag as Category)
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

            foreach (var cat in selected)
            {
                // удаляем категорию из заметок
                foreach (var note in DataStore.Notes)
                {
                    if (note.Categories.Any(c => c.Id == cat.Id))
                    {
                        note.SetCategories(
                            note.Categories.Where(c => c.Id != cat.Id)
                        );
                    }
                }

                DataStore.Categories.Remove(cat);
            }

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
    }
}
