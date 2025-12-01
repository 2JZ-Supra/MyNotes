using System.Windows;
using Services;
using Data.InMemory;

namespace UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Инициализация сервисов
            AppServices.Initialize(
                new InMemoryNoteRepository(),
                new InMemoryCategoryRepository()
            );

            // 2. Теперь можно создавать MainWindow
            var main = new MainWindow();
            main.Show();
        }
    }
}
