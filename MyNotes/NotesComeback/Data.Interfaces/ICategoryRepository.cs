using Domain;
using System.Collections.ObjectModel;

namespace Data.Interfaces
{
    public interface ICategoryRepository
    {
        event EventHandler? CategoriesChanged;
        void Add(Category category);
        void Remove(Category category);
        List<Category> GetAll();
    }
}
