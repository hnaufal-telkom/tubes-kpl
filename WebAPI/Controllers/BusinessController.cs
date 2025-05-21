using CoreLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly BusinessTripService _businessTripService;
        private readonly UserService _userService;
        private readonly Serilog.ILogger _logger;

        public BusinessController(BusinessTripService businessTripService, UserService userService, Serilog.ILogger logger)
        {
            _businessTripService = businessTripService;
            _userService = userService;
            _logger = logger.ForContext<BusinessController>();
        }

        [HttpGet("GetAllTrip")]
        public IActionResult GetAllTrip()
        {
            try
            {
                var trips = _businessTripService.GetAllTrips();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching business trips");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetTripPending")]
        public IActionResult GetTripPending()
        {
            try
            {
                var trips = _businessTripService.GetPendingRequests();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching pending business trips");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetTripById(int id)
        {
            try
            {
                var trip = _businessTripService.GetTripById(id);
                if (trip == null)
                {
                    return NotFound("Business trip not found");
                }
                return Ok(trip);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching business trip by ID");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByUserId/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            try
            {
                var trips = _businessTripService.GetByUserId(userId);
                return Ok(trips);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Warning(ex, "No business trips found for user ID: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching business trips by user ID");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SubmitRequest")]
        public IActionResult SubmitRequest([FromBody] BusinessTripDTO tripDto)
        {
            try
            {
                var user = _userService.GetUserById(tripDto.UserId);
                var trip = _businessTripService.SubmitRequest(tripDto.UserId, tripDto.Destination, tripDto.StartDate, tripDto.EndDate, tripDto.Purpose, tripDto.EstimateCost);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error submitting business trip request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ApproveRequest/{tripId}&{approverId}")]
        public IActionResult ApproveRequest(int tripId, int approverId)
        {
            try
            {
                _businessTripService.ApproveRequest(tripId, approverId);
                return Ok("Business trip request approved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error approving business trip request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RejectRequest/{tripId}&{approverId}")]
        public IActionResult RejectRequest(int tripId, int approverId, [FromBody] string reason)
        {
            try
            {
                _businessTripService.RejectRequest(tripId, approverId, reason);
                return Ok("Business trip request rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rejecting business trip request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateActualCost/{tripId}&{actualCost}")]
        public IActionResult UpdateActualCost(int tripId, decimal actualCost)
        {
            try
            {
                _businessTripService.UpdateActualCost(tripId, actualCost);
                return Ok("Actual cost updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating actual cost");
                return BadRequest(ex.Message);
            }
        }
    }
}
