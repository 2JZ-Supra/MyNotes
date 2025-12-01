using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Domain
{
    public class Note : INotifyPropertyChanged
    {
        public Guid Id { get; private set; }

        private string _title = "";
        private string _content = "";
        private bool _isFavorite;

        // EF Core должен уметь менять коллекцию
        public List<Category> Categories { get; private set; } = new();

        public DateTime CreatedAt { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        // EF Core требует конструктор без параметров
        private Note() { }

        public Note(string title, string content = "", IEnumerable<Category>? categories = null, bool isFavorite = false)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;

            _title = title;
            _content = content ?? "";
            Categories = categories?.ToList() ?? new List<Category>();
            _isFavorite = isFavorite;
        }

        public string Title
        {
            get => _title;
            private set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Content
        {
            get => _content;
            private set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set { _isFavorite = value; OnPropertyChanged(nameof(IsFavorite)); }
        }

        public void UpdateTitle(string newTitle) => Title = newTitle;
        public void UpdateContent(string newContent) => Content = newContent;
        public void SetCategories(IEnumerable<Category> categories) => Categories = categories.ToList();
        public void SetFavorite(bool value) => IsFavorite = value;

        public string CategoriesDisplay =>
            Categories.Any()
                ? string.Join(", ", Categories.Select(c => c.Name))
                : string.Empty;

        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
