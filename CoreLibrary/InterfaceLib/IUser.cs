using CoreLibrary.ModelLib;

namespace CoreLibrary.InterfaceLib
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
        User GetByEmail(string email);
        void Add(User user);
        void Update(User user);
        void Delete(int id);
        bool EmailCheck(string email);
        int GenerateId();
    }

    public interface IUserService
    {
        User Register(string name, string email, string password, Role role, decimal basicSalary);
        User Authenticate(string email, string password);
        IEnumerable<User> GetAllUser();
        User GetUserById(int id);
        void UpdateUserProfile(int userId, string username, string email);
        void ChangePassword(int userId, string currentPassword, string newPassword);
        void DeleteUserAccount(int adminId, int userId);
        void ChangeRole(int adminId, int userId, Role role);
        void UpdateUser(User user);
        void UpdateSalary(int userId, decimal salary, int approverId);
    }
}
