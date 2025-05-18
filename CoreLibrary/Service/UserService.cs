using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger _logger;

        public UserService(IUserRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger.ForContext<UserService>();
        }

        public User Register(string name, string email, string password, Role role, decimal basicSalary)
        {
            _logger.Information("Registering User: {Name}, Email: {Email}, Role: {Role}", name, email, role);

            ValidateRegistration(name, email, password, basicSalary);

            var user = new User
            {
                Id = _repository.GenerateId(),
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                BasicSalary = basicSalary
            };

            _repository.Add(user);
            _logger.Information("User {Name} registered successfully as {Role}", name, role);
            return user;
        }

        public User Authenticate(string email, string password)
        {
            var user = _repository.GetByEmail(email) ?? throw new KeyNotFoundException("User not found");

            if (user.Password != password)
            {
                _logger.Warning("Invalid credentials for email: {Email}", email);
                throw new KeyNotFoundException("Invalid credentials");
            }

            _logger.Information("User {Name} authenticated successfully", user.Name);
            return user;
        }

        public IEnumerable<User> GetAllUser()
        {
            _logger.Information("Getting all active users");
            return _repository.GetAll();
        }

        public User GetUserById(int userId) => _repository.GetById(userId);

        public void UpdateUserProfile(int userId, string name, string email)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email is required");
            var user = GetUserAndValidate(userId);
            user.Name = name;
            user.Email = email;
            _repository.Update(user);
            _logger.Information("User details updated successfully for ID: {UserId}", userId);
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword)) throw new ArgumentException("New Password is required");
            if (newPassword.Length < 8)
            {
                _logger.Error("New password too short: {Length} characters", newPassword.Length);
                throw new ArgumentException("Password must be at least 8 characters");
            }

            var user = GetUserAndValidate(userId);

            if (user.Password != currentPassword)
            {
                _logger.Error("Current password does not match for user ID: {UserId}", userId);
                throw new ArgumentException("Current password is incorrect");
            }

            user.Password = newPassword;
            _repository.Update(user);
            _logger.Information("Password changed successfully for user ID: {UserId}", userId);
        }

        public void DeleteUserAccount(int adminId, int userId)
        {
            ValidateAdminPermission(adminId);
            _repository.Delete(userId);
            _logger.Information("User account deleted successfully for user ID: {UserId}", userId);
        }

        public void ChangeRole(int adminId, int userId, Role role)
        {
            ValidateAdminPermission(adminId);

            var user = GetUserAndValidate(userId);

            if (user.Role == role)
            {
                _logger.Warning("User Role with ID: {UserId} already {Role}", userId, role);
                throw new ArgumentException("User Role already satisfied");
            }

            user.Role = role;
            _repository.Update(user);
            _logger.Information("Role changed successfully for user ID: {UserId}", userId);
        }

        public void UpdateSalary(int userId, decimal salary, int approverId)
        {
            ValidateAdminPermission(approverId);

            var user = GetUserAndValidate(userId);
            user.BasicSalary = salary;

            _repository.Update(user);
            _logger.Information("Salary updated successfully for user ID: {UserId}", userId);
        }

        public void UpdateUser(User user)
        {
            var existingUser = GetUserAndValidate(user.Id);

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.Role = user.Role;
            existingUser.RemainingLeaveDays = user.RemainingLeaveDays;

            _repository.Update(existingUser);
            _logger.Information("User details updated successfully for ID: {Id}", user.Id);
        }

        #region Private Helper Methods

        private void ValidateRegistration(string name, string email, string password, decimal basicSalary)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email is required");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is required");

            if (_repository.EmailCheck(email))
            {
                _logger.Warning("Email already registered");
                throw new ArgumentException("Email already registered");
            }

            if (password.Length < 8)
            {
                _logger.Warning("Password too short: {Length} characters", password.Length);
                throw new ArgumentException("Password must be at least 8 characters");
            }

            if (basicSalary < 0)
            {
                _logger.Warning("Basic salary cannot be negative: {Salary}", basicSalary);
                throw new ArgumentException("Basic salary cannot be negative");
            }
        }

        private User GetUserAndValidate(int userId)
        {
            try
            {
                return _repository.GetById(userId);
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning("User with ID: {UserId} was not found", userId);
                throw;
            }
        }

        private void ValidateAdminPermission(int adminId)
        {
            var admin = _repository.GetById(adminId);
            if (!RoleExtensions.CanManageUsers(admin.Role))
            {
                _logger.Warning("Admin System Role Required! {AdminId} not Admin", adminId);
                throw new ArgumentException("Role SysAdmin needed");
            }
        }

        #endregion
    }
}