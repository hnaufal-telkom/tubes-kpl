using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Service
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IBusinessTripRepository _businessTripRepository;
        private readonly ILogger _logger;

        public PayrollService(
            IPayrollRepository payrollRepository,
            IUserRepository userRepository,
            ILeaveRequestRepository leaveRequestRepository,
            IBusinessTripRepository businessTripRepository,
            ILogger logger)
        {
            _payrollRepository = payrollRepository;
            _userRepository = userRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _businessTripRepository = businessTripRepository;
            _logger = logger;
        }

        public Payroll GeneratePayroll(int userId, DateTime periodStart, DateTime periodEnd)
        {
            _logger.Information("Generating payroll for user {UserId} from {PeriodStart} to {PeriodEnd}", userId, periodStart, periodEnd);
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                _logger.Error("User with ID {UserId} not found", userId);
                throw new ArgumentException($"User with ID {userId} not found");
            }
            var leaveDays = _leaveRequestRepository.GetByUserId(userId)
                .Where(l => l.StartDate >= periodStart && l.EndDate <= periodEnd)
                .Sum(l => (l.EndDate - l.StartDate).TotalDays + 1);
            var businessTripDays = _businessTripRepository.GetByUserId(userId)
                .Where(b => b.StartDate >= periodStart && b.EndDate <= periodEnd)
                .Sum(b => (b.EndDate - b.StartDate).TotalDays + 1);

            var allUsers = _payrollRepository.GetAll().ToList();
            int newId = 0;
            while (allUsers.Any(u => u.Id == newId))
            {
                newId++;
            }

            var payroll = new Payroll
            {
                Id = newId,
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BasicSalary = user.BasicSalary,
                PaymentDate = DateTime.Now.AddDays(7),
                IsPaid = false
            };

            if (leaveDays > 0)
            {
                payroll.BasicSalary -= (decimal)leaveDays * (user.BasicSalary / 30);
                _logger.Information("Leave days deducted: {LeaveDays}", leaveDays);
            }

            _payrollRepository.Add(payroll);
            _logger.Information("Payroll generated successfully for user {UserId}", userId);
            return payroll;
        }

        public void MarkAsPaid(int payrollId, int approverId)
        {
            var user = _userRepository.GetById(approverId);
            if (RoleExtensions.CanManagePayroll(user.Role) || RoleExtensions.CanManageSystem(user.Role))
            {
                _logger.Error("Approver with ID {ApproverId} not found", approverId);
                throw new ArgumentException($"Approver with ID {approverId} not found");
            }
            _logger.Information("Marking payroll {PayrollId} as paid", payrollId);
            var payroll = _payrollRepository.GetById(payrollId);
            if (payroll == null)
            {
                _logger.Error("Payroll with ID {PayrollId} not found", payrollId);
                throw new ArgumentException($"Payroll with ID {payrollId} not found");
            }
            payroll.IsPaid = true;
            payroll.PaymentDate = DateTime.Now;
            _payrollRepository.Update(payroll);
            _logger.Information("Payroll {PayrollId} marked as paid successfully", payrollId);
        }

        public IEnumerable<Payroll> GetPayrollsByUserId(int userId)
        {
            _logger.Information("Getting payrolls for user {UserId}", userId);
            var payrolls = _payrollRepository.GetAll().Where(p => p.UserId == userId).ToList();
            if (!payrolls.Any())
            {
                _logger.Warning("No payrolls found for user {UserId}", userId);
            }
            return payrolls;
        }

        public IEnumerable<Payroll> GetPayrollsByPeriod(DateTime start, DateTime end)
        {
            _logger.Information("Getting payrolls between {Start} and {End}", start, end);
            var payrolls = _payrollRepository.GetByPeriod(start, end).ToList();
            if (!payrolls.Any())
            {
                _logger.Warning("No payrolls found between {Start} and {End}", start, end);
            }
            return payrolls;
        }
    }
}
