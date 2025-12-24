using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow()
        {
            InitializeComponent();
        }

        private void CardNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(" ", "");
            if (text.Length > 16) text = text.Substring(0, 16);

            string formatted = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted += " ";
                formatted += text[i];
            }

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.CaretIndex = formatted.Length;
            }
        }

        private void ExpiryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace("/", "");
            if (text.Length > 4) text = text.Substring(0, 4);

            if (text.Length >= 2)
            {
                string month = text.Substring(0, 2);
                string year = text.Length > 2 ? text.Substring(2) : "";
                string formatted = $"{month}/{year}";

                if (textBox.Text != formatted)
                {
                    textBox.Text = formatted;
                    textBox.CaretIndex = formatted.Length;
                }
            }
        }

        private void CvvBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void CardNumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                string currentPath = Directory.GetCurrentDirectory();

                File.WriteAllText("paid.txt", "true");

                MessageBox.Show("Оплата прошла успешно! Приложение активировано.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            string cardNumber = CardNumberBox.Text.Replace(" ", "");
            if (cardNumber.Length != 16 || !Regex.IsMatch(cardNumber, @"^\d{16}$"))
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CardNumberBox.Focus();
                return false;
            }

            if (!Regex.IsMatch(ExpiryBox.Text, @"^\d{2}/\d{2}$"))
            {
                MessageBox.Show("Срок действия должен быть в формате ММ/ГГ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExpiryBox.Focus();
                return false;
            }

            string monthStr = ExpiryBox.Text.Substring(0, 2);
            if (!int.TryParse(monthStr, out int month) || month < 1 || month > 12)
            {
                MessageBox.Show("Месяц должен быть от 01 до 12", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExpiryBox.Focus();
                return false;
            }

            if (CvvBox.Text.Length != 3 || !Regex.IsMatch(CvvBox.Text, @"^\d{3}$"))
            {
                MessageBox.Show("CVV должен содержать 3 цифры", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CvvBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите имя владельца карты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return false;
            }

            return true;
        }

        public static bool IsPaid()
        {
            try
            {
                return File.Exists("paid.txt");
            }
            catch
            {
                return false;
            }
        }
    }
}