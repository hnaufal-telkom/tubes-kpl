using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/cuti/requests")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestController(ILeaveRequestService service) => _service = service;

        [HttpGet]
        public ActionResult<List<LeaveRequest>> GetAll() => Ok(_service.GetAllLeaveRequests());

        [HttpGet("{id}")]
        public ActionResult<LeaveRequest> GetById(int id)
        {
            var leaveRequest = _service.GetLeaveRequestById(id);
            if (leaveRequest == null) return NotFound();
            return Ok(leaveRequest);
        }

        [HttpPost]
        public ActionResult<LeaveRequest> Create(LeaveRequest leaveRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdRequest = _service.Create(leaveRequest);
            return CreatedAtAction(nameof(GetById), new { id = createdRequest.Id }, createdRequest);
        }

        [HttpPut("{id}")]
        public ActionResult<LeaveRequest> Update(int id, LeaveRequest leaveRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != leaveRequest.Id) return BadRequest("ID mismatch");

            var updatedRequest = _service.Update(id, leaveRequest);
            if (updatedRequest == null) return NotFound();

            return Ok(updatedRequest);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (!_service.Delete(id)) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/approve")]
        public ActionResult<LeaveRequest> Approve(int id, [FromBody] string approverNotes)
        {
            var approvedRequest = _service.Approve(id, approverNotes);
            if (approvedRequest == null) return NotFound();
            return Ok(approvedRequest);
        }

        [HttpPost("{id}/reject")]
        public ActionResult<LeaveRequest> Reject(int id, [FromBody] string approverNotes)
        {
            var rejectedRequest = _service.Reject(id, approverNotes);
            if (rejectedRequest == null) return NotFound();
            return Ok(rejectedRequest);
        }
    }
}
