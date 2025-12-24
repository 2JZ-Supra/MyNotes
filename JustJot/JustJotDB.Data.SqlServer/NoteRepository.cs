using Data.Interfaces;
using Domain;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace JustJotDB.Data.SqlServer
{
    public class NoteRepository : INoteRepository
    {
        private readonly JustJotDbContext _dbContext;

        public NoteRepository(JustJotDbContext context)
        {
            _dbContext = context;
        }

        public List<Note> GetAll()
        {
            return _dbContext.Notes
                .Include(n => n.Categories)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public List<Note> GetAll(NoteFilter filter)
        {
            var query = _dbContext.Notes
                .Include(n => n.Categories)
                .AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= filter.EndDate.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(n => n.Categories.Any(c => c.Id == filter.CategoryId.Value));
            }

            if (filter.IsFavorite.HasValue)
            {
                query = query.Where(n => n.IsFavorite == filter.IsFavorite.Value);
            }

            return query.OrderByDescending(n => n.CreatedAt).ToList();
        }

        public Note? GetById(int id)
        {
            return _dbContext.Notes
                .Include(n => n.Categories)
                .FirstOrDefault(n => n.Id == id);
        }

        public void Add(Note note)
        {
            if (note.Categories != null && note.Categories.Any())
            {
                var categoryIds = note.Categories.Select(c => c.Id).ToList();
                var existingCategories = _dbContext.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToList();

                note.Categories = existingCategories;
            }

            _dbContext.Notes.Add(note);
            _dbContext.SaveChanges();
        }

        public void Remove(Note note)
        {
            _dbContext.Notes.Remove(note);
            _dbContext.SaveChanges();
        }

        public void Update(Note note)
        {
            var existingNote = _dbContext.Notes
                .Include(n => n.Categories)
                .FirstOrDefault(n => n.Id == note.Id);

            if (existingNote == null)
                return;

            _dbContext.Entry(existingNote).CurrentValues.SetValues(new
            {
                note.Title,
                note.Content,
                note.IsFavorite
            });

            existingNote.Categories.Clear();

            if (note.Categories != null && note.Categories.Any())
            {
                var categoryIds = note.Categories.Select(c => c.Id).ToList();
                var categoriesToAdd = _dbContext.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToList();

                foreach (var category in categoriesToAdd)
                {
                    existingNote.Categories.Add(category);
                }
            }

            _dbContext.SaveChanges();
        }

        public void SetFavorite(int noteId, bool isFavorite)
        {
            var note = _dbContext.Notes.Find(noteId);
            if (note != null)
            {
                note.IsFavorite = isFavorite;
                _dbContext.SaveChanges();
            }
        }

        public void RemoveCategoryFromNote(int noteId, int categoryId)
        {
            var note = _dbContext.Notes
                .Include(n => n.Categories)
                .FirstOrDefault(n => n.Id == noteId);

            if (note != null)
            {
                var category = note.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (category != null)
                {
                    note.Categories.Remove(category);
                    _dbContext.SaveChanges();
                }
            }
        }

        public bool HasNotesInCategory(int categoryId)
        {
            return _dbContext.Notes
                .Any(n => n.Categories.Any(c => c.Id == categoryId));
        }

        public int GetNoteCountByCategory(int categoryId)
        {
            return _dbContext.Notes
                .Count(n => n.Categories.Any(c => c.Id == categoryId));
        }
    }
}