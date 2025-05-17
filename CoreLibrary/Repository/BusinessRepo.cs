using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Repository
{
    public class InMemoryBusinessTripRepository : IBusinessTripRepository
    {
        private readonly List<BusinessTrip> _trips = new();
        private readonly object _lock = new();
        private readonly ILogger _logger;

        public IEnumerable<BusinessTrip> GetAll()
        {
            lock (_lock)
            {
                _logger.Information($"Getting all business trip");
                return _trips.ToList();
            }
        }

        public BusinessTrip GetById(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Getting business trip by ID: {id}");
                var trip = _trips.FirstOrDefault(t => t.Id == id);
                if ( trip == null )
                {
                    _logger.Warning($"Business trip with ID {id} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                return trip;
            }
        }

        public IEnumerable<BusinessTrip> GetByUserId(int userId)
        {
            lock (_lock)
            {
                _logger.Information($"Getting business trip by user ID: {userId}");
                var trips = _trips.Where(t => t.UserId == userId).ToList();
                if (trips.Count == 0)
                {
                    _logger.Warning($"No business trips found for user ID {userId}");
                    throw new KeyNotFoundException("No business trips found for this user");
                }
                return trips;
            }
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            lock (_lock)
            {
                _logger.Information("Getting all pending business trip requests");
                return _trips.Where(t => t.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(BusinessTrip trip)
        {
            lock (_lock)
            {
                _logger.Information($"Adding business trip for user ID: {trip.UserId}");
                _trips.Add(trip);
                _logger.Information($"Business trip for user ID {trip.UserId} added successfully");
            }
        }

        public void Update(BusinessTrip trip)
        {
            lock (_lock)
            {
                _logger.Information($"Updating business trip for user ID: {trip.UserId}");
                var index = _trips.FindIndex(t => t.Id == trip.Id);
                if ( index >= 0)
                {
                    _trips[index] = trip;
                    _logger.Information($"Business trip for user ID {trip.UserId} updated successfully");
                }
                else
                {
                    _logger.Warning($"Business trip with ID {trip.Id} not found for update");
                    throw new KeyNotFoundException("Business trip not found");
                }
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Deleting Business trip with ID: {id}");
                var count = _trips.RemoveAll(t => t.Id == id);
                if (count > 0)
                {
                    _logger.Warning($"Business trip with ID {id} not found for deletion");
                    throw new KeyNotFoundException("Business trip not found");
                }
                else
                {
                    _logger.Information($"Business trip with ID {id} deleted successfully");
                }
            }
        }
    }
}
