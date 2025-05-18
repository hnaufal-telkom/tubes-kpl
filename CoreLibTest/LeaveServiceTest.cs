using CoreLibrary.ModelLib;
using CoreLibrary.Repository;
using CoreLibrary.Service;
using FluentAssertions;
using Serilog;

namespace CoreLibrary.Tests.ServiceTest
{
    public class LeaveServiceTests
    {
        private readonly UserService _userService;
        private readonly LeaveService _leaveService;
        private readonly User _adminUser;
        private readonly User _employeeUser;
        private readonly LeaveRequest _leaveRequest;
        private readonly ILogger _logger;

        public LeaveServiceTests()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var repository = new InMemoryUserRepository(_logger);
            var leaveRepository = new InMemoryLeaveRequestRepository(_logger);

            _userService = new UserService(repository, _logger);
            _leaveService = new LeaveService(leaveRepository, _userService, _logger);

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

            _leaveRequest = _leaveService.SubmitRequest(
                userId: _employeeUser.Id,
                startDate: DateTime.Now,
                endDate: DateTime.Now.AddDays(5),
                description: "Vacation"
            );
        }

        #region Success Tests

        [Fact]
        public void SubmitRequest_WithValidData_CreatesRequest()
        {
            var result = _leaveService.SubmitRequest(
                userId: _employeeUser.Id,
                startDate: DateTime.Now.AddDays(1),
                endDate: DateTime.Now.AddDays(6),
                description: "Sick Leave"
            );

            result.Should().NotBeNull();
            result.UserId.Should().Be(_employeeUser.Id);
            result.Description.Should().Be("Sick Leave");
            result.Status.Should().Be(RequestStatus.Pending);
        }

        [Fact]
        public void ApproveRequest_ByAdmin_ApprovesRequest()
        {
            _leaveService.ApproveRequest(_leaveRequest.Id, _adminUser.Id);

            var approvedRequest = _leaveService.GetById(_leaveRequest.Id);
            approvedRequest.Status.Should().Be(RequestStatus.Approved);
            approvedRequest.ApproverId.Should().Be(_adminUser.Id);
        }

        [Fact]
        public void RejectRequest_ByAdmin_RejectsRequest()
        {
            _leaveService.RejectRequest(
                _leaveRequest.Id,
                _adminUser.Id,
                "Nooooooo");

            var rejectedRequest = _leaveService.GetById(_leaveRequest.Id);
            rejectedRequest.Status.Should().Be(RequestStatus.Rejected);
            rejectedRequest.ApproverId.Should().Be(_adminUser.Id);
            rejectedRequest.RejectionReason.Should().Be("Nooooooo");
        }

        [Fact]
        public void GetAllRequests_ReturnsAllRequests()
        {
            var requests = _leaveService.GetAllRequests();

            requests.Should().Contain(lr => lr.Id == _leaveRequest.Id);
        }

        [Fact]
        public void GetPendingRequests_ReturnsOnlyPending()
        {
            var pendingRequests = _leaveService.GetPendingRequest();

            pendingRequests.Should().Contain(lr =>
                lr.Id == _leaveRequest.Id &&
                lr.Status == RequestStatus.Pending);
        }

        [Fact]
        public void GetById_WithValidId_ReturnsRequest()
        {
            var request = _leaveService.GetById(_leaveRequest.Id);

            request.Should().NotBeNull();
            request.Id.Should().Be(_leaveRequest.Id);
        }

        #endregion

        #region Failure Tests

        [Theory]
        [InlineData(1, -1)]
        [InlineData(-2, -1)]
        public void SubmitRequest_WithInvalidDates_ThrowsException(int startDays, int endDays)
        {
            var startDate = DateTime.Now.AddDays(startDays);
            var endDate = DateTime.Now.AddDays(endDays);

            _leaveService.Invoking(s => s.SubmitRequest(
                _employeeUser.Id,
                startDate,
                endDate,
                "Invalid dates"))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApproveRequest_ByNonAdmin_ThrowsException()
        {
            _leaveService.Invoking(s => s.ApproveRequest(
                _leaveRequest.Id,
                _employeeUser.Id))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void RejectRequest_ByNonAdmin_ThrowsException()
        {
            _leaveService.Invoking(s => s.RejectRequest(
                _leaveRequest.Id,
                _employeeUser.Id,
                "Not allowed"))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetById_WithInvalidId_ThrowsException()
        {
            _leaveService.Invoking(s => s.GetById(999))
                .Should().Throw<KeyNotFoundException>();
        }

        #endregion
    }
}