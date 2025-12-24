using Domain;
using Domain.Filters;

namespace Data.Interfaces
{
    public interface ICategoryRepository
    {
        List<Category> GetAll();
        List<Category> GetAll(CategoryFilter filter);
        Category? GetById(int id);
        void Add(Category category);
        void Remove(Category category);
        void Update(Category category);
        bool IsCategoryUsed(int categoryId);
    }
}