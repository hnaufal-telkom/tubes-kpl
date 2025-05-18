using CoreLibrary.ModelLib;
using CoreLibrary.Repository;
using CoreLibrary.Service;
using FluentAssertions;
using Serilog;
using Xunit;

namespace CoreLibrary.Tests.ServiceTest
{
    public class PayrollServiceTests
    {
        private readonly UserService _userService;
        private readonly PayrollService _payrollService;
        private readonly User _adminUser;
        private readonly User _employeeUser;
        private readonly Payroll _payroll;
        private readonly ILogger _logger;

        public PayrollServiceTests()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var payrollRepo = new InMemoryPayrollRepository(_logger);
            var userRepo = new InMemoryUserRepository(_logger);
            var leaveRepo = new InMemoryLeaveRequestRepository(_logger);
            var businessTripRepo = new InMemoryBusinessTripRepository(_logger);

            _userService = new UserService(userRepo, _logger);
            _payrollService = new PayrollService(payrollRepo, userRepo, leaveRepo, businessTripRepo, _logger);

            _adminUser = _userService.Register(
                name: "Fizryan",
                email: "fizryan@mail.com",
                password: "password123",
                role: Role.SysAdmin,
                basicSalary: 50000000
            );

            _employeeUser = _userService.Register(
                name: "Naufal",
                email: "naufal@mail.com",
                password: "pass0000122231",
                role: Role.Employee,
                basicSalary: 2000000
            );

            _payroll = _payrollService.GeneratePayroll(
                userId: _employeeUser.Id,
                periodStart: DateTime.Now,
                periodEnd: DateTime.Now.AddDays(30)
            );
        }

        #region Success Tests

        [Fact]
        public void GeneratePayroll_WithValidData_CreatesPayroll()
        {
            var result = _payrollService.GeneratePayroll(
                userId: _employeeUser.Id,
                periodStart: DateTime.Now.AddDays(1),
                periodEnd: DateTime.Now.AddDays(31)
            );

            result.Should().NotBeNull();
            result.UserId.Should().Be(_employeeUser.Id);
            result.BasicSalary.Should().Be(_employeeUser.BasicSalary);
            result.IsPaid.Should().BeFalse();
        }

        [Fact]
        public void MarkAsPaid_ByAdmin_MarksPayrollAsPaid()
        {
            _payrollService.MarkAsPaid(_payroll.Id, _adminUser.Id);

            var payroll = _payrollService.GetPayrollsByUserId(_employeeUser.Id).First();
            payroll.IsPaid.Should().BeTrue();
            payroll.Id.Should().Be(_adminUser.Id);
        }

        [Fact]
        public void GetPayrollsByUserId_WithValidId_ReturnsPayrolls()
        {
            var payrolls = _payrollService.GetPayrollsByUserId(_employeeUser.Id);

            payrolls.Should().NotBeNullOrEmpty();
            payrolls.Should().ContainSingle();
            payrolls.First().UserId.Should().Be(_employeeUser.Id);
        }

        [Fact]
        public void GetPayrollsByPeriod_WithValidDates_ReturnsPayrolls()
        {
            var payrolls = _payrollService.GetPayrollsByPeriod(
                start: DateTime.Now.AddDays(-1),
                end: DateTime.Now.AddDays(40)
            );

            payrolls.Should().NotBeNullOrEmpty();
            payrolls.Should().Contain(p => p.Id == _payroll.Id);
        }

        #endregion

        #region Failure Tests

        [Fact]
        public void GeneratePayroll_WithInvalidUserId_ThrowsException()
        {
            Assert.Throws<KeyNotFoundException>(() => _payrollService.GeneratePayroll(
                userId: 9999,
                periodStart: DateTime.Now,
                periodEnd: DateTime.Now.AddDays(30)
            ));
        }

        [Fact]
        public void MarkAsPaid_WithInvalidApprover_ThrowsException()
        {
            _payrollService.Invoking(s => s.MarkAsPaid(
                payrollId: _payroll.Id,
                approverId: 9999)
            ).Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetPayrollsByUserId_WithInvalidId_ThrowsException()
        {
            _payrollService.Invoking(s => s.GetPayrollsByUserId(9999))
                .Should().Throw<KeyNotFoundException>();
        }

        [Theory]
        [InlineData(1, -1)]
        [InlineData(2, 1)]
        public void GetPayrollsByPeriod_WithInvalidDates_ThrowsException(int startDays, int endDays)
        {
            var startDate = DateTime.Now.AddDays(startDays);
            var endDate = DateTime.Now.AddDays(endDays);

            _payrollService.Invoking(s => s.GetPayrollsByPeriod(startDate, endDate))
                .Should().Throw<ArgumentException>();
        }

        #endregion
    }
}