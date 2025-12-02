using Domain;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace UI
{
    public partial class NoteWindow : Window
    {
        public Note? EditingNote { get; private set; }
        private bool _isNew;

        // временные данные, чтобы Note создавать только при сохранении
        private string _tempTitle = "";
        private string _tempContent = "";
        private List<Category> _tempCategories = new List<Category>();
        private bool _tempIsFavorite = false;

        public NoteWindow(Note? note = null)
        {
            InitializeComponent();

            if (note == null)
            {
                // создаём новую заметку (но НЕ объект Note!)
                _isNew = true;
            }
            else
            {
                // редактируем существующую — подгружаем данные
                _isNew = false;

                _tempTitle = note.Title;
                _tempContent = note.Content;
                _tempCategories = note.Categories.ToList();
                _tempIsFavorite = note.IsFavorite;

                EditingNote = note; // оригинальная заметка (мы её перезапишем при сохранении)
            }

            DataToControls();
        }

        private void DataToControls()
        {
            TitleTextBox.Text = _tempTitle;
            ContentTextBox.Text = _tempContent;
            FavoriteCheckBox.IsChecked = _tempIsFavorite;
        }

        private void ControlsToData()
        {
            _tempTitle = TitleTextBox.Text;
            _tempContent = ContentTextBox.Text;
            _tempIsFavorite = FavoriteCheckBox.IsChecked == true;
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            var cw = new CategoryWindow(_tempCategories);
            cw.Owner = this;
            if (cw.ShowDialog() == true)
            {
                _tempCategories = cw.SelectedCategories.ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ControlsToData();

            if (string.IsNullOrWhiteSpace(_tempTitle))
            {
                MessageBox.Show("Заголовок не может быть пустым.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isNew)
            {
                // создаём новую заметку здесь!
                EditingNote = new Note(
                    title: _tempTitle,
                    content: _tempContent,
                    categories: _tempCategories,
                    isFavorite: _tempIsFavorite
                );

                AppServices.NotesRepo.Add(EditingNote);
            }
            else
            {
                // редактируем существующую
                if (EditingNote != null)
                {
                    EditingNote.UpdateTitle(_tempTitle);
                    EditingNote.UpdateContent(_tempContent);
                    EditingNote.SetCategories(_tempCategories);
                    EditingNote.SetFavorite(_tempIsFavorite);

                    AppServices.NotesRepo.Update(EditingNote);
                    // Обновляем объект в UI коллекции тем, что лежит в БД
                    var updated = AppServices.NotesRepo.Notes.FirstOrDefault(n => n.Id == EditingNote.Id);
                    if (updated != null)
                    {
                        int index = AppServices.NotesRepo.Notes.IndexOf(EditingNote);
                        if (index >= 0)
                            AppServices.NotesRepo.Notes[index] = updated;
                    }

                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }

}
