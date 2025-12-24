using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using Domain.Filters;
using Domain.Statistics;

namespace Services
{
    public class StatisticsService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ICategoryRepository _categoryRepository;

        public StatisticsService(INoteRepository noteRepository, ICategoryRepository categoryRepository)
        {
            _noteRepository = noteRepository;
            _categoryRepository = categoryRepository;
        }

        public List<NotesByMonth> GetNotesByMonth(DateTime startDate, DateTime endDate)
        {
            var filter = new NoteFilter { StartDate = startDate, EndDate = endDate };
            var notes = _noteRepository.GetAll(filter);

            return notes
                .GroupBy(n => new { n.CreatedAt.Year, n.CreatedAt.Month })
                .Select(g => new NotesByMonth
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();
        }

        public List<CategoryStatItem> GetCategoryStatistics()
        {
            var allNotes = _noteRepository.GetAll();
            var allCategories = _categoryRepository.GetAll();

            var result = new List<CategoryStatItem>();

            foreach (var category in allCategories)
            {
                var noteCount = allNotes.Count(n => n.Categories.Any(c => c.Id == category.Id));

                result.Add(new CategoryStatItem
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    NoteCount = noteCount
                });
            }

            return result.OrderByDescending(c => c.NoteCount).ThenBy(c => c.CategoryName).ToList();
        }

        public List<CategoryStatItem> GetCategoryStatistics(bool sortDescending)
        {
            var stats = GetCategoryStatistics();

            return sortDescending
                ? stats.OrderByDescending(c => c.NoteCount).ThenBy(c => c.CategoryName).ToList()
                : stats.OrderBy(c => c.NoteCount).ThenBy(c => c.CategoryName).ToList();
        }

        public CategoryStatSummary GetCategoryStatSummary()
        {
            var stats = GetCategoryStatistics();

            var usedCategories = stats.Where(c => c.NoteCount > 0).ToList();
            var mostPopular = usedCategories.FirstOrDefault();

            return new CategoryStatSummary
            {
                TotalCategories = stats.Count,
                UsedCategories = usedCategories.Count,
                UnusedCategories = stats.Count - usedCategories.Count,
                TotalNotesInCategories = stats.Sum(c => c.NoteCount),
                MostPopularCategoryName = mostPopular?.CategoryName ?? "Нет данных",
                MostPopularCategoryCount = mostPopular?.NoteCount ?? 0
            };
        }

        public object GetGeneralStatistics()
        {
            var allNotes = _noteRepository.GetAll();
            var allCategories = _categoryRepository.GetAll();

            return new
            {
                TotalNotes = allNotes.Count,
                TotalCategories = allCategories.Count,
                FavoriteNotes = allNotes.Count(n => n.IsFavorite),
                NotesLast30Days = allNotes.Count(n => n.CreatedAt >= DateTime.Now.AddDays(-30))
            };
        }
    }

    public class CategoryStatItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int NoteCount { get; set; }
    }

    public class CategoryStatSummary
    {
        public int TotalCategories { get; set; }
        public int UsedCategories { get; set; }
        public int UnusedCategories { get; set; }
        public int TotalNotesInCategories { get; set; }
        public string MostPopularCategoryName { get; set; } = string.Empty;
        public int MostPopularCategoryCount { get; set; }
    }
}