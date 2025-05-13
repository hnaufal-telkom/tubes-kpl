using MainLibrary;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers([FromServices] IUserService userService)
        {
            return Ok(userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id, [FromServices] IUserService userService)
        {
            var user = userService.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id, [FromServices] IUserService userService)
        {
            var user = userService.GetUserById(id);
            if (user == null) return NotFound();

            userService.DeleteUser(id);
            return NoContent();
        }
    }
}