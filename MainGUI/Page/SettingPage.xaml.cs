using CoreLibrary.InterfaceLib;
using System.Windows;

namespace MainGUI.Page
{
    public partial class SettingPage : System.Windows.Controls.Page
    {
        private IUserService _userService;
        private int _currentUserId;

        public SettingPage(IUserService service, int currentUserId)
        {
            InitializeComponent();
            _userService = service;
            _currentUserId = currentUserId;
        }

        private void UpdateEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newEmail = NewEmailTextBox.Text.Trim();

                if (string.IsNullOrEmpty(newEmail))
                {
                    MessageBox.Show("Please enter an email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _userService.UpdateUserProfile(_currentUserId, _userService.GetUserById(_currentUserId).Name, newEmail);

                MessageBox.Show("Email updated succesfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                NewEmailTextBox.Text = string.Empty;
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
        private void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldPassword = OldPasswordTextBox.Password.Trim();
                string newPassword = NewPasswordTextBox.Password.Trim();
                string confirmPassword = ConfirmPasswordTextBox.Password.Trim();

                if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Please make sure all inputs are valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("New password and confirmation do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _userService.ChangePassword(_currentUserId, oldPassword, newPassword);

                MessageBox.Show("Password updated succesfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OldPasswordTextBox.Password = string.Empty;
                NewPasswordTextBox.Password = string.Empty;
                ConfirmPasswordTextBox.Password = string.Empty;
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
    }
}
