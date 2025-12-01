using Data.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

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
    }
}
