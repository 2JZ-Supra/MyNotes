using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Domain
{
    public class Note : INotifyPropertyChanged
    {
        public Guid Id { get; }
        private string _title;
        private string _content;
        private List<Category> _categories;
        private bool _isFavorite;
        public DateTime CreatedAt { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Note(string title, string content = "", IEnumerable<Category>? categories = null, bool isFavorite = false)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;

            _title = title;
            _content = content ?? "";
            _categories = categories?.ToList() ?? new List<Category>();
            _isFavorite = isFavorite;
        }

        public string Title
        {
            get => _title;
            private set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Content
        {
            get => _content;
            private set
            {
                if (_content == value) return;
                _content = value ?? "";
                OnPropertyChanged(nameof(Content));
            }
        }

        public List<Category> Categories
        {
            get => _categories;
            private set
            {
                _categories = value ?? new List<Category>();
                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(CategoriesDisplay));
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite == value) return;
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        // Методы изменения (редактирование):
        public void UpdateTitle(string newTitle) => Title = newTitle;

        public void UpdateContent(string newContent) => Content = newContent ?? "";

        public void SetCategories(IEnumerable<Category> categories) => Categories = categories?.ToList() ?? new List<Category>();

        public void SetFavorite(bool value) => IsFavorite = value;

        public string CategoriesDisplay =>
            Categories.Any()
                ? string.Join(", ", Categories.Select(c => c.Name))
                : string.Empty;

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
