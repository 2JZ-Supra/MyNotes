using Data.Interfaces;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotesComeback.Data.SqlServer
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly NotesDbContext _db;

        // уведомляем UI об изменении 
        public event EventHandler? CategoriesChanged;

        public CategoryRepository(NotesDbContext db)
        {
            _db = db;
        }

        public void Add(Category category)
        {
            _db.Categories.Add(category);
            _db.SaveChanges();

            // уведомляем подписчиков
            CategoriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(Category category)
        {
            _db.Categories.Remove(category);
            _db.SaveChanges();

            // уведомляем подписчиков
            CategoriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<Category> GetAll()
        {
            return _db.Categories.ToList();
        }
    }
}
