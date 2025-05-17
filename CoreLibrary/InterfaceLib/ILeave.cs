using CoreLibrary.ModelLib;

namespace CoreLibrary.InterfaceLib
{
    public interface ILeaveRequestRepository
    {
        IEnumerable<LeaveRequest> GetAll();
        LeaveRequest GetById(int id);
        IEnumerable<LeaveRequest> GetByUserId(int userId);
        IEnumerable<LeaveRequest> GetPendingRequests();
        void Add(LeaveRequest request);
        void Update(LeaveRequest request);
        void Delete(int id);
    }

    public interface ILeaveRequestService
    {
        LeaveRequest SubmitRequest(int userId, DateTime startDate, DateTime endDate, string description);
        void ApproveRequest(int requestId, int approverId);
        void RejectRequest(int requestId, int approverId, string reason);
        IEnumerable<LeaveRequest> GetAllRequests();
        IEnumerable<LeaveRequest> GetPendingRequest();
        LeaveRequest GetById(int requestId);
        IEnumerable<LeaveRequest> GetByUserId(int userId);
    }
}
