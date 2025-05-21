using CoreLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using CoreLibrary;
using WebAPI.DTO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly Serilog.ILogger _logger;

        public UsersController(UserService userService,Serilog.ILogger logger)
        {
            _userService = userService;
            _logger = logger.ForContext<UsersController>();
        }

        [HttpPost("CreateDummy")]
        public IActionResult CreateDummy()
        {
            try
            {
                var names = new[] { "Fizryan", "Haidar", "Naufal", "Fathir" };
                var emails = new[] {
                "fizryan@mail.com", "haidar@mail.com", "naufal@mail.com", "fathir@mail.com"
                };
                var passwords = new[] {
                    "password123", "MonsterAndSouls1", "superNova865", "dasd11121"
                };
                var salaries = new[] {
                    50000000, 45000000, 40000000, 50000000
                };
                var roles = new[] {
                    Role.SysAdmin, Role.Employee, Role.HRD, Role.Supervisor
                };

                for (int i = 0; i < names.Length; i++)
                {
                    _userService.Register(
                        name: names[i],
                        email: emails[i],
                        password: passwords[i],
                        role: roles[i],
                        basicSalary: salaries[i]
                    );
                }
                return Ok("Dummy users created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating dummy users");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllUser")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userService.GetAllUser();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving all users");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserById/{userId}")]
        public IActionResult GetUserById(int userId)
        {
            try
            {
                var user = _userService.GetUserById(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving user by ID");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserDTO user)
        {
            try
            {
                var registeredUser = _userService.Register(user.Name, user.Email, user.Password, user.Role, user.BasicSalary);
                var users = _userService.GetUserById(registeredUser.Id);
                return CreatedAtAction("Register", new { users.Id }, users);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering user");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] AuthRequest user)
        {
            try
            {
                var authenticatedUser = _userService.Authenticate(user.Email, user.Password);
                return Ok(authenticatedUser);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error authenticating user");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] ChangePasswordUser user)
        {
            try
            {
                var users = _userService.GetAllUser().FirstOrDefault(u => u.Email == user.Email);
                _userService.ChangePassword(users.Id, user.OldPassword, user.NewPassword);
                return Ok("Password updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating password");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ChangeUserProfile/{userId}")]
        public IActionResult UpdateUser(int userId, [FromBody] UserProfile user)
        {
            try
            {
                _userService.UpdateUserProfile(userId, user.Name, user.Email);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating user");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ChangeUserRole/{adminId}&{userId}&{role}")]
        public IActionResult UpdateUserRole(int adminId, int userId, int role)
        {
            try
            {
                _userService.ChangeRole(adminId, userId, (Role)role);
                return Ok("User role updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating user role");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ChangeUserSalary/{adminId}&{userId}&{salary}")]
        public IActionResult UpdateUserSalary(int adminId, int userId, decimal salary)
        {
            try
            {
                _userService.UpdateSalary(userId, salary, adminId);
                return Ok("User salary updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating user salary");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteUser/{adminId}&{userId}")]
        public IActionResult DeleteUser(int adminId, int userId)
        {
            try
            {
                _userService.DeleteUserAccount(adminId, userId);
                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting user");
                return BadRequest(ex.Message);
            }
        }
    }
}
