using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<List<User>> GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public ActionResult<User> AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (UserRole.validRoles().Contains(user.Role.ToLower()) == false)
            {
                ModelState.AddModelError("Role", "Invalid role. Valid roles are: " + string.Join(", ", UserRole.validRoles()));
                return BadRequest(ModelState);
            }
            var newUser = new User
            {
                Name = user.Name,
                Username = user.Username,
                Password = user.Password,
                Department = user.Department,
                Role = user.Role.ToLower()
            };

            var addUser = _userService.AddUser(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = addUser.Id }, addUser);
        }

        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser(int id, [FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var existingUser = _userService.GetUserById(id);
            if (existingUser == null) return NotFound();
            if (UserRole.validRoles().Contains(user.Role.ToLower()) == false)
            {
                ModelState.AddModelError("Role", "Invalid role. Valid roles are: " + string.Join(", ", UserRole.validRoles()));
                return BadRequest(ModelState);
            }

            existingUser.Name = user.Name;
            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.Department = user.Department;
            existingUser.Role = user.Role.ToLower();

            var UpdateUser = _userService.UpdateUser(id, existingUser);
            return Ok(UpdateUser);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            if (!_userService.DeleteUser(id)) return NotFound();
            return NoContent();
        }
    }
}
