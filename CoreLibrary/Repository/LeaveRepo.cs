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

        public IEnumerable<LeaveRequest> GetAll()
        {
            lock (_lock)
            {
                _logger.Information("Getting all Leave Request");
                return _requests.ToList();
            }
        }

        public LeaveRequest GetById(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Getting leave request by ID: {id}");
                var request = _requests.FirstOrDefault(x => x.Id == id);
                if (request == null)
                {
                    _logger.Warning($"Leave request with ID {id} not found");
                    throw new KeyNotFoundException("Leave request not found");
                }
                return request;
            }
        }

        public IEnumerable<LeaveRequest> GetByUserId(int userId)
        {
            lock (_lock)
            {
                _logger.Information($"Getting leave request for user ID: {userId}");
                return _requests.Where(x => x.UserId == userId);
            }
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            lock (_lock)
            {
                _logger.Information("Getting all pending leave requests");
                return _requests.Where(r => r.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(LeaveRequest request)
        {
            lock ( _lock)
            {
                _logger.Information($"Adding leave request for user ID: {request.UserId}");
                _requests.Add(request);
                _logger.Information($"Leave request for user ID {request.UserId} added successfully");
            }
        }

        public void Update(LeaveRequest request)
        {
            lock ( _lock)
            {
                _logger.Information($"Updating leave request for user ID: {request.UserId}");
                var index = _requests.FindIndex(r => r.Id == request.Id);
                if (index >= 0)
                {
                    _requests[index] = request;
                    _logger.Information($"Leave request for user ID {request.UserId} updated successfully");
                }
                else
                {
                    _logger.Warning($"Leave request with ID {request.Id} not found for update");
                    throw new KeyNotFoundException("Leave request not found");
                }
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Deleting leave request with ID: {id}");
                var count = _requests.RemoveAll(r => r.Id == id);
                if (count > 0)
                {
                    _logger.Warning($"Request with ID {id} not found");
                    throw new KeyNotFoundException("Request not found");
                }
                else
                {
                    _logger.Information($"Request wtih ID {id} deleted successfully");
                }
            }
        }
    }
}
