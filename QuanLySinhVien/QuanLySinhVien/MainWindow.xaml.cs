using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Data;
using OfficeOpenXml;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace K204160661
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Employee> _employees;
        private Employee _selectedEmployee;
        private string path = @"nhanvien.xlsx";

        public MainWindow()
        {
            InitializeComponent();
            ClearInputFields();
            _employees = new ObservableCollection<Employee>();
            lvEmployees.ItemsSource = _employees;
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            if (File.Exists(path))
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                    int totalRows = workSheet.Dimension.Rows;

                    for (int i = 1; i <= totalRows; i++)
                    {
                        Employee employee = new Employee();
                        employee.EmployeeID = workSheet.Cells[i, 1].Value.ToString();
                        employee.FullName = workSheet.Cells[i, 2].Value.ToString();
                        employee.Gender = workSheet.Cells[i, 3].Value.ToString();
                        employee.PhoneNumber = workSheet.Cells[i, 4].Value.ToString();

                        if (DateTime.TryParseExact(workSheet.Cells[i, 5].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                        {
                            employee.StartDate = startDate;
                        }
                        else
                        {
                            employee.StartDate = DateTime.Now;
                        }

                        employee.EmployeeType = workSheet.Cells[i, 6].Value.ToString();
                        employee.SalesOrFuelAllowance = Double.Parse(workSheet.Cells[i, 7].Value.ToString());
                        _employees.Add(employee);
                    }
                }
            }

            this.Closing += MainWindow_Closing;

            cbGender.SelectedIndex = 0;
            dpStartDate.SelectedDate = DateTime.Now;
            rbSale.IsChecked = true;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void rbSale_Checked(object sender, RoutedEventArgs e)
        {
            if (lbSales == null || tbSales == null || lbFuelAllowance == null || tbFuelAllowance == null)
                return;

            lbSales.Visibility = Visibility.Visible;
            tbSales.Visibility = Visibility.Visible;

            lbFuelAllowance.Visibility = Visibility.Collapsed;
            tbFuelAllowance.Visibility = Visibility.Collapsed;
        }

        private void rbDelivery_Checked(object sender, RoutedEventArgs e)
        {
            if (lbSales == null || tbSales == null || lbFuelAllowance == null || tbFuelAllowance == null)
                return;

            lbSales.Visibility = Visibility.Collapsed;
            tbSales.Visibility = Visibility.Collapsed;

            lbFuelAllowance.Visibility = Visibility.Visible;
            tbFuelAllowance.Visibility = Visibility.Visible;
        }

        private void ClearInputFields()
        {
            tbEmployeeID.Clear();
            tbFullName.Clear();
            cbGender.SelectedIndex = 0;
            tbPhoneNumber.Clear();
            dpStartDate.SelectedDate = DateTime.Now;
            rbSale.IsChecked = true;
            tbSales.Clear();

            lvEmployees.ItemContainerStyle = new Style(typeof(ListViewItem));
        }

        private bool IsValidInput()
        {
            if (string.IsNullOrWhiteSpace(tbEmployeeID.Text))
            {
                MessageBox.Show("Mã nhân viên không được để trống.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbFullName.Text))
            {
                MessageBox.Show("Họ tên không được để trống.");
                return false;
            }

            if (dpStartDate.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Ngày vào làm không được lớn hơn ngày hiện tại.");
                return false;
            }

            return true;
        }

        private Employee CreateEmployeeFromInput()
        {
            Employee newEmployee = new Employee
            {
                EmployeeID = tbEmployeeID.Text,
                FullName = tbFullName.Text,
                Gender = (string)((ComboBoxItem)cbGender.SelectedItem).Content,
                PhoneNumber = tbPhoneNumber.Text,
                StartDate = dpStartDate.SelectedDate.Value
            };

            if (rbSale.IsChecked == true)
            {
                newEmployee.EmployeeType = "Bán hàng";
                newEmployee.SalesOrFuelAllowance = double.Parse(tbSales.Text);
            }
            else
            {
                newEmployee.EmployeeType = "Giao nhận";
                newEmployee.SalesOrFuelAllowance = double.Parse(tbFuelAllowance.Text);
            }

            return newEmployee;
        }

        private void SaveToExcel()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet workSheet;
                if (package.Workbook.Worksheets["Sheet1"] == null)
                {
                    workSheet = package.Workbook.Worksheets.Add("Sheet1");
                }
                else
                {
                    workSheet = package.Workbook.Worksheets["Sheet1"];
                    workSheet.Cells.Clear();
                }

                int rowNumber = 1;
                foreach (var employee in _employees)
                {
                    workSheet.Cells[rowNumber, 1].Value = employee.EmployeeID;
                    workSheet.Cells[rowNumber, 2].Value = employee.FullName;
                    workSheet.Cells[rowNumber, 3].Value = employee.Gender;
                    workSheet.Cells[rowNumber, 4].Value = employee.PhoneNumber;
                    workSheet.Cells[rowNumber, 5].Value = employee.StartDate.ToString("dd/MM/yyyy");
                    workSheet.Cells[rowNumber, 6].Value = employee.EmployeeType;
                    workSheet.Cells[rowNumber, 7].Value = employee.SalesOrFuelAllowance;
                    rowNumber++;
                }

                package.Save();
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();
            tbEmployeeID.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInput())
            {
                var newEmployee = CreateEmployeeFromInput();
                _employees.Add(newEmployee);
                lvEmployees.SelectedItem = newEmployee;
                SaveToExcel();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvEmployees.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này không?", "Xác nhận", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _employees.Remove((Employee)lvEmployees.SelectedItem);
                    ClearInputFields();
                    SaveToExcel();
                }
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee != null)
            {
                _selectedEmployee.EmployeeID = tbEmployeeID.Text;
                _selectedEmployee.FullName = tbFullName.Text;
                _selectedEmployee.Gender = (string)((ComboBoxItem)cbGender.SelectedItem).Content;
                _selectedEmployee.PhoneNumber = tbPhoneNumber.Text;
                _selectedEmployee.StartDate = dpStartDate.SelectedDate.Value;

                if (rbSale.IsChecked == true)
                {
                    _selectedEmployee.EmployeeType = "Bán hàng";
                    _selectedEmployee.SalesOrFuelAllowance = double.Parse(tbSales.Text);
                }
                else
                {
                    _selectedEmployee.EmployeeType = "Giao nhận";
                    _selectedEmployee.SalesOrFuelAllowance = double.Parse(tbFuelAllowance.Text);
                }

                // Refresh the ListView to update the row color
                lvEmployees.Items.Refresh();
            }
        }
        private void btnSort_Click(object sender, RoutedEventArgs e)
        {
            var sortedEmployees = _employees.OrderByDescending(employee => employee.YearsOfExperience)
                                            .ThenBy(employee => employee.FullName)
                                            .ToList();

            _employees.Clear();

            foreach (var employee in sortedEmployees)
            {
                _employees.Add(employee);
            }
        }

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            int saleCount = _employees.Count(employee => employee.EmployeeType == "Bán hàng");
            int deliveryCount = _employees.Count(employee => employee.EmployeeType == "Giao nhận");

            double totalSaleSalary = _employees.Where(employee => employee.EmployeeType == "Bán hàng").Sum(employee => employee.Salary);
            double totalDeliverySalary = _employees.Where(employee => employee.EmployeeType == "Giao nhận").Sum(employee => employee.Salary);

            MessageBox.Show($"Công ty hiện có {saleCount} nhân viên bán hàng với tổng lương chi là {totalSaleSalary}, {deliveryCount} nhân viên giao nhận với tổng lương chi là {totalDeliverySalary}.");
        }

        private void lvEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEmployee = (Employee)lvEmployees.SelectedItem;

            if (_selectedEmployee != null)
            {
                tbEmployeeID.Text = _selectedEmployee.EmployeeID;
                tbFullName.Text = _selectedEmployee.FullName;
                cbGender.SelectedItem = _selectedEmployee.Gender == "Nam" ? cbGender.Items[0] : cbGender.Items[1];
                tbPhoneNumber.Text = _selectedEmployee.PhoneNumber;
                dpStartDate.SelectedDate = _selectedEmployee.StartDate;

                if (_selectedEmployee.EmployeeType == "Bán hàng")
                {
                    rbSale.IsChecked = true;
                    tbSales.Text = _selectedEmployee.SalesOrFuelAllowance.ToString();
                }
                else
                {
                    rbDelivery.IsChecked = true;
                    tbFuelAllowance.Text = _selectedEmployee.SalesOrFuelAllowance.ToString();
                }
            }
        }

        public class Employee : INotifyPropertyChanged
{
    private string _employeeID;
    public string EmployeeID
    {
        get { return _employeeID; }
        set
        {
            if (_employeeID != value)
            {
                _employeeID = value;
                OnPropertyChanged("EmployeeID");
            }
        }
    }

    private string _fullName;
    public string FullName
    {
        get { return _fullName; }
        set
        {
            if (_fullName != value)
            {
                _fullName = value;
                OnPropertyChanged("FullName");
            }
        }
    }

    private string _gender;
    public string Gender
    {
        get { return _gender; }
        set
        {
            if (_gender != value)
            {
                _gender = value;
                OnPropertyChanged("Gender");
            }
        }
    }

    private string _phoneNumber;
    public string PhoneNumber
    {
        get { return _phoneNumber; }
        set
        {
            if (_phoneNumber != value)
            {
                _phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }
    }

    private DateTime _startDate;
    public DateTime StartDate
    {
        get { return _startDate; }
        set
        {
            if (_startDate != value)
            {
                _startDate = value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("YearsOfExperience");
                OnPropertyChanged("IsSenior");
                OnPropertyChanged("Salary");
            }
        }
    }

    private string _employeeType;
    public string EmployeeType
    {
        get { return _employeeType; }
        set
        {
            if (_employeeType != value)
            {
                _employeeType = value;
                OnPropertyChanged("EmployeeType");
                OnPropertyChanged("Salary");
            }
        }
    }

    private double _salesOrFuelAllowance;
    public double SalesOrFuelAllowance
    {
        get { return _salesOrFuelAllowance; }
        set
        {
            if (_salesOrFuelAllowance != value)
            {
                _salesOrFuelAllowance = value;
                OnPropertyChanged("SalesOrFuelAllowance");
                OnPropertyChanged("Salary");
            }
        }
    }

    public double YearsOfExperience
    {
        get
        {
            return (DateTime.Now - StartDate).TotalDays / 365;
        }
    }

            public string IsSenior
            {
                get
                {
                    if (YearsOfExperience > 5)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }

            public double Salary
    {
        get
        {
            double baseSalary = 7000000;
            if (EmployeeType == "Bán hàng")
            {
                return baseSalary + SalesOrFuelAllowance * 0.1;
            }
            else
            {
                return baseSalary + SalesOrFuelAllowance;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
    }
}
