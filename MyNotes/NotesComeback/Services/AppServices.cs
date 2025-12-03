using Data.Interfaces;

namespace Services
{
    public static class AppServices
    {
        public static INoteRepository? NotesRepo { get; private set; }
        public static ICategoryRepository? CategoriesRepo { get; private set; }

        public static void Initialize(INoteRepository notesRepo, ICategoryRepository categoriesRepo)
        {
            NotesRepo = notesRepo;
            CategoriesRepo = categoriesRepo;
        }
    }
}
