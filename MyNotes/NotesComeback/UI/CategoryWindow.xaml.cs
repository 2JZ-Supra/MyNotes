using Domain;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UI
{
    public partial class CategoryWindow : Window
    {
        public List<Category> SelectedCategories { get; private set; } = new List<Category>();
        private readonly IEnumerable<Category>? _initialSelected;

        public CategoryWindow(IEnumerable<Category>? initiallySelected = null)
        {
            InitializeComponent();
            _initialSelected = initiallySelected;
            this.Loaded += CategoryWindow_Loaded;
        }

        private void CategoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCategories();
        }

        private void RefreshCategories()
        {
            CategoriesPanel.Children.Clear();

            if (AppServices.CategoriesRepo == null)
                return;

            foreach (var cat in AppServices.CategoriesRepo.Categories)
            {
                var cb = new CheckBox
                {
                    Content = cat.Name,
                    Tag = cat,
                    Margin = new Thickness(0, 5, 0, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                if (_initialSelected != null && _initialSelected.Any(c => c.Id == cat.Id))
                    cb.IsChecked = true;

                CategoriesPanel.Children.Add(cb);
            }
        }

        private void DeleteCategory_Click(object? sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.Tag is Category cat)) return;

            var res = MessageBox.Show(
                $"Удалить категорию \"{cat.Name}\"? Она будет удалена из всех заметок.",
                "Подтвердите удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            // 1) Удаляем категорию из всех заметок
            if (AppServices.NotesRepo != null)
            {
                // Проходим по копии коллекции заметок, чтобы безопасно модифицировать категории внутри заметок
                foreach (var note in AppServices.NotesRepo.Notes.ToList())
                {
                    // Если заметка содержит такую категорию — удаляем её
                    if (note.Categories.Any(c => c.Id == cat.Id))
                    {
                        // Попытка использовать SetCategories (если реализовано в модели Note),
                        // иначе очищаем/пересобираем коллекцию вручную.
                        try
                        {
                            note.SetCategories(note.Categories.Where(c => c.Id != cat.Id));
                        }
                        catch
                        {
                            // fallback: если SetCategories отсутствует, изменим коллекцию вручную
                            var remaining = note.Categories.Where(c => c.Id != cat.Id).ToList();
                            note.Categories.Clear();
                            foreach (var rc in remaining)
                                note.Categories.Add(rc);
                        }
                    }
                }
            }

            // 2) Удаляем категорию из коллекции репозитория
            AppServices.CategoriesRepo?.Remove(cat);

            // 3) Обновляем UI
            RefreshCategories();
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewCategory();
            win.Owner = this;
            bool? res = win.ShowDialog();

            // Обновляем список сразу после закрытия диалога (если была успешная добавка)
            if (res == true)
            {
                RefreshCategories();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategories = CategoriesPanel.Children
                .OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag as Category)
                .Where(cat => cat != null)
                .Cast<Category>()
                .ToList();

            this.DialogResult = true;
            this.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new EditCategoriesWindow();
            win.Owner = this;
            win.ShowDialog();

            // после закрытия окна обновляем список
            RefreshCategories();
        }
    }
}
