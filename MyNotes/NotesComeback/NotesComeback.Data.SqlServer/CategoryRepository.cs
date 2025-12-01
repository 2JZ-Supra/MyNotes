using Data.Interfaces;
using Domain;
using System.Collections.ObjectModel;

namespace NotesComeback.Data.SqlServer
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly NotesDbContext _db;

        public ObservableCollection<Category> Categories { get; } = new();

        public CategoryRepository(NotesDbContext db)
        {
            _db = db;

            foreach (var c in _db.Categories)
                Categories.Add(c);
        }

        public void Add(Category category)
        {
            _db.Categories.Add(category);
            _db.SaveChanges();
            Categories.Add(category);
        }

        public void Remove(Category category)
        {
            _db.Categories.Remove(category);
            _db.SaveChanges();
            Categories.Remove(category);
        }
    }
}
