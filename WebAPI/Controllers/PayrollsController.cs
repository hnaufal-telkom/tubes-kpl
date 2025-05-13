using MainLibrary;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollsController : ControllerBase
    {
        private readonly PayrollService _payrollService;

        public PayrollsController(PayrollService payrollService)
        {
            _payrollService = payrollService;
        }


        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<Payroll>> GetByUserId(string userId)
        {
            try
            {
                return Ok(_payrollService.GetByUserId(userId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<Payroll> Create([FromBody] Payroll payroll)
        {
            try
            {
                var createdPayroll = _payrollService.GeneratePayroll(
                    payroll.UserId,
                    payroll.PeriodStart,
                    payroll.PeriodEnd);

                return CreatedAtAction(nameof(GetByUserId), new { userId = createdPayroll.UserId }, createdPayroll);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/pay")]
        public IActionResult MarkAsPaid(string id)
        {
            try
            {
                _payrollService.MarkAsPaid(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
