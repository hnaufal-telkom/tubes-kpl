using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Service
{
    public class BusinessTripService : IBusinessTripService
    {
        private readonly IBusinessTripRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public BusinessTripService(IBusinessTripRepository repository, IUserRepository userRepository, ILogger logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _logger = logger.ForContext<BusinessTripService>();
        }

        public BusinessTrip SubmitRequest(int userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost)
        {
            _logger.Information("Submitting business trip request for user ID: {UserId}", userId);

            ValidateTripDates(startDate, endDate);
            ValidateCost(estimateCost);

            var user = GetUser(userId);

            var trip = CreateBusinessTrip(userId, destination, startDate, endDate, purpose, estimateCost);

            _repository.Add(trip);
            _logger.Information("Business trip request submitted successfully for user ID: {UserId}", userId);

            return trip;
        }

        public void ApproveRequest(int tripId, int approverId)
        {
            _logger.Information("Approving business trip request ID: {TripId} by approver ID: {ApproverId}", tripId, approverId);

            var trip = GetTrip(tripId);
            var approver = ValidateApprover(approverId);

            UpdateTripStatus(trip, RequestStatus.Approved, approverId);

            _logger.Information("Business trip request ID: {TripId} approved successfully by approver ID: {ApproverId}", tripId, approverId);
        }

        public void RejectRequest(int tripId, int approverId, string reason)
        {
            _logger.Information("Rejecting business trip request ID: {TripId} by approver ID: {ApproverId}", tripId, approverId);

            var trip = GetTrip(tripId);
            var approver = ValidateApprover(approverId);

            UpdateTripStatus(trip, RequestStatus.Rejected, approverId, reason);

            _logger.Information("Business trip request ID: {TripId} rejected successfully by approver ID: {ApproverId}", tripId, approverId);
        }

        public void UpdateActualCost(int tripId, decimal actualCost)
        {
            _logger.Information("Updating actual cost for business trip ID: {TripId}", tripId);

            var trip = GetTrip(tripId);
            ValidateCost(actualCost);

            trip.ActualCost = actualCost;
            _repository.Update(trip);

            _logger.Information("Actual cost updated successfully for business trip ID: {TripId}", tripId);
        }

        public IEnumerable<BusinessTrip> GetAllTrips()
        {
            _logger.Information("Getting all business trips");
            return _repository.GetAll();
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            _logger.Information("Getting all pending business trip requests");
            return _repository.GetPendingRequests();
        }

        public BusinessTrip GetTripById(int tripId) => GetTrip(tripId);

        public IEnumerable<BusinessTrip> GetByUserId(int userId)
        {
            _logger.Information("Getting business trips by user ID: {UserId}", userId);

            var trips = _repository.GetByUserId(userId);

            if (!trips.Any())
            {
                _logger.Warning("No business trips found for user ID: {UserId}", userId);
                throw new KeyNotFoundException("No business trips found for this user");
            }

            return trips;
        }

        #region Private Helper Methods

        private void ValidateTripDates(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Today)
            {
                _logger.Error("Start date cannot be in the past");
                throw new ArgumentException("Start date cannot be in the past");
            }

            if (endDate < startDate)
            {
                _logger.Error("End date cannot be before start date");
                throw new ArgumentException("End date cannot be before start date");
            }
        }

        private void ValidateCost(decimal cost)
        {
            if (cost < 0)
            {
                _logger.Error("Cost cannot be negative: {Cost}", cost);
                throw new ArgumentException("Cost cannot be negative");
            }
        }

        private User GetUser(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                _logger.Warning("User with ID: {UserId} not found", userId);
                throw new KeyNotFoundException("User not found");
            }

            return user;
        }

        private BusinessTrip CreateBusinessTrip(int userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost) =>
            new BusinessTrip
            {
                Id = _repository.GenerateId(),
                UserId = userId,
                Destination = destination,
                StartDate = startDate,
                EndDate = endDate,
                Purpose = purpose,
                EstimatedCost = estimateCost,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

        private BusinessTrip GetTrip(int tripId)
        {
            var trip = _repository.GetById(tripId);

            if (trip == null)
            {
                _logger.Warning("Business trip with ID: {TripId} not found", tripId);
                throw new KeyNotFoundException("Business trip not found");
            }

            return trip;
        }

        private User ValidateApprover(int approverId)
        {
            var approver = _userRepository.GetById(approverId);

            if (!RoleExtensions.CanApproveTrips(approver.Role))
            {
                _logger.Warning("Approver with ID: {ApproverId} is not authorized", approverId);
                throw new UnauthorizedAccessException("Approver not authorized");
            }

            return approver;
        }

        private void UpdateTripStatus(BusinessTrip trip, RequestStatus status, int approverId, string rejectionReason = null)
        {
            trip.Status = status;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;

            if (status == RequestStatus.Rejected)
            {
                trip.RejectionReason = rejectionReason;
            }

            _repository.Update(trip);
        }

        #endregion
    }
}