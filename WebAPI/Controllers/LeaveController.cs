using CoreLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveService _LeaveService;
        private readonly UserService _userService;
        private readonly Serilog.ILogger _logger;

        public LeaveController(LeaveService leaveService, UserService userService, Serilog.ILogger logger)
        {
            _LeaveService = leaveService;
            _userService = userService;
            _logger = logger.ForContext<UsersController>();
        }

        [HttpGet("GetAllLeaveRequest")]
        public IActionResult GetAllLeaveRequest()
        {
            try
            {
                var leaveRequests = _LeaveService.GetAllRequests();
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching leave requests");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetLeaveRequestPending")]
        public IActionResult GetLeaveRequestPending()
        {
            try
            {
                var leaveRequests = _LeaveService.GetPendingRequest();
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching pending leave requests");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetLeaveRequestFailed(int id)
        {
            try
            {
                var leaveRequest = _LeaveService.GetById(id);
                if (leaveRequest == null)
                {
                    return NotFound("Leave request not found");
                }
                return Ok(leaveRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching leave request by ID");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetLeaveRequestByUserId/{userId}")]
        public IActionResult GetLeaveRequestByUserId(int userId)
        {
            try
            {
                var leaveRequests = _LeaveService.GetByUserId(userId);
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching leave requests by user ID");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SubmitRequest")]
        public IActionResult SubmitRequest([FromBody] LeaveDTO leaveRequestDto)
        {
            try
            {
                var user = _userService.GetUserById(leaveRequestDto.UserId);
                var leaveRequest = _LeaveService.SubmitRequest(
                    userId: leaveRequestDto.UserId,
                    startDate: leaveRequestDto.StartDate,
                    endDate: leaveRequestDto.EndDate,
                    description: leaveRequestDto.Description
                );
                return Ok(leaveRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error submitting leave request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ApproveRequest/{requestId}&{approverId}")]
        public IActionResult ApproveRequest(int requestId, int approverId)
        {
            try
            {
                _LeaveService.ApproveRequest(requestId, approverId);
                return Ok("Leave request approved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error approving leave request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RejectRequest/{requestId}&{approverId}")]
        public IActionResult RejectRequest(int requestId, int approverId, [FromBody] string reason)
        {
            try
            {
                _LeaveService.RejectRequest(requestId, approverId, reason);
                return Ok("Leave request rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rejecting leave request");
                return BadRequest(ex.Message);
            }
        }
    }
}
