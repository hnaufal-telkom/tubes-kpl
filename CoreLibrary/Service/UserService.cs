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
            _logger.Information($"Registering User: {name}, Email: {email}, Role: {role}");
            if (_repository.EmailCheck(email))
            {
                _logger.Warning($"Email already registered");
                throw new ArgumentException("Email already registered");
            }

            if (password.Length < 8)
            {
                _logger.Warning($"Password too short: {password.Length} characters");
                throw new ArgumentException("Password must be at least 8 characters");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.Warning("Name, email, and password are required");
                throw new ArgumentException("Name, email, and password are required");
            }

            if (basicSalary < 0)
            {
                _logger.Warning($"Basic salary cannot be negative: {basicSalary}");
                throw new ArgumentException("Basic salary cannot be negative");
            }

            var allUsers = _repository.GetAll().ToList();
            int newId = 0;
            while (allUsers.Any(u => u.Id == newId))
            {
                newId++;
            }

            var user = new User
            {
                Id = newId,
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                JoinDate = DateTime.Now,
                RemainingLeaveDays = 12,
                BasicSalary = basicSalary
            };

            _repository.Add(user);
            _logger.Information($"User {name} registered successfully, {role}");
            return user;
        }

        public User Authenticate(string email, string password)
        {
            try
            {
                var user = _repository.GetByEmail(email);
                if (user.Password == password)
                {
                    _logger.Information($"User {user.Name} authenticated successfully");
                    return user;
                }
                _logger.Warning($"Invalid credentials for email: {email}");
                throw new KeyNotFoundException("Invalid credentials");
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with email {email} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public IEnumerable<User> GetAllUser()
        {
            _logger.Information("Getting all active users");
            return _repository.GetAll();
        }

        public User GetUserById(int userId)
        {
            _logger.Information($"Getting user by ID: {userId}");
            return _repository.GetById(userId);
        }

        public void UpdateUserProfile(int userId, string name, string email)
        {
            try
            {
                _logger.Information($"Updating user profile for ID: {userId}, Name: {name}, Email: {email}");
                var user = _repository.GetById(userId);
                user.Name = name;
                user.Email = email;
                _repository.Update(user);
                _logger.Information($"User details updated successfully for ID: {userId}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with email {userId} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                _logger.Information($"Changing password for user ID: {userId}");
                if (newPassword.Length < 8)
                {
                    _logger.Error($"New password too short: {newPassword.Length} characters");
                    throw new ArgumentException("Password must be at least 8 characters");
                }
                var user = _repository.GetById(userId);
                if (user.Password != currentPassword)
                {
                    _logger.Error($"Current password does not match for user ID: {userId}");
                    throw new ArgumentException("Current password is incorrect");
                }
                user.Password = newPassword;
                _repository.Update(user);
                _logger.Information($"Password changed successfully for user ID: {userId}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with email {userId} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public void DeleteUserAccount(int adminId, int userId)
        {
            try
            {
                _logger.Information($"Deleting account for user ID: {userId}");
                var admin = _repository.GetById(adminId);
                if (RoleExtensions.CanManageUsers(admin.Role) || RoleExtensions.CanManageSystem(admin.Role))
                {
                    _repository.Delete(userId);
                    _logger.Information($"User account deleted successfully for user ID: {userId}");
                }
                else
                {
                    _logger.Warning($"Admin System Role Required! {adminId} not Admin");
                    throw new ArgumentException("Role SysAdmin needed");
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with ID: {userId} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public void ChangeRole(int adminId, int userId, Role role)
        {
            try
            {
                _logger.Information($"Changing Role for user ID: {userId}");
                var adminUser = _repository.GetById(adminId);
                var user = _repository.GetById(userId);
                if (RoleExtensions.CanManageUsers(adminUser.Role) || RoleExtensions.CanManageSystem(adminUser.Role))
                {
                    if (user.Role == role)
                    {
                        _logger.Warning($"User Role with ID : {userId} already {role}");
                        throw new ArgumentException("User Role already satisfied");
                    }
                    else
                    {
                        user.Role = role;
                        _repository.Update(user);
                        _logger.Information($"Role changed successfully for user ID: {userId}");
                    }
                }
                else
                {
                    _logger.Warning($"Admin System Role Required! {adminId} not Admin");
                    throw new ArgumentException("Role SysAdmin needed");
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with ID: {userId} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                _logger.Information($"Updating user details for ID: {user.Id}");
                var existingUser = _repository.GetById(user.Id);
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Password = user.Password;
                existingUser.Role = user.Role;
                existingUser.RemainingLeaveDays = user.RemainingLeaveDays;
                _repository.Update(existingUser);
                _logger.Information($"User details updated successfully for ID: {user.Id}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with ID: {user.Id} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public void UpdateSalary(int userId, decimal salary, int approverId)
        {
            try
            {
                _logger.Information($"Updating salary for user ID: {userId}");
                var approver = _repository.GetById(approverId);
                if (RoleExtensions.CanManagePayroll(approver.Role) || RoleExtensions.CanManageSystem(approver.Role))
                {
                    var user = _repository.GetById(userId);
                    user.BasicSalary = salary;
                    _repository.Update(user);
                    _logger.Information($"Salary updated successfully for user ID: {userId}");
                }
                else
                {
                    _logger.Warning($"Admin System Role Required! {approverId} not Admin");
                    throw new ArgumentException("Role SysAdmin needed");
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with ID: {userId} was not found.");
                throw new KeyNotFoundException("User not found");
            }
        }
    }
}