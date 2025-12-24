using Data.Interfaces;
using Domain;
using Domain.Filters;
using System.Diagnostics;
using System.Collections.Generic;

namespace Services
{
    public class NoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ICategoryRepository _categoryRepository;

        public NoteService(INoteRepository noteRepository, ICategoryRepository categoryRepository)
        {
            _noteRepository = noteRepository;
            _categoryRepository = categoryRepository;
        }

        public List<Note> GetNotes(NoteFilter? filter = null)
        {
            return filter == null
                ? _noteRepository.GetAll()
                : _noteRepository.GetAll(filter);
        }

        public Note? GetNote(int id)
        {
            return _noteRepository.GetById(id);
        }

        public void CreateNote(string title, string content, bool isFavorite, List<int>? categoryIds = null)
        {
            var note = new Note
            {
                Title = title,
                Content = content,
                IsFavorite = isFavorite,
                Categories = new List<Category>()
            };

            if (categoryIds != null)
            {
                foreach (var categoryId in categoryIds)
                {
                    var category = _categoryRepository.GetById(categoryId);
                    if (category != null)
                    {
                        note.Categories.Add(category);
                    }
                }
            }

            _noteRepository.Add(note);
        }

        public void DeleteNote(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note != null)
            {
                _noteRepository.Remove(note);
            }
        }

        public void ToggleFavorite(int noteId)
        {
            var note = _noteRepository.GetById(noteId);
            if (note != null)
            {
                note.IsFavorite = !note.IsFavorite;
                _noteRepository.Update(note);
            }
        }

        public List<Note> GetFavorites()
        {
            return GetNotes(new NoteFilter { IsFavorite = true });
        }

        public void UpdateNote(int id, string title, string content, bool isFavorite, List<int>? categoryIds = null)
        {
            var note = new Note
            {
                Id = id,
                Title = title,
                Content = content,
                IsFavorite = isFavorite,
                Categories = new List<Category>()
            };

            if (categoryIds != null)
            {
                foreach (var categoryId in categoryIds)
                {
                    note.Categories.Add(new Category { Id = categoryId });
                }
            }

            _noteRepository.Update(note);
        }
    }
}