using MainLibrary;
using System.Collections.Generic;

namespace WebAPI.Service
{
    public class BusinessTripService
    {
        private readonly IBusinessTripRepository _tripRepository;

        public BusinessTripService(IBusinessTripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        public IEnumerable<BusinessTrip> GetPendingBusinessTrips() =>
            _tripRepository.GetPendingBusinessRequest();

        public BusinessTrip GetBusinessTripById(string id) =>
            _tripRepository.GetBusinessTripById(id);

        public IEnumerable<BusinessTrip> GetByUserId(string userId) =>
            _tripRepository.GetBusinessTripByUserId(userId);
    }
}