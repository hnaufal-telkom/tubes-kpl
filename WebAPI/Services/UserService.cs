using WebAPI.Models;

namespace WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new();

        public List<User> GetAllUsers() => _users;
        
        public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);
        
        public User AddUser(User user)
        {
            user.Id = _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;
            _users.Add(user);
            return user;
        }

        public User? UpdateUser(int id, User user)
        {
            var existingUser = GetUserById(id);
            if (existingUser == null) return null;

            existingUser.Name = user.Name;
            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.Department = user.Department;
            existingUser.Role = user.Role;
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            return existingUser;
        }

        public bool DeleteUser(int id)
        {
            var user = GetUserById(id);
            if (user == null) return false;
            return _users.Remove(user);
        }
    }
}
