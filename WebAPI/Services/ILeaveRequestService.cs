using WebAPI.Models;

namespace WebAPI.Services
{
    public interface ILeaveRequestService
    {
        List<LeaveRequest> GetAllLeaveRequests();

        LeaveRequest? GetLeaveRequestById(int id);

        LeaveRequest Create(LeaveRequest leaveRequest);

        LeaveRequest? Update(int id, LeaveRequest leaveRequest);

        bool Delete(int id);

        LeaveRequest? Approve(int id, string approverNotes);
        LeaveRequest? Reject(int id, string approverNotes);

    }
}
