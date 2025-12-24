using Data.Interfaces;
using JustJotDB.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services;
using System;
using System.IO;
using System.Windows;

namespace UI
{
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (PaymentWindow.IsPaid())
            {
                OpenMainWindow();
            }
            else
            {
                ShowPaymentWindow();
            }
        }

        private void ShowPaymentWindow()
        {
            var paymentWindow = new PaymentWindow();
            paymentWindow.ShowDialog();

            if (paymentWindow.DialogResult == true)
            {
                OpenMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        private void OpenMainWindow()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.database.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("Не удалось найти строку подключения в конфигурации", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                var options = new DbContextOptionsBuilder<JustJotDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                var dbContext = new JustJotDbContext(options);

                if (!dbContext.Database.CanConnect())
                {
                    MessageBox.Show("Не удалось подключиться к базе данных", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                ICategoryRepository categoryRepository = new CategoryRepository(dbContext);
                INoteRepository noteRepository = new NoteRepository(dbContext);

                var noteService = new NoteService(noteRepository, categoryRepository);
                var categoryService = new CategoryService(categoryRepository, noteRepository);
                var statisticsService = new StatisticsService(noteRepository, categoryRepository);

                var mainWindow = new MainWindow(noteService, categoryService, statisticsService);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}