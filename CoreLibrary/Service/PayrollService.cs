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
            _logger = logger.ForContext<PayrollService>();
        }

        public Payroll GeneratePayroll(int userId, DateTime periodStart, DateTime periodEnd)
        {
            _logger.Information("Generating payroll for user {UserId} from {PeriodStart} to {PeriodEnd}",
                userId, periodStart.ToString("yyyy-MM-dd"), periodEnd.ToString("yyyy-MM-dd"));

            var user = GetUser(userId);
            var leaveDays = CalculateLeaveDays(userId, periodStart, periodEnd);
            var payroll = CreatePayroll(user, periodStart, periodEnd, leaveDays);

            _payrollRepository.Add(payroll);
            _logger.Information("Payroll generated successfully for user {UserId}", userId);

            return payroll;
        }

        public void MarkAsPaid(int payrollId, int approverId)
        {
            _logger.Information("Marking payroll {PayrollId} as paid by approver {ApproverId}", payrollId, approverId);

            ValidateApprover(approverId);
            var payroll = GetPayroll(payrollId);

            UpdatePayrollAsPaid(payroll);

            _logger.Information("Payroll {PayrollId} marked as paid successfully", payrollId);
        }

        public IEnumerable<Payroll> GetPayrollsByUserId(int userId)
        {
            _logger.Information("Getting payrolls for user {UserId}", userId);
            var payrolls = _payrollRepository.GetAll().Where(p => p.UserId == userId).ToList();
            if (payrolls == null || !payrolls.Any())
            {
                _logger.Warning("No payrolls found for user {UserId}", userId);
                throw new KeyNotFoundException($"No payrolls found for user with ID {userId}");
            }

            if (!payrolls.Any())
            {
                _logger.Warning("No payrolls found for user {UserId}", userId);
            }

            return payrolls;
        }

        public IEnumerable<Payroll> GetPayrollsByPeriod(DateTime start, DateTime end)
        {
            if (start > end)
            {
                _logger.Error("Start date {Start} is after end date {End}", start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
                throw new ArgumentException("Start date must be before end date");
            }

            _logger.Information("Getting payrolls between {Start} and {End}",
                start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

            var payrolls = _payrollRepository.GetByPeriod(start, end).ToList();

            if (!payrolls.Any())
            {
                _logger.Warning("No payrolls found between {Start} and {End}",
                    start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
            }

            return payrolls;
        }

        #region Private Helper Methods

        private User GetUser(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                _logger.Error("User with ID {UserId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            return user;
        }

        private int CalculateLeaveDays(int userId, DateTime periodStart, DateTime periodEnd)
        {
            var leaveDays = _leaveRequestRepository.GetByUserId(userId)
                .Where(l => l.StartDate >= periodStart && l.EndDate <= periodEnd && l.Status == RequestStatus.Approved)
                .Sum(l => (l.EndDate - l.StartDate).Days + 1);

            if (leaveDays > 0)
            {
                _logger.Information("Leave days calculated: {LeaveDays}", leaveDays);
            }

            return leaveDays;
        }

        private Payroll CreatePayroll(User user, DateTime periodStart, DateTime periodEnd, int leaveDays)
        {
            var dailySalary = user.BasicSalary / 30;
            var salaryDeduction = leaveDays > 0 ? dailySalary * leaveDays : 0;

            return new Payroll
            {
                Id = _payrollRepository.GenerateId(),
                UserId = user.Id,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BasicSalary = user.BasicSalary - salaryDeduction,
                PaymentDate = DateTime.Now.AddDays(7),
                IsPaid = false
            };
        }

        private void ValidateApprover(int approverId)
        {
            var approver = _userRepository.GetById(approverId);

            if (!RoleExtensions.CanManagePayroll(approver.Role))
            {
                _logger.Error("Approver with ID {ApproverId} not authorized", approverId);
                throw new UnauthorizedAccessException($"Approver with ID {approverId} not authorized");
            }
        }

        private Payroll GetPayroll(int payrollId)
        {
            var payroll = _payrollRepository.GetById(payrollId);

            if (payroll == null)
            {
                _logger.Error("Payroll with ID {PayrollId} not found", payrollId);
                throw new KeyNotFoundException($"Payroll with ID {payrollId} not found");
            }

            return payroll;
        }

        private void UpdatePayrollAsPaid(Payroll payroll)
        {
            payroll.IsPaid = true;
            payroll.PaymentDate = DateTime.Now;
            _payrollRepository.Update(payroll);
        }

        #endregion
    }
}