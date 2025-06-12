using CoreLibrary;
using CoreLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly PayrollService _payrollService;
        private readonly UserService _userService;
        private readonly Serilog.ILogger _logger;
        public PayrollController(PayrollService payrollService, UserService userService, Serilog.ILogger logger)
        {
            _payrollService = payrollService;
            _userService = userService;
            _logger = logger.ForContext<PayrollController>();
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
                _logger.Information("Dummy users created successfully");
                return Ok("Dummy users created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating dummy users");
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetPayrollsByUserId/{userId}")]
        public IActionResult GetPayrollsByUserId(int userId)
        {
            try
            {
                var payrolls = _payrollService.GetPayrollsByUserId(userId);
                _logger.Information("Payrolls retrieved successfully for user {UserId}", userId);
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving payrolls for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetPayrollByPeriod")]
        public IActionResult GetPayrollByPeriod([FromBody] PayrollDTO payrollDTO)
        {
            try
            {
                var payrolls = _payrollService.GetPayrollsByPeriod(payrollDTO.PeriodStart, payrollDTO.PeriodEnd);
                _logger.Information("Payrolls retrieved successfully for period {PeriodStart} to {PeriodEnd}",
                    payrollDTO.PeriodStart.ToString("yyyy-MM-dd"), payrollDTO.PeriodEnd.ToString("yyyy-MM-dd"));
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving payrolls for period {PeriodStart} to {PeriodEnd}",
                    payrollDTO.PeriodStart.ToString("yyyy-MM-dd"), payrollDTO.PeriodEnd.ToString("yyyy-MM-dd"));
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GeneratePayroll/{userId}")]
        public IActionResult GeneratePayroll(int userId, [FromBody] PayrollDTO payrollDTO)
        {
            try
            {
                var payroll = _payrollService.GeneratePayroll(
                    userId: userId,
                    periodStart: payrollDTO.PeriodStart,
                    periodEnd: payrollDTO.PeriodEnd
                );
                _logger.Information("Payroll generated successfully for user {UserId}", userId);
                return Ok(payroll);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating payroll for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("MarkAsPaid/{payrollId}&{approverId}")]
        public IActionResult MarkAsPaid(int payrollId, int approverId)
        {
            try
            {
                _payrollService.MarkAsPaid(payrollId, approverId);
                _logger.Information("Payroll {PayrollId} marked as paid by approver {ApproverId}", payrollId, approverId);
                return Ok("Payroll marked as paid successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking payroll {PayrollId} as paid by approver {ApproverId}", payrollId, approverId);
                return BadRequest(ex.Message);
            }
        }
    }
}
