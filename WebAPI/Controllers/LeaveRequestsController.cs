using MainLibrary;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly LeaveService _leaveService;

        public LeaveRequestsController(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<LeaveRequest>> GetPendingRequests()
        {
            return Ok(_leaveService.GetPendingRequests());
        }

        [HttpGet("{id}")]
        public ActionResult<LeaveRequest> GetById(string id)
        {
            try
            {
                var request = _leaveService.GetPendingRequests().FirstOrDefault(lr => lr.Id == id);
                if (request == null) return NotFound();
                return Ok(request);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<LeaveRequest> Create([FromBody] LeaveRequest request)
        {
            try
            {
                var createdRequest = _leaveService.SubmitRequest(
                    request.UserId,
                    request.StartDate,
                    request.EndDate,
                    request.Description);

                return CreatedAtAction(nameof(GetById), new { id = createdRequest.Id }, createdRequest);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/approve")]
        public IActionResult Approve(string id, [FromQuery] string approverId)
        {
            try
            {
                _leaveService.ApproveRequest(id, approverId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/reject")]
        public IActionResult Reject(string id, [FromQuery] string approverId, [FromQuery] string reason)
        {
            try
            {
                _leaveService.RejectRequest(id, approverId, reason);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
