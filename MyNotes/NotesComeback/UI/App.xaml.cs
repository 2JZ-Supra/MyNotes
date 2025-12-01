using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotesComeback.Data.SqlServer;
using Services;
using System.IO;
using System.Windows;

namespace UI
{
    public partial class App : Application
    {
        private NotesDbContext _db = null!;
        private INoteRepository _notesRepo = null!;
        private ICategoryRepository _categoriesRepo = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загружаем конфигурацию
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.database.json")
                .Build();

            // Создаём DbContext
            var factory = new NotesDbContextFactory();
            _db = factory.CreateDbContext(config);

            // Применяем миграции
            _db.Database.Migrate();

            // Создаём SQL репозитории
            _notesRepo = new NoteRepository(_db);
            _categoriesRepo = new CategoryRepository(_db);

            // Регистрируем их в AppServices
            AppServices.Initialize(_notesRepo, _categoriesRepo);

            // Запускаем главное окно (без аргументов!!!)
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _db?.Dispose();
            base.OnExit(e);
        }
    }
}
