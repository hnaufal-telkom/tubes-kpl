using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using CoreLibrary.Service;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainGUI.Page.Supervisor
{
    public partial class TripPage : System.Windows.Controls.Page
    {
        private IBusinessTripService _businessTripService;
        private int _loggedInUserId;

        public TripPage(IBusinessTripService tripService, int loggedInUserId)
        {
            InitializeComponent();
            _businessTripService = tripService;
            _loggedInUserId = loggedInUserId;

            LoadTripHistory();
            LoadAllTripReq();
        }

        private void SubmitTripRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string destination = DestinationTextBox.Text;
                string purpose = (PurposeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;
                string estimatedCostText = EstimatedCostTextBox.Text;
                string notes = NotesTextBox.Text;

                if (string.IsNullOrEmpty(destination) || string.IsNullOrEmpty(purpose) ||
                    !startDate.HasValue || !endDate.HasValue || string.IsNullOrEmpty(estimatedCostText))
                {
                    MessageBox.Show("Please fill in all required fields (Destination, Purpose, Start Date, End Date, Estimated Cost).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(estimatedCostText, out decimal estimatedCost))
                {
                    MessageBox.Show("Estimated Cost must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _businessTripService.SubmitRequest(_loggedInUserId, destination, startDate.Value, endDate.Value, purpose, estimatedCost);

                MessageBox.Show("Business trip request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DestinationTextBox.Text = string.Empty;
                PurposeComboBox.SelectedItem = null;
                StartDatePicker.SelectedDate = null;
                EndDatePicker.SelectedDate = null;
                EstimatedCostTextBox.Text = string.Empty;
                NotesTextBox.Text = string.Empty;

                LoadTripHistory();
                LoadAllTripReq();
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

        private void LoadTripHistory()
        {
            TripHistoryStackPanel.Children.Clear();

            try
            {
                IEnumerable<BusinessTrip> userTripRequests = _businessTripService.GetByUserId(_loggedInUserId)
                                                                               .OrderByDescending(tr => tr.RequestDate);

                if (!userTripRequests.Any())
                {
                    TripHistoryStackPanel.Children.Add(CreateNoDataTextBlock("No business trip requests found."));
                    return;
                }

                foreach (var request in userTripRequests)
                {
                    TripHistoryStackPanel.Children.Add(CreateTripRequestRow(request));
                }
            }
            catch (KeyNotFoundException)
            {
                TripHistoryStackPanel.Children.Add(CreateNoDataTextBlock("No business trip requests found."));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trip history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TripHistoryStackPanel.Children.Add(CreateNoDataTextBlock("Error loading data."));
            }
        }

        private void LoadAllTripReq()
        {
            PendingTripStackPanel.Children.Clear();

            try
            {
                IEnumerable<BusinessTrip> userLeaveRequest = _businessTripService.GetPendingRequests().OrderByDescending(l => l.RequestDate);

                foreach (var request in userLeaveRequest)
                {
                    PendingTripStackPanel.Children.Add(CreateTripRequestRow(request));
                }
            }
            catch
            {
                PendingTripStackPanel.Children.Add(CreateNoDataTextBlock("No leave requests found."));
            }
        }

        private Border CreateTripRequestRow(BusinessTrip request)
        {
            var rowBorder = new Border { Style = (Style)Application.Current.FindResource("TableRowStyle") };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Destination
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });   // Purpose
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Start Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // End Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Status
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Approve & Reject

            var destinationTextBlock = new TextBlock
            {
                Text = request.Destination,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(destinationTextBlock, 0);
            grid.Children.Add(destinationTextBlock);

            var purposeTextBlock = new TextBlock
            {
                Text = request.Purpose,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(purposeTextBlock, 1);
            grid.Children.Add(purposeTextBlock);

            var startDateTextBlock = new TextBlock
            {
                Text = request.StartDate.ToString("yyyy-MM-dd"),
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(startDateTextBlock, 2);
            grid.Children.Add(startDateTextBlock);

            var endDateTextBlock = new TextBlock
            {
                Text = request.EndDate.ToString("yyyy-MM-dd"),
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(endDateTextBlock, 3);
            grid.Children.Add(endDateTextBlock);

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
            Grid.SetColumn(statusTextBlock, 4);
            grid.Children.Add(statusTextBlock);

            rowBorder.Child = grid;
            return rowBorder;
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BusinessTrip request)
            {
                _businessTripService.ApproveRequest(request.Id, _loggedInUserId);
                MessageBox.Show($"Request Approved!");

                LoadTripHistory();
                LoadAllTripReq();
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BusinessTrip request)
            {
                _businessTripService.RejectRequest(request.Id, _loggedInUserId, "GABOLEH");
                MessageBox.Show($"Request Rejected!");

                LoadTripHistory();
                LoadAllTripReq();
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
