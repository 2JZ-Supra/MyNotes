using Data.Interfaces;
using Domain;
using Domain.Filters;

namespace Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly INoteRepository _noteRepository;

        public CategoryService(ICategoryRepository categoryRepository, INoteRepository noteRepository)
        {
            _categoryRepository = categoryRepository;
            _noteRepository = noteRepository;
        }

        public List<Category> GetCategories(CategoryFilter? filter = null)
        {
            return filter == null
                ? _categoryRepository.GetAll()
                : _categoryRepository.GetAll(filter);
        }

        public Category? GetCategory(int id)
        {
            return _categoryRepository.GetById(id);
        }

        public void CreateCategory(string name)
        {
            var existing = GetCategories().FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                throw new InvalidOperationException($"Категория '{name}' уже существует");

            _categoryRepository.Add(new Category { Name = name });
        }

        public void UpdateCategory(int id, string name)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return;

            category.Name = name;
            _categoryRepository.Update(category);
        }

        public void DeleteCategory(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return;

            if (_categoryRepository.IsCategoryUsed(id))
                throw new InvalidOperationException("Нельзя удалить категорию, которая используется в заметках");

            _categoryRepository.Remove(category);
        }

        public void DeleteCategoryWithCleanup(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return;

            var notesWithCategory = _noteRepository.GetAll(new NoteFilter { CategoryId = id });

            foreach (var note in notesWithCategory)
            {
                _noteRepository.RemoveCategoryFromNote(note.Id, id);
            }

            _categoryRepository.Remove(category);
        }
    }
}