using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Repository
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly object _lock = new();
        private readonly ILogger _logger;

        public IEnumerable<User> GetAll()
        {
            lock (_lock)
            {
                _logger.Information("Getting all users");
                return _users.ToList();
            }
        }

        public User GetById(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Getting user by ID: {id}");
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    _logger.Information($"User with ID {id} not found");
                    throw new KeyNotFoundException("User not found");
                }
                return user;
            }
        }

        public User GetByEmail(string email)
        {
            lock (_lock)
            {
                Log.Information($"Getting user by email: {email}");
                var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    Log.Warning($"User with email {email} not found");
                    throw new KeyNotFoundException("User not found");
                }
                return user;
            }
        }

        public void Add(User user)
        {
            lock (_lock)
            {
                _logger.Information($"Adding user: {user.Name}");
                if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) {
                    _logger.Error($"Email {user.Email} already exists");
                    throw new InvalidOperationException("Email already exists");
                }

                _users.Add(user);
                _logger.Information($"User {user.Name} added successfully");
            }
        }

        public void Update(User user)
        {
            lock (_lock)
            {
                _logger.Information($"Updating user: {user.Id}");
                var index = _users.FindIndex(u => u.Id == user.Id);
                if (index >= 0)
                {
                    _users[index] = user;
                    _logger.Information($"User {user.Name} updated successfully");
                }
                else
                {
                    _logger.Warning($"User with ID {user.Id} not found for update");
                    throw new KeyNotFoundException("User not found");
                }
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Deleting user with ID: {id}");
                var count = _users.RemoveAll(u => u.Id == id);
                if (count !> 0)
                {
                    _logger.Warning($"User with ID {id} not found for deletion");
                    throw new KeyNotFoundException("User not found");
                }
                else
                {
                    _logger.Information($"User with ID {id} deleted successfully");
                }
            }
        }

        public bool EmailCheck(string email)
        {
            lock (_lock)
            {
                _logger.Debug($"Checking if email exists: {email}");
                return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
