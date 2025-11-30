using System;
using System.Windows;
using Domain;

namespace UI
{
    public partial class NewCategory : Window
    {
        public NewCategory()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var name = CategoryNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название категории не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Не допускаем дубликатов (по имени)
            if (DataStore.Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Категория с таким именем уже существует.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DataStore.Categories.Add(new Category(name)); // ObservableCollection — хорошо уведомит биндинги
            this.DialogResult = true;
            this.Close();
        }
    }
}
