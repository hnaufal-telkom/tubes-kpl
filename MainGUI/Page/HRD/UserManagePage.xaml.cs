using CoreLibrary;
using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MainGUI.Page.HRD
{
    public partial class UserManagePage : System.Windows.Controls.Page
    {
        private readonly IUserService _userService;
        private User _selectedUser;
        private int _loggedInUserId;

        public UserManagePage(IUserService userService, int loggedInUserId)
        {
            InitializeComponent();
            _userService = userService;
            _loggedInUserId = loggedInUserId;

            PopulateRoleComboBox();
            LoadAllUsers();
        }

        private void PopulateRoleComboBox()
        {
            RoleComboBox.ItemsSource = Enum.GetValues(typeof(Role)).Cast<Role>();
            RoleComboBox.SelectedIndex = 0;
        }

        private void LoadAllUsers()
        {
            UserListStackPanel.Children.Clear();
            try
            {
                IEnumerable<User> users = _userService.GetAllUser();
                foreach (User user in users)
                {
                    UserListStackPanel.Children.Add(CreateUserRow(user));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateUserRow(User user)
        {
            var rowBorder = new Border { Style = (Style)Application.Current.FindResource("TableRowStyle") };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            grid.Children.Add(CreateTextBlock(user.Name, 0));
            grid.Children.Add(CreateTextBlock(user.Email, 1));
            grid.Children.Add(CreateTextBlock(user.Role.ToString(), 2));
            grid.Children.Add(CreateTextBlock(user.BasicSalary.ToString("C"), 3));

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var editButton = new Button { Content = "✏️ Edit", Tag = user };
            editButton.Click += EditUser_Click;

            var deleteButton = new Button { Content = "🗑️ Delete", Tag = user, Margin = new Thickness(10, 0, 0, 0) };
            deleteButton.Click += DeleteUser_Click;

            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);
            Grid.SetColumn(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            rowBorder.Child = grid;
            return rowBorder;
        }

        private TextBlock CreateTextBlock(string text, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = (Brush)Application.Current.FindResource("TextColor"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                (_selectedUser == null && string.IsNullOrWhiteSpace(UserPasswordBox.Password)) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_selectedUser == null)
                {
                    var newUser = new User
                    {
                        Name = NameTextBox.Text,
                        Email = EmailTextBox.Text,
                        Password = UserPasswordBox.Password,
                        BasicSalary = decimal.TryParse(SalaryTextBox.Text, out var salary) ? salary : 0,
                        Role = (Role)RoleComboBox.SelectedItem
                    };
                    _userService.Register(newUser.Name, newUser.Email, newUser.Password, newUser.Role, newUser.BasicSalary);
                    MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _selectedUser.Name = NameTextBox.Text;
                    _selectedUser.Email = EmailTextBox.Text;
                    _selectedUser.BasicSalary = decimal.TryParse(SalaryTextBox.Text, out var salary) ? salary : 0;
                    _selectedUser.Role = (Role)RoleComboBox.SelectedItem;

                    if (!string.IsNullOrWhiteSpace(UserPasswordBox.Password))
                    {
                        _selectedUser.Password = UserPasswordBox.Password;
                    }

                    _userService.UpdateUser(_selectedUser);
                    MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ClearForm();
                LoadAllUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User user)
            {
                _selectedUser = user;

                FormTitle.Text = "Edit User";
                NameTextBox.Text = user.Name;
                EmailTextBox.Text = user.Email;
                SalaryTextBox.Text = user.BasicSalary.ToString();
                RoleComboBox.SelectedItem = user.Role;
                UserPasswordBox.Password = "";

                AddUserButton.Content = "Update User";
                CancelEditButton.Visibility = Visibility.Visible;
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User user)
            {
                if (user.Id == _loggedInUserId)
                {
                    MessageBox.Show("You cannot delete your own account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result = MessageBox.Show($"Are you sure you want to delete {user.Name}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _selectedUser = user;
                        _userService.DeleteUserAccount(_loggedInUserId, _selectedUser.Id);
                        LoadAllUsers();
                        MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedUser = null;
            FormTitle.Text = "Add New User";
            NameTextBox.Text = "";
            EmailTextBox.Text = "";
            UserPasswordBox.Password = "";
            SalaryTextBox.Text = "";
            RoleComboBox.SelectedIndex = 0;
            AddUserButton.Content = "Add User";
            CancelEditButton.Visibility = Visibility.Collapsed;
        }
    }
}
