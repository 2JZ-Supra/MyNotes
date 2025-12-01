using Data.Interfaces;

namespace Services
{
    /// <summary>
    /// Простая статическая фабрика/локатор сервисов — инициализируем при старте приложения.
    /// Позже замените на DI (Autofac / MS DI) или вызовите Initialize с реализациями EF.
    /// </summary>
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
