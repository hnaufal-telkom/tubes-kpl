using MainLibrary;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new()
        {
            new User {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                Password = "password",
                Role = Role.Employee,
                JoinDate = DateTime.Now,
                IsActive = true
            }
        };

        public User GetUserById(string id) => _users.FirstOrDefault(u => u.Id == id);

        public IEnumerable<User> GetAllUsers() => _users;

        public void AddUser(User user) => _users.Add(user);

        public void UpdateUser(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index != -1) _users[index] = user;
        }

        public void DeleteUser(string id) => _users.RemoveAll(u => u.Id == id);

        public bool Exists(string email) => _users.Any(u => u.Email == email);
    }
}