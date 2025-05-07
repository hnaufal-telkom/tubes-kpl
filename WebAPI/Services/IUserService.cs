using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        User? GetUserById(int id);
        User AddUser(User user);
        User? UpdateUser(int id, User user);
        bool DeleteUser(int id);
    }
}
