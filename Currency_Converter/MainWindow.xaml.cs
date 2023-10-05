using System.Windows;
using System.Windows.Input;

namespace Currency_Converter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    // Этот класс is partial потому что состоит из 2 частей - основной(окно) и придаточной(.cs файл)
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            lblCurrency.Content = "Hello World";
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lblCurrency.Content = "";
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            lblCurrency.Content = "Button Clicker";
        }

        // Обычно для валидации используются 2 отдельных обработчика событий
        // Первый проверяет введенный текст по нажатию клавиши
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int val;
            if (!int.TryParse(e.Text, out val) && e.Text != "-")
            {
                e.Handled = true; // отклоняем ввод
            }
        }

        // Второй метод обрабатывет именно нажатие клавиши (В частности нам интересено нажатие Space)
        //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Space)
        //    {
        //        e.Handled = true; // если пробел, отклоняем ввод
        //    }
        //}


    }
}
