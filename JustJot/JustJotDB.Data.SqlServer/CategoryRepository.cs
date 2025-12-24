using Data.Interfaces;
using Domain;
using Domain.Filters;

namespace JustJotDB.Data.SqlServer
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly JustJotDbContext _dbContext;

        public CategoryRepository(JustJotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Category> GetAll()
        {
            return _dbContext.Categories.ToList();
        }

        public List<Category> GetAll(CategoryFilter filter)
        {
            var query = _dbContext.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(filter.NameContains))
            {
                query = query.Where(c => c.Name.Contains(filter.NameContains));
            }

            return query.ToList();
        }

        public Category? GetById(int id)
        {
            return _dbContext.Categories.Find(id);
        }

        public void Add(Category category)
        {
            _dbContext.Categories.Add(category);
            _dbContext.SaveChanges();
        }

        public void Remove(Category category)
        {
            _dbContext.Categories.Remove(category);
            _dbContext.SaveChanges();
        }

        public void Update(Category category)
        {
            _dbContext.Categories.Update(category);
            _dbContext.SaveChanges();
        }

        public bool IsCategoryUsed(int categoryId)
        {
            return _dbContext.Notes
                .Any(n => n.Categories.Any(c => c.Id == categoryId));
        }
    }
}