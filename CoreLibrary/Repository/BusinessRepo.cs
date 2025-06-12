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

        public InMemoryBusinessTripRepository(ILogger logger)
        {
            _logger = logger.ForContext<InMemoryBusinessTripRepository>();
        }

        public IEnumerable<BusinessTrip> GetAll()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving all business trips");
                return new List<BusinessTrip>(_trips);
            }
        }

        public BusinessTrip GetById(int id)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving business trip by ID: {TripId}", id);
                return FindTrip(t => t.Id == id) ?? throw TripNotFound(id);
            }
        }

        public IEnumerable<BusinessTrip> GetByUserId(int userId)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving business trips for user ID: {UserId}", userId);
                var trips = _trips.Where(t => t.UserId == userId).ToList();

                if (trips.Count == 0)
                {
                    _logger.Warning("No business trips found for user ID: {UserId}", userId);
                    throw TripNotFound(userId);
                }

                return trips;
            }
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving pending business trip requests");
                return _trips.Where(t => t.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(BusinessTrip trip)
        {
            lock (_lock)
            {
                _logger.Information("Adding business trip for user ID: {UserId}", trip.UserId);
                trip.Id = GenerateId();
                _trips.Add(trip);
                _logger.Information("Business trip added successfully with ID: {TripId}", trip.Id);
            }
        }

        public void Update(BusinessTrip trip)
        {
            lock (_lock)
            {
                _logger.Information("Updating business trip ID: {TripId}", trip.Id);

                var index = _trips.FindIndex(t => t.Id == trip.Id);
                if (index < 0)
                {
                    _logger.Warning("Business trip ID: {TripId} not found for update", trip.Id);
                    throw TripNotFound(trip.Id);
                }

                _trips[index] = trip;
                _logger.Information("Business trip ID: {TripId} updated successfully", trip.Id);
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information("Deleting business trip ID: {TripId}", id);

                var count = _trips.RemoveAll(t => t.Id == id);
                if (count == 0)
                {
                    _logger.Warning("Business trip ID: {TripId} not found for deletion", id);
                    throw TripNotFound(id);
                }

                _logger.Information("Business trip ID: {TripId} deleted successfully", id);
            }
        }

        public int GenerateId()
        {
            lock (_lock)
            {
                _logger.Debug("Generating new business trip ID");
                return _trips.Count == 0 ? 1 : _trips.Max(t => t.Id) + 1;
            }
        }

        #region Private Helper Methods

        private BusinessTrip? FindTrip(Func<BusinessTrip, bool> predicate)
        {
            return _trips.FirstOrDefault(predicate);
        }

        private KeyNotFoundException TripNotFound(int id)
        {
            return new KeyNotFoundException($"Business trip with ID {id} not found");
        }

        #endregion
    }
}