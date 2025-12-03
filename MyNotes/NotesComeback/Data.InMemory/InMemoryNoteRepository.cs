//using Domain;
//using Data.Interfaces;
//using System.Collections.ObjectModel;

//// InMemory — ничего не сохраняет в БД, просто обновляем объект в коллекции.
//namespace Data.InMemory
//{
//    public class InMemoryNoteRepository : INoteRepository
//    {
//        public ObservableCollection<Note> Notes { get; } = new ObservableCollection<Note>();
//        public void Add(Note note) => Notes.Add(note);
//        public void Remove(Note note) => Notes.Remove(note);
//        public void Update(Note note)
//        {
//            if (note == null) return;

//            // Находим заметку в коллекции по Id
//            var existing = Notes.FirstOrDefault(n => n.Id == note.Id);
//            if (existing == null) return;

//            // Обновляем поля заметки
//            existing.UpdateTitle(note.Title);
//            existing.UpdateContent(note.Content);
//            existing.SetFavorite(note.IsFavorite);

//            // Обновляем категории
//            existing.SetCategories(note.Categories);
//        }
//    }
//}
