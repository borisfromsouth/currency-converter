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
            txtCurrency.Text = string.Empty;
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double сonvertedValue; // переменная для хранения введенного количества валют

            if (txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();  // поставить фокус на поле ввода валюты

                return;
            }
            else if (cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();

                return;
            }
            else if (cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();

                return;
            }

            if(cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                сonvertedValue = double.Parse(txtCurrency.Text);

                // N3 - формат дробого числа с 3-мя числами после запятой; по дефолту добавит три нуля после запятой
                lblCurrency.Content = cmbToCurrency.Text + " " + сonvertedValue.ToString("N3");
            }
            else
            {
                сonvertedValue = double.Parse(cmbFromCurrency.SelectedValue.ToString()) * 
                    double.Parse(txtCurrency.Text) / 
                    double.Parse(cmbToCurrency.SelectedValue.ToString());
                lblCurrency.Content = cmbToCurrency.Text + " " + сonvertedValue.ToString("N3");
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

        // Заполнение combobox-ов
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
