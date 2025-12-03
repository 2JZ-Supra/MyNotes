using Data.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotesComeback.Data.SqlServer
{
    public class NoteRepository : INoteRepository
    {
        private readonly NotesDbContext _db;

        public event EventHandler? NotesChanged;

        public NoteRepository(NotesDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public List<Note> GetAll()
        {
            return _db.Notes
                .Include(n => n.Categories)
                .ToList();
        }

        public void Add(Note note)
        {
            _db.Notes.Add(note);
            _db.SaveChanges();
            NotesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(Note note)
        {
            var tracked = _db.Notes.Find(note.Id);
            if (tracked != null)
            {
                _db.Notes.Remove(tracked);
                _db.SaveChanges();
                NotesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Update(Note note)
        {
            _db.Notes.Update(note);
            _db.SaveChanges();
            NotesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
