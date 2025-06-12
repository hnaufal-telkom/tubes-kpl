using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using System.Windows;

namespace MainGUI.Page.Supervisor
{
    public partial class DashboardPage : System.Windows.Controls.Page
    {
        private IUserService _userService;
        private ILeaveRequestService _leaveService;
        private IBusinessTripService _businessTripService;
        private int _loggedInUserId;

        public DashboardPage(IUserService userService, ILeaveRequestService leaveService, IBusinessTripService businessTripService, int loggedInUserId)
        {
            InitializeComponent();
            _userService = userService;
            _leaveService = leaveService;
            _businessTripService = businessTripService;
            _loggedInUserId = loggedInUserId;

            LoadEmployeeDashboardData();
        }

        private void LoadEmployeeDashboardData()
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                HelloSignDashboard.Text = $"Hello {mainWindow.LoggedInUserName} 👋";
                LastLoginDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                LastLoginTime.Text = "📅 " + DateTime.Now.ToString("hh:mm:dd");

                User currentUser = _userService.GetUserById(_loggedInUserId);

                if (currentUser != null)
                {
                    try
                    {
                        RemainingLeaveDaysTextBlock.Text = currentUser.RemainingLeaveDays.ToString();
                        var latestLeaveRequest = _leaveService.GetByUserId(_loggedInUserId).OrderByDescending(l => l.RequestDate).FirstOrDefault();
                        if (latestLeaveRequest != null)
                        {
                            LatestLeaveStatusTextBlock.Text = latestLeaveRequest.Status.ToString();
                            LatestLeaveRequestDate.Text = "Last Request " + latestLeaveRequest.RequestDate.ToString("dd/MM/yyyy");
                            if (latestLeaveRequest.Status == RequestStatus.Approved)
                            {
                                LatestLeaveStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorGreen");
                            }
                            else if (latestLeaveRequest.Status == RequestStatus.Rejected)
                            {
                                LatestLeaveStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorRed");
                            }
                            else
                            {
                                LatestLeaveStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorYellow");
                            }
                        }
                        else
                        {
                            LatestLeaveStatusTextBlock.Text = "No Request";
                            LatestLeaveRequestDate.Text = "N/A";
                            LatestLeaveStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SubTextColor");
                        }
                    }
                    catch
                    {
                        LatestLeaveStatusTextBlock.Text = "No Request";
                        LatestLeaveRequestDate.Text = "N/A";
                        LatestLeaveStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SubTextColor");
                    }
                    try
                    {
                        var latestTripRequest = _businessTripService.GetByUserId(_loggedInUserId).OrderByDescending(t => t.RequestDate).FirstOrDefault();
                        if (latestTripRequest != null)
                        {
                            LatestTripStatusTextBlock.Text = latestTripRequest.Status.ToString();
                            LatestTripRequestDate.Text = "Last Request " + latestTripRequest.RequestDate.ToString("dd/MM/yyyy");
                            if (latestTripRequest.Status == RequestStatus.Approved)
                            {
                                LatestTripStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorGreen");
                            }
                            else if (latestTripRequest.Status == RequestStatus.Rejected)
                            {
                                LatestTripStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorRed");
                            }
                            else
                            {
                                LatestTripStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorYellow");
                            }
                        }
                        else
                        {
                            LatestTripStatusTextBlock.Text = "No Request";
                            LatestTripRequestDate.Text = "N/A";
                            LatestTripStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SubTextColor");
                        }
                    }
                    catch
                    {
                        LatestTripStatusTextBlock.Text = "No Request";
                        LatestTripRequestDate.Text = "N/A";
                        LatestTripStatusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SubTextColor");
                    }
                    try
                    {
                        var totalLeaveRequests = _leaveService.GetPendingRequest().Count();
                        if (totalLeaveRequests > 0)
                        {
                            TotalLeaveRequestPending.Text = totalLeaveRequests.ToString();
                            LatestPendingLeaveRequest.Text = "You have " + totalLeaveRequests + " pending leave requests.";
                        }
                        else
                        {
                            TotalLeaveRequestPending.Text = "0";
                        }
                    }
                    catch
                    {
                        TotalLeaveRequestPending.Text = "0";
                    }
                    try
                    {
                        var totalTripRequests = _businessTripService.GetPendingRequests().Count();
                        if (totalTripRequests > 0)
                        {
                            TotalTripRequestPending.Text = totalTripRequests.ToString();
                            LatestPendingLeaveRequest.Text = "You have " + totalTripRequests + " pending leave requests.";
                        }
                        else
                        {
                            TotalTripRequestPending.Text = "0";
                        }
                    }
                    catch
                    {
                        TotalTripRequestPending.Text = "0";
                    }
                }
            }
        }
    }
}
