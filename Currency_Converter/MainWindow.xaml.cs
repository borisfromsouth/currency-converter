using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Currency_Converter
{
    // Этот класс is partial потому что состоит из 2 частей - основной(окно) и придаточной(.cs файл)
    public partial class MainWindow : Window
    {
        private string apiToken = "d336d81f030b4f17a335a22e142d23e1";
        private string httpRequest = $"https://openexchangerates.org/api/latest.json?app_id=YOUR_APP_ID";

        private SqlConnection con = new SqlConnection();
        private SqlCommand cmd = new SqlCommand();
        private SqlDataAdapter adapter = new SqlDataAdapter();

        private JsonRootStructure apiData = new JsonRootStructure();

        private int CurrencyId = 0;
        private double FromAmount = 0;
        private double ToAmount = 0;
        
        //private int[] numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public MainWindow()
        {
            InitializeComponent();
            GetApiData();

            //OddNumbers();
            //var query = from number in numbers where number > 5 orderby number descending select number;
            //foreach ( var item in query )
            //{
            //    MessageBox.Show( item.ToString() );
            //}

            //BindCurrency();
            //ReFresher();
        }

        //private void GetApiData()
        //{
        //    httpRequest = httpRequest.Replace("YOUR_APP_ID", apiToken);
        //    HttpClient httpClient = new HttpClient();
        //    var httpResonse = httpClient.GetAsync(httpRequest);
        //}
        
        private async void GetApiData()
        {
            apiData = await GetApiData<JsonRootStructure>();
            BindCurrency();
        }

        // Параллельное получение запросов
        public async Task<JsonRootStructure> GetApiData<T>()
        {
            JsonRootStructure root = new JsonRootStructure();
            try
            {
                using (HttpClient client = new HttpClient() ) 
                {
                    httpRequest = httpRequest.Replace("YOUR_APP_ID", apiToken);

                    client.Timeout = TimeSpan.FromMinutes(1); // выставляем тайм-аут
                    HttpResponseMessage responseMessage = await client.GetAsync(httpRequest);

                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string responceString = await responseMessage.Content.ReadAsStringAsync();
                        JsonRootStructure responceObject = JsonConvert.DeserializeObject<JsonRootStructure>(responceString);

                        MessageBox.Show("TimeStamp:" + responceObject.timestamp, "Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                        return responceObject;
                    }

                    return root; // пустой ответ 
                }
            }
            catch (Exception ex)
            {
                return root; // пустой ответ 
            }
        }

        //private void OddNumbers()
        //{
        //    IEnumerable<int> oddNumbers = from number in numbers where number % 2 != 0 select number;
        //    foreach (int i in oddNumbers)
        //    {
        //        MessageBox.Show(i.ToString());
        //    }
        //}

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

            if (txtCurrency2.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency2.Focus();  // поставить фокус на поле ввода валюты

                return;
            }
            else if (cmbFromCurrency2.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency2.Focus();

                return;
            }
            else if (cmbToCurrency2.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency2.Focus();

                return;
            }

            if(cmbFromCurrency2.Text == cmbToCurrency2.Text)
            {
                сonvertedValue = double.Parse(txtCurrency2.Text);

                // N3 - формат дробого числа с 3-мя числами после запятой; по дефолту добавит три нуля после запятой
                lblCurrency.Content = cmbToCurrency2.Text + " " + сonvertedValue.ToString("N3");
            }
            else
            {
                сonvertedValue = FromAmount * double.Parse(txtCurrency2.Text) / ToAmount;
                lblCurrency.Content = cmbToCurrency2.Text + " " + сonvertedValue.ToString("N3");
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

            //=====================================================================================
            // Вкладка API

            DataTable dataTableApi = new DataTable();
            dataTableApi.Columns.Add("CurrencyName");
            dataTableApi.Columns.Add("Value");

            dataTableApi.Rows.Add("--SELECT--", 0);

            //GetApiData();

            dataTableApi.Rows.Add("AED", apiData.rates.AED);
            dataTableApi.Rows.Add("AUD", apiData.rates.AUD);
            dataTableApi.Rows.Add("BGN", apiData.rates.BGN);
            dataTableApi.Rows.Add("BTC", apiData.rates.BTC);
            dataTableApi.Rows.Add("BYN", apiData.rates.BYN);
            dataTableApi.Rows.Add("BYR", apiData.rates.BYR);
            dataTableApi.Rows.Add("CAD", apiData.rates.CAD);
            dataTableApi.Rows.Add("CNH", apiData.rates.CNH);
            dataTableApi.Rows.Add("CZK", apiData.rates.CZK);
            dataTableApi.Rows.Add("EUR", apiData.rates.EUR);
            dataTableApi.Rows.Add("GBP", apiData.rates.GBP);
            dataTableApi.Rows.Add("HKD", apiData.rates.HKD);
            dataTableApi.Rows.Add("INR", apiData.rates.INR);
            dataTableApi.Rows.Add("JPY", apiData.rates.JPY);
            dataTableApi.Rows.Add("KPW", apiData.rates.KPW);
            dataTableApi.Rows.Add("KRW", apiData.rates.KRW);
            dataTableApi.Rows.Add("KZT", apiData.rates.KZT);
            dataTableApi.Rows.Add("PLN", apiData.rates.PLN);
            dataTableApi.Rows.Add("RUB", apiData.rates.RUB);
            dataTableApi.Rows.Add("THB", apiData.rates.THB);
            dataTableApi.Rows.Add("TRY", apiData.rates.TRY);
            dataTableApi.Rows.Add("UAH", apiData.rates.UAH);
            dataTableApi.Rows.Add("USD", apiData.rates.USD);

            cmbFromCurrency2.ItemsSource = dataTableApi.DefaultView;
            cmbToCurrency2.ItemsSource = dataTableApi.DefaultView;

            // делаем привязку объекта таблицы к combobox
            cmbFromCurrency2.DisplayMemberPath = "CurrencyName"; // визуальное представление объекта
            cmbFromCurrency2.SelectedValuePath = "Value"; // значение объекта
            cmbFromCurrency2.SelectedIndex = 0; // задаем первичное значение combobox

            cmbToCurrency2.DisplayMemberPath = "CurrencyName";
            cmbToCurrency2.SelectedValuePath = "Value";
            cmbToCurrency2.SelectedIndex = 0;
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

        private void Convert2_Click(object sender, RoutedEventArgs e)
        {
            double сonvertedValue; // переменная для хранения введенного количества валют

            if (txtCurrency2.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency2.Focus();  // поставить фокус на поле ввода валюты

                return;
            }
            else if (cmbFromCurrency2.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency2.Focus();

                return;
            }
            else if (cmbToCurrency2.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency2.Focus();

                return;
            }

            if (cmbFromCurrency2.Text == cmbToCurrency2.Text)
            {
                сonvertedValue = double.Parse(txtCurrency2.Text);

                // N3 - формат дробого числа с 3-мя числами после запятой; по дефолту добавит три нуля после запятой
                lblCurrency2.Content = cmbToCurrency2.Text + " " + сonvertedValue.ToString("N3");
            }
            else
            {
                сonvertedValue = ToAmount * double.Parse(txtCurrency2.Text) / FromAmount;
                lblCurrency2.Content = cmbToCurrency2.Text + " " + сonvertedValue.ToString("N3");
            }
        }

        private void Clear2_Click(object sender, RoutedEventArgs e)
        {
            txtCurrency2.Text = string.Empty;
            //txtCurrencyName2.Text = string.Empty;
            btnSave.Content = "Save";
            //GetData();
            //CurrencyId = 0;
            BindCurrency();
            txtCurrency2.Focus();
        }

        private void cmbFromCurrency2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FromAmount = double.Parse(cmbFromCurrency2.SelectedValue.ToString());
        }

        private void cmbToCurrency2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToAmount = double.Parse(cmbToCurrency2.SelectedValue.ToString()); ;
        }
    }
}