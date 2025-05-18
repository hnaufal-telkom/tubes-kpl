using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Repository
{
    public class InMemoryLeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly List<LeaveRequest> _requests = new();
        private readonly object _lock = new();
        private readonly ILogger _logger;

        public InMemoryLeaveRequestRepository(ILogger logger)
        {
            _logger = logger.ForContext<InMemoryLeaveRequestRepository>();
        }

        public IEnumerable<LeaveRequest> GetAll()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving all leave requests");
                return new List<LeaveRequest>(_requests);
            }
        }

        public LeaveRequest GetById(int id)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving leave request by ID: {RequestId}", id);
                return FindRequest(r => r.Id == id) ?? throw RequestNotFound(id);
            }
        }

        public IEnumerable<LeaveRequest> GetByUserId(int userId)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving leave requests for user ID: {UserId}", userId);
                return _requests.Where(r => r.UserId == userId).ToList();
            }
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving pending leave requests");
                return _requests.Where(r => r.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(LeaveRequest request)
        {
            lock (_lock)
            {
                _logger.Information("Adding leave request for user ID: {UserId}", request.UserId);
                request.Id = GenerateId();
                _requests.Add(request);
                _logger.Information("Leave request added successfully with ID: {RequestId}", request.Id);
            }
        }

        public void Update(LeaveRequest request)
        {
            lock (_lock)
            {
                _logger.Information("Updating leave request ID: {RequestId}", request.Id);

                var index = _requests.FindIndex(r => r.Id == request.Id);
                if (index < 0)
                {
                    _logger.Warning("Leave request ID {RequestId} not found for update", request.Id);
                    throw RequestNotFound(request.Id);
                }

                _requests[index] = request;
                _logger.Information("Leave request ID {RequestId} updated successfully", request.Id);
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information("Deleting leave request ID: {RequestId}", id);

                var count = _requests.RemoveAll(r => r.Id == id);
                if (count == 0)
                {
                    _logger.Warning("Leave request ID {RequestId} not found for deletion", id);
                    throw RequestNotFound(id);
                }

                _logger.Information("Leave request ID {RequestId} deleted successfully", id);
            }
        }

        public int GenerateId()
        {
            lock (_lock)
            {
                _logger.Debug("Generating new leave request ID");
                return _requests.Count == 0 ? 1 : _requests.Max(r => r.Id) + 1;
            }
        }

        #region Private Helper Methods

        private LeaveRequest? FindRequest(Func<LeaveRequest, bool> predicate)
        {
            return _requests.FirstOrDefault(predicate);
        }

        private KeyNotFoundException RequestNotFound(int id)
        {
            return new KeyNotFoundException($"Leave request with ID {id} not found");
        }

        #endregion
    }
}