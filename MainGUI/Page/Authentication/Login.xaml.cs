using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using System.Windows;
using System.Windows.Controls;

namespace MainGUI.Page.Authentication
{
    public partial class Login : System.Windows.Controls.Page
    {
        private MainWindow _mainWindow;
        private IUserService _userService;
        private ILeaveRequestService _leaveService;
        private IBusinessTripService _businessTripService;
        private IPayrollService _payrollService;
        public Login(MainWindow mw, IUserService userService, ILeaveRequestService leaveService, IBusinessTripService businessTripService, IPayrollService payrollService)
        {
            InitializeComponent();
            this._mainWindow = mw;
            this._userService = userService;
            this._leaveService = leaveService;
            this._businessTripService = businessTripService;
            this._payrollService = payrollService;
        }

        #region Event Handlers
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            try
            {
                User authenticatedUser = _userService.Authenticate(email, password);

                if (authenticatedUser != null)
                {
                    if (_mainWindow != null)
                    {
                        _mainWindow.LoggedInUserName = authenticatedUser.Name;
                        _mainWindow.LoggedInUserRole = authenticatedUser.Role;
                    }

                    if (this.NavigationService != null)
                    {
                        while (this.NavigationService.CanGoBack)
                        {
                            this.NavigationService.RemoveBackEntry();
                        }

                        System.Windows.Controls.Page dashboardPage = null;

                        switch (authenticatedUser.Role)
                        {
                            case Role.Employee:
                                dashboardPage = new MainGUI.Page.Dashboard(_userService, _leaveService, _businessTripService, authenticatedUser.Id, _payrollService);
                                break;
                            case Role.HRD:
                                dashboardPage = new MainGUI.Page.Dashboard(_userService, _leaveService, _businessTripService, authenticatedUser.Id, _payrollService);
                                break;
                            case Role.Supervisor:
                                dashboardPage = new MainGUI.Page.Dashboard(_userService, _leaveService, _businessTripService, authenticatedUser.Id, _payrollService);
                                break;
                            case Role.Finance:
                                dashboardPage = new MainGUI.Page.Dashboard(_userService, _leaveService, _businessTripService, authenticatedUser.Id, _payrollService);
                                break;
                            case Role.SysAdmin:
                                dashboardPage = new MainGUI.Page.Dashboard(_userService, _leaveService, _businessTripService, authenticatedUser.Id, _payrollService);
                                break;
                            default:
                                MessageBox.Show("Unauthorized access", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                        }

                        if (dashboardPage != null)
                        {
                            this.NavigationService.Navigate(dashboardPage);
                        }
                        else
                        {
                            MessageBox.Show("Dashboard page not found for the user role.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Navigation service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid email or password", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during authentication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
