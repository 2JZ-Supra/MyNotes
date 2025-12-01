using Domain;
using Data.Interfaces;
using System.Collections.ObjectModel;

namespace Data.InMemory
{
    public class InMemoryNoteRepository : INoteRepository
    {
        public ObservableCollection<Note> Notes { get; } = new ObservableCollection<Note>();
        public void Add(Note note) => Notes.Add(note);
        public void Remove(Note note) => Notes.Remove(note);
    }
}
