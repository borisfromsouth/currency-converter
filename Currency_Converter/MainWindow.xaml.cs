using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Currency_Converter
{
    // Этот класс is partial потому что состоит из 2 частей - основной(окно) и придаточной(.cs файл)
    public partial class MainWindow : Window
    {
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private int CurrencyId = 0;
        private double FromAmount = 0;
        private double ToAmount = 0;

        public MainWindow()
        {
            InitializeComponent();

            BindCurrency();
            ReFresher();
        }

        // CRUD - Create, Read, Update, Delete

        private void ConnectDb()
        {
            string connect = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            con = new SqlConnection(connect);
            con.Open();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
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
                сonvertedValue = FromAmount * double.Parse(txtCurrency.Text) / ToAmount;
                lblCurrency.Content = cmbToCurrency.Text + " " + сonvertedValue.ToString("N3");
            }
        }

        // Обычно для валидации используются 2 отдельных обработчика событий
        // Первый проверяет введенный текст по нажатию клавиши
        //private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        //{
        //    int val;
        //    if (!int.TryParse(e.Text, out val) && e.Text != "-")
        //    {
        //        e.Handled = true; // отклоняем ввод
        //    }
        //}

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
            ConnectDb();
            DataTable dataTable = new DataTable();
            cmd = new SqlCommand("Select Id, CurrencyName from Currency_Master", con);
            cmd.CommandType = CommandType.Text;
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dataTable);

            // заполняем начальное состояние таблицы
            DataRow dataRow = dataTable.NewRow();
            dataRow["Id"] = 0;
            dataRow["CurrencyName"] = "--SELECT--";
            dataTable.Rows.InsertAt(dataRow, 0);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dataTable.DefaultView;
                cmbToCurrency.ItemsSource = dataTable.DefaultView;
            }
            con.Close();

            // делаем привязку объекта таблицы к combobox
            cmbFromCurrency.DisplayMemberPath = "CurrencyName"; // визуальное представление объекта
            cmbFromCurrency.SelectedValuePath = "Id"; // значение объекта
            cmbFromCurrency.SelectedIndex = 0; // задаем первичное значение combobox

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void NumberValidationTextBox(object sendetr, TextCompositionEventArgs e)
        {
            // Пояснение по выражению Regex-a
            // [0-9] - диапазон символов (для букв - [a-v])
            // ^ - указатель начала строки
            // + - предыдущий символ повторяется 1 и более раз
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClearControls()
        {
            lblCurrency.Content = "";
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0) cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0) cmbToCurrency.SelectedIndex = 0;

            txtCurrency.Focus();
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                DataRowView rowSelected = dataGrid.CurrentItem as DataRowView;

                if (rowSelected != null && dgvCurrency.Items.Count > 0 && dataGrid.SelectedCells.Count > 0)
                {
                    CurrencyId = int.Parse(rowSelected["Id"].ToString());

                    if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                    {
                        txtAmount.Text = rowSelected["Amount"].ToString();
                        txtCurrencyName.Text = rowSelected["CurrencyName"].ToString();
                        btnSave.Content = "Update";
                    }
                    if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                    {
                        /*if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {*/
                            ConnectDb();
                            DataTable dataTable = new DataTable();
                            cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyId);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            //MessageBox.Show("Data delete successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //}
                        ReFresher();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " Problem with activ row cells", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {
                    if (CurrencyId > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            ConnectDb();
                            DataTable dataTable = new DataTable();
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyId);
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        //if (MessageBox.Show("Are you sure you want to save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        //{
                            ConnectDb();
                            DataTable dataTable = new DataTable();
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            //MessageBox.Show("Data added successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //}
                    }
                }
                ReFresher();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReFresher()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetData()
        {
            ConnectDb();
            DataTable dataTable = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Currency_Master", con);
            cmd.CommandType = CommandType.Text;
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dataTable);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            con.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReFresher();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbFromCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbFromCurrency.SelectedValue != null && int.Parse(cmbFromCurrency.SelectedValue.ToString()) != 0 && cmbFromCurrency.SelectedIndex != 0)
                {
                    int CurrencyFromId = int.Parse(cmbFromCurrency.SelectedValue.ToString());

                    ConnectDb();
                    DataTable dataTable = new DataTable();

                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyFromId", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CurrencyFromId", CurrencyFromId);
                    adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0) FromAmount = double.Parse(dataTable.Rows[0]["Amount"].ToString());
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbToCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbToCurrency.SelectedValue != null && int.Parse(cmbToCurrency.SelectedValue.ToString()) != 0 && cmbToCurrency.SelectedIndex != 0)
                {
                    int CurrencyToId = int.Parse(cmbToCurrency.SelectedValue.ToString());

                    ConnectDb();
                    DataTable dataTable = new DataTable();
                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyToId", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CurrencyToId", CurrencyToId);

                    adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count > 0) ToAmount = double.Parse(dataTable.Rows[0]["Amount"].ToString());
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void cmbFromCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbFromCurrency_SelectionChanged(sender, null);
            }
        }

        private void cmbToCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbToCurrency_SelectionChanged(sender, null);
            }
        }
    }
}