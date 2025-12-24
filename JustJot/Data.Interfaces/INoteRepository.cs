using Domain;
using Domain.Filters;

namespace Data.Interfaces
{
    public interface INoteRepository
    {
        List<Note> GetAll();
        List<Note> GetAll(NoteFilter filter);
        Note? GetById(int id);
        void Add(Note note);
        void Remove(Note note);
        void Update(Note note);
        void SetFavorite(int noteId, bool isFavorite);
        void RemoveCategoryFromNote(int noteId, int categoryId);
        bool HasNotesInCategory(int categoryId);
        int GetNoteCountByCategory(int categoryId);
    }
}