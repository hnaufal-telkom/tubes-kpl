using WebAPI.Models;

namespace WebAPI.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private static readonly List<LeaveRequest> _requests = new();
        private static int _nextId = 1;

        public List<LeaveRequest> GetAllLeaveRequests() => _requests;

        public LeaveRequest? GetLeaveRequestById(int id) => _requests.FirstOrDefault(r => r.Id == id);

        public LeaveRequest Create(LeaveRequest leaveRequest)
        {
            leaveRequest.Id = _nextId++;
            leaveRequest.Status = "Pending";
            leaveRequest.CreatedAt = DateTime.UtcNow;
            leaveRequest.UpdateAt = DateTime.UtcNow;
            _requests.Add(leaveRequest);
            return leaveRequest;
        }

        public LeaveRequest? Update(int id, LeaveRequest leaveRequest)
        {
            var existingRequest = GetLeaveRequestById(id);
            if (existingRequest == null) return null;

            existingRequest.EmployeeId = leaveRequest.EmployeeId;
            existingRequest.LeaveType = leaveRequest.LeaveType;
            existingRequest.StartDate = leaveRequest.StartDate;
            existingRequest.EndDate = leaveRequest.EndDate;
            existingRequest.Reason = leaveRequest.Reason;
            existingRequest.UpdateAt = DateTime.UtcNow;

            return existingRequest;
        }

        public bool Delete(int id)
        {
            var existingRequest = GetLeaveRequestById(id);
            if (existingRequest == null) return false;
            return _requests.Remove(existingRequest);
        }

        public LeaveRequest? Approve(int id, string approverNotes)
        {
            var existingRequest = GetLeaveRequestById(id);
            if (existingRequest == null) return null;

            existingRequest.Status = "Approved";
            existingRequest.ApproverNotes = approverNotes;
            existingRequest.UpdateAt = DateTime.UtcNow;

            return existingRequest;
        }

        public LeaveRequest? Reject(int id, string approverNotes)
        {
            var existingRequest = GetLeaveRequestById(id);
            if (existingRequest == null) return null;

            existingRequest.Status = "Rejected";
            existingRequest.ApproverNotes = approverNotes;
            existingRequest.UpdateAt = DateTime.UtcNow;

            return existingRequest;
        }
    }
}
