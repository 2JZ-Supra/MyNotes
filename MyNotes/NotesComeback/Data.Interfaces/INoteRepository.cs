using Domain;
using System;
using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface INoteRepository
    {
        event EventHandler? NotesChanged;

        List<Note> GetAll();
        void Add(Note note);
        void Remove(Note note);
        void Update(Note note);
    }
}
