using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MainGUI.Page.Finance
{
    public partial class FinancePage : System.Windows.Controls.Page
    {
        private readonly IPayrollService _payrollService;
        private readonly IUserService _userService;
        private readonly int _loggedInUserId;

        public FinancePage(IPayrollService payrollService, IUserService userService, int loggedInUserId)
        {
            InitializeComponent();
            _payrollService = payrollService;
            _userService = userService;
            _loggedInUserId = loggedInUserId;

            PeriodPickerStart.SelectedDate = DateTime.Now;
            PeriodPickerEnd.SelectedDate = DateTime.Now.AddDays(1);
        }

        public void GeneratePayrollButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(IDUser.Text) || PeriodStart.SelectedDate == null || DueDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date and due date for the payroll.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime startDate = PeriodStart.SelectedDate ?? DateTime.Now;
            DateTime endDate = DueDate.SelectedDate ?? DateTime.Now.AddDays(1);

            if (startDate >= endDate)
            {
                MessageBox.Show("Start date must be before the due date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int userId = int.Parse(IDUser.Text);
                Payroll payroll = _payrollService.GeneratePayroll(userId, startDate, endDate);
                if (payroll == null)
                {
                    MessageBox.Show("No payrolls generated for the selected period.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("payroll(s) generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate payrolls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadPayrollsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (PeriodPickerStart.SelectedDate == null || PeriodPickerEnd.SelectedDate == null)
            {
                MessageBox.Show("Please select a period to load payrolls.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PendingPayrollStackPanel.Children.Clear();  
            ProcessedPayrollStackPanel.Children.Clear();

            DateTime startPeriod = PeriodPickerStart.SelectedDate.Value;
            DateTime endPeriod = PeriodPickerEnd.SelectedDate.Value;

            if (startPeriod >= endPeriod)
            {
                MessageBox.Show("Start date must be before the end date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                foreach (Payroll payroll in _payrollService.GetPayrollsByPeriod(startPeriod, endPeriod))
                {
                    if (payroll.IsPaid)
                    {
                        ProcessedPayrollStackPanel.Children.Add(CreatePayrollRow(payroll, isPending: false));
                    }
                    else
                    {
                        PendingPayrollStackPanel.Children.Add(CreatePayrollRow(payroll, isPending: true));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load payroll data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreatePayrollRow(Payroll payroll, bool isPending)
        {
            var rowBorder = new Border { Style = (Style)Application.Current.FindResource("TableRowStyle") };
            var grid = new Grid();

            for (int i = 0; i < 5; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(i == 0 ? 2 : 1.5, GridUnitType.Star) });
            }

            var employee = _userService.GetUserById(payroll.UserId);

            string periodText = $"{payroll.PeriodStart:dd MMM yyyy} - {payroll.PeriodEnd:dd MMM yyyy}";


            grid.Children.Add(CreateTextBlock(employee?.Name ?? "Unknown User", 0));
            grid.Children.Add(CreateTextBlock(periodText,1));
            grid.Children.Add(CreateTextBlock(payroll.BasicSalary.ToString(), 2));
            grid.Children.Add(CreateTextBlock(payroll.PaymentDate.ToString(), 3));

            if (isPending)
            {
                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

                var payButton = new Button
                {
                    Content = "💰 Pay",
                    Tag = payroll,     
                    Margin = new Thickness(0, 0, 5, 0)
                };
                payButton.Click += PayButton_Click;

                buttonPanel.Children.Add(payButton);

                Grid.SetColumn(buttonPanel, 4);
                grid.Children.Add(buttonPanel);
            }
            else
            {
                    var statusBlock = CreateTextBlock("🤑 Paid", 4);
                    statusBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    statusBlock.FontWeight = FontWeights.SemiBold;
                    statusBlock.Foreground = (Brush)Application.Current.FindResource("AccentColorGreen");
                    grid.Children.Add(statusBlock);
            }

            rowBorder.Child = grid;
            return rowBorder;
        }

        private TextBlock CreateTextBlock(string text, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = (Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5, 0, 5, 0)
            };
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Payroll payroll)
            {
                var result = MessageBox.Show($"Are you sure you want to process payment for {payroll.BasicSalary:C} to {(_userService.GetUserById(payroll.UserId)?.Name)}?",
                                             "Confirm Payment", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        payroll.IsPaid = true;
                        _payrollService.MarkAsPaid(payroll.Id, _loggedInUserId);
                        MessageBox.Show("Payment processed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPayrollsButton_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to process payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
