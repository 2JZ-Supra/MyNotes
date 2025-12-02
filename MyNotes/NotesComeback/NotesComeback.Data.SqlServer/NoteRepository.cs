using Data.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using static Azure.Core.HttpHeader;

namespace NotesComeback.Data.SqlServer
{
    public class NoteRepository : INoteRepository
    {
        private readonly NotesDbContext _db;

        public ObservableCollection<Note> Notes { get; } = new();

        public NoteRepository(NotesDbContext db)
        {
            _db = db;

            var notes = _db.Notes
                .Include(n => n.Categories)
                .ToList();

            foreach (var n in notes)
                Notes.Add(n);
        }

        public void Add(Note note)
        {
            _db.Notes.Add(note);
            _db.SaveChanges();
            Notes.Add(note);
        }

        public void Remove(Note note)
        {
            _db.Notes.Remove(note);
            _db.SaveChanges();
            Notes.Remove(note);
        }

        public void Update(Note note)
        {
            if (note == null) return;

            // Загружаем отслеживаемую сущность из БД с коллекцией категорий
            var tracked = _db.Notes
                .Include(n => n.Categories)
                .FirstOrDefault(n => n.Id == note.Id);

            if (tracked == null) return;

            // Сначала копируем информацию, которая нам нужна (чтобы не ломать коллекцию во время итерации)
            var newCategoryIds = note.Categories?.Select(c => c.Id).ToList() ?? new List<Guid>();

            // Обновляем скалярные поля
            tracked.UpdateTitle(note.Title);
            tracked.UpdateContent(note.Content);
            tracked.SetFavorite(note.IsFavorite);

            // Получаем отслеживаемые объекты категорий по id
            var cats = new List<Category>();
            foreach (var id in newCategoryIds)
            {
                var cat = _db.Categories.Find(id);
                if (cat != null) cats.Add(cat);
            }

            // ВАЖНО: вызываем SetCategories у tracked — он очистит коллекцию, добавит элементы и вызовет OnPropertyChanged
            tracked.SetCategories(cats);

            // Сохраняем в БД
            _db.SaveChanges();

            // Обновляем элемент в ObservableCollection, если нужно (замена same-instance не всегда требуется)
            var index = Notes.ToList().FindIndex(n => n.Id == tracked.Id);
            if (index >= 0)
            {
                Notes[index] = tracked;
            }
        }


    }
}
