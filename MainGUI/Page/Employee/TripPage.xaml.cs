using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using CoreLibrary.Service;
using System.Windows;
using System.Windows.Controls;

namespace MainGUI.Page.Employee
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

        private Border CreateTripRequestRow(BusinessTrip request)
        {
            var rowBorder = new Border { Style = (Style)Application.Current.FindResource("TableRowStyle") };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Destination
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });   // Purpose
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Start Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // End Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });   // Status

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
            Grid.SetColumn(statusTextBlock, 4);
            grid.Children.Add(statusTextBlock);

            rowBorder.Child = grid;
            return rowBorder;
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
