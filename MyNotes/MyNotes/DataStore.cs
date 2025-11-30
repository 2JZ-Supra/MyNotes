using System.Collections.ObjectModel;
using Domain;

namespace UI
{
    public static class DataStore
    {
        public static ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>()
        {
            new Category("Дом"),
            new Category("Работа"),
            new Category("Учёба"),
            new Category("Важное"),
            new Category("Игры"),
            new Category("Фильмы"),
            new Category("Аниме"),
            new Category("Книги"),
            new Category("Идеи"),
            new Category("111"),
            new Category("222"),
            new Category("333"),
            new Category("444"),
            new Category("555")
        };

        public static ObservableCollection<Note> Notes { get; } = new ObservableCollection<Note>();
    }
}
