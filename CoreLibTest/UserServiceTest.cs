using Serilog;
using CoreLibrary.Service;
using CoreLibrary.Repository;
using CoreLibrary.ModelLib;
using FluentAssertions;

namespace CoreLibrary.Tests.ServiceTest
{
    public class UserServiceTests
    {
        private readonly UserService _userService;
        private readonly User _adminUser;
        private readonly User _employeeUser;
        private readonly ILogger _logger;

        public UserServiceTests()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var repository = new InMemoryUserRepository(_logger);
            _userService = new UserService(repository, _logger);

            _adminUser = _userService.Register(
                name: "Fizryan",
                email: "fizryan@mail.com",
                password: "password123",
                role: Role.SysAdmin,
                basicSalary: 50000000
            );

            _employeeUser = _userService.Register(
                name: "Naufal",
                email: "naufal@mail.com",
                password: "pass0000122231",
                role: Role.Employee,
                basicSalary: 2000000
            );
        }

        #region Success Tests

        [Fact]
        public void RegisterUser_WithValidData_ReturnsUser()
        {
            var result = _userService.Register(
                name: "Haidar",
                email: "haidar@mail.com",
                password: "password321",
                role: Role.Employee,
                basicSalary: 5000000
            );

            result.Should().NotBeNull();
            result.Name.Should().Be("Haidar");
            result.Email.Should().Be("haidar@mail.com");
        }

        [Fact]
        public void Authenticate_WithValidCredentials_ReturnsUser()
        {
            var auth = _userService.Authenticate(
                email: _employeeUser.Email,
                password: _employeeUser.Password
            );

            auth.Should().NotBeNull();
            auth.Email.Should().Be(_employeeUser.Email);
            auth.Password.Should().Be(_employeeUser.Password);
        }

        [Fact]
        public void GetAllUser_WhenCalled_ReturnsAllUsers()
        {
            var users = _userService.GetAllUser();

            users.Should().NotBeNullOrEmpty()
                .And.Contain(u => u.Email == _adminUser.Email)
                .And.Contain(u => u.Email == _employeeUser.Email);
        }

        [Fact]
        public void GetUserById_WithValidId_ReturnsUser()
        {
            var user = _userService.GetUserById(_adminUser.Id);

            user.Should().NotBeNull();
            user.Id.Should().Be(_adminUser.Id);
            user.Name.Should().Be(_adminUser.Name);
        }

        [Fact]
        public void UpdateUserProfile_WithValidData_UpdatesUser()
        {
            _userService.UpdateUserProfile(
                userId: _employeeUser.Id,
                name: "Naufal Updated",
                email: _employeeUser.Email
            );

            var updatedUser = _userService.GetUserById(_employeeUser.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.Name.Should().Be("Naufal Updated");
        }

        [Fact]
        public void ChangePassword_WithValidCurrentPassword_UpdatesPassword()
        {
            _userService.ChangePassword(
                userId: _employeeUser.Id,
                currentPassword: _employeeUser.Password,
                newPassword: "newpassword123"
            );

            var updatedUser = _userService.GetUserById(_employeeUser.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.Password.Should().Be("newpassword123");
        }

        [Fact]
        public void DeleteUser_WithValidId_DeletesUser()
        {
            _userService.DeleteUserAccount(_adminUser.Id, _employeeUser.Id);
            _userService.Invoking(s => s.GetUserById(_employeeUser.Id))
                .Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void ChangeRole_WithValidData_UpdatesRole()
        {
            _userService.ChangeRole(
                adminId: _adminUser.Id,
                userId: _employeeUser.Id,
                role: Role.Supervisor
            );

            var updatedUser = _userService.GetUserById(_employeeUser.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.Role.Should().Be(Role.Supervisor);
        }

        [Fact]
        public void UpdateSalary_WithValidData_UpdatesSalary()
        {
            _userService.UpdateSalary(
                userId: _employeeUser.Id,
                salary: 3000000,
                approverId: _adminUser.Id
            );

            var updatedUser = _userService.GetUserById(_employeeUser.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.BasicSalary.Should().Be(3000000);
        }

        #endregion

        #region Failure Tests

        [Theory]
        [InlineData("", "email@test.com", "password123", Role.Employee, 5000000)]
        [InlineData("Name", "email@test.com", "short", Role.Employee, 5000000)]
        [InlineData("Name", "email@test.com", "password123", Role.Employee, -1000)]
        public void RegisterUser_WithInvalidData_ThrowsException(
            string name, string email, string password, Role role, decimal basicSalary)
        {
            _userService.Invoking(s => s.Register(
                    name: name,
                    email: email,
                    password: password,
                    role: role,
                    basicSalary: basicSalary
                ))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Authenticate_WithInvalidPassword_ThrowsException()
        {
            _userService.Invoking(s => s.Authenticate(
                    email: _adminUser.Email,
                    password: "wrongpassword"
                ))
                .Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetUserById_WithInvalidId_ThrowsException()
        {
            _userService.Invoking(s => s.GetUserById(9999))
                .Should().Throw<KeyNotFoundException>();
        }

        [Theory]
        [InlineData("", "valid@email.com")]
        public void UpdateUserProfile_WithInvalidData_ThrowsException(string name, string email)
        {
            _userService.Invoking(s => s.UpdateUserProfile(
                    userId: _employeeUser.Id,
                    name: name,
                    email: email
                ))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ChangePassword_WithInvalidCurrentPassword_ThrowsException()
        {
            _userService.Invoking(s => s.ChangePassword(
                    userId: _employeeUser.Id,
                    currentPassword: "wrongpassword",
                    newPassword: "newpassword123"
                ))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DeleteUser_WithInvalidId_ThrowsException()
        {
            _userService.Invoking(s => s.DeleteUserAccount(_adminUser.Id, 9999))
                .Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void ChangeRole_WithNonAdminUser_ThrowsException()
        {
            _userService.Invoking(s => s.ChangeRole(
                    adminId: _employeeUser.Id,
                    userId: _adminUser.Id,
                    role: Role.SysAdmin
                ))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateSalary_WithInvalidApprover_ThrowsException()
        {
            _userService.Invoking(s => s.UpdateSalary(
                    userId: _employeeUser.Id,
                    salary: 3000000,
                    approverId: 9999
                ))
                .Should().Throw<KeyNotFoundException>();
        }

        #endregion
    }
}