using System.Data;
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

            BindCurrency();
            lblCurrency.Content = "Hello World";
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lblCurrency.Content = "";
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            //Create a variable as ConvertedValue with double data type to store currency converted value
            //double ConvertedValue;

            if (/*txtCurrency.Text == null || */txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();  // поставить фокус на поле ввода валюты

                return;
            }
            else if (/*cmbFromCurrency.SelectedValue == null || */cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();

                return;
            }
            else if (/*cmbToCurrency.SelectedValue == null || */cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();

                return;
            }
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
        private void txtCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // если пробел, отклоняем ввод
            }
        }

        private void BindCurrency()
        {
            DataTable btCurrency = new DataTable();

            btCurrency.Columns.Add("Text");
            btCurrency.Columns.Add("Value");

            btCurrency.Rows.Add("--Select--", 0);
            btCurrency.Rows.Add("INR", 1);
            btCurrency.Rows.Add("USD", 75);
            btCurrency.Rows.Add("EUR", 85);
            btCurrency.Rows.Add("SAR", 20);
            btCurrency.Rows.Add("POUND", 5);
            btCurrency.Rows.Add("DEM", 43);

            // В источник данных для combobox ставим представление таблицы
            cmbFromCurrency.ItemsSource = cmbToCurrency.ItemsSource = btCurrency.DefaultView;

            // делаем привязку объекта таблицы к combobox
            cmbFromCurrency.DisplayMemberPath = "Text"; // визуальное представление объекта
            cmbFromCurrency.SelectedValuePath = "Value"; // значение объекта
            cmbFromCurrency.SelectedIndex = 0; // задаем первичное значение combobox

            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }
    }
}
