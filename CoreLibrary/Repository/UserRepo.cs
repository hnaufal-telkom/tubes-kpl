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

        public InMemoryUserRepository(ILogger logger)
        {
            _logger = logger.ForContext<InMemoryUserRepository>();
        }

        public IEnumerable<User> GetAll()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving all users");
                return new List<User>(_users);
            }
        }

        public User GetById(int id)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving user by ID: {UserId}", id);
                return FindUser(u => u.Id == id) ?? throw UserNotFound(id);
            }
        }

        public User GetByEmail(string email)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving user by email: {Email}", email);
                return FindUser(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    ?? throw UserNotFound(email);
            }
        }

        public void Add(User user)
        {
            lock (_lock)
            {
                _logger.Information("Adding new user: {UserName}", user.Name);

                if (EmailExists(user.Email))
                {
                    _logger.Error("Email {Email} already exists", user.Email);
                    throw new InvalidOperationException("Email already exists");
                }

                user.Id = GenerateId();
                _users.Add(user);
                _logger.Information("User {UserName} added successfully with ID: {UserId}", user.Name, user.Id);
            }
        }

        public void Update(User user)
        {
            lock (_lock)
            {
                _logger.Information("Updating user ID: {UserId}", user.Id);

                var index = _users.FindIndex(u => u.Id == user.Id);
                if (index < 0)
                {
                    _logger.Warning("User ID {UserId} not found for update", user.Id);
                    throw UserNotFound(user.Id);
                }

                _users[index] = user;
                _logger.Information("User ID {UserId} updated successfully", user.Id);
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information("Deleting user ID: {UserId}", id);

                var count = _users.RemoveAll(u => u.Id == id);
                if (count == 0)
                {
                    _logger.Warning("User ID {UserId} not found for deletion", id);
                    throw UserNotFound(id);
                }

                _logger.Information("User ID {UserId} deleted successfully", id);
            }
        }

        public bool EmailCheck(string email)
        {
            lock (_lock)
            {
                _logger.Debug("Checking email existence: {Email}", email);
                return EmailExists(email);
            }
        }

        public int GenerateId()
        {
            lock (_lock)
            {
                _logger.Debug("Generating new user ID");
                return _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;
            }
        }

        #region Private Helper Methods

        private User? FindUser(Func<User, bool> predicate)
        {
            return _users.FirstOrDefault(predicate);
        }

        private bool EmailExists(string email)
        {
            return _users.Exists(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private KeyNotFoundException UserNotFound(int id)
        {
            return new KeyNotFoundException($"User with ID {id} not found");
        }

        private KeyNotFoundException UserNotFound(string email)
        {
            return new KeyNotFoundException($"User with email {email} not found");
        }

        #endregion
    }
}