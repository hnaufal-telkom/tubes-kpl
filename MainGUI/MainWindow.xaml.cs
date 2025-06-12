using MainGUI.Page.Authentication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using CoreLibrary.InterfaceLib;
using CoreLibrary.Service;
using CoreLibrary.Repository;
using CoreLibrary.ModelLib;
using Serilog;
using CoreLibrary;

namespace MainGUI
{
    public partial class MainWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty LoggedInUserNameProperty =
            DependencyProperty.Register("LoggedInUserName", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public string LoggedInUserName
        {
            get { return (string)GetValue(LoggedInUserNameProperty); }
            set { SetValue(LoggedInUserNameProperty, value); }
        }

        public static readonly DependencyProperty LoggedInUserRoleProperty =
            DependencyProperty.Register("LoggedInUserRole", typeof(Role), typeof(MainWindow), new PropertyMetadata(Role.Employee));

        public Role LoggedInUserRole
        {
            get { return (Role)GetValue(LoggedInUserRoleProperty); }
            set { SetValue(LoggedInUserRoleProperty, value); }
        }
        #endregion

        private readonly IUserService _userService;
        private readonly ILeaveRequestService _leaveService;
        private readonly IBusinessTripService _businessTripService;
        private readonly IPayrollService _payrollService;

        public MainWindow()
        {
            InitializeComponent();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/app_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            IUserRepository userRepository = new InMemoryUserRepository(Log.Logger);
            ILeaveRequestRepository leaveRepository = new InMemoryLeaveRequestRepository(Log.Logger);
            IBusinessTripRepository businessTripRepository = new InMemoryBusinessTripRepository(Log.Logger);
            IPayrollRepository payrollRepository = new InMemoryPayrollRepository(Log.Logger);

            _userService = new UserService(userRepository, Log.Logger);
            _leaveService = new LeaveService(leaveRepository, _userService, Log.Logger);
            _businessTripService = new BusinessTripService(businessTripRepository, userRepository, Log.Logger);
            _payrollService = new PayrollService(payrollRepository, userRepository, leaveRepository, businessTripRepository, Log.Logger);

            SeedUsers();
            SeedRequests();

            LoadLoginPage();
            MainFrame.Navigating += MainFrame_Navigating;
        }

        #region Page Navigation
        private void LoadLoginPage()
        {
            Login login = new Login(this, _userService, _leaveService, _businessTripService, _payrollService);
            if (MainFrame != null)
            {
                MainFrame.Navigate(login);
            }
        }

        public Frame GetMainFrame()
        {
            return MainFrame;
        }
        #endregion

        #region Event Handlers
        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        #endregion


        #region Dummy Data
        private void SeedUsers()
        {
            try
            {
                if (_userService.Authenticate("a", "12341234") == null)
                {
                    _userService.Register("Siti", "a", "12341234", Role.Employee, 3000000);
                }
            }
            catch (KeyNotFoundException)
            {
                _userService.Register("Siti", "a", "12341234", Role.Employee, 3000000);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error seeding employee user: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error seeding employee user: {Message}", ex.Message);
            }

            try
            {
                if (_userService.Authenticate("b", "123123123") == null)
                {
                    _userService.Register("Asep", "b", "123123123", Role.HRD, 5000000);
                }
            }
            catch (KeyNotFoundException)
            {
                _userService.Register("Asep", "b", "123123123", Role.HRD, 5000000);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error seeding HRD user: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error seeding HRD user: {Message}", ex.Message);
            }

            try
            {
                if (_userService.Authenticate("c", "123123123") == null)
                {
                    _userService.Register("Deni", "c", "123123123", Role.Supervisor, 7000000);
                }
            }
            catch (KeyNotFoundException)
            {
                _userService.Register("Deni", "c", "123123123", Role.Supervisor, 7000000);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error seeding supervisor user: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error seeding supervisor user: {Message}", ex.Message);
            }

            try
            {
                if (_userService.Authenticate("d", "123123123") == null)
                {
                    _userService.Register("Tuti", "d", "123123", Role.Finance, 4000000);
                }
            }
            catch (KeyNotFoundException)
            {
                _userService.Register("Tuti", "d", "123123123", Role.Finance, 4000000);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error seeding finance user: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error seeding finance user: {Message}", ex.Message);
            }

            try
            {
                if (_userService.Authenticate("e", "123123123") == null)
                {
                    _userService.Register("Cecep", "e", "123123123", Role.SysAdmin, 6000000);
                }
            }
            catch (KeyNotFoundException)
            {
                _userService.Register("Cecep", "e", "123123123", Role.SysAdmin, 6000000);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error seeding SysAdmin user: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error seeding SysAdmin user: {Message}", ex.Message);
            }
        }

        private void SeedRequests()
        {
            User employeeUser = null;
            try { employeeUser = _userService.Authenticate("employee@mail.com", "Employee123"); } catch { /* ignore */ }

            User supervisorUser = null;
            try { supervisorUser = _userService.Authenticate("supervisor@mail.com", "Supervisor123"); } catch { /* ignore */ }

            if (employeeUser != null && supervisorUser != null)
            {
                try
                {
                    var pendingLeave = _leaveService.GetByUserId(employeeUser.Id).FirstOrDefault(l => l.Status == RequestStatus.Pending);
                    if (pendingLeave == null)
                    {
                        _leaveService.SubmitRequest(employeeUser.Id, DateTime.Today.AddDays(10), DateTime.Today.AddDays(12), "Family vacation");
                    }
                }
                catch (Exception ex) { Log.Warning(ex, "Error seeding pending leave request: {Message}", ex.Message); }

                try
                {
                    var approvedLeave = _leaveService.GetByUserId(employeeUser.Id).FirstOrDefault(l => l.Status == RequestStatus.Approved && l.StartDate < DateTime.Today);
                    if (approvedLeave == null)
                    {
                        var req = _leaveService.SubmitRequest(employeeUser.Id, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3), "Personal reasons");
                        _leaveService.ApproveRequest(req.Id, supervisorUser.Id);
                    }
                }
                catch (Exception ex) { Log.Warning(ex, "Error seeding approved leave request: {Message}", ex.Message); }

                try
                {
                    var approvedTrip = _businessTripService.GetByUserId(employeeUser.Id).FirstOrDefault(t => t.Status == RequestStatus.Approved && t.StartDate < DateTime.Today);
                    if (approvedTrip == null)
                    {
                        var trip = _businessTripService.SubmitRequest(employeeUser.Id, "Bandung", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-8), "Client meeting", 1500000);
                        _businessTripService.ApproveRequest(trip.Id, supervisorUser.Id);
                    }
                }
                catch (Exception ex) { Log.Warning(ex, "Error seeding approved trip request: {Message}", ex.Message); }

                try
                {
                    var pendingTrip = _businessTripService.GetByUserId(employeeUser.Id).FirstOrDefault(t => t.Status == RequestStatus.Pending);
                    if (pendingTrip == null)
                    {
                        _businessTripService.SubmitRequest(employeeUser.Id, "Surabaya", DateTime.Today.AddDays(20), DateTime.Today.AddDays(22), "Training", 2000000);
                    }
                }
                catch (Exception ex) { Log.Warning(ex, "Error seeding pending trip request: {Message}", ex.Message); }
            }
        }
        #endregion
    }
}