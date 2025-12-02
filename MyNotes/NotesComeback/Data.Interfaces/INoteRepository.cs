using Domain;
using System.Collections.ObjectModel;

namespace Data.Interfaces
{
    public interface INoteRepository
    {
        ObservableCollection<Note> Notes { get; }
        void Add(Note note);
        void Remove(Note note);

        void Update(Note note);
    }
}