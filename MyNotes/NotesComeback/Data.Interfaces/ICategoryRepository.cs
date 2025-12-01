using Domain;
using System.Collections.ObjectModel;

namespace Data.Interfaces
{
    public interface ICategoryRepository
    {
        ObservableCollection<Category> Categories { get; }
        void Add(Category category);
        void Remove(Category category);
    }
}
