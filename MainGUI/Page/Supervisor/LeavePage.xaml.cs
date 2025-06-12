using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainGUI.Page.Supervisor
{

    public partial class LeavePage : System.Windows.Controls.Page
    {
        private ILeaveRequestService _leaveService;
        private int _loggedInUserId;

        public LeavePage(ILeaveRequestService leaveService, int loggedInUserId)
        {
            InitializeComponent();
            _leaveService = leaveService;
            _loggedInUserId = loggedInUserId;

            LoadLeaveHistory();
            LoadAllLeaveReq();
        }

        private void SubmitLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;
                string description = DescriptionTextBox.Text;

                if (!startDate.HasValue || !endDate.HasValue || string.IsNullOrEmpty(description))
                {
                    MessageBox.Show("Please fill in all required fields (Leave Type, Start Date, End Date, Description).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _leaveService.SubmitRequest(_loggedInUserId, startDate.Value, endDate.Value, description);

                MessageBox.Show("Leave request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                StartDatePicker.SelectedDate = null;
                EndDatePicker.SelectedDate = null;
                DescriptionTextBox.Text = string.Empty;

                LoadLeaveHistory();
                LoadAllLeaveReq();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Submission Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLeaveHistory()
        {
            LeaveHistoryStackPanel.Children.Clear();

            try
            {
                IEnumerable<LeaveRequest> userLeaveRequest = _leaveService.GetByUserId(_loggedInUserId).OrderByDescending(l => l.RequestDate);
                
                foreach (var request in userLeaveRequest)
                {
                    LeaveHistoryStackPanel.Children.Add(CreateLeaveRequestRow(request));
                }
            }
            catch
            {
                LeaveHistoryStackPanel.Children.Add(CreateNoDataTextBlock("No leave requests found."));
            }
        }

        private void LoadAllLeaveReq()
        {
            PendingLeaveStackPanel.Children.Clear();

            try
            {
                IEnumerable<LeaveRequest> userLeaveRequest = _leaveService.GetPendingRequest().OrderByDescending(l => l.RequestDate);

                foreach (var request in userLeaveRequest)
                {
                    PendingLeaveStackPanel.Children.Add(CreateLeaveRequestRow(request));
                }
            }
            catch
            {
                PendingLeaveStackPanel.Children.Add(CreateNoDataTextBlock("No leave requests found."));
            }
        }

        private Border CreateLeaveRequestRow(LeaveRequest request)
        {
            var rowBorder = new Border { Style = (Style)Application.Current.FindResource("TableRowStyle") };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Type
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Start Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // End Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Duration
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Status
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });   // Approve & Reject

            var typeTextBlock = new TextBlock
            {
                Text = request.Description,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(typeTextBlock, 0);
            grid.Children.Add(typeTextBlock);

            var startDateTextBlock = new TextBlock
            {
                Text = request.StartDate.ToString("yyyy-MM-dd"),
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(startDateTextBlock, 1);
            grid.Children.Add(startDateTextBlock);

            var endDateTextBlock = new TextBlock
            {
                Text = request.EndDate.ToString("yyyy-MM-dd"),
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(endDateTextBlock, 2);
            grid.Children.Add(endDateTextBlock);

            var duration = (request.EndDate - request.StartDate).Days + 1;
            var durationTextBlock = new TextBlock
            {
                Text = $"{duration} Day{(duration > 1 ? "s" : "")}",
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(durationTextBlock, 3);
            grid.Children.Add(durationTextBlock);

            var statusTextBlock = new TextBlock
            {
                Text = request.Status.ToString(),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (request.Status == RequestStatus.Approved)
            {
                statusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorGreen");
            }
            else if (request.Status == RequestStatus.Rejected)
            {
                statusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorRed");
            }
            else
            {
                statusTextBlock.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorYellow");
            }
            Grid.SetColumn(statusTextBlock, 4);
            grid.Children.Add(statusTextBlock);

            if (request.Status == RequestStatus.Pending)
            {
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var approveButton = new Button
                {
                    Content = new TextBlock { Text = "\uE10B", FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorGreen"), FontSize = 16 },
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    ToolTip = "Approve",
                    Tag = request
                };
                approveButton.Click += ApproveButton_Click;

                var rejectButton = new Button
                {
                    Content = new TextBlock { Text = "\uE10A", FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("AccentColorRed"), FontSize = 16 },
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    ToolTip = "Reject",
                    Margin = new Thickness(10, 0, 0, 0),
                    Tag = request
                };
                rejectButton.Click += RejectButton_Click;

                buttonPanel.Children.Add(approveButton);
                buttonPanel.Children.Add(rejectButton);

                Grid.SetColumn(buttonPanel, 5);
                grid.Children.Add(buttonPanel);
            }

            rowBorder.Child = grid;
            return rowBorder;
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LeaveRequest request)
            {
                _leaveService.ApproveRequest(request.Id, _loggedInUserId);
                MessageBox.Show($"Request Approved!");

                LoadLeaveHistory();
                LoadAllLeaveReq();
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LeaveRequest request)
            {
                _leaveService.RejectRequest(request.Id, _loggedInUserId, "GABOLEH");
                MessageBox.Show($"Request Rejected!");

                LoadLeaveHistory();
                LoadAllLeaveReq();
            }
        }

        private TextBlock CreateNoDataTextBlock(string message)
        {
            return new TextBlock
            {
                Text = message,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SubTextColor"),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
        }
    }
}
