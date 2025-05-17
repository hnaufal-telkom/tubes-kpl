using CoreLibrary.ModelLib;

namespace CoreLibrary.InterfaceLib
{
    public interface IBusinessTripRepository
    {
        IEnumerable<BusinessTrip> GetAll();
        BusinessTrip GetById(int id);
        IEnumerable<BusinessTrip> GetByUserId(int userId);
        IEnumerable<BusinessTrip> GetPendingRequests();
        void Add(BusinessTrip businessTrip);
        void Update(BusinessTrip businessTrip);
        void Delete(int id);
    }

    public interface IBusinessTripService
    {
        BusinessTrip SubmitRequest(int userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost);
        void ApproveRequest(int tripId, int approverId);
        void RejectRequest(int tripId, int approverId, string reason);
        void UpdateActualCost(int tripId, decimal actualCost);
        IEnumerable<BusinessTrip> GetAllTrips();
        IEnumerable<BusinessTrip> GetPendingRequests();
        BusinessTrip GetTripById(int tripId);
        IEnumerable<BusinessTrip> GetByUserId(int userId);
    }
}