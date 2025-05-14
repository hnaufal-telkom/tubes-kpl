using MainLibrary;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _usersService;

        public UsersController(IUserService userService)
        {
            _usersService = userService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            var repo = (InMemoryUserRepository)_usersService.GetType()
                .GetField("_repository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_usersService);
            return Ok(repo.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetById(string id)
        {
            try
            {
                return Ok(_usersService.GetUserById(id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<User> Create([FromBody] User user)
        {
            try
            {
                var createdUser = _usersService.Register(user.Name, user.Email, user.Password, user.Role);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] User user)
        {
            if (id != user.Id) return BadRequest("ID mismatch");

            try
            {
                _usersService.UpdateUser(user);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _usersService.DeactivateUser(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
