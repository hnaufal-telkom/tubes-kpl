using System;
using Xunit;
using MainLibrary;
using Serilog;


namespace MainLibraryTest
{
    public class TestFixture : IDisposable
    {
        public TestFixture()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/MainLibraryTest.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Test suite started at {Timestamp}", DateTime.Now);
        }

        public void Dispose()
        {
            Log.Information("Test suite completed at {Timestamp}", DateTime.Now);
            Log.CloseAndFlush();
        }
    }

    public class UserServiceTests : IClassFixture<TestFixture>
    {
        private readonly IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly ILogger _logger;

        public UserServiceTests(TestFixture fixture)
        {
            _userRepository = new InMemoryUserRepository();
            _logger = Log.Logger.ForContext<UserServiceTests>();
            _userService = new UserService(_userRepository, _logger);
        }

        [Fact]
        public void Register_ValidUser_ReturnsUserWithId()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@example.com";
            var password = "password123";
            var role = Role.Employee;

            // Act
            var user = _userService.Register(name, email, password, role);

            // Assert
            Assert.NotNull(user.Id);
            Assert.Equal(name, user.Name);
            Assert.Equal(email, user.Email);
            Assert.Equal(role, user.Role);
            Assert.True(user.IsActive);

            _logger.Information("User registration test passed for {Email}", email);
        }

        [Fact]
        public void Register_InvalidEmail_ThrowsException()
        {
            // Arrange
            var invalidEmail = "invalid-email";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _userService.Register("John Doe", invalidEmail, "password123", Role.Employee));

            Assert.Contains("Invalid email format", ex.Message);
            _logger.Warning("Invalid email test passed as expected");
        }

        [Fact]
        public void Authenticate_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var user = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);

            // Act
            var authenticatedUser = _userService.Authenticate("john@example.com", "password123");

            // Assert
            Assert.NotNull(authenticatedUser);
            Assert.Equal(user.Id, authenticatedUser.Id);
            _logger.Information("Authentication test passed for user {UserId}", user.Id);
        }

        [Fact]
        public void ChangePassword_ValidCurrentPassword_UpdatesPassword()
        {
            // Arrange
            var user = _userService.Register("John Doe", "john@example.com", "oldpassword", Role.Employee);

            // Act
            _userService.ChangePassword(user.Id, "oldpassword", "newpassword");

            // Assert
            var updatedUser = _userService.Authenticate("john@example.com", "newpassword");
            Assert.NotNull(updatedUser);
            _logger.Information("Password change test passed for user {UserId}", user.Id);
        }

        [Fact]
        public void DeactivateUser_SetsIsActiveToFalse()
        {
            // Arrange
            var user = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);

            // Act
            _userService.DeactivateUser(user.Id);

            // Assert
            var deactivatedUser = _userService.GetUserById(user.Id);
            Assert.False(deactivatedUser.IsActive);
            _logger.Information("User deactivation test passed for user {UserId}", user.Id);
        }
    }

    public class LeaveServiceTests : IClassFixture<TestFixture>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRequestRepository _leaveRepository;
        private readonly UserService _userService;
        private readonly LeaveService _leaveService;
        private readonly ILogger _logger;

        public LeaveServiceTests(TestFixture fixture)
        {
            _userRepository = new InMemoryUserRepository();
            _leaveRepository = new InMemoryLeaveRequestRepository();
            _logger = Log.Logger.ForContext<LeaveServiceTests>();
            _userService = new UserService(_userRepository, _logger);
            _leaveService = new LeaveService(_leaveRepository, _userService, _logger);
        }

        [Fact]
        public void SubmitRequest_ValidDates_CreatesRequest()
        {
            // Arrange
            var user = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var startDate = DateTime.Today.AddDays(1);
            var endDate = startDate.AddDays(2);

            // Act
            var request = _leaveService.SubmitRequest(user.Id, startDate, endDate, "Vacation");

            // Assert
            Assert.NotNull(request.Id);
            Assert.Equal(RequestStatus.Pending, request.Status);
            Assert.Equal(3, request.Duration);
            _logger.Information("Leave request submission test passed for user {UserId}", user.Id);
        }

        [Fact]
        public void ApproveRequest_UpdatesStatusAndDeductsLeaveDays()
        {
            // Arrange
            var employee = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var supervisor = _userService.Register("Supervisor", "super@example.com", "password123", Role.Supervisor);
            var startDate = DateTime.Today.AddDays(1);
            var endDate = startDate.AddDays(2);
            var request = _leaveService.SubmitRequest(employee.Id, startDate, endDate, "Vacation");

            // Act
            _leaveService.ApproveRequest(request.Id, supervisor.Id);

            // Assert
            var updatedRequest = _leaveRepository.GetById(request.Id);
            var updatedUser = _userService.GetUserById(employee.Id);

            Assert.Equal(RequestStatus.Approved, updatedRequest.Status);
            Assert.Equal(9, updatedUser.RemainingLeaveDays); // 12 - 3 days
            _logger.Information("Leave approval test passed for request {RequestId}", request.Id);
        }

        [Fact]
        public void RejectRequest_UpdatesStatusWithReason()
        {
            // Arrange
            var employee = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var supervisor = _userService.Register("Supervisor", "super@example.com", "password123", Role.Supervisor);
            var request = _leaveService.SubmitRequest(employee.Id, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), "Vacation");

            // Act
            _leaveService.RejectRequest(request.Id, supervisor.Id, "Too many people on leave");

            // Assert
            var updatedRequest = _leaveRepository.GetById(request.Id);
            Assert.Equal(RequestStatus.Rejected, updatedRequest.Status);
            Assert.Equal("Too many people on leave", updatedRequest.RejectionReason);
            _logger.Information("Leave rejection test passed for request {RequestId}", request.Id);
        }
    }

    public class BusinessTripServiceTests : IClassFixture<TestFixture>
    {
        private readonly IUserRepository _userRepository;
        private readonly IBusinessTripRepository _tripRepository;
        private readonly UserService _userService;
        private readonly BusinessTripService _tripService;
        private readonly ILogger _logger;

        public BusinessTripServiceTests(TestFixture fixture)
        {
            _userRepository = new InMemoryUserRepository();
            _tripRepository = new InMemoryBusinessTripRepository();
            _logger = Log.Logger.ForContext<BusinessTripServiceTests>();
            _userService = new UserService(_userRepository, _logger);
            _tripService = new BusinessTripService(_tripRepository, _userService, _logger);
        }

        [Fact]
        public void SubmitRequest_ValidData_CreatesRequest()
        {
            // Arrange
            var user = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);

            // Act
            var trip = _tripService.SubmitRequest(
                user.Id,
                "New York",
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3),
                "Conference",
                1000m);

            // Assert
            Assert.NotNull(trip.Id);
            Assert.Equal(RequestStatus.Pending, trip.Status);
            Assert.Equal(1000m, trip.EstimatedCost);
            _logger.Information("Business trip submission test passed for user {UserId}", user.Id);
        }

        [Fact]
        public void ApproveRequest_UpdatesStatus()
        {
            // Arrange
            var employee = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var supervisor = _userService.Register("Supervisor", "super@example.com", "password123", Role.Supervisor);
            var trip = _tripService.SubmitRequest(
                employee.Id,
                "New York",
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3),
                "Conference",
                1000m);

            // Act
            _tripService.ApproveRequest(trip.Id, supervisor.Id);

            // Assert
            var updatedTrip = _tripRepository.GetById(trip.Id);
            Assert.Equal(RequestStatus.Approved, updatedTrip.Status);
            Assert.Equal(supervisor.Id, updatedTrip.ApproverId);
            _logger.Information("Business trip approval test passed for trip {TripId}", trip.Id);
        }
    }

    public class PayrollServiceTests : IClassFixture<TestFixture>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPayrollRepository _payrollRepository;
        private readonly ILeaveRequestRepository _leaveRepository;
        private readonly IBusinessTripRepository _tripRepository;
        private readonly UserService _userService;
        private readonly PayrollService _payrollService;
        private readonly ILogger _logger;

        public PayrollServiceTests(TestFixture fixture)
        {
            _userRepository = new InMemoryUserRepository();
            _payrollRepository = new InMemoryPayrollRepository();
            _leaveRepository = new InMemoryLeaveRequestRepository();
            _tripRepository = new InMemoryBusinessTripRepository();
            _logger = Log.Logger.ForContext<PayrollServiceTests>();
            _userService = new UserService(_userRepository, _logger);
            _payrollService = new PayrollService(
                _payrollRepository,
                _userService,
                _leaveRepository,
                _tripRepository,
                _logger);
        }

        [Fact]
        public void GeneratePayroll_ForEmployee_CreatesPayroll()
        {
            // Arrange
            var employee = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var periodStart = new DateTime(2023, 1, 1);
            var periodEnd = new DateTime(2023, 1, 31);

            // Act
            var payroll = _payrollService.GeneratePayroll(employee.Id, periodStart, periodEnd);

            // Assert
            Assert.NotNull(payroll.Id);
            Assert.Equal(employee.Id, payroll.UserId);
            Assert.False(payroll.IsPaid);
            _logger.Information("Payroll generation test passed for user {UserId}", employee.Id);
        }

        [Fact]
        public void MarkAsPaid_UpdatesPaymentStatus()
        {
            // Arrange
            var employee = _userService.Register("John Doe", "john@example.com", "password123", Role.Employee);
            var payroll = _payrollService.GeneratePayroll(employee.Id, new DateTime(2023, 1, 1), new DateTime(2023, 1, 31));

            // Act
            _payrollService.MarkAsPaid(payroll.Id);

            // Assert
            var updatedPayroll = _payrollRepository.GetById(payroll.Id);
            Assert.True(updatedPayroll.IsPaid);
            _logger.Information("Payroll payment test passed for payroll {PayrollId}", payroll.Id);
        }
    }

    public class RoleExtensionsTests
    {
        [Fact]
        public void CanManageUsers_ReturnsCorrectValues()
        {
            Assert.False(Role.Employee.CanManageUsers());
            Assert.True(Role.HRD.CanManageUsers());
            Assert.True(Role.Supervisor.CanManageUsers());
            Log.Logger.Information("RoleExtensions.CanManageUsers test passed");
        }

        [Fact]
        public void CanApproveLeave_ReturnsCorrectValues()
        {
            Assert.False(Role.Employee.CanApproveLeave());
            Assert.False(Role.HRD.CanApproveLeave());
            Assert.True(Role.Supervisor.CanApproveLeave());
            Log.Logger.Information("RoleExtensions.CanApproveLeave test passed");
        }
    }
}