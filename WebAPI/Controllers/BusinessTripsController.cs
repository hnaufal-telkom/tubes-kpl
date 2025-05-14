using MainLibrary;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessTripsController : ControllerBase
    {
        private readonly BusinessTripService _tripService;

        public BusinessTripsController(BusinessTripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BusinessTrip>> GetPendingRequests()
        {
            return Ok(_tripService.GetPendingRequests());
        }

        [HttpGet("{id}")]
        public ActionResult<BusinessTrip> GetById(string id)
        {
            try
            {
                var trip = _tripService.GetPendingRequests().FirstOrDefault(bt => bt.Id == id);
                if (trip == null) return NotFound();
                return Ok(trip);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<BusinessTrip> Create([FromBody] BusinessTrip trip)
        {
            try
            {
                var createdTrip = _tripService.SubmitRequest(
                    trip.UserId,
                    trip.Destination,
                    trip.StartDate,
                    trip.EndDate,
                    trip.Purpose,
                    trip.EstimatedCost);

                return CreatedAtAction(nameof(GetById), new { id = createdTrip.Id }, createdTrip);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/approve")]
        public IActionResult Approve(string id, [FromQuery] string approverId)
        {
            try
            {
                _tripService.ApproveRequest(id, approverId);
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
                _tripService.RejectRequest(id, approverId, reason);
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
