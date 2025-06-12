using CoreLibrary;
using CoreLibrary.InterfaceLib;
using MainGUI.Page.Authentication;
using MainGUI.Page.Employee;
using MainGUI.Page.HRD;
using System.Windows;

namespace MainGUI.Page
{
    public partial class Dashboard : System.Windows.Controls.Page
    {
        private IUserService _userService;
        private ILeaveRequestService _leaveService;
        private IBusinessTripService _businessTripService;
        private int _loggedInUserId;
        private IPayrollService _payrollService;

        public Dashboard(IUserService userService, ILeaveRequestService leaveService, IBusinessTripService businessTripService, int loggedInUserId, IPayrollService payrollService)
        {
            InitializeComponent();
            _userService = userService;
            _leaveService = leaveService;
            _businessTripService = businessTripService;
            _loggedInUserId = loggedInUserId;
            _payrollService = payrollService;
            SetName();
            
            if (_userService.GetUserById(_loggedInUserId).Role == Role.HRD || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
            {
                ManageUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                ManageUserButton.Visibility = Visibility.Collapsed;
            }

            if (_userService.GetUserById(_loggedInUserId).Role == Role.Finance || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
            {
                ManagePayrollButton.Visibility = Visibility.Visible;
            }
            else
            {
                ManagePayrollButton.Visibility = Visibility.Collapsed;
            }

            NavigateToDashboard(null, null);
        }

        private void SetName()
        {
            MainGUI.MainWindow mainWindow = App.Current.MainWindow as MainGUI.MainWindow;
            if (mainWindow != null)
            {
                NameDashboardBar.Text = $"{mainWindow.LoggedInUserRole}";
            }
            else
            {
                NameDashboardBar.Text = "Hello, User 👋";
            }
        }

        #region Navigation Methods
        private void NavigateToDashboard(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_userService.GetUserById(_loggedInUserId).Role == Role.Supervisor || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
                {
                    DashboardFrame.Content = new Supervisor.DashboardPage(_userService, _leaveService, _businessTripService, _loggedInUserId);
                }
                else
                {
                    DashboardFrame.Content = new Employee.DashboardPage(_userService, _leaveService, _businessTripService, _loggedInUserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to Dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToLeave(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_userService.GetUserById(_loggedInUserId).Role == Role.Supervisor || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
                {
                    DashboardFrame.Content = new Supervisor.LeavePage(_leaveService, _loggedInUserId);
                }
                else
                {
                    DashboardFrame.Content = new Employee.LeavePage(_leaveService, _loggedInUserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to Leave: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToTrip(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_userService.GetUserById(_loggedInUserId).Role == Role.Supervisor)
                {
                    DashboardFrame.Content = new Supervisor.TripPage(_businessTripService, _loggedInUserId);
                }
                else
                {
                    DashboardFrame.Content = new Employee.TripPage(_businessTripService, _loggedInUserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to Trip: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToUserManagement(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_userService.GetUserById(_loggedInUserId).Role == Role.HRD || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
                {
                    DashboardFrame.Content = new HRD.UserManagePage(_userService, _loggedInUserId);
                }
                else
                {
                    MessageBox.Show("You do not have permission to access User Management.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to User Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NavigateToPayroll(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_userService.GetUserById(_loggedInUserId).Role == Role.Finance || _userService.GetUserById(_loggedInUserId).Role == Role.SysAdmin)
                {
                    DashboardFrame.Content = new Finance.FinancePage(_payrollService, _userService, _loggedInUserId);
                }
                else
                {
                    MessageBox.Show("You do not have permission to access Payroll Management Management.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to Payroll Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NavigateToSettings(object sender, System.Windows.RoutedEventArgs e)
        {
            DashboardFrame.Content = new SettingPage(_userService, _loggedInUserId);
        }

        private void LogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                while (this.NavigationService.CanGoBack)
                {
                    this.NavigationService.RemoveBackEntry();
                }
                this.NavigationService.Navigate(new Login(App.Current.MainWindow as MainWindow, _userService, _leaveService, _businessTripService, _payrollService));
            }
        }
        #endregion
    }
}
