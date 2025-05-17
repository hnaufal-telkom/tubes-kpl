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
            _logger = logger;
        }

        public BusinessTrip SubmitRequest(int userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost)
        {
            try
            {
                _logger.Information($"Submitting business trip request for user ID: {userId}");
                
                if (startDate < DateTime.Now || endDate < startDate)
                {
                    _logger.Error("Invalid date range");
                    throw new ArgumentException("Invalid date range");
                }

                if (estimateCost < 0)
                {
                    _logger.Error("Estimated cost cannot be negative");
                    throw new ArgumentException("Estimated cost cannot be negative");
                }

                var user = _userRepository.GetById(userId);
                if (user == null)
                {
                    _logger.Warning($"User with ID {userId} not found");
                    throw new KeyNotFoundException("User not found");
                }

                var allUsers = _repository.GetAll().ToList();
                int newId = 0;
                while (allUsers.Any(u => u.Id == newId))
                {
                    newId++;
                }

                var trip = new BusinessTrip
                {
                    Id = newId,
                    UserId = userId,
                    Destination = destination,
                    StartDate = startDate,
                    EndDate = endDate,
                    Purpose = purpose,
                    EstimatedCost = estimateCost,
                    RequestDate = DateTime.Now,
                    Status = RequestStatus.Pending
                };
                _repository.Add(trip);
                _logger.Information($"Business trip request submitted successfully for user ID: {userId}");
                return trip;
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("User not found");
                throw new KeyNotFoundException("User not found");
            }
            catch (ArgumentException)
            {
                _logger.Error("Invalid Request");
                throw new ArgumentException("Invalid Request");
            }
        }

        public void ApproveRequest(int tripId, int approverId)
        {
            try
            {
                _logger.Information($"Approving business trip request ID: {tripId} by approver ID: {approverId}");
                var trip = _repository.GetById(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Business trip with ID {tripId} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                var approver = _userRepository.GetById(approverId);
                if (RoleExtensions.CanApproveTrips(approver.Role) || RoleExtensions.CanManageSystem(approver.Role))
                {
                    _logger.Warning($"Approver with ID {approverId} not found or not an admin");
                    throw new KeyNotFoundException("Approver not found or not an admin");
                }
                trip.Status = RequestStatus.Approved;
                trip.ApproverId = approverId;
                trip.ApprovalDate = DateTime.Now;
                _repository.Update(trip);
                _logger.Information($"Business trip request ID: {tripId} approved successfully by approver ID: {approverId}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("Business trip or approver not found");
                throw new KeyNotFoundException("Business trip or approver not found");
            }
        }

        public void RejectRequest(int tripId, int approverId, string reason)
        {
            try
            {
                _logger.Information($"Rejecting business trip request ID: {tripId} by approver ID: {approverId}");
                var trip = _repository.GetById(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Business trip with ID {tripId} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                var approver = _userRepository.GetById(approverId);
                if (RoleExtensions.CanApproveTrips(approver.Role) || RoleExtensions.CanManageSystem(approver.Role))
                {
                    _logger.Warning($"Approver with ID {approverId} not found or not an admin");
                    throw new KeyNotFoundException("Approver not found or not an admin");
                }
                trip.Status = RequestStatus.Rejected;
                trip.ApproverId = approverId;
                trip.RejectionReason = reason;
                trip.ApprovalDate = DateTime.Now;
                _repository.Update(trip);
                _logger.Information($"Business trip request ID: {tripId} rejected successfully by approver ID: {approverId}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("Business trip or approver not found");
                throw new KeyNotFoundException("Business trip or approver not found");
            }
        }

        public void UpdateActualCost(int tripId, decimal actualCost)
        {
            try
            {
                _logger.Information($"Updating actual cost for business trip ID: {tripId}");
                var trip = _repository.GetById(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Business trip with ID {tripId} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                if (actualCost < 0)
                {
                    _logger.Error("Actual cost cannot be negative");
                    throw new ArgumentException("Actual cost cannot be negative");
                }
                trip.ActualCost = actualCost;
                _repository.Update(trip);
                _logger.Information($"Actual cost updated successfully for business trip ID: {tripId}");
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("Business trip not found");
                throw new KeyNotFoundException("Business trip not found");
            }
            catch (ArgumentException)
            {
                _logger.Error("Invalid Request");
                throw new ArgumentException("Invalid Request");
            }
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

        public BusinessTrip GetTripById(int tripId)
        {
            _logger.Information($"Getting business trip by ID: {tripId}");
            var trip = _repository.GetById(tripId);
            if (trip == null)
            {
                _logger.Warning($"Business trip with ID {tripId} not found");
                throw new KeyNotFoundException("Business trip not found");
            }
            return trip;
        }

        public IEnumerable<BusinessTrip> GetByUserId(int userId)
        {
            _logger.Information($"Getting business trips by user ID: {userId}");
            var trips = _repository.GetByUserId(userId);
            if (trips == null || !trips.Any())
            {
                _logger.Warning($"No business trips found for user ID {userId}");
                throw new KeyNotFoundException("No business trips found for this user");
            }
            return trips;
        }
    }
}
