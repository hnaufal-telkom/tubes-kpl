using CoreLibrary.ModelLib;
using CoreLibrary.Repository;
using CoreLibrary.Service;
using FluentAssertions;
using Serilog;

namespace CoreLibrary.Tests.ServiceTest
{
    public class BusinessTripServiceTests
    {
        private readonly UserService _userService;
        private readonly BusinessTripService _businessTripService;
        private readonly User _adminUser;
        private readonly User _employeeUser;
        private readonly BusinessTrip _businessTrip;
        private readonly ILogger _logger;

        public BusinessTripServiceTests()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var userRepository = new InMemoryUserRepository(_logger);
            var businessTripRepository = new InMemoryBusinessTripRepository(_logger);

            _userService = new UserService(userRepository, _logger);
            _businessTripService = new BusinessTripService(businessTripRepository, userRepository, _logger);

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

            _businessTrip = _businessTripService.SubmitRequest(
                userId: _employeeUser.Id,
                destination: "Jakarta",
                startDate: DateTime.Now,
                endDate: DateTime.Now.AddDays(5),
                purpose: "Business Meeting",
                estimateCost: 1000000
            );
        }

        #region Success Tests

        [Fact]
        public void SubmitRequest_WithValidData_CreatesBusinessTrip()
        {
            var result = _businessTripService.SubmitRequest(
                userId: _employeeUser.Id,
                destination: "Bandung",
                startDate: DateTime.Now.AddDays(1),
                endDate: DateTime.Now.AddDays(6),
                purpose: "Client Meeting",
                estimateCost: 2000000
            );

            result.Should().NotBeNull();
            result.Destination.Should().Be("Bandung");
            result.Status.Should().Be(RequestStatus.Pending);
            result.UserId.Should().Be(_employeeUser.Id);
        }

        [Fact]
        public void ApproveRequest_ByAdmin_ApprovesTrip()
        {
            _businessTripService.ApproveRequest(_businessTrip.Id, _adminUser.Id);

            var trip = _businessTripService.GetTripById(_businessTrip.Id);
            trip.Status.Should().Be(RequestStatus.Approved);
            trip.ApproverId.Should().Be(_adminUser.Id);
        }

        [Fact]
        public void RejectRequest_ByAdmin_RejectsTrip()
        {
            _businessTripService.RejectRequest(
                _businessTrip.Id,
                _adminUser.Id,
                "Budget exceeded");

            var trip = _businessTripService.GetTripById(_businessTrip.Id);
            trip.Status.Should().Be(RequestStatus.Rejected);
            trip.RejectionReason.Should().Be("Budget exceeded");
        }

        [Fact]
        public void UpdateActualCost_WithValidAmount_UpdatesCost()
        {
            _businessTripService.UpdateActualCost(_businessTrip.Id, 1500000);

            var trip = _businessTripService.GetTripById(_businessTrip.Id);
            trip.ActualCost.Should().Be(1500000);
        }

        [Fact]
        public void GetAllTrips_ReturnsAllTrips()
        {
            var trips = _businessTripService.GetAllTrips();

            trips.Should().Contain(t => t.Id == _businessTrip.Id);
        }

        [Fact]
        public void GetPendingRequests_ReturnsPendingTrips()
        {
            var pendingTrips = _businessTripService.GetPendingRequests();

            pendingTrips.Should().Contain(t =>
                t.Id == _businessTrip.Id &&
                t.Status == RequestStatus.Pending);
        }

        [Fact]
        public void GetTripById_WithValidId_ReturnsTrip()
        {
            var trip = _businessTripService.GetTripById(_businessTrip.Id);

            trip.Should().NotBeNull();
            trip.Id.Should().Be(_businessTrip.Id);
        }

        [Fact]
        public void GetByUserId_WithValidId_ReturnsUserTrips()
        {
            var trips = _businessTripService.GetByUserId(_employeeUser.Id);

            trips.Should().Contain(t => t.Id == _businessTrip.Id);
        }

        #endregion

        #region Failure Tests

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(-1, 1)]
        public void SubmitRequest_WithInvalidDates_ThrowsException(int startDays, int endDays)
        {
            var startDate = DateTime.Now.AddDays(startDays);
            var endDate = DateTime.Now.AddDays(endDays);

            _businessTripService.Invoking(s => s.SubmitRequest(
                _employeeUser.Id,
                "Invalid dates",
                startDate,
                endDate,
                "Should fail",
                1000000))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApproveRequest_ByNonAdmin_ThrowsException()
        {
            _businessTripService.Invoking(s => s.ApproveRequest(
                _businessTrip.Id,
                _employeeUser.Id))
                .Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void RejectRequest_ByNonAdmin_ThrowsException()
        {
            _businessTripService.Invoking(s => s.RejectRequest(
                _businessTrip.Id,
                _employeeUser.Id,
                "Not allowed"))
                .Should().Throw<UnauthorizedAccessException>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1000000)]
        public void UpdateActualCost_WithInvalidAmount_ThrowsException(decimal amount)
        {
            _businessTripService.Invoking(s => s.UpdateActualCost(
                _businessTrip.Id,
                amount))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTripById_WithInvalidId_ThrowsException()
        {
            _businessTripService.Invoking(s => s.GetTripById(9999))
                .Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetByUserId_WithInvalidId_ThrowsException()
        {
            _businessTripService.Invoking(s => s.GetByUserId(9999))
                .Should().Throw<KeyNotFoundException>();
        }

        #endregion
    }
}