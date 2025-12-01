using Domain;
using Data.Interfaces;
using System.Collections.ObjectModel;

namespace Data.InMemory
{
    public class InMemoryCategoryRepository : ICategoryRepository
    {
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>()
        {
            new Category("Дом"),
            new Category("Работа"),
            new Category("Учёба"),
            new Category("Важное"),
            new Category("Игры"),
            new Category("Фильмы"),
            new Category("Аниме"),
            new Category("Книги"),
            new Category("Идеи")
            // убрал тестовые 111/222/... — если нужны, добавите
        };

        public void Add(Category category) => Categories.Add(category);
        public void Remove(Category category) => Categories.Remove(category);
    }
}
